using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerDenied : NetPackage
{
	public GameUtils.KickPlayerData kickData;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool processed;

	public override bool FlushQueue => true;

	public override bool AllowedBeforeAuth => true;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackagePlayerDenied Setup(GameUtils.KickPlayerData _kickData)
	{
		kickData = _kickData;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		kickData.reason = (GameUtils.EKickReason)_reader.ReadInt32();
		kickData.apiResponseEnum = _reader.ReadInt32();
		kickData.banUntil = DateTime.FromBinary(_reader.ReadInt64());
		kickData.customReason = _reader.ReadString();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			ThreadManager.AddSingleTaskMainThread("PlayerDenied.ProcessPackage", [PublicizedFrom(EAccessModifier.Private)] (object _taskInfo) =>
			{
				ProcessPackage(GameManager.Instance.World, GameManager.Instance);
			});
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((int)kickData.reason);
		_writer.Write(kickData.apiResponseEnum);
		_writer.Write(kickData.banUntil.ToBinary());
		_writer.Write(kickData.customReason);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!processed)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
			_callbacks.ShowMessagePlayerDenied(kickData);
			processed = true;
		}
	}

	public override int GetLength()
	{
		return 20;
	}
}
