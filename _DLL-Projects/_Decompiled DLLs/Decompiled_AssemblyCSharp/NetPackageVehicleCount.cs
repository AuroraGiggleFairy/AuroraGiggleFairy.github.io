using UnityEngine.Scripting;

[Preserve]
public class NetPackageVehicleCount : NetPackage
{
	public int vehicleCount;

	public int turretCount;

	public int droneCount;

	public NetPackageVehicleCount Setup()
	{
		vehicleCount = VehicleManager.GetServerVehicleCount();
		turretCount = TurretTracker.GetServerTurretCount();
		droneCount = DroneManager.GetServerDroneCount();
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		vehicleCount = _reader.ReadInt32();
		turretCount = _reader.ReadInt32();
		droneCount = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(vehicleCount);
		_writer.Write(turretCount);
		_writer.Write(droneCount);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		VehicleManager.SetServerVehicleCount(vehicleCount);
		TurretTracker.SetServerTurretCount(turretCount);
		DroneManager.SetServerDroneCount(droneCount);
	}

	public override int GetLength()
	{
		return 12;
	}
}
