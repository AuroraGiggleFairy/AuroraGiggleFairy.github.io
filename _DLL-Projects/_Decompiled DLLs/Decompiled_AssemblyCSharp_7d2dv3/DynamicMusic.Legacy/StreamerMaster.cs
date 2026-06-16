using System.Collections.Generic;
using MusicUtils.Enums;

namespace DynamicMusic.Legacy;

public class StreamerMaster
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static EnumDictionary<ThreatLevelLegacyType, Queue<ThreatLevelStreamer>> Streamers;

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicMusicManager dynamicMusicManager;

	public static ThreatLevelStreamer currentStreamer;

	public bool IsReplacementNecessary
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (currentStreamer != null)
			{
				if (currentStreamer.HasReachedLastHyperbar)
				{
					return !currentStreamer.IsPlaying;
				}
				return false;
			}
			return true;
		}
	}

	public static StreamerMaster Create()
	{
		return new StreamerMaster();
	}

	public static void Init(DynamicMusicManager _dynamicMusicManager)
	{
		_dynamicMusicManager.StreamerMaster = Create();
		_dynamicMusicManager.StreamerMaster.dynamicMusicManager = _dynamicMusicManager;
		Streamers = new EnumDictionary<ThreatLevelLegacyType, Queue<ThreatLevelStreamer>>();
	}

	public void Tick()
	{
		LayerReserve.Tick();
		if (currentStreamer != null)
		{
			currentStreamer.Tick();
		}
		if (IsReplacementNecessary)
		{
			Log.Out("Getting new currentStreamer!");
			ReplaceCurrentStreamer();
		}
		if (!dynamicMusicManager.IsInDeadWindow)
		{
			if (dynamicMusicManager.FrequencyManager.CanScheduleTrack && dynamicMusicManager.IsPlayAllowed)
			{
				Play();
			}
		}
		else
		{
			Stop();
		}
	}

	public void Play()
	{
		if (currentStreamer != null)
		{
			currentStreamer.Play();
		}
	}

	public void Pause()
	{
		if (currentStreamer != null)
		{
			currentStreamer.Pause();
		}
	}

	public void UnPause()
	{
		if (currentStreamer != null)
		{
			currentStreamer.UnPause();
		}
	}

	public void Stop()
	{
		if (currentStreamer != null && (currentStreamer.IsPlaying || currentStreamer.IsPaused))
		{
			currentStreamer.Stop();
			ReplaceCurrentStreamer();
		}
	}

	public void Cleanup()
	{
		if (currentStreamer != null)
		{
			currentStreamer.Cleanup();
			currentStreamer = null;
		}
		if (Streamers != null)
		{
			Streamers.Clear();
			Streamers = null;
		}
	}

	public void ReplaceCurrentStreamer()
	{
		if (currentStreamer != null)
		{
			currentStreamer.Cleanup();
		}
		ThreatLevelStreamer item = ThreatLevelStreamer.Create(ThreatLevelLegacyType.Exploration);
		if (Streamers.TryGetValue(ThreatLevelLegacyType.Exploration, out var value))
		{
			if (value.Count > 0)
			{
				currentStreamer = value.Dequeue();
				value.Enqueue(item);
			}
			else
			{
				currentStreamer = item;
				value.Enqueue(ThreatLevelStreamer.Create(ThreatLevelLegacyType.Exploration));
			}
		}
		else
		{
			Streamers.Add(ThreatLevelLegacyType.Exploration, value = new Queue<ThreatLevelStreamer>());
			value.Enqueue(ThreatLevelStreamer.Create(ThreatLevelLegacyType.Exploration));
			currentStreamer = item;
		}
	}
}
