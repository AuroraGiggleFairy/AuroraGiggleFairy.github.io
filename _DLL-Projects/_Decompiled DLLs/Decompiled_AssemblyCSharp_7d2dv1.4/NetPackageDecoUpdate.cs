using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDecoUpdate : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public PooledExpandableMemoryStream ms = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstPackage = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int decoSize = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int decosPerPackage = 32768;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageDecoUpdate Setup(List<DecoObject> _decoList, ref int _currentIndex)
	{
		firstPackage = _currentIndex == 0;
		int num = Math.Min(32768, _decoList.Count - _currentIndex);
		int num2 = _currentIndex + num;
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
		{
			pooledBinaryWriter.SetBaseStream(ms);
			pooledBinaryWriter.Write(num);
			for (int i = _currentIndex; i < num2; i++)
			{
				_decoList[i].Write(pooledBinaryWriter);
			}
		}
		_currentIndex = num2;
		return this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~NetPackageDecoUpdate()
	{
		MemoryPools.poolMemoryStream.FreeSync(ms);
	}

	public override void read(PooledBinaryReader _br)
	{
		firstPackage = _br.ReadBoolean();
		int length = _br.ReadInt32();
		StreamUtils.StreamCopy(_br.BaseStream, ms, length);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(firstPackage);
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
			DecoManager.Instance.Read(pooledBinaryReader, int.MaxValue, firstPackage);
		}
	}

	public override int GetLength()
	{
		return (int)ms.Length;
	}
}
