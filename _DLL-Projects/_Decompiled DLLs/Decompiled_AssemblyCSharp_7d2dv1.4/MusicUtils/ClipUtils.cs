using System;
using System.Collections;
using UnityEngine;

namespace MusicUtils;

public static class ClipUtils
{
	public static AudioClip LoadClipImmediate(string _path)
	{
		return LoadManager.LoadAsset<AudioClip>(_path, null, null, false, true).Asset;
	}

	public static IEnumerator LoadClipFrom(string _path, Func<AudioClip, float[]> _onAudioClipLoad, Action _onFinish)
	{
		LoadManager.AssetRequestTask<AudioClip> req = LoadManager.LoadAsset<AudioClip>(_path);
		yield return new WaitUntil([PublicizedFrom(EAccessModifier.Internal)] () => req.IsDone);
		if ((bool)req.Asset)
		{
			yield return StripClip(req.Asset, _onAudioClipLoad(req.Asset));
			_onFinish?.Invoke();
		}
	}

	public static IEnumerator StripClip(AudioClip _clip, float[] _data)
	{
		float[] buffer = MemoryPools.poolFloat.Alloc(44100);
		int cursor = 0;
		yield return null;
		while (cursor < _clip.samples)
		{
			_clip.GetData(buffer, cursor);
			yield return null;
			for (int i = 0; i < buffer.Length; i += 2)
			{
				if (cursor >= _clip.samples)
				{
					break;
				}
				_data[2 * cursor] = buffer[i];
				_data[2 * cursor++ + 1] = buffer[i + 1];
			}
			yield return null;
		}
		MemoryPools.poolFloat.Free(buffer);
	}
}
