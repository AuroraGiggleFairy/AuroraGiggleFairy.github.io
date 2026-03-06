using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Platform;

public static class GeneratedTextManager
{
	public enum TextFilteringMode
	{
		None,
		Filter,
		FilterOtherPlatforms,
		FilterWithSafeString
	}

	public enum BbCodeSupportMode
	{
		NotSupported,
		Supported,
		SupportedAndAddEscapes
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class AuthoredTextDetails
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public string baseText;

		[PublicizedFrom(EAccessModifier.Private)]
		public string baseTextEscaped;

		[PublicizedFrom(EAccessModifier.Private)]
		public string filteredTextBase;

		[PublicizedFrom(EAccessModifier.Private)]
		public string filteredTextBBSupported;

		[PublicizedFrom(EAccessModifier.Private)]
		public string filteredTextBBEscaped;

		public string BaseText => baseText;

		public AuthoredTextDetails(string _baseText)
		{
			SetText(_baseText);
		}

		public bool IsFiltered()
		{
			return filteredTextBase != null;
		}

		public void SetText(string _baseText)
		{
			if (_baseText != baseText)
			{
				baseText = _baseText;
				baseTextEscaped = null;
				filteredTextBase = null;
				filteredTextBBSupported = null;
				filteredTextBBEscaped = null;
			}
		}

		public void SetFilteredText(string _filteredText)
		{
			filteredTextBase = _filteredText;
		}

		public string GetDisplayText(bool _filtered, BbCodeSupportMode _bbSupportMode)
		{
			if (_filtered)
			{
				switch (_bbSupportMode)
				{
				case BbCodeSupportMode.NotSupported:
					return filteredTextBase;
				case BbCodeSupportMode.Supported:
					if (filteredTextBBSupported == null && filteredTextBase != null)
					{
						filteredTextBBSupported = ReconstructFilteredTextWithBbCodes(baseText, filteredTextBase);
					}
					return filteredTextBBSupported;
				case BbCodeSupportMode.SupportedAndAddEscapes:
					if (filteredTextBBEscaped == null && filteredTextBase != null)
					{
						filteredTextBBEscaped = Utils.EscapeBbCodes(filteredTextBase);
					}
					return filteredTextBBEscaped;
				default:
					return null;
				}
			}
			switch (_bbSupportMode)
			{
			case BbCodeSupportMode.NotSupported:
			case BbCodeSupportMode.Supported:
				return baseText;
			case BbCodeSupportMode.SupportedAndAddEscapes:
				if (baseTextEscaped == null && baseText != null)
				{
					baseTextEscaped = Utils.EscapeBbCodes(baseText);
				}
				return baseTextEscaped;
			default:
				return baseText;
			}
		}
	}

	public const string SafeString = "{...}";

	public const string BlockedString = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object lockObj = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static ConditionalWeakTable<AuthoredText, AuthoredTextDetails> authoredTextReferences = new ConditionalWeakTable<AuthoredText, AuthoredTextDetails>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<AuthoredText, List<(BbCodeSupportMode, Action<string>)>> pendingFilterCallbacks = new Dictionary<AuthoredText, List<(BbCodeSupportMode, Action<string>)>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, List<(BbCodeSupportMode, Action<string>)>> pendingFilterCallbacksStrings = new Dictionary<string, List<(BbCodeSupportMode, Action<string>)>>();

