using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePackageIds : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int toSendCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] mappings;

	[PublicizedFrom(EAccessModifier.Private)]
	public VersionInformation compatVersion;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool serverUseEAC;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasHostUserAndToken;

	[PublicizedFrom(EAccessModifier.Private)]
	public (PlatformUserIdentifierAbs userId, string token) hostUserAndToken;

	public override bool FlushQueue => true;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public override bool AllowedBeforeAuth => true;

	public NetPackagePackageIds Setup()
	{
		toSendCount = NetPackageManager.KnownPackageCount;
		serverUseEAC = PlatformManager.CrossplatformPlatform?.AntiCheatServer?.ServerEacEnabled() == true;
		if (serverUseEAC)
		{
			hasHostUserAndToken = PlatformManager.CrossplatformPlatform?.AntiCheatServer?.GetHostUserIdAndToken(out hostUserAndToken) == true;
		}
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		compatVersion = VersionInformation.Read(_reader);
		int num = _reader.ReadInt32();
		mappings = new string[num];
		for (int i = 0; i < num; i++)
		{
			mappings[i] = _reader.ReadString();
		}
		serverUseEAC = _reader.ReadBoolean();
		hasHostUserAndToken = _reader.ReadBoolean();
		if (hasHostUserAndToken)
		{
			hostUserAndToken = (userId: PlatformUserIdentifierAbs.FromStream(_reader, _errorOnEmpty: false, _inclCustomData: true), token: _reader.ReadString());
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		Constants.cVersionInformation.Write(_writer);
		Type[] packageMappings = NetPackageManager.PackageMappings;
		_writer.Write(packageMappings.Length);
		Type[] array = packageMappings;
		foreach (Type type in array)
		{
			_writer.Write(type.Name);
		}
		_writer.Write(serverUseEAC);
		_writer.Write(hasHostUserAndToken);
		if (hasHostUserAndToken)
		{
			hostUserAndToken.userId.ToStream(_writer, _inclCustomData: true);
			_writer.Write(hostUserAndToken.token ?? "");
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!compatVersion.EqualsMinor(Constants.cVersionInformation))
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
			string longStringNoBuild = compatVersion.LongStringNoBuild;
			_callbacks.ShowMessagePlayerDenied(new GameUtils.KickPlayerData(GameUtils.EKickReason.VersionMismatch, 0, default(DateTime), longStringNoBuild));
		}
		NetPackageManager.IdMappingsReceived(mappings);
		if (serverUseEAC)
		{
			IAntiCheatClient antiCheatClient = PlatformManager.MultiPlatform.AntiCheatClient;
			if (antiCheatClient == null || !antiCheatClient.ClientAntiCheatEnabled())
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
				GameManager.Instance.ShowMessagePlayerDenied(new GameUtils.KickPlayerData(GameUtils.EKickReason.EosEacViolation, 4));
				return;
			}
			PlatformManager.MultiPlatform.AntiCheatClient.ConnectToServer(hostUserAndToken, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendLogin();
			}, [PublicizedFrom(EAccessModifier.Internal)] (string errorMessage) =>
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
				GameManager.Instance.ShowMessagePlayerDenied(new GameUtils.KickPlayerData(GameUtils.EKickReason.CrossPlatformAuthenticationFailed, 50, default(DateTime), errorMessage));
			});
		}
		else if (!hasHostUserAndToken && (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() && Submission.Enabled)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
			GameManager instance = GameManager.Instance;
			string longStringNoBuild = PlatformManager.NativePlatform.PlatformIdentifier.ToStringCached();
			instance.ShowMessagePlayerDenied(new GameUtils.KickPlayerData(GameUtils.EKickReason.UnsupportedPlatform, 0, default(DateTime), longStringNoBuild));
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendLogin();
		}
	}

	public override int GetLength()
	{
		return 2 + toSendCount * 32;
	}
}
