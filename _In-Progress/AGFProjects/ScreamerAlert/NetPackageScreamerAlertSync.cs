using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NetPackageScreamerAlertSync : NetPackage
{
    public Dictionary<Vector3, List<int>> syncedData = new Dictionary<Vector3, List<int>>();
    public string screamerAlertMessage = string.Empty;
    public Vector3 hordeAlertPosition = Vector3.zero;
    public float hordeAlertEndTime = 0f;

    public NetPackageScreamerAlertSync() {}
    public NetPackageScreamerAlertSync(Dictionary<Vector3, List<int>> data, string screamerMsg, Vector3 hordePos, float hordeEnd)
    {
        syncedData = data.ToDictionary(entry => entry.Key, entry => new List<int>(entry.Value));
        screamerAlertMessage = screamerMsg;
        hordeAlertPosition = hordePos;
        hordeAlertEndTime = hordeEnd;
    }

    public override void read(PooledBinaryReader _reader)
    {
        syncedData.Clear();
        int count = _reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            float x = _reader.ReadSingle();
            float y = _reader.ReadSingle();
            float z = _reader.ReadSingle();
            int listCount = _reader.ReadInt32();
            var ids = new List<int>();
            for (int j = 0; j < listCount; j++)
            {
                ids.Add(_reader.ReadInt32());
            }
            syncedData[new Vector3(x, y, z)] = ids;
        }
        screamerAlertMessage = _reader.ReadString();
        hordeAlertPosition = new Vector3(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
        hordeAlertEndTime = _reader.ReadSingle();
    }

    public override void write(PooledBinaryWriter _writer)
    {
        base.write(_writer);
        _writer.Write(syncedData.Count);
        foreach (var kvp in syncedData)
        {
            _writer.Write(kvp.Key.x);
            _writer.Write(kvp.Key.y);
            _writer.Write(kvp.Key.z);
            _writer.Write(kvp.Value.Count);
            foreach (var id in kvp.Value)
            {
                _writer.Write(id);
            }
        }
        _writer.Write(screamerAlertMessage ?? string.Empty);
        _writer.Write(hordeAlertPosition.x);
        _writer.Write(hordeAlertPosition.y);
        _writer.Write(hordeAlertPosition.z);
        _writer.Write(hordeAlertEndTime);
    }

    public override int GetLength()
    {
        int length = 4; // for syncedData.Count
        foreach (var kvp in syncedData)
        {
            length += 12; // 3 floats for Vector3
            length += 4; // for kvp.Value.Count
            length += 4 * kvp.Value.Count; // each int in the list
        }
        length += (screamerAlertMessage?.Length ?? 0) * sizeof(char) + 4; // string + float
        length += 12 + 4; // Vector3 + float
        return length;
    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        ScreamerAlertManager.ClientTargetScreamerIds = syncedData.ToDictionary(entry => entry.Key, entry => new List<int>(entry.Value));
        ScreamerAlertsController.Instance.screamerAlertMessage = screamerAlertMessage;
        // ScreamerAlertsController.Instance.screamerHordeAlertMessage = hordeAlertMessage; // REMOVED
        ScreamerAlertsController.Instance.hordeAlertPosition = hordeAlertPosition;
        ScreamerAlertsController.Instance.hordeAlertEndTime = hordeAlertEndTime;
        ScreamerAlertsController.Instance.UpdateScreamerAlertUI();
        XUiC_ScreamerAlerts.Instance?.RefreshBindingsSelfAndChildren();
    }
}
