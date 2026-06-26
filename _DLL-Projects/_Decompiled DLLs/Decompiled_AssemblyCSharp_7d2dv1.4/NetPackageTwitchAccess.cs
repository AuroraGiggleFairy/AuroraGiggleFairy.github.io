using Audio;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageTwitchAccess : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasAccess;

	public NetPackageTwitchAccess Setup()
	{
		return this;
	}

	public NetPackageTwitchAccess Setup(bool _hasAccess)
	{
		hasAccess = _hasAccess;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		hasAccess = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(hasAccess);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				bool flag = (GameManager.Instance.adminTools?.Users.GetUserPermissionLevel(base.Sender) ?? 1000) <= GamePrefs.GetInt(EnumGamePrefs.TwitchServerPermission);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageTwitchAccess>().Setup(flag), _onlyClientsAttachedToAnEntity: false, base.Sender.entityId);
			}
			else if (hasAccess)
			{
				GameEventManager.Current.HandleGameEventAccessApproved();
			}
			else
			{
				Manager.PlayInsidePlayerHead("Misc/password_fail");
				TwitchManager.Current.DeniedPermission();
			}
		}
	}

	public override int GetLength()
	{
		return 30;
	}
}
