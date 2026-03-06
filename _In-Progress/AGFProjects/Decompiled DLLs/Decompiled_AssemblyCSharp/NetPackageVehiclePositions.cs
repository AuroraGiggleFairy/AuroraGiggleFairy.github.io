using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageVehiclePositions : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<(int entityId, Vector3 position)> positions;

	public NetPackageVehiclePositions Setup(List<(int entityId, Vector3 position)> _positions)
	{
		positions = new List<(int, Vector3)>(_positions);
		return this;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		EntityPlayerLocal primaryPlayer = _world.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			primaryPlayer.Waypoints.SetEntityVehicleWaypointFromVehicleManager(positions);
		}
	}

	public override void read(PooledBinaryReader _reader)
	{
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
