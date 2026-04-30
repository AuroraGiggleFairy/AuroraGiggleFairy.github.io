using UnityEngine;

public class SpawnPoint
{
	public SpawnPosition spawnPosition;

	public int team;

	public int activeInGameMode;

	public SpawnPoint()
	{
		spawnPosition = SpawnPosition.Undef;
		team = 0;
		activeInGameMode = 0;
	}

	public SpawnPoint(Vector3i _blockPos)
	{
		spawnPosition = new SpawnPosition(_blockPos, 0f);
		team = 0;
		activeInGameMode = -1;
	}

	public SpawnPoint(Vector3 _position, float _heading)
	{
		spawnPosition = new SpawnPosition(_position, _heading);
		team = 0;
		activeInGameMode = -1;
	}

	public void Read(IBinaryReaderOrWriter _readerOrWriter, uint _version)
	{
		spawnPosition.Read(_readerOrWriter, _version);
		team = _readerOrWriter.ReadWrite(0);
		activeInGameMode = _readerOrWriter.ReadWrite(0);
	}

	public void Read(PooledBinaryReader _br, uint _version)
	{
		spawnPosition.Read(_br, _version);
		team = _br.ReadInt32();
		activeInGameMode = _br.ReadInt32();
	}

	public void Write(PooledBinaryWriter _bw)
	{
		spawnPosition.Write(_bw);
		_bw.Write(team);
		_bw.Write(activeInGameMode);
	}

	public override int GetHashCode()
	{
		return spawnPosition.ToBlockPos().GetHashCode();
	}
}
