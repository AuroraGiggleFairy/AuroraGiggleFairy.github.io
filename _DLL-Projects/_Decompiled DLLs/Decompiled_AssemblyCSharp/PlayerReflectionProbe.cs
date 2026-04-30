using System;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(ReflectionProbe))]
public class PlayerReflectionProbe : MonoBehaviour
{
	public int RefreshRate = 30;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static PlayerReflectionProbe playerReflectionProbe = null;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ReflectionProbe[] allReflectionProbes;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ReflectionProbe reflectionProbe;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool hasReflections = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int lastRenderID = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float nextUpdate;

	public float fWaterDisCheck = 2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ReflectionProbeTimeSlicingMode timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bEyeNearWaterSurface;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int raycastMask = 16;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float updateEyeNearWaterTimer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float eyeNearWaterUpdateFreq = 1.25f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTimeUpdatedProbe;

	public float updateProbeWhileEyeNearWaterFreq = 3f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] viewDistantToWaterTransparencyDistance = new int[13]
	{
		32, 32, 32, 32, 32, 32, 36, 40, 44, 48,
		52, 56, 60
	};

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Camera mainCamera = null;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int catchUpFrameCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxFramesToUpdate = 14;

	public static void UpdateCamera(Camera _mainCamera)
	{
		mainCamera = _mainCamera;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		allReflectionProbes = base.transform.parent.GetComponentsInChildren<ReflectionProbe>();
		reflectionProbe = GetComponent<ReflectionProbe>();
		playerReflectionProbe = this;
		SetReflectionSettings(GamePrefs.GetInt(EnumGamePrefs.OptionsGfxReflectQuality), GamePrefs.GetBool(EnumGamePrefs.OptionsGfxReflectShadows));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Destroy()
	{
		playerReflectionProbe = null;
		reflectionProbe = null;
		allReflectionProbes = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (!hasReflections || reflectionProbe == null || mainCamera == null)
		{
			return;
		}
		if (Time.time > updateEyeNearWaterTimer + eyeNearWaterUpdateFreq)
		{
			bEyeNearWaterSurface = false;
			if (Physics.SphereCast(new Ray(mainCamera.transform.position + Vector3.up, Vector3.down), 0.5f, out var hitInfo, fWaterDisCheck, 16))
			{
				bEyeNearWaterSurface = hitInfo.distance < float.PositiveInfinity;
			}
		}
		bool flag = !bEyeNearWaterSurface || Time.time > lastTimeUpdatedProbe + updateProbeWhileEyeNearWaterFreq;
		flag &= Time.realtimeSinceStartup > nextUpdate;
		flag &= reflectionProbe.enabled;
		if (lastRenderID == -1 || catchUpFrameCount < 14)
		{
			for (int i = 0; i < allReflectionProbes.Length; i++)
			{
				allReflectionProbes[i].refreshMode = ReflectionProbeRefreshMode.EveryFrame;
				allReflectionProbes[i].timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
			}
			flag = true;
		}
		else if (catchUpFrameCount == 14)
		{
			for (int j = 0; j < allReflectionProbes.Length; j++)
			{
				allReflectionProbes[j].refreshMode = ReflectionProbeRefreshMode.ViaScripting;
				allReflectionProbes[j].timeSlicingMode = timeSlicingMode;
			}
		}
		catchUpFrameCount++;
		if (flag)
		{
			for (int k = 0; k < allReflectionProbes.Length; k++)
			{
				lastRenderID = allReflectionProbes[k].RenderProbe();
			}
			lastTimeUpdatedProbe = Time.time;
			nextUpdate = Time.realtimeSinceStartup + 1000f / (float)RefreshRate * 0.001f;
		}
	}

	public static void SetReflectionSettings(int qualityLevel, bool bReflectedShadows)
	{
		if ((bool)playerReflectionProbe)
		{
			playerReflectionProbe.setReflectionSettings(qualityLevel, bReflectedShadows);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setReflectionSettings(int qualityLevel, bool bReflectedShadows)
	{
		if (reflectionProbe == null)
		{
			return;
		}
		lastRenderID = -1;
		catchUpFrameCount = 0;
		RenderSettings.reflectionBounces = 1;
		hasReflections = qualityLevel > 0;
		for (int i = 0; i < allReflectionProbes.Length; i++)
		{
			allReflectionProbes[i].enabled = hasReflections;
		}
		Shader.SetGlobalFloat("_ReflectionsOn", hasReflections ? 1 : 0);
		if (!hasReflections)
		{
			Shader.EnableKeyword("GAME_NOREFLECTION");
			return;
		}
		reflectionProbe.nearClipPlane = 0.1f;
		reflectionProbe.intensity = 1f;
		Shader.DisableKeyword("GAME_NOREFLECTION");
		switch (qualityLevel)
		{
		case 1:
		{
			RefreshRate = 3;
			reflectionProbe.farClipPlane = 30f;
			reflectionProbe.shadowDistance = (bReflectedShadows ? 10 : 0);
			reflectionProbe.resolution = 128;
			timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
			reflectionProbe.intensity = 0.5f;
			string[] options6 = new string[2] { "ReflectionsOnly", "Terrain" };
			SetOptions(options6);
			break;
		}
		case 2:
		{
			RefreshRate = 5;
			reflectionProbe.farClipPlane = 100f;
			reflectionProbe.shadowDistance = (bReflectedShadows ? 20 : 0);
			reflectionProbe.resolution = 256;
			timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
			reflectionProbe.intensity = 0.55f;
			string[] options2 = new string[3] { "Trees", "ReflectionsOnly", "Terrain" };
			SetOptions(options2);
			break;
		}
		case 3:
		{
			RefreshRate = 15;
			reflectionProbe.farClipPlane = 200f;
			reflectionProbe.shadowDistance = (bReflectedShadows ? 50 : 0);
			reflectionProbe.resolution = 256;
			timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
			reflectionProbe.intensity = 0.7f;
			string[] options4 = new string[3] { "Trees", "ReflectionsOnly", "Terrain" };
			SetOptions(options4);
			break;
		}
		case 4:
		{
			RefreshRate = 18;
			reflectionProbe.farClipPlane = 280f;
			reflectionProbe.shadowDistance = (bReflectedShadows ? 70 : 0);
			reflectionProbe.resolution = 256;
			timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
			reflectionProbe.intensity = 0.85f;
			string[] options3 = new string[3] { "Trees", "ReflectionsOnly", "Terrain" };
			SetOptions(options3);
			break;
		}
		case 5:
		{
			RefreshRate = 24;
			reflectionProbe.farClipPlane = 425f;
			reflectionProbe.shadowDistance = (bReflectedShadows ? 100 : 0);
			reflectionProbe.resolution = 512;
			timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
			string[] options5 = new string[3] { "Trees", "ReflectionsOnly", "Terrain" };
			SetOptions(options5);
			break;
		}
		default:
		{
			RefreshRate = 30;
			reflectionProbe.farClipPlane = 600f;
			reflectionProbe.shadowDistance = (bReflectedShadows ? 150 : 0);
			reflectionProbe.resolution = 512;
			timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
			string[] options = new string[3] { "Trees", "ReflectionsOnly", "Terrain" };
			SetOptions(options);
			break;
		}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetOptions(string[] cullingMasks)
	{
		reflectionProbe.cullingMask = LayerMask.GetMask(cullingMasks);
	}
}