	public static string GetDisplayTextImmediately(AuthoredText _authoredText, bool _checkBlockState, TextFilteringMode _filteringMode = TextFilteringMode.Filter, BbCodeSupportMode _bbSupportMode = BbCodeSupportMode.SupportedAndAddEscapes)
	{
		AuthoredTextDetails orCreateFilterDetails = GetOrCreateFilterDetails(_authoredText);
		if (string.IsNullOrEmpty(orCreateFilterDetails?.BaseText) || GameManager.IsDedicatedServer)
		{
			return orCreateFilterDetails?.GetDisplayText(_filtered: false, _bbSupportMode);
		}
		if (_checkBlockState && _authoredText.Author != null && !PlatformManager.MultiPlatform.User.PlatformUserId.Equals(_authoredText.Author))
		{
			PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(_authoredText.Author);
			if (playerData != null && playerData.PlatformData.Blocked[EBlockType.TextChat].IsBlocked())
			{
				return "";
			}
		}
		if (PlatformManager.MultiPlatform.TextCensor == null || ShouldSkipFiltering(_authoredText.Author, _filteringMode))
		{
			return orCreateFilterDetails.GetDisplayText(_filtered: false, _bbSupportMode);
		}
		if (orCreateFilterDetails.IsFiltered())
		{
			return orCreateFilterDetails.GetDisplayText(_filtered: true, _bbSupportMode);
		}
		return "{...}";
	}

	public static void GetDisplayText(AuthoredText _authoredText, Action<string> _textReadyCallback, bool _runCallbackIfReadyNow, bool _checkBlockState, TextFilteringMode _filteringMode = TextFilteringMode.Filter, BbCodeSupportMode _bbSupportMode = BbCodeSupportMode.SupportedAndAddEscapes)
	{
		AuthoredTextDetails orCreateFilterDetails = GetOrCreateFilterDetails(_authoredText);
		if (string.IsNullOrEmpty(orCreateFilterDetails?.BaseText) || GameManager.IsDedicatedServer)
		{
			if (_runCallbackIfReadyNow)
			{
				_textReadyCallback?.Invoke(orCreateFilterDetails?.GetDisplayText(_filtered: false, _bbSupportMode));
			}
			return;
		}
		if (_checkBlockState && _authoredText.Author != null && !PlatformManager.MultiPlatform.User.PlatformUserId.Equals(_authoredText.Author))
		{
			PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(_authoredText.Author);
			if (playerData != null && playerData.PlatformData.Blocked[EBlockType.TextChat].IsBlocked())
			{
				if (_runCallbackIfReadyNow)
				{
					_textReadyCallback?.Invoke("");
				}
				return;
			}
		}
		if (PlatformManager.MultiPlatform.TextCensor == null || ShouldSkipFiltering(_authoredText.Author, _filteringMode))
		{
			if (_runCallbackIfReadyNow)
			{
				_textReadyCallback?.Invoke(orCreateFilterDetails.GetDisplayText(_filtered: false, _bbSupportMode));
			}
			return;
		}
		if (orCreateFilterDetails.IsFiltered())
		{
			if (_runCallbackIfReadyNow)
			{
				_textReadyCallback?.Invoke(orCreateFilterDetails.GetDisplayText(_filtered: true, _bbSupportMode));
			}
			return;
		}
		if (_filteringMode == TextFilteringMode.FilterWithSafeString)
		{
			_textReadyCallback?.Invoke("{...}");
		}
		bool flag;
		lock (lockObj)
		{
			flag = pendingFilterCallbacks.ContainsKey(_authoredText);
			if (!flag)
			{
				pendingFilterCallbacks.Add(_authoredText, null);
			}
			if (_textReadyCallback != null)
			{
				if (pendingFilterCallbacks[_authoredText] == null)
				{
					pendingFilterCallbacks[_authoredText] = new List<(BbCodeSupportMode, Action<string>)>();
				}
				pendingFilterCallbacks[_authoredText].Add((_bbSupportMode, _textReadyCallback));
			}
		}
		if (!flag)
		{
			string textToFilter = GetTextToFilter(orCreateFilterDetails.BaseText, _bbSupportMode);
			PlatformManager.MultiPlatform.TextCensor.CensorProfanity(textToFilter, _authoredText.Author, [PublicizedFrom(EAccessModifier.Internal)] (CensoredTextResult _censorResult) =>
			{
				FilterTextCallback(_authoredText, _censorResult, _bbSupportMode);
			});
		}
	}

