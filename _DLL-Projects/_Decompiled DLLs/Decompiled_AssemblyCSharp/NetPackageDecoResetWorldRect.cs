using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDecoResetWorldRect : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public PooledExpandableMemoryStream ms = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageDecoResetWorldRect Setup(Rect _worldRect)
	{
		using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
		pooledBinaryWriter.SetBaseStream(ms);
		pooledBinaryWriter.Write((int)_worldRect.x);
		pooledBinaryWriter.Write((int)_worldRect.y);
		pooledBinaryWriter.Write((int)_worldRect.width);
		pooledBinaryWriter.Write((int)_worldRect.height);
		return this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~NetPackageDecoResetWorldRect()
	{
		MemoryPools.poolMemoryStream.FreeSync(ms);
	}

	public override void read(PooledBinaryReader _br)
	{
		int length = _br.ReadInt32();
		StreamUtils.StreamCopy(_br.BaseStream, ms, length);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((int)ms.Length);
		ms.WriteTo(_bw.BaseStream);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
		lock (ms)
		{
			pooledBinaryReader.SetBaseStream(ms);
			ms.Position = 0L;
			int num = pooledBinaryReader.ReadInt32();
			int num2 = pooledBinaryReader.ReadInt32();
			int num3 = pooledBinaryReader.ReadInt32();
			int num4 = pooledBinaryReader.ReadInt32();
			Rect worldRect = new Rect(num, num2, num3, num4);
			DecoManager.Instance.ResetDecosInWorldRect(worldRect);
		}
	}

	public override int GetLength()
	{
		return (int)ms.Length;
	}
}
