using System.Collections.Generic;
using DynamicMusic.Legacy.ObjectModel;
using MusicUtils.Enums;
using UnityEngine;

namespace DynamicMusic.Legacy;

public class ThreatLevelStreamer
{
	public ThreatLevelLegacyType threatLevel;

	public static int numCreated;

	public readonly int id;

	public bool IsPaused;

	public Dictionary<LayerType, LayerStreamer> LayerStreamers;

	public bool InitFinished
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			foreach (LayerStreamer value in LayerStreamers.Values)
			{
				if (!value.InitFinished)
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool HasReachedLastHyperbar
	{
		get
		{
			foreach (LayerStreamer value in LayerStreamers.Values)
			{
				if (!value.HasReachedLastHyperbar)
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool IsPlaying
	{
		get
		{
			foreach (LayerStreamer value in LayerStreamers.Values)
			{
				if (value.IsPlaying)
				{
					return true;
				}
			}
			return false;
		}
	}

	public static ThreatLevelStreamer Create(ThreatLevelLegacyType _tl)
	{
		return new ThreatLevelStreamer(_tl);
	}

	public static ThreatLevelStreamer Create(ThreatLevelLegacyType _tl, DynamicMusic.Legacy.ObjectModel.ThreatLevel _groupTL, ThreatLevelConfig _config)
	{
		return new ThreatLevelStreamer(_tl, _groupTL, _config);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreatLevelStreamer(ThreatLevelLegacyType _tl)
	{
		id = numCreated++;
		this.threatLevel = _tl;
		MusicGroup musicGroup = MusicGroup.AllGroups[0];
		ThreatLevelConfig threatLevelConfig = ConfigSet.AllConfigSets[musicGroup.ConfigIDs[0]][_tl];
		DynamicMusic.Legacy.ObjectModel.ThreatLevel threatLevel = musicGroup[_tl];
		LayerStreamers = new EnumDictionary<LayerType, LayerStreamer>();
		foreach (KeyValuePair<LayerType, LayerConfig> item in threatLevelConfig)
		{
			LayerStreamers.Add(item.Key, LayerStreamer.Create(threatLevel[item.Key], item.Value, this));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreatLevelStreamer(ThreatLevelLegacyType _tl, DynamicMusic.Legacy.ObjectModel.ThreatLevel _groupTL, ThreatLevelConfig _config)
	{
		id = numCreated++;
		LayerStreamers = new EnumDictionary<LayerType, LayerStreamer>(_config.Count);
		foreach (KeyValuePair<LayerType, LayerConfig> item in _config)
		{
			LayerStreamers.Add(item.Key, LayerStreamer.Create(_groupTL[item.Key], item.Value, this));
		}
	}

	public void Cleanup()
	{
		foreach (LayerStreamer value in LayerStreamers.Values)
		{
			value.Cleanup();
		}
	}

	public void Play()
	{
		if (!InitFinished)
		{
			return;
		}
		double time = AudioSettings.dspTime + 0.25;
		Log.Out($"Calling Play on {id}");
		foreach (LayerStreamer value in LayerStreamers.Values)
		{
			value.Play(time);
		}
	}

	public void Pause()
	{
		IsPaused = true;
		foreach (LayerStreamer value in LayerStreamers.Values)
		{
			value.Pause();
		}
	}

	public void UnPause()
	{
		IsPaused = false;
		foreach (LayerStreamer value in LayerStreamers.Values)
		{
			value.UnPause();
		}
	}

	public void Stop()
	{
		foreach (LayerStreamer value in LayerStreamers.Values)
		{
			value.Stop();
		}
	}

	public void Tick()
	{
		if (!IsPlaying)
		{
			return;
		}
		foreach (LayerStreamer value in LayerStreamers.Values)
		{
			value.Tick();
		}
	}
}
