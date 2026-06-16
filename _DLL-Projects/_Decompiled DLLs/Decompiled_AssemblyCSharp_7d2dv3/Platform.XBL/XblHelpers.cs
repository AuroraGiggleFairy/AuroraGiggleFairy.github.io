using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.XGamingRuntime;
using Unity.XGamingRuntime.Interop;

namespace Platform.XBL;

public static class XblHelpers
{
	public delegate void ErrorDelegate(int _hresult, string _operationFriendlyName, string _errorMessage);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<int, string> hresultToFriendlyErrorLookup;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly IReadOnlyDictionary<int, string> s_hrToName;

	public static event ErrorDelegate OnError;

	public static bool Succeeded(int _hresult, string _operationFriendlyName, bool _logToConsole = true, bool _printSuccess = false)
	{
		if (Unity.XGamingRuntime.Interop.HR.SUCCEEDED(_hresult))
		{
			if (_printSuccess && _logToConsole)
			{
				Log.Out("[XBL] Success: " + _operationFriendlyName);
			}
			return true;
		}
		if (!hresultToFriendlyErrorLookup.TryGetValue(_hresult, out var value))
		{
			value = _operationFriendlyName + " failed. Error code: " + GetHRName(_hresult);
		}
		if (_logToConsole)
		{
			Log.Error($"[XBL] Error: 0x{_hresult:X8} - {value}");
		}
		XblHelpers.OnError?.Invoke(_hresult, _operationFriendlyName, value);
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static XblHelpers()
	{
		hresultToFriendlyErrorLookup = new Dictionary<int, string>();
		s_hrToName = (from f in new Type[3]
			{
				typeof(Unity.XGamingRuntime.Interop.HR),
				typeof(Unity.XGamingRuntime.HR),
				typeof(HREx)
			}.SelectMany([PublicizedFrom(EAccessModifier.Internal)] (Type t) => t.GetFields(BindingFlags.Static | BindingFlags.Public))
			where f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(int)
			select f).ToDictionary([PublicizedFrom(EAccessModifier.Internal)] (FieldInfo f) => (int)f.GetValue(null), [PublicizedFrom(EAccessModifier.Internal)] (FieldInfo f) => f.Name);
		hresultToFriendlyErrorLookup[-2143330041] = "IAP_UNEXPECTED: Does the player you are signed in as have a license for the game? You can get one by downloading your game from the store and purchasing it first. If you can't find your game in the store, have you published it in Partner Center?";
		hresultToFriendlyErrorLookup[-2015035361] = "Missing Game Config";
	}

	public static string GetHRName(int hr)
	{
		if (!s_hrToName.TryGetValue(hr, out var value))
		{
			return "UNKNOWN";
		}
		return value;
	}

	public static void LogHR(int hr, string identifier, bool failWarn = false)
	{
		bool flag = Unity.XGamingRuntime.Interop.HR.SUCCEEDED(hr);
		string text = (flag ? "SUCCEEDED" : "FAILED");
		string txt = $"[HResult] {text} (0x{hr:X8} = {GetHRName(hr)}) {identifier}";
		if (flag)
		{
			Log.Out(txt);
		}
		else if (failWarn)
		{
			Log.Warning(txt);
		}
		else
		{
			Log.Error(txt);
		}
	}
}