	public static void GetDisplayText(string _text, PlatformUserIdentifierAbs _author, Action<string> _textReadyCallback, bool _checkBlockState, TextFilteringMode _filteringMode = TextFilteringMode.Filter, BbCodeSupportMode _bbSupportMode = BbCodeSupportMode.SupportedAndAddEscapes)
	{
		if (_textReadyCallback == null)
		{
			Log.Warning("Could not get display text \"" + _text + "\", no callback action provided");
		}
		if (string.IsNullOrEmpty(_text) || GameManager.IsDedicatedServer)
		{
			_textReadyCallback((_bbSupportMode == BbCodeSupportMode.SupportedAndAddEscapes) ? Utils.EscapeBbCodes(_text) : _text);
			return;
		}
		if (_checkBlockState && _author != null && !PlatformManager.MultiPlatform.User.PlatformUserId.Equals(_author))
		{
			PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(_author);
			if (playerData != null && playerData.PlatformData.Blocked[EBlockType.TextChat].IsBlocked())
			{
				_textReadyCallback?.Invoke("");
				return;
			}
		}
		if (PlatformManager.MultiPlatform.TextCensor == null || ShouldSkipFiltering(_author, _filteringMode))
		{
			_textReadyCallback((_bbSupportMode == BbCodeSupportMode.SupportedAndAddEscapes) ? Utils.EscapeBbCodes(_text) : _text);
			return;
		}
		if (_filteringMode == TextFilteringMode.FilterWithSafeString)
		{
			_textReadyCallback("{...}");
		}
		lock (lockObj)
		{
			if (!pendingFilterCallbacksStrings.ContainsKey(_text))
			{
				pendingFilterCallbacksStrings.Add(_text, new List<(BbCodeSupportMode, Action<string>)>());
			}
			pendingFilterCallbacksStrings[_text].Add((_bbSupportMode, _textReadyCallback));
		}
		string textToFilter = GetTextToFilter(_text, _bbSupportMode);
		PlatformManager.MultiPlatform.TextCensor.CensorProfanity(textToFilter, _author, [PublicizedFrom(EAccessModifier.Internal)] (CensoredTextResult _censorResult) =>
		{
			FilterTextCallbackStrings(_text, _censorResult);
		});
	}

	public static void PrefilterText(AuthoredText _authoredText, TextFilteringMode _filteringMode = TextFilteringMode.Filter)
	{
		GetDisplayText(_authoredText, null, _runCallbackIfReadyNow: false, _checkBlockState: false, _filteringMode);
	}

	public static bool IsFiltered(AuthoredText _authoredText)
	{
		if (_authoredText == null)
		{
			return false;
		}
		if (PlatformManager.MultiPlatform.TextCensor == null || string.IsNullOrEmpty(_authoredText.Text) || GameManager.IsDedicatedServer)
		{
			return true;
		}
		if (authoredTextReferences.TryGetValue(_authoredText, out var value))
		{
			return value.IsFiltered();
		}
		return false;
	}

