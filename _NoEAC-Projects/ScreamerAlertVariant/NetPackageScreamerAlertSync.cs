using System.Collections.Generic;
using UnityEngine;

public class NetPackageScreamerAlertSync : NetPackage
{
    public List<int> screamerIds = new List<int>();
    public List<Vector3> screamerPositions = new List<Vector3>();
    public List<int> hordeIds = new List<int>();
    public string screamerAlertMessage = string.Empty;
    public string hordeAlertMessage = string.Empty;
    public Vector3 hordeAlertPosition = Vector3.zero;
    public float hordeAlertEndTime = 0f;

    public NetPackageScreamerAlertSync() {
    }

    public NetPackageScreamerAlertSync(HashSet<int> screamerIds, HashSet<int> hordeIds, string screamerMsg, string hordeMsg, Vector3 hordePos, float hordeEnd)
    {
        this.screamerIds = new List<int>(screamerIds);
        // Positions will be set separately
        this.hordeIds = new List<int>(hordeIds);
        this.screamerAlertMessage = screamerMsg;
        this.hordeAlertMessage = hordeMsg;
        this.hordeAlertPosition = hordePos;
        this.hordeAlertEndTime = hordeEnd;
    }

    public override void read(PooledBinaryReader reader)
    {
        screamerIds.Clear();
        screamerPositions.Clear();
        hordeIds.Clear();
        int screamerCount = reader.ReadInt32();
        for (int i = 0; i < screamerCount; i++)
            screamerIds.Add(reader.ReadInt32());
        for (int i = 0; i < screamerCount; i++)
            screamerPositions.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
        int hordeCount = reader.ReadInt32();
        for (int i = 0; i < hordeCount; i++)
            hordeIds.Add(reader.ReadInt32());
        screamerAlertMessage = reader.ReadString();
        hordeAlertMessage = reader.ReadString();
        hordeAlertPosition = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        hordeAlertEndTime = reader.ReadSingle();
    }

    public override void write(PooledBinaryWriter writer)
    {
        base.write(writer);
        writer.Write(screamerIds.Count);
        foreach (var id in screamerIds)
            writer.Write(id);
        // Write positions for each screamer
        foreach (var id in screamerIds)
        {
            // Find position for this id
            Vector3 pos = Vector3.zero;
            if (ScreamerAlertManager.Instance != null && ScreamerAlertManager.Instance.screamerPositions.TryGetValue(id, out var p))
                pos = p;
            writer.Write(pos.x);
            writer.Write(pos.y);
            writer.Write(pos.z);
        }
        writer.Write(hordeIds.Count);
        foreach (var id in hordeIds)
            writer.Write(id);
        writer.Write(screamerAlertMessage ?? string.Empty);
        writer.Write(hordeAlertMessage ?? string.Empty);
        writer.Write(hordeAlertPosition.x);
        writer.Write(hordeAlertPosition.y);
        writer.Write(hordeAlertPosition.z);
        writer.Write(hordeAlertEndTime);
    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (ScreamerAlertManager.Instance == null)
            return;
        if (ScreamerAlertsController.Instance == null)
            return;
        ScreamerAlertManager.Instance.persistentScreamerIds = new HashSet<int>(screamerIds);
        ScreamerAlertManager.Instance.syncedScreamerIds = new HashSet<int>(screamerIds);
        ScreamerAlertManager.Instance.syncedScreamerPositions = new Dictionary<int, Vector3>();
        for (int i = 0; i < screamerIds.Count && i < screamerPositions.Count; i++)
        {
            ScreamerAlertManager.Instance.syncedScreamerPositions[screamerIds[i]] = screamerPositions[i];
        }
        ScreamerAlertManager.Instance.persistentHordeZombieIds = new HashSet<int>(hordeIds);
        // Only update horde alert message and timing from server; screamer alert message is now always local
        ScreamerAlertsController.Instance.screamerHordeAlertMessage = hordeAlertMessage;
        ScreamerAlertsController.Instance.hordeAlertPosition = hordeAlertPosition;
        ScreamerAlertsController.Instance.hordeAlertEndTime = hordeAlertEndTime;
        // Do not set screamerAlertMessage from server; let each client calculate it locally
        ScreamerAlertsController.Instance.UpdateAlertMessage();
        ScreamerAlertsController.Instance.UpdateScreamerAlertUI();
        if (XUiC_ScreamerAlerts.Instance != null)
        {
            XUiC_ScreamerAlerts.Instance.RefreshBindingsSelfAndChildren();
        }
    }

    public override int GetLength()
    {
        int num = 4 + 4 * screamerIds.Count + 4 + 4 * hordeIds.Count;
        // Include 3 floats (12 bytes) per screamer position payload.
        num += 12 * screamerIds.Count;
        num += (screamerAlertMessage?.Length ?? 0) * 2 + 4;
        num += (hordeAlertMessage?.Length ?? 0) * 2 + 4;
        num += 16;
        return num;
    }
}
