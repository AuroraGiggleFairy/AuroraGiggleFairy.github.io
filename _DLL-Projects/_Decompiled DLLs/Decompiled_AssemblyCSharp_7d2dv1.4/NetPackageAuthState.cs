using Platform;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageAuthState : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string stateKey;

	public override bool FlushQueue => true;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public override bool AllowedBeforeAuth => true;

	public NetPackageAuthState Setup(string _authStateKey)
	{
		stateKey = _authStateKey ?? "";
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		stateKey = _reader.ReadString();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(stateKey);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			Log.Out("Login: " + stateKey);
			if (!string.IsNullOrEmpty(stateKey))
			{
				string format = Localization.Get(stateKey);
				format = string.Format(format, PlatformManager.NativePlatform.PlatformDisplayName, PlatformManager.CrossplatformPlatform?.PlatformDisplayName);
				XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, format);
			}
		}
	}

	public override int GetLength()
	{
		return 4;
	}
}
