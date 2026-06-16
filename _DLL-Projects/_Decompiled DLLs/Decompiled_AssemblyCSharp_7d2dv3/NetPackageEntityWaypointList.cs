using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityWaypointList : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public short listType;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<(int entityId, Vector3 position)> positions;

	public NetPackageEntityWaypointList Setup(eWayPointListType _listType, List<(int entityId, Vector3 position)> _positions)
	{
		listType = (short)_listType;
		positions = new List<(int, Vector3)>(_positions);
		return this;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		EntityPlayerLocal primaryPlayer = _world.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			if (listType == 0)
			{
				primaryPlayer.Waypoints.SetEntityVehicleWaypointFromVehicleManager(positions);
			}
			else
			{
				primaryPlayer.Waypoints.SetDroneWaypointsFromDroneManager(positions);
			}
		}
	}

	public override void read(PooledBinaryReader _reader)
	{
		listType = _reader.ReadInt16();
		int num = _reader.ReadInt32();
		positions = new List<(int, Vector3)>();
		for (int i = 0; i < num; i++)
		{
			positions.Add((_reader.ReadInt32(), StreamUtils.ReadVector3(_reader)));
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(listType);
		_writer.Write(positions.Count);
		for (int i = 0; i < positions.Count; i++)
		{
			_writer.Write(positions[i].entityId);
			StreamUtils.Write(_writer, positions[i].position);
		}
	}

	public override int GetLength()
	{
		return 0;
	}
}
