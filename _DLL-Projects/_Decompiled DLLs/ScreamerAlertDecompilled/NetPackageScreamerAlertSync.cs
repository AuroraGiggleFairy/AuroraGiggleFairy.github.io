using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class NetPackageScreamerAlertSync : NetPackage
{
	public Dictionary<Vector3, List<int>> syncedData = new Dictionary<Vector3, List<int>>();

	public string screamerAlertMessage = string.Empty;

	public Vector3 hordeAlertPosition = Vector3.zero;

	public float hordeAlertEndTime = 0f;

	public NetPackageScreamerAlertSync()
	{
	}

	public NetPackageScreamerAlertSync(Dictionary<Vector3, List<int>> data, string screamerMsg, Vector3 hordePos, float hordeEnd)
	{
		syncedData = data.ToDictionary((KeyValuePair<Vector3, List<int>> entry) => entry.Key, (KeyValuePair<Vector3, List<int>> entry) => new List<int>(entry.Value));
		screamerAlertMessage = screamerMsg;
		hordeAlertPosition = hordePos;
		hordeAlertEndTime = hordeEnd;
	}

	public override void read(PooledBinaryReader _reader)
	{
		syncedData.Clear();
		int num = _reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			float x = _reader.ReadSingle();
			float y = _reader.ReadSingle();
			float z = _reader.ReadSingle();
			int num2 = _reader.ReadInt32();
			List<int> list = new List<int>();
			for (int j = 0; j < num2; j++)
			{
				list.Add(_reader.ReadInt32());
			}
			syncedData[new Vector3(x, y, z)] = list;
		}
		screamerAlertMessage = _reader.ReadString();
		hordeAlertPosition = new Vector3(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
		hordeAlertEndTime = _reader.ReadSingle();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		((BinaryWriter)_writer).Write(syncedData.Count);
		foreach (KeyValuePair<Vector3, List<int>> syncedDatum in syncedData)
		{
			((BinaryWriter)_writer).Write(syncedDatum.Key.x);
			((BinaryWriter)_writer).Write(syncedDatum.Key.y);
			((BinaryWriter)_writer).Write(syncedDatum.Key.z);
			((BinaryWriter)_writer).Write(syncedDatum.Value.Count);
			foreach (int item in syncedDatum.Value)
			{
				((BinaryWriter)_writer).Write(item);
			}
		}
		((BinaryWriter)_writer).Write(screamerAlertMessage ?? string.Empty);
		((BinaryWriter)_writer).Write(hordeAlertPosition.x);
		((BinaryWriter)_writer).Write(hordeAlertPosition.y);
		((BinaryWriter)_writer).Write(hordeAlertPosition.z);
		((BinaryWriter)_writer).Write(hordeAlertEndTime);
	}

	public override int GetLength()
	{
		int num = 4;
		foreach (KeyValuePair<Vector3, List<int>> syncedDatum in syncedData)
		{
			num += 12;
			num += 4;
			num += 4 * syncedDatum.Value.Count;
		}
		int num2 = num;
		int? num3 = ((screamerAlertMessage != null) ? new int?(screamerAlertMessage.Length) : ((int?)null));
		num = num2 + ((num3.HasValue ? num3.Value : 0) * 2 + 4);
		return num + 16;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		ScreamerAlertManager.ClientTargetScreamerIds = syncedData.ToDictionary((KeyValuePair<Vector3, List<int>> entry) => entry.Key, (KeyValuePair<Vector3, List<int>> entry) => new List<int>(entry.Value));
		ScreamerAlertsController.Instance.screamerAlertMessage = screamerAlertMessage;
		ScreamerAlertsController.Instance.hordeAlertPosition = hordeAlertPosition;
		ScreamerAlertsController.Instance.hordeAlertEndTime = hordeAlertEndTime;
		ScreamerAlertsController.Instance.UpdateScreamerAlertUI();
		if (XUiC_ScreamerAlerts.Instance != null)
		{
			XUiC_ScreamerAlerts.Instance.RefreshBindingsSelfAndChildren();
		}
	}
}
