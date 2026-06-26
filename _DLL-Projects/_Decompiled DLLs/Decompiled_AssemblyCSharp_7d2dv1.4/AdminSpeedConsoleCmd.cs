using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class AdminSpeedConsoleCmd : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string info = "AdminSpeed";

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> parameters;

	public override bool IsExecuteOnClient => true;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		parameters = _params;
		string s = ((_params.Count == 0) ? "nope" : _params[0].ToLower());
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		float result = 0f;
		if (!float.TryParse(s, out result))
		{
			result = ((primaryPlayer.GodModeSpeedModifier == 15f) ? 5 : 15);
		}
		primaryPlayer.GodModeSpeedModifier = result;
		Log.Out("Admin speed: " + result);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetParam(List<string> _params, int index)
	{
		if (_params == null)
		{
			return null;
		}
		if (index >= _params.Count)
		{
			return null;
		}
		return _params[index];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetParamAsInt(int index)
	{
		if (parameters == null)
		{
			return -1;
		}
		if (index >= parameters.Count)
		{
			return -1;
		}
		int result = -1;
		int.TryParse(parameters[index], out result);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { info, "as" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return info;
	}
}
