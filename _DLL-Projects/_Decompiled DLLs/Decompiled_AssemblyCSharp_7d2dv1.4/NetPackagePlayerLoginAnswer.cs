using System.Text;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerLoginAnswer : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bAllowed;

	[PublicizedFrom(EAccessModifier.Private)]
	public string data;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformLobbyId platformLobbyId;

	[PublicizedFrom(EAccessModifier.Private)]
	public (PlatformUserIdentifierAbs userId, string token) platformUserAndToken;

	[PublicizedFrom(EAccessModifier.Private)]
	public (PlatformUserIdentifierAbs userId, string token) crossplatformUserAndToken;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	public override bool FlushQueue => true;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackagePlayerLoginAnswer Setup(bool _bAllowed, string _data, PlatformLobbyId _platformLobbyId, (PlatformUserIdentifierAbs userId, string token) _platformUserAndToken, (PlatformUserIdentifierAbs userId, string token) _crossplatformUserAndToken)
	{
		bAllowed = _bAllowed;
		data = _data;
		platformLobbyId = _platformLobbyId;
		platformUserAndToken = _platformUserAndToken;
		crossplatformUserAndToken = _crossplatformUserAndToken;
		RecalcLength();
		return this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RecalcLength()
	{
		Encoding uTF = Encoding.UTF8;
		length = 1 + data.GetBinaryWriterLength(uTF) + platformLobbyId.GetWriteLength(uTF) + platformUserAndToken.userId.GetToStreamLength(uTF, _inclCustomData: true) + (platformUserAndToken.token ?? "").GetBinaryWriterLength(uTF) + crossplatformUserAndToken.userId.GetToStreamLength(uTF, _inclCustomData: true) + (crossplatformUserAndToken.token ?? "").GetBinaryWriterLength(uTF);
	}

	public override void read(PooledBinaryReader _br)
	{
		bAllowed = _br.ReadBoolean();
		data = _br.ReadString();
		platformLobbyId = PlatformLobbyId.Read(_br);
		platformUserAndToken = (userId: PlatformUserIdentifierAbs.FromStream(_br, _errorOnEmpty: false, _inclCustomData: true), token: _br.ReadString());
		crossplatformUserAndToken = (userId: PlatformUserIdentifierAbs.FromStream(_br, _errorOnEmpty: false, _inclCustomData: true), token: _br.ReadString());
		RecalcLength();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(bAllowed);
		_bw.Write(data);
		platformLobbyId.Write(_bw);
		platformUserAndToken.userId.ToStream(_bw, _inclCustomData: true);
		_bw.Write(platformUserAndToken.token ?? "");
		crossplatformUserAndToken.userId.ToStream(_bw, _inclCustomData: true);
		_bw.Write(crossplatformUserAndToken.token ?? "");
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (bAllowed)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.PlayerAllowed(data, platformLobbyId, platformUserAndToken, crossplatformUserAndToken);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.PlayerDenied(data);
		}
	}

	public override int GetLength()
	{
		return length;
	}
}
