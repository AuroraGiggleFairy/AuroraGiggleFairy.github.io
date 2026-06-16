using System;
using System.Collections.Generic;
using UniLinq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class ScreenEffects : MonoBehaviour
{
	public class ScreenEffect : PostProcessEffectSubRenderer
	{
		public string Name;

		public Material Material;

		public float Intensity;

		public float TargetIntensity;

		public float FadeTime;

		public GameObject particlePrefab;

		public ParticleSystem[] particleSystems;

		public override void Render(PostProcessRenderContext context)
		{
			Material.SetFloat("Intensity", Mathf.Clamp01(Intensity));
			context.command.Blit(context.source, context.destination, Material);
		}
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDefaultFadeTime = 4f;

	public List<ScreenEffect> loadedEffects = new List<ScreenEffect>();

	public List<ScreenEffect> activeEffects = new List<ScreenEffect>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<ParticleSystem> particleSystems = new List<ParticleSystem>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public NativeArray<ParticleSystem.Particle> particles;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Matrix4x4 prevViewProjMatrix;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Camera cameraRef;

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
		cameraRef = GetComponent<Camera>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		prevViewProjMatrix = GL.GetGPUProjectionMatrix(cameraRef.nonJitteredProjectionMatrix, renderIntoTexture: false) * cameraRef.worldToCameraMatrix;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		if (Instance != null)
		{
			Log.Warning("ScreenEffects instance already exists!");
		}
		else
		{
			Instance = this;
		}
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
		particles = new NativeArray<ParticleSystem.Particle>(64, Allocator.Persistent);
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
		particles.Dispose();
		if (Instance == this)
		{
			Instance = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreRender()
	{
		Matrix4x4 value = GL.GetGPUProjectionMatrix(cameraRef.nonJitteredProjectionMatrix, renderIntoTexture: false) * cameraRef.worldToCameraMatrix;
		Shader.SetGlobalMatrix("_PrevViewProjMatrix", prevViewProjMatrix);
		Shader.SetGlobalMatrix("_CurrViewProjMatrix", value);
		Shader.SetGlobalMatrix("_CurrViewProjMatrix_Inverse", value.inverse);
		prevViewProjMatrix = value;
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
			float intensity = screenEffect.Intensity;
			if (screenEffect.FadeTime <= 0f)
			{
				screenEffect.Intensity = screenEffect.TargetIntensity;
			}
			else if (screenEffect.TargetIntensity > screenEffect.Intensity)
			{
				screenEffect.Intensity = Utils.FastMin(screenEffect.Intensity + Time.deltaTime / screenEffect.FadeTime, screenEffect.TargetIntensity);
			}
			else if (screenEffect.TargetIntensity < screenEffect.Intensity)
			{
				screenEffect.Intensity = Utils.FastMax(screenEffect.Intensity - Time.deltaTime / screenEffect.FadeTime, screenEffect.TargetIntensity);
			}
			if (screenEffect.Intensity != intensity)
			{
				if (screenEffect.Name == "NightVision")
				{
					world.m_WorldEnvironment.SetNightVision(screenEffect.Intensity);
				}
				if (screenEffect.particleSystems != null)
				{
					for (int i = 0; i < screenEffect.particleSystems.Length; i++)
					{
						ParticleSystem particleSystem = screenEffect.particleSystems[i];
						int num2 = particleSystem.GetParticles(particles);
						for (int j = 0; j < num2; j++)
						{
							ParticleSystem.Particle value = particles[j];
							Color32 startColor = value.startColor;
							startColor.a = (byte)(screenEffect.Intensity * 255f);
							value.startColor = startColor;
							particles[j] = value;
						}
						particleSystem.SetParticles(particles, num2);
					}
				}
			}
			if (screenEffect.Intensity == screenEffect.TargetIntensity && screenEffect.Intensity <= 0f)
			{
				activeEffects.RemoveAt(num);
				if ((bool)screenEffect.particlePrefab)
				{
					UnityEngine.Object.Destroy(screenEffect.particlePrefab);
					screenEffect.particlePrefab = null;
				}
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
			GameObject gameObject = Resources.Load<GameObject>("ScreenEffects/" + _name);
			if ((bool)gameObject)
			{
				gameObject = (screenEffect.particlePrefab = UnityEngine.Object.Instantiate(gameObject));
				EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
				if ((bool)primaryPlayer)
				{
					Transform obj = gameObject.transform;
					obj.SetParent(primaryPlayer.cameraTransform, worldPositionStays: false);
					obj.SetLocalPositionAndRotation(new Vector3(0f, 0f, 0.12f), Quaternion.identity);
				}
				gameObject.GetComponentsInChildren(particleSystems);
				if (particleSystems.Count > 0)
				{
					screenEffect.particleSystems = particleSystems.ToArray();
					particleSystems.Clear();
				}
			}
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
}
