using System.Collections.Generic;
using Unity.XGamingRuntime;

namespace Platform.XBL;

public static class XblHelpers
{
	public delegate void ErrorDelegate(int _hresult, string _operationFriendlyName, string _errorMessage);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<int, string> hresultToFriendlyErrorLookup;

	public static event ErrorDelegate OnError;

	public static bool Succeeded(int _hresult, string _operationFriendlyName, bool _logToConsole = true, bool _printSuccess = false)
	{
		if (Succeeded(_hresult))
		{
			if (_printSuccess && _logToConsole)
			{
				Log.Out("[XBL] Success: " + _operationFriendlyName);
			}
			return true;
		}
		if (!hresultToFriendlyErrorLookup.TryGetValue(_hresult, out var value))
		{
			value = _operationFriendlyName + " failed. Error code: " + HR.NameOf(_hresult);
		}
		if (_logToConsole)
		{
			Log.Error($"[XBL] Error: 0x{_hresult:X8} - {value}");
		}
		XblHelpers.OnError?.Invoke(_hresult, _operationFriendlyName, value);
		return false;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static bool Succeeded(int _hresult)
	{
		return _hresult >= 0;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static bool Failed(int _hresult)
	{
		return _hresult < 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static XblHelpers()
	{
		hresultToFriendlyErrorLookup = new Dictionary<int, string>();
		hresultToFriendlyErrorLookup[-2143330041] = "IAP_UNEXPECTED: Does the player you are signed in as have a license for the game? You can get one by downloading your game from the store and purchasing it first. If you can't find your game in the store, have you published it in Partner Center?";
		hresultToFriendlyErrorLookup[-2015035361] = "Missing Game Config";
	}
}