	public static bool IsFiltering(AuthoredText _authoredText)
	{
		lock (lockObj)
		{
			return _authoredText != null && pendingFilterCallbacks.ContainsKey(_authoredText);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool ShouldSkipFiltering(PlatformUserIdentifierAbs _author, TextFilteringMode _mode)
	{
		switch (_mode)
		{
		case TextFilteringMode.None:
			return true;
		case TextFilteringMode.Filter:
		case TextFilteringMode.FilterWithSafeString:
			return false;
		case TextFilteringMode.FilterOtherPlatforms:
		{
			if (PlatformUserManager.TryGetNativePlatform(_author, out var platform))
			{
				return platform == PlatformManager.NativePlatform.PlatformIdentifier;
			}
			return false;
		}
		default:
			throw new NotImplementedException($"Cannot determine if filtering should be skipped for filtering mode {_mode}");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static AuthoredTextDetails GetOrCreateFilterDetails(AuthoredText _authoredText)
	{
		if (_authoredText == null)
		{
			return null;
		}
		if (authoredTextReferences.TryGetValue(_authoredText, out var value))
		{
			value.SetText(_authoredText.Text);
			return value;
		}
		value = new AuthoredTextDetails(_authoredText.Text);
		authoredTextReferences.Add(_authoredText, value);
		return value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void FilterTextCallback(AuthoredText _authoredText, CensoredTextResult _censorResult, BbCodeSupportMode _originalBBSupport)
	{
		List<(BbCodeSupportMode, Action<string>)> value;
		lock (lockObj)
		{
			if (!pendingFilterCallbacks.TryGetValue(_authoredText, out value))
			{
				Log.Error("Invalid callback information during text filtering.");
				return;
			}
		}
		if (!authoredTextReferences.TryGetValue(_authoredText, out var value2))
		{
			Log.Error("Authored Text filter details not found.");
			return;
		}
		if (GetTextToFilter(value2.BaseText, _originalBBSupport) != _censorResult.OriginalText)
		{
			Log.Warning("Text has changed during filtering process, displayed texts may be outdated.");
		}
		if (_censorResult.Success)
		{
			value2.SetFilteredText(_censorResult.CensoredText);
		}
		else if (_authoredText.Author.Equals(PlatformManager.MultiPlatform.User.PlatformUserId))
		{
			value2.SetFilteredText(value2.BaseText);
		}
		else
		{
			value2.SetFilteredText("{...}");
		}
		lock (lockObj)
		{
			pendingFilterCallbacks.Remove(_authoredText);
		}
		if (value == null)
		{
			return;
		}
		foreach (var item in value)
		{
			var (bbSupportMode, _) = item;
			item.Item2?.Invoke(value2.GetDisplayText(_filtered: true, bbSupportMode));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void FilterTextCallbackStrings(string _text, CensoredTextResult _censorResult)
	{
		List<(BbCodeSupportMode, Action<string>)> value;
		lock (lockObj)
		{
			if (!pendingFilterCallbacksStrings.TryGetValue(_text, out value))
			{
				Log.Error("Invalid callback information during text filtering.");
				return;
			}
			pendingFilterCallbacksStrings.Remove(_text);
		}
		if (value == null)
		{
			return;
		}
		foreach (var item3 in value)
		{
			BbCodeSupportMode item = item3.Item1;
			Action<string> item2 = item3.Item2;
			string obj = item switch
			{
				BbCodeSupportMode.Supported => ReconstructFilteredTextWithBbCodes(_text, _censorResult.CensoredText), 
				BbCodeSupportMode.SupportedAndAddEscapes => Utils.EscapeBbCodes(_censorResult.CensoredText), 
				_ => _censorResult.CensoredText, 
			};
			item2?.Invoke(obj);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetTextToFilter(string _baseText, BbCodeSupportMode _bbSupport)
	{
		if (_baseText == null)
		{
			return null;
		}
		if (_bbSupport != BbCodeSupportMode.Supported)
		{
			return _baseText;
		}
		return Utils.GetVisibileTextWithBbCodes(_baseText);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string ReconstructFilteredTextWithBbCodes(string originalText, string filteredText)
	{
		int num = 0;
		int num2 = 0;
		StringBuilder stringBuilder = new StringBuilder();
		while (num < originalText.Length && num2 < filteredText.Length)
		{
			var (num3, num4, flag) = Utils.FindNextBbCode(originalText, num);
			if (num3 == -1)
			{
				break;
			}
			int num5 = num3 - num;
			stringBuilder.Append(filteredText, num2, num5);
			num2 += num5;
			stringBuilder.Append(originalText, num3, num4);
			num = num3 + num4;
			if (flag)
			{
				num2 += num4 - 4;
			}
		}
		if (num2 < filteredText.Length)
		{
			stringBuilder.Append(filteredText, num2, filteredText.Length - num2);
		}
		return stringBuilder.ToString();
	}
}
