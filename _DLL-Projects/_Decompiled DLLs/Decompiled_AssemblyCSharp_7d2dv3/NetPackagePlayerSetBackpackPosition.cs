using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerSetBackpackPosition : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int playerId;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3i> positions;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackagePlayerSetBackpackPosition Setup(int _playerId, List<Vector3i> _positions)
	{
		playerId = _playerId;
		positions = _positions;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		playerId = _br.ReadInt32();
		int num = _br.ReadByte();
		positions = new List<Vector3i>();
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				positions.Add(StreamUtils.ReadVector3i(_br));
			}
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(playerId);
		if (positions == null)
		{
			_bw.Write((byte)0);
			return;
		}
		_bw.Write((byte)positions.Count);
		for (int i = 0; i < positions.Count; i++)
		{
			StreamUtils.Write(_bw, positions[i]);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (GameManager.Instance.World != null)
		{
			EntityPlayerLocal entityPlayerLocal = GameManager.Instance.World.GetEntity(playerId) as EntityPlayerLocal;
			if (entityPlayerLocal != null)
			{
				entityPlayerLocal.SetDroppedBackpackPositions(positions);
			}
		}
	}

	public override int GetLength()
	{
		return 16;
	}
}
