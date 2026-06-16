using System.Collections;
using MusicUtils.Enums;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public abstract class LayeredSection<T> : Section, ISection, IPlayable, IFadeable, ICleanable where T : ILayerMixer, new()
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_FillStreamMarker = new ProfilerMarker("DynamicMusic.LayeredSection.FillStream");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_LoadContentMarker = new ProfilerMarker("DynamicMusic.LayeredSection.LoadContentCoroutine");

	[PublicizedFrom(EAccessModifier.Protected)]
	public T Mixer;

	public int cursor;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool preloaded;

	public LayeredSection()
	{
		Mixer = new T();
	}

	public override void Init()
	{
		base.Init();
		Reset();
		bool isReady = (base.IsInitialized = true);
		IsReady = isReady;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Reset()
	{
		cursor = 0;
		bool isReady = (IsDone = false);
		IsReady = isReady;
		if ((bool)src)
		{
			src.loop = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override IEnumerator PlayCoroutine()
	{
		yield return LoadContentCoroutine();
		src?.Play();
	}

	public override IEnumerator PreloadRoutine()
	{
		Log.Out("Preloading LayeredSection {0} : Type: {1}", base.Sect.ToString(), GetType().ToString());
		yield return LoadContentCoroutine();
		preloaded = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator LoadContentCoroutine()
	{
		IsReady = false;
		ref T mixer = ref Mixer;
		SectionType sect = base.Sect;
		mixer.Sect = sect;
		if (!preloaded)
		{
			yield return Mixer.Load();
		}
		else
		{
			preloaded = false;
			yield return null;
		}
		if (!src)
		{
			using (s_LoadContentMarker.Auto())
			{
				AudioSource component = DataLoader.LoadAsset<GameObject>(Content.SourcePathFor[base.Sect]).GetComponent<AudioSource>();
				src = Object.Instantiate(component);
				src.transform.SetParent(Section.parent.transform);
				src.name = base.Sect.ToString();
				src.loop = true;
				src.priority = 0;
				src.clip = AudioClip.Create(base.Sect.ToString(), Content.SamplesFor[base.Sect], 2, 44100, stream: true, FillStream);
			}
		}
		LayeredSection<T> layeredSection = this;
		LayeredSection<T> layeredSection2 = this;
		bool isReady = true;
		layeredSection2.IsInitialized = true;
		layeredSection.IsReady = isReady;
		LoadRoutine = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void FillStream(float[] data)
	{
		using (s_FillStreamMarker.Auto())
		{
			for (int i = 0; i < data.Length; i++)
			{
				int num = i;
				ref T mixer = ref Mixer;
				int idx = cursor++;
				data[num] = mixer[idx];
			}
		}
	}
}
