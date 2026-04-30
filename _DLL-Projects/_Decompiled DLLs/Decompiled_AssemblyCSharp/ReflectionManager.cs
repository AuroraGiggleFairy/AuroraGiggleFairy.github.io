using System.Collections.Generic;
using ShinyScreenSpaceRaytracedReflections;
using UnityEngine;
using UnityEngine.Rendering;

public class ReflectionManager
{
	public class Probe
	{
		public Transform t;

		public ReflectionProbe reflectionProbe;

		public Vector3 worldPos;

		public Vector3 forward;

		public float distSq;

		public float lightLevel;

		public float updateTime;
	}

	public class Sorter : IComparer<Probe>
	{
		public int Compare(Probe _p1, Probe _p2)
		{
			if (_p1.distSq < _p2.distSq)
			{
				return -1;
			}
			if (!(_p1.distSq <= _p2.distSq))
			{
				return 1;
			}
			return 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct Options
	{
		public int rate;

		public float rateScale;

		public float rateRender;

		public float playerVel;

		public float farClip;

		public float shadowDist;

		public int resolution;

		public float intensity;

		public int mask;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cProbeCount = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cUpdateAge = 8f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cUpdatePlayerDistance = 0.3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cUpdateLightDistance = 15f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cBlendInPerSec = 5f / 6f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cOffsetY = 1.55f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cIntensityNoShadowsScale = 0.85f;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject managerObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public Probe mainProbe;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture mainTex;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Probe> probes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasCopySupport;

	[PublicizedFrom(EAccessModifier.Private)]
	public float blendPer;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 blendPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Probe blendProbe;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture blendTex;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture captureTex;

	[PublicizedFrom(EAccessModifier.Private)]
	public Probe renderProbe;

	[PublicizedFrom(EAccessModifier.Private)]
	public float renderDuration;

	[PublicizedFrom(EAccessModifier.Private)]
	public int renderId;

	[PublicizedFrom(EAccessModifier.Private)]
	public float renderFixTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public Sorter sorter = new Sorter();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSkyLayer = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Options[] optionsData = new Options[7]
	{
		default(Options),
		new Options
		{
			rate = 1,
			rateScale = 0.5f,
			rateRender = 2f,
			playerVel = 0.1f,
			farClip = 30f,
			shadowDist = 25f,
			resolution = 64,
			intensity = 0.55f,
			mask = 268435968
		},
		new Options
		{
			rate = 1,
			rateScale = 1f,
			rateRender = 2f,
			playerVel = 0.1f,
			farClip = 90f,
			shadowDist = 30f,
			resolution = 128,
			intensity = 0.65f,
			mask = 276824576
		},
		new Options
		{
			rate = 1,
			rateScale = 1.2f,
			rateRender = 2f,
			playerVel = 0.1f,
			farClip = 180f,
			shadowDist = 50f,
			resolution = 256,
			intensity = 0.66f,
			mask = 276824576
		},
		new Options
		{
			rate = 2,
			rateScale = 1.6f,
			rateRender = 2f,
			playerVel = 0.08f,
			farClip = 280f,
			shadowDist = 70f,
			resolution = 256,
			intensity = 0.67f,
			mask = 276824576
		},
		new Options
		{
			rate = 3,
			rateScale = 12f,
			rateRender = 5f,
			playerVel = 0.01f,
			farClip = 425f,
			shadowDist = 100f,
			resolution = 512,
			intensity = 0.68f,
			mask = 276824576
		},
		new Options
		{
			rate = 3,
			rateScale = 12f,
			rateRender = 5f,
			playerVel = 0.01f,
			farClip = 600f,
			shadowDist = 150f,
			resolution = 512,
			intensity = 0.68f,
			mask = 276824576
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static Options optionsSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] waterTransparencyDistances = new int[13]
	{
		32, 32, 32, 32, 32, 32, 36, 40, 44, 48,
		52, 56, 60
	};

	public static ReflectionManager Create(EntityPlayerLocal player)
	{
		ReflectionManager reflectionManager = new ReflectionManager();
		reflectionManager.player = player;
		reflectionManager.Init();
		return reflectionManager;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		Object.Destroy(player.gameObject.GetComponentInChildren<PlayerReflectionProbe>().gameObject);
		managerObj = new GameObject("ReflectionManager");
		managerObj.layer = 2;
		Transform transform = managerObj.transform;
		hasCopySupport = SystemInfo.copyTextureSupport != CopyTextureSupport.None;
		probes = new List<Probe>();
		if (hasCopySupport)
		{
			for (int i = 0; i < 1; i++)
			{
				Probe item = AddProbe(transform, isMain: false);
				probes.Add(item);
			}
		}
		mainProbe = AddProbe(player.transform, isMain: true);
		int quality = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxReflectQuality);
		bool useShadows = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxReflectShadows);
		ApplyProbeOptions(quality, useShadows);
	}

	public static void ApplyOptions(bool useSimple = false)
	{
		int quality = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxReflectQuality);
		bool useShadows = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxReflectShadows);
		if (useSimple)
		{
			quality = 0;
			useShadows = false;
		}
		World world = GameManager.Instance.World;
		if (world != null)
		{
			List<EntityPlayerLocal> localPlayers = world.GetLocalPlayers();
			for (int num = localPlayers.Count - 1; num >= 0; num--)
			{
				localPlayers[num].renderManager.reflectionManager.ApplyProbeOptions(quality, useShadows);
			}
		}
	}

