using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class ScreenEffects : MonoBehaviour
{
	public class ScreenEffect
	{
		public string Name;

		public Material Material;

		public float Intensity;

		public float TargetIntensity;

		public float FadeTime;
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDefaultFadeTime = 4f;

	public List<ScreenEffect> loadedEffects = new List<ScreenEffect>();

	public List<ScreenEffect> activeEffects = new List<ScreenEffect>();

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public static ScreenEffects Instance
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		Init();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		Material[] array = Resources.LoadAll<Material>("ScreenEffects/");
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].name;
			loadedEffects.Add(new ScreenEffect
			{
				Name = text,
				Material = UnityEngine.Object.Instantiate(array[i]),
				TargetIntensity = 0f,
				Intensity = 0f
			});
		}
		SortEffects();
		if (Instance != null)
		{
			Debug.LogWarning("Detected multiple ScreenEffects instances when only one is expected.");
		}
		else
		{
			Instance = this;
		}
	}

	public void ResetEffects()
	{
		Material[] array = Resources.LoadAll<Material>("ScreenEffects/");
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].name;
			for (int j = 0; j < loadedEffects.Count; j++)
			{
				ScreenEffect screenEffect = loadedEffects[j];
				if (screenEffect.Name == text)
				{
					if (screenEffect.Material != null)
					{
						UnityEngine.Object.Destroy(screenEffect.Material);
					}
					screenEffect.Material = UnityEngine.Object.Instantiate(array[i]);
				}
			}
		}
		SortEffects();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortEffects()
	{
		loadedEffects = loadedEffects.OrderBy([PublicizedFrom(EAccessModifier.Internal)] (ScreenEffect se) => se.Material.renderQueue).ToList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		for (int i = 0; i < loadedEffects.Count; i++)
		{
			UnityEngine.Object.Destroy(loadedEffects[i].Material);
		}
		if (Instance == this)
		{
			Instance = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		int num = 0;
		while (num < activeEffects.Count)
		{
			ScreenEffect screenEffect = activeEffects[num];
			if (screenEffect == null)
			{
				activeEffects.RemoveAt(num);
				continue;
			}
			if (screenEffect.FadeTime <= 0f)
			{
				screenEffect.Intensity = screenEffect.TargetIntensity;
			}
			else if (screenEffect.TargetIntensity > screenEffect.Intensity)
			{
				screenEffect.Intensity = Mathf.Min(screenEffect.Intensity + Time.deltaTime / screenEffect.FadeTime, screenEffect.TargetIntensity);
			}
			else if (screenEffect.TargetIntensity < screenEffect.Intensity)
			{
				screenEffect.Intensity = Mathf.Max(screenEffect.Intensity - Time.deltaTime / screenEffect.FadeTime, screenEffect.TargetIntensity);
			}
			if (screenEffect.Name == "NightVision")
			{
				world.m_WorldEnvironment.SetNightVision(screenEffect.Intensity);
			}
			if (screenEffect.Intensity == screenEffect.TargetIntensity && screenEffect.Intensity <= 0f)
			{
				activeEffects.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	public void SetScreenEffect(string _name, float _intensity = 1f, float _fadeTime = 4f)
	{
		ScreenEffect screenEffect = Find(_name, activeEffects);
		if (screenEffect == null)
		{
			if (_intensity <= 0f)
			{
				return;
			}
			screenEffect = Find(_name, loadedEffects);
			if (screenEffect == null)
			{
				return;
			}
			int renderQueue = screenEffect.Material.renderQueue;
			int index = activeEffects.Count;
			for (int i = 0; i < activeEffects.Count; i++)
			{
				ScreenEffect screenEffect2 = activeEffects[i];
				if (screenEffect2 != null && renderQueue <= screenEffect2.Material.renderQueue)
				{
					index = i;
					break;
				}
			}
			activeEffects.Insert(index, screenEffect);
		}
		screenEffect.TargetIntensity = _intensity;
		screenEffect.FadeTime = _fadeTime;
	}

	public void DisableScreenEffects()
	{
		for (int i = 0; i < activeEffects.Count; i++)
		{
			DisableScreenEffect(activeEffects[i].Name);
		}
	}

	public void DisableScreenEffect(string _name)
	{
		SetScreenEffect(_name, 0f, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ScreenEffect Find(string _name, List<ScreenEffect> _list)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			ScreenEffect screenEffect = _list[i];
			if (screenEffect != null && screenEffect.Name == _name)
			{
				return screenEffect;
			}
		}
		return null;
	}

	public void RenderScreenEffects(PostProcessRenderContext context)
	{
		int count = activeEffects.Count;
		if (count == 0)
		{
			Debug.LogWarning("RenderEffect called when no effects are active, incurring a redundant blit. The check in ScreenEffectsProxy.IsEnabledAndSupported is supposed to avoid cases like this.");
			context.command.Blit(context.source, context.destination);
			return;
		}
		int num = -1;
		int num2 = -1;
		if (count > 1)
		{
			num = Shader.PropertyToID("_TempRT1");
			context.command.GetTemporaryRT(num, context.width, context.height);
			if (count > 2)
			{
				num2 = Shader.PropertyToID("_TempRT2");
				context.command.GetTemporaryRT(num2, context.width, context.height);
			}
		}
		RenderTargetIdentifier renderTargetIdentifier = context.source;
		for (int i = 0; i < count; i++)
		{
			ScreenEffect screenEffect = activeEffects[i];
			screenEffect.Material.SetFloat("Intensity", Mathf.Clamp01(screenEffect.Intensity));
			if (i >= count - 1)
			{
				context.command.Blit(renderTargetIdentifier, context.destination, screenEffect.Material);
			}
			else if (i == 0)
			{
				context.command.Blit(context.source, num, screenEffect.Material);
				renderTargetIdentifier = num;
			}
			else
			{
				RenderTargetIdentifier renderTargetIdentifier2 = ((renderTargetIdentifier == num) ? num2 : num);
				context.command.Blit(renderTargetIdentifier, renderTargetIdentifier2, screenEffect.Material);
				renderTargetIdentifier = renderTargetIdentifier2;
			}
		}
		if (count > 1)
		{
			context.command.ReleaseTemporaryRT(num);
			if (count > 2)
			{
				context.command.ReleaseTemporaryRT(num2);
			}
		}
	}
}
