using System;
using System.Collections.Generic;
using Platform.Local;
using Platform.Steam;

public class ClientInfo : IEquatable<ClientInfo>
{
	public enum EDeviceType
	{
		Linux,
		Mac,
		Windows,
		PlayStation,
		Xbox,
		Unknown
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int lastClientNumber;

	public INetworkServer network;

	public readonly int ClientNumber;

	public long litenetPeerConnectId = -1L;

	public PlatformUserIdentifierAbs PlatformId = new UserIdentifierLocal("<none>");

	public PlatformUserIdentifierAbs CrossplatformId;

	public ulong DiscordUserId;

	public bool requiresAntiCheat = true;

	public EDeviceType device = EDeviceType.Unknown;

	public bool loginDone;

	public bool acAuthDone;

	public INetConnection[] netConnection;

	public bool bAttachedToEntity;

	public int entityId = -1;

	public string playerName;

	public string compatibilityVersion;

	public readonly Dictionary<string, int> groupMemberships = new Dictionary<string, int>(StringComparer.Ordinal);

	public int groupMembershipsWaiting;

	public PlayerDataFile latestPlayerData;

	public int ping;

	public bool disconnecting;

	public PlatformUserIdentifierAbs InternalId => CrossplatformId ?? PlatformId;

	public string ip => network.GetIP(this);

	public ClientInfo()
	{
		int num;
		do
		{
			num = ++lastClientNumber;
			if (num > 1000000)
			{
				num = (lastClientNumber = 1);
			}
		}
		while (SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForClientNumber(num) != null);
		ClientNumber = num;
	}

	public override string ToString()
	{
		string text = null;
		if (PlatformId is UserIdentifierSteam userIdentifierSteam)
		{
			text = userIdentifierSteam.OwnerId?.CombinedString;
		}
		return string.Format("EntityID={0}, PltfmId='{1}', CrossId='{2}', OwnerID='{3}', PlayerName='{4}', ClientNumber='{5}'", entityId, PlatformId?.CombinedString ?? "<unknown>", CrossplatformId?.CombinedString ?? "<unknown/none>", text ?? "<unknown/none>", playerName, ClientNumber);
	}

	public void UpdatePing()
	{
		ping = network.GetPing(this);
	}

	public void SendPackage(NetPackage _package)
	{
		if (!_package.AllowedBeforeAuth && !loginDone)
		{
			Log.Warning($"Ignoring {_package}, not logged in yet");
			return;
		}
		netConnection[_package.Channel].AddToSendQueue(_package);
		if (_package.FlushQueue)
		{
			netConnection[_package.Channel].FlushSendQueue();
		}
	}

	public void SetAntiCheatEncryption(IEncryptionModule encryptionModule)
	{
		INetConnection[] array = netConnection;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetEncryptionModule(encryptionModule);
		}
	}

	public bool Equals(ClientInfo _other)
	{
		return this == _other;
	}
}
