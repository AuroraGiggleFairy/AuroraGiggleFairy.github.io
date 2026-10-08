using System.Collections;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageGameStats : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public PooledExpandableMemoryStream ms = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageGameStats Setup(GameStats _gs)
	{
		using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
		pooledBinaryWriter.SetBaseStream(ms);
		_gs.Write(pooledBinaryWriter);
		return this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~NetPackageGameStats()
	{
		MemoryPools.poolMemoryStream.FreeSync(ms);
	}

	public override void read(PooledBinaryReader _reader)
	{
		int length = _reader.ReadInt16();
		StreamUtils.StreamCopy(_reader.BaseStream, ms, length);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((short)ms.Length);
		ms.WriteTo(_writer.BaseStream);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		ThreadManager.StartCoroutine(readStatsCo());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator readStatsCo()
	{
		while (GameManager.Instance.World == null && SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			yield return null;
		}
		if (GameManager.Instance.World == null)
		{
			yield break;
		}
		using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false))
		{
			lock (ms)
			{
				pooledBinaryReader.SetBaseStream(ms);
				ms.Position = 0L;
				GameStats.Instance.Read(pooledBinaryReader);
			}
		}
		GameManager.Instance.GetGameStateManager().OnUpdateTick();
	}

	public override int GetLength()
	{
		return (int)ms.Length;
	}
}
