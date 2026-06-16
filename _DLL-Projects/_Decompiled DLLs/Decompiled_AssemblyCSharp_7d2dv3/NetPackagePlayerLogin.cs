using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerLogin : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string playerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public (PlatformUserIdentifierAbs userId, string token) platformUserAndToken;

	[PublicizedFrom(EAccessModifier.Private)]
	public (PlatformUserIdentifierAbs userId, string token) crossplatformUserAndToken;

	[PublicizedFrom(EAccessModifier.Private)]
	public string version;

	[PublicizedFrom(EAccessModifier.Private)]
	public string compVersion;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong discordUserId;

	public override bool AllowedBeforeAuth => true;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackagePlayerLogin Setup(string _playerName, (PlatformUserIdentifierAbs userId, string token) _platformUserAndToken, (PlatformUserIdentifierAbs userId, string token) _crossplatformUserAndToken, string _version, string _compVersion, ulong _discordUserId)
	{
		playerName = _playerName;
		platformUserAndToken = _platformUserAndToken;
		crossplatformUserAndToken = _crossplatformUserAndToken;
		version = _version;
		compVersion = _compVersion;
		discordUserId = _discordUserId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		Log.Out("NPPL.Read");
		playerName = _br.ReadString();
		platformUserAndToken = (userId: PlatformUserIdentifierAbs.FromStream(_br, _errorOnEmpty: false, _inclCustomData: true), token: _br.ReadString());
		crossplatformUserAndToken = (userId: PlatformUserIdentifierAbs.FromStream(_br, _errorOnEmpty: false, _inclCustomData: true), token: _br.ReadString());
		version = _br.ReadString();
		compVersion = _br.ReadString();
		discordUserId = _br.ReadUInt64();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		Log.Out("NPPL.Write");
		base.write(_bw);
		_bw.Write(playerName);
		platformUserAndToken.userId.ToStream(_bw, _inclCustomData: true);
		_bw.Write(platformUserAndToken.token ?? "");
		crossplatformUserAndToken.userId.ToStream(_bw, _inclCustomData: true);
		_bw.Write(crossplatformUserAndToken.token ?? "");
		_bw.Write(version);
		_bw.Write(compVersion);
		_bw.Write(discordUserId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_callbacks.PlayerLoginRPC(base.Sender, playerName, platformUserAndToken, crossplatformUserAndToken, compVersion, discordUserId);
	}

	public override int GetLength()
	{
		return 120;
	}
}