	public void Destroy()
	{
		Object.Destroy(managerObj);
		probes.Clear();
		if ((bool)mainTex)
		{
			mainTex.Release();
			mainTex = null;
		}
		Object.Destroy(mainProbe.reflectionProbe.gameObject);
		mainProbe = null;
		if ((bool)blendTex)
		{
			blendTex.Release();
			blendTex = null;
		}
		if ((bool)captureTex)
		{
			captureTex.Release();
			captureTex = null;
		}
	}

	public void LightChanged(Vector3 lightPos)
	{
		Probe probe = mainProbe;
		if (probes.Count > 0)
		{
			probe = probes[0];
		}
		if ((lightPos - probe.worldPos).sqrMagnitude <= 225f)
		{
			probe.updateTime = 0f;
		}
	}

	public void FrameUpdate()
	{
		if (optionsSelected.resolution == 0)
		{
			return;
		}
		int count = probes.Count;
		if (renderProbe != null)
		{
			ReflectionProbe reflectionProbe = renderProbe.reflectionProbe;
			if (renderFixTime > 0f)
			{
				renderFixTime -= Time.deltaTime;
				if (renderFixTime <= 0f)
				{
					reflectionProbe.enabled = false;
					reflectionProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
					renderProbe = null;
				}
			}
			else if (reflectionProbe.IsFinishedRendering(renderId))
			{
				if (hasCopySupport && renderProbe == blendProbe)
				{
					blendProbe = null;
				}
				renderProbe = null;
			}
			else
			{
				renderDuration += Time.deltaTime;
				if (renderDuration > 2f)
				{
					renderFixTime = 1f;
					reflectionProbe.refreshMode = ReflectionProbeRefreshMode.EveryFrame;
				}
			}
		}
		Vector3 position = player.position;
		position.y += 1.55f;
		if (player.IsCrouching)
		{
			position.y -= 0.2f;
		}
		Vector3 vector = player.GetVelocityPerSecond() * optionsSelected.playerVel;
		float num = vector.magnitude + 0.15f;
		Vector3 vector2 = position + vector;
		if (num > 0.2f && Physics.SphereCast(position - Origin.position, 0.2f, vector, out var _, num - 0.2f, 1082195968))
		{
			vector2 = position;
		}
		Probe probe = mainProbe;
		if (count > 0)
		{
			SortProbes(vector2);
			probe = probes[0];
			if (probe != blendProbe && probe != renderProbe)
			{
				Graphics.CopyTexture(mainTex, blendTex);
				blendProbe = probe;
				blendPer = 0.05f;
				blendPos = vector2;
			}
			if (blendPer > 0f)
			{
				float magnitude = (vector2 - blendPos).magnitude;
				blendPos = vector2;
				float deltaTime = Time.deltaTime;
				float num2 = magnitude / 0.75f;
				num2 += 5f / 6f * optionsSelected.rateScale * deltaTime;
				num2 *= 1f - Mathf.Pow(blendPer, 0.7f);
				blendPer += num2;
				blendPer += ((renderProbe != null) ? (optionsSelected.rateRender * optionsSelected.rateScale * deltaTime) : 0f);
				if (blendPer < 0.95f)
				{
					ReflectionProbe.BlendCubemap(blendTex, captureTex, blendPer, mainTex);
				}
				else
				{
					ReflectionProbe.BlendCubemap(blendTex, captureTex, 1f, mainTex);
					blendPer = 0f;
				}
			}
		}
		if (renderProbe != null)
		{
			return;
		}
		Probe probe2 = null;
		if (Time.time - probe.updateTime >= 8f)
		{
			probe2 = probe;
		}
		float worldLightLevelInRange = LightManager.GetWorldLightLevelInRange(probe.worldPos, 40f);
		float num3 = worldLightLevelInRange - probe.lightLevel;
		if (num3 < -0.15f || num3 > 0.15f)
		{
			probe2 = probe;
		}
		Vector3 forward = player.cameraTransform.forward;
		if (Vector3.Dot(forward, probe.forward) < 0.7f)
		{
			probe2 = probe;
		}
		float sqrMagnitude = (vector2 - probe.worldPos).sqrMagnitude;
		float num4 = 0.3f / optionsSelected.rateScale;
		if (sqrMagnitude >= num4 * num4)
		{
			probe2 = probe;
			if (count > 1)
			{
				probe2 = probes[count - 1];
			}
		}
		if (probe2 != null)
		{
			probe2.lightLevel = worldLightLevelInRange;
			probe2.updateTime = Time.time;
			probe2.worldPos = vector2;
			probe2.forward = forward;
			probe2.t.position = vector2 - Origin.position;
			ReflectionProbe reflectionProbe2 = probe2.reflectionProbe;
			reflectionProbe2.enabled = true;
			int num5 = renderId;
			renderId = reflectionProbe2.RenderProbe(captureTex);
			if (renderId == num5)
			{
				Log.Warning("{0} ReflectionManager #{1}, rid {2}, probe stuck", GameManager.frameCount, probes.IndexOf(renderProbe), renderId);
			}
			renderProbe = probe2;
			renderDuration = 0f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortProbes(Vector3 pos)
	{
		for (int num = probes.Count - 1; num >= 0; num--)
		{
			Probe probe = probes[num];
			probe.distSq = (pos - probe.worldPos).sqrMagnitude;
		}
		probes.Sort(sorter);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Probe AddProbe(Transform parentT, bool isMain)
	{
		Probe probe = new Probe();
		GameObject obj = new GameObject("RProbe")
		{
			layer = 2
		};
		(probe.t = obj.transform).SetParent(parentT, worldPositionStays: false);
		ReflectionProbe reflectionProbe = (probe.reflectionProbe = obj.AddComponent<ReflectionProbe>());
		reflectionProbe.enabled = false;
		reflectionProbe.mode = ReflectionProbeMode.Realtime;
		reflectionProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
		reflectionProbe.blendDistance = 20f;
		reflectionProbe.center = Vector3.zero;
		reflectionProbe.size = Vector3.zero;
		reflectionProbe.clearFlags = ReflectionProbeClearFlags.SolidColor;
		if (isMain)
		{
			reflectionProbe.blendDistance = 400f;
			reflectionProbe.size = new Vector3(400f, 400f, 400f);
			reflectionProbe.importance = 10;
			if (hasCopySupport)
			{
				reflectionProbe.mode = ReflectionProbeMode.Custom;
			}
		}
		return probe;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyProbeOptions(int quality, bool useShadows)
	{
		if (quality < 0 || quality >= optionsData.Length)
		{
			quality = optionsData.Length - 1;
		}
		optionsSelected = optionsData[quality];
		for (int num = probes.Count - 1; num >= 0; num--)
		{
			Probe probe = probes[num];
			ApplyProbeOptions(probe, useShadows);
		}
		ApplyProbeOptions(mainProbe, useShadows);
		if (optionsSelected.resolution > 0)
		{
			mainProbe.reflectionProbe.enabled = true;
		}
		bool flag = optionsSelected.resolution > 0;
		Shader.SetGlobalFloat("_ReflectionsOn", flag ? 1 : 0);
		if (!flag)
		{
			Shader.EnableKeyword("GAME_NOREFLECTION");
		}
		else
		{
			Shader.DisableKeyword("GAME_NOREFLECTION");
		}
		if (!mainTex || mainTex.width != optionsSelected.resolution)
		{
			if ((bool)mainTex)
			{
				mainTex.Release();
				mainTex = null;
			}
			if (flag)
			{
				mainTex = CreateTexture(autoGenMips: false);
				mainTex.name = "probeMain";
				mainProbe.reflectionProbe.customBakedTexture = mainTex;
				if (!hasCopySupport)
				{
					captureTex = mainTex;
				}
			}
		}
		if (hasCopySupport)
		{
			if (!blendTex || blendTex.width != optionsSelected.resolution)
			{
				if ((bool)blendTex)
				{
					blendTex.Release();
					blendTex = null;
				}
				if (flag)
				{
					blendTex = CreateTexture(autoGenMips: false);
					blendTex.name = "probeBlend";
				}
			}
			if (!captureTex || captureTex.width != optionsSelected.resolution)
			{
				if ((bool)captureTex)
				{
					captureTex.Release();
					captureTex = null;
				}
				if (flag && hasCopySupport)
				{
					captureTex = CreateTexture(autoGenMips: false);
					captureTex.name = "probeCap";
				}
			}
		}
		ApplyWaterSetting();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyProbeOptions(Probe probe, bool useShadows)
	{
		ReflectionProbe reflectionProbe = probe.reflectionProbe;
		reflectionProbe.enabled = false;
		if (optionsSelected.resolution != 0)
		{
			reflectionProbe.nearClipPlane = 0.1f;
			reflectionProbe.farClipPlane = optionsSelected.farClip;
			reflectionProbe.shadowDistance = (useShadows ? optionsSelected.shadowDist : 0f);
			int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxSSReflections);
			reflectionProbe.intensity = optionsSelected.intensity * (useShadows ? 1f : 0.85f) * ((num > 0) ? 0.91f : 1f);
			reflectionProbe.resolution = optionsSelected.resolution;
			reflectionProbe.cullingMask = optionsSelected.mask;
			if (optionsSelected.rate <= 1)
			{
				reflectionProbe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
			}
			else if (optionsSelected.rate <= 2)
			{
				reflectionProbe.timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
			}
			else
			{
				reflectionProbe.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyWaterSetting()
	{
	}

	public void ApplyCameraOptions(Camera camera)
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxSSReflections);
		ShinySSRR component = camera.GetComponent<ShinySSRR>();
		if ((bool)component)
		{
			component.enabled = num >= 1;
			component.jitter = 0.2f;
			switch (num)
			{
			case 1:
				component.ApplyRaytracingPreset(RaytracingPreset.Fast);
				component.minimumBlur = 0.5f;
				break;
			case 2:
				component.ApplyRaytracingPreset(RaytracingPreset.Medium);
				component.minimumBlur = 0.35f;
				component.sampleCount = 32;
				component.maxRayLength = 16f;
				break;
			case 3:
				component.ApplyRaytracingPreset(RaytracingPreset.High);
				component.minimumBlur = 0.3f;
				break;
			case 4:
				component.ApplyRaytracingPreset(RaytracingPreset.Superb);
				component.minimumBlur = 0.2f;
				break;
			}
			component.refineThickness = false;
			component.temporalFilter = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture CreateTexture(bool autoGenMips)
	{
		RenderTextureFormat colorFormat = RenderTextureFormat.ARGB32;
		RenderTextureDescriptor desc = new RenderTextureDescriptor(optionsSelected.resolution, optionsSelected.resolution, colorFormat, 0);
		desc.dimension = TextureDimension.Cube;
		desc.useMipMap = true;
		desc.autoGenerateMips = autoGenMips;
		RenderTexture renderTexture = new RenderTexture(desc);
		renderTexture.Create();
		if (!autoGenMips)
		{
			renderTexture.GenerateMips();
		}
		return renderTexture;
	}
}
