using System.Collections.Generic;
using MusicUtils.Enums;
using UnityEngine;

namespace DynamicMusic.Legacy.ObjectModel;

public class InstrumentID : Dictionary<PlacementType, string>
{
	public delegate void LoadFinishedAction();

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator<bool> thisEnumerator;

	public static string BundlePath;

	public string Name;

	public string SourceName;

	public float Volume = 1f;

	public int Frames;

	public int Samples;

	public int Channels;

	public int Frequency = 44100;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumDictionary<PlacementType, AudioClip> Clips;

	public EnumDictionary<PlacementType, float[]> ClipData;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasGrabbedClipProperties;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isClipLoaded;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsLoaded
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public event LoadFinishedAction OnLoadFinished;

	public InstrumentID()
		: base(3)
	{
		IsLoaded = false;
		ClipData = new EnumDictionary<PlacementType, float[]>(3);
		Clips = new EnumDictionary<PlacementType, AudioClip>(3);
	}

	public void Load()
	{
		if (!IsLoaded)
		{
			if (thisEnumerator == null)
			{
				thisEnumerator = LoadClip();
			}
			thisEnumerator.MoveNext();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator<bool> LoadClip()
	{
		if (!IsLoaded)
		{
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<PlacementType, string> kvp = enumerator.Current;
				LoadManager.AssetRequestTask<AudioClip> requestTask = LoadManager.LoadAsset<AudioClip>(kvp.Value);
				while (!requestTask.IsDone)
				{
					yield return false;
				}
				AudioClip clip = requestTask.Asset;
				if (clip != null)
				{
					if (clip.loadState == AudioDataLoadState.Unloaded)
					{
						clip.LoadAudioData();
						while (clip.loadState != AudioDataLoadState.Loaded)
						{
							yield return false;
							if (clip.loadState == AudioDataLoadState.Failed)
							{
								Log.Warning($"clip load failed in {Name}");
								break;
							}
						}
					}
					if (!hasGrabbedClipProperties)
					{
						Frames = clip.samples;
						Channels = clip.channels;
						Frequency = clip.frequency;
						Samples = Frames * Channels;
						hasGrabbedClipProperties = true;
					}
					else if (Frames != clip.samples || Channels != clip.channels || Frequency != clip.frequency)
					{
						Log.Warning($"Inconsistent clip properties for clips in {Name}");
					}
					int samplesPerPass = 44100;
					int samplesGrabbed = 0;
					int samplesToGrab = Utils.FastMin(samplesPerPass, Samples - samplesGrabbed);
					float[] sampleData = MemoryPools.poolFloat.Alloc(Samples);
					float[] buffer = MemoryPools.poolFloat.Alloc(samplesPerPass);
					while (samplesGrabbed < Samples)
					{
						if (Samples - samplesGrabbed < samplesPerPass)
						{
							buffer = new float[Samples - samplesGrabbed];
						}
						clip.GetData(buffer, samplesGrabbed / 2);
						buffer.CopyTo(sampleData, samplesGrabbed);
						samplesGrabbed += samplesToGrab;
						yield return false;
					}
					MemoryPools.poolFloat.Free(buffer);
					ClipData.Add(kvp.Key, sampleData);
					yield return false;
					clip.UnloadAudioData();
					isClipLoaded = false;
				}
				else
				{
					Log.Warning($"Loaded resource {kvp.Value} could not be boxed as an AudioClip");
				}
			}
		}
		IsLoaded = true;
		this.OnLoadFinished?.Invoke();
		yield return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRequestFinished(AudioClip clip)
	{
		isClipLoaded = true;
	}

	public void Unload()
	{
		foreach (float[] value in ClipData.Values)
		{
			MemoryPools.poolFloat.Free(value);
		}
		ClipData.Clear();
		thisEnumerator = null;
		IsLoaded = (hasGrabbedClipProperties = false);
		Frames = (Samples = (Channels = 0));
		Frequency = 44100;
	}
}
