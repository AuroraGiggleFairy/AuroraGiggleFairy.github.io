using DynamicMusic.Legacy.ObjectModel;
using MusicUtils.Enums;
using UnityEngine;

namespace DynamicMusic.Legacy;

public class LayerStreamer
{
	public bool InitFinished;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int parentId;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly InstrumentID instrumentID;

	[PublicizedFrom(EAccessModifier.Private)]
	public LayerConfig LayerConfig;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioSource Src;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hyperbar;

	[PublicizedFrom(EAccessModifier.Private)]
	public int cursor;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] currentClipData;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool InFillStream;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool HasReachedLastHyperbar
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsPlaying
	{
		get
		{
			if (!Src)
			{
				return false;
			}
			return Src.isPlaying;
		}
	}

	public static LayerStreamer Create(Layer _layer, LayerConfig _layerConfig, ThreatLevelStreamer _parent = null)
	{
		return new LayerStreamer(_layer, _layerConfig, _parent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public LayerStreamer(Layer _layer, LayerConfig _layerConfig, ThreatLevelStreamer _parent)
	{
		parentId = _parent.id;
		HasReachedLastHyperbar = false;
		LayerConfig = _layerConfig;
		instrumentID = _layer.GetInstrumentID();
		if (instrumentID.IsLoaded)
		{
			OnClipSetLoad();
		}
		else
		{
			instrumentID.OnLoadFinished += OnClipSetLoad;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnClipSetLoad()
	{
		Src = Object.Instantiate(Resources.Load<AudioSource>(instrumentID.SourceName));
		Src.volume = instrumentID.Volume;
		Src.clip = AudioClip.Create(instrumentID.Name, instrumentID.Frames, instrumentID.Channels, instrumentID.Frequency, stream: true, FillStream);
		InitFinished = true;
	}

	public void Cleanup()
	{
		if ((bool)Src)
		{
			Object.Destroy(Src.gameObject);
		}
		instrumentID.OnLoadFinished -= OnClipSetLoad;
		instrumentID.Unload();
	}

	public void Play(double _time)
	{
		Src.PlayScheduled(_time);
		Src.loop = true;
	}

	public void Pause()
	{
		if ((bool)Src && Src.isPlaying)
		{
			Src.Pause();
		}
	}

	public void UnPause()
	{
		if ((bool)Src)
		{
			Src.UnPause();
		}
	}

	public void Stop()
	{
		if ((bool)Src && Src.isPlaying)
		{
			Src.Stop();
		}
	}

	public void Tick()
	{
		if (HasReachedLastHyperbar && (bool)Src && Src.loop)
		{
			Src.loop = false;
			Log.Out($"Set loop to false on {instrumentID.Name}");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FillStream(float[] data)
	{
		if (!InFillStream)
		{
			InFillStream = true;
			int num = data.Length;
			for (int i = 0; i < num; i++)
			{
				if (cursor == 0)
				{
					if (LayerConfig.TryGetValue((byte)hyperbar++, out var value))
					{
						currentClipData = instrumentID.ClipData[value];
						HasReachedLastHyperbar = value == PlacementType.End;
					}
					else
					{
						currentClipData = null;
					}
				}
				data[i] = ((currentClipData != null) ? currentClipData[cursor] : 0f);
				cursor++;
				cursor %= instrumentID.Samples;
			}
			InFillStream = false;
		}
		else
		{
			Log.Warning("FillStream was called while it was still running.");
		}
	}
}
