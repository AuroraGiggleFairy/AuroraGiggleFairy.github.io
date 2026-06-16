using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Platform.XBL;

public class TextCensor : ITextCensor
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string isAlpNum = "^[a-zA-Z0-9]+$";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string nonAlphabeticBefore = "(?<![a-zA-Z])";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string nonAlphabeticAfter = "(?![a-zA-Z])";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool fetchStarted;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool fetchComplete;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<string> bannedPatterns = new HashSet<string>();

	public void Init(IPlatform _owner)
	{
	}

	public void Update()
	{
		if (!fetchStarted)
		{
			fetchStarted = true;
			if (PlatformManager.MultiPlatform.RemoteFileStorage != null)
			{
				ThreadManager.StartCoroutine(RetrieveBannedWords());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator RetrieveBannedWords()
	{
		IRemoteFileStorage storage = PlatformManager.MultiPlatform.RemoteFileStorage;
		if (storage != null)
		{
			while (!storage.IsReady)
			{
				yield return null;
			}
			storage.GetFile("BannedWordsXBL.txt", StorageProviderCallback);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StorageProviderCallback(IRemoteFileStorage.EFileDownloadResult _result, string _errorDetails, byte[] _data)
	{
		if (_result != IRemoteFileStorage.EFileDownloadResult.Ok)
		{
			Log.Warning("Retrieving banned words list failed: " + _result.ToStringCached() + " (" + _errorDetails + ")");
			return;
		}
		using (MemoryStream stream = new MemoryStream(_data))
		{
			using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				if (Regex.IsMatch(text, "^[a-zA-Z0-9]+$"))
				{
					bannedPatterns.Add("(?<![a-zA-Z])" + Regex.Escape(text) + "(?![a-zA-Z])");
				}
				else
				{
					bannedPatterns.Add(Regex.Escape(text));
				}
			}
		}
		fetchComplete = true;
	}

	public void CensorProfanity(string _input, PlatformUserIdentifierAbs _author, Action<CensoredTextResult> _callback)
	{
		if (string.IsNullOrEmpty(_input) || _input.Length == 0 || !fetchComplete)
		{
			_callback(new CensoredTextResult(_success: true, _input, _input));
			return;
		}
		if (_author != null)
		{
			if (PlatformManager.CrossplatformPlatform?.User != null && PlatformManager.CrossplatformPlatform.User.PlatformUserId.Equals(_author))
			{
				_callback(new CensoredTextResult(_success: true, _input, _input));
				return;
			}
			if (PlatformManager.NativePlatform?.User != null && (PlatformManager.NativePlatform.User.PlatformUserId.Equals(_author) || PlatformManager.NativePlatform.User.IsFriend(_author)))
			{
				_callback(new CensoredTextResult(_success: true, _input, _input));
				return;
			}
		}
		Task.Run([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			char[] array = _input.ToCharArray();
			foreach (string bannedPattern in bannedPatterns)
			{
				foreach (Match item in Regex.Matches(_input, bannedPattern, RegexOptions.IgnoreCase))
				{
					for (int i = item.Index; i < item.Index + item.Length; i++)
					{
						array[i] = '*';
					}
				}
			}
			_callback(new CensoredTextResult(_success: true, _input, new string(array)));
		});
	}
}
