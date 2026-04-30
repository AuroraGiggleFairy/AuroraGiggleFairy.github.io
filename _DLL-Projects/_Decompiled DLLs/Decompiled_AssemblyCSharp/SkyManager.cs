using System;
using System.Runtime.CompilerServices;
using PI.NGSS;
using UnityEngine;

public class SkyManager : MonoBehaviour
{
	public static SkyManager skyManager;

	public static float dayCount;

	public static bool indoorFogOn = true;

	public static Material atmosphereMtrl;

	public static Transform atmosphereSphere;

	public static Material cloudsSphereMtrl;

	public static Transform cloudsSphere;

	public static bool bUpdateSunMoonNow = false;

	public static float sSunFadeHeight = 0.1f;

	public static float dayPercent;

	public static GameRandom random;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float sMaxSunIntensity = 0.7f;

	public float maxSunIntensity = 0.7f;

	public float sunHeight = 0.1f;

	public float moonHeight = 0.095f;

	public float sunFadeHeight = 0.1f;

	public float cloudSpeed = 0.05f;

	public float ShowFogDensity;

	public Color ShowSkyColor;

	public Color ShowFogColor;

	public float maxFarClippingPlane = 1500f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int dawnTime = 4;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int duskTime = 22;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static ulong timeOfDay;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int bloodmoonDay = 7;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float cloudTransition = 0f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float fogDensity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float fogLightScale;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float fogStart = 20f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float fogEnd = 80f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float fogDebugDensity = -1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float fogDebugStart;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float fogDebugEnd;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Color fogDebugColor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float worldRotationTime = 0f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float worldRotation = 0f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float worldRotationTarget = 0f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float sunIntensity = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float starIntensity = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float weatherLightScale;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool bNeedsReset;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Color SkyColor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Color fogColor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform sunLightT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Light sunLight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 sunDirV;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 sunMoonDirV;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float[] moonPhases = new float[7] { 0.05f, 0.35f, 0.55f, 0.75f, 1.4f, 1.63f, 1.82f };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform moonLightT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Light moonLight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform moonSpriteT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Material moonSpriteMat;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float moonBright;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Color moonLightColor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Color moonSpriteColor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Quaternion moonLightRot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture cloudMainTex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture cloudMainTexOld;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture cloudBlendTex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture cloudBlendTexOld;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject parent;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Camera mainCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static NGSS_FrustumShadows_7DTD frustumShadows;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cWorldRotationUpdateFreq = 0.2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cStarRotationSpeed = 0.004f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bUpdateShaders;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector4 fogParams;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 sunAxis;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 sunStartV;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 moonStartV;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 starAxis;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool triggerLightning;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int lightningFlashes;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float[] lightningEndTimes = new float[5] { 1f, 2f, 3f, 4f, 5f };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int lightningIndex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 lightningDir;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 lightningDirVel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float lightningIntensity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static SunShaftsEffect.SunSettings sunShaftSettings;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 targetSkyPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bMovingSky = true;

	public static Transform SunLightT => sunLightT;

	public static bool IsBloodMoonVisible()
	{
		return GameUtils.IsBloodMoonTime((duskHour: duskTime - 4, dawnHour: dawnTime + 2), (int)TimeOfDay(), bloodmoonDay, (int)dayCount);
	}

	public static float BloodMoonVisiblePercent()
	{
		int num = (int)dayCount;
		float num2 = TimeOfDay();
		if (num == bloodmoonDay)
		{
			float num3 = (float)duskTime - num2;
			if (num3 < 0f)
			{
				return 1f;
			}
			float num4 = 1f - num3 / 4f;
			if (num4 >= 0f)
			{
				return num4;
			}
			return 0f;
		}
		return (num > 1 && num == bloodmoonDay + 1 && num2 <= (float)(dawnTime + 2)) ? 1 : 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		GameOptionsManager.ShadowDistanceChanged += OnShadowDistanceChanged;
		GameStats.OnChangedDelegates += OnGameStatsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		GameOptionsManager.ShadowDistanceChanged -= OnShadowDistanceChanged;
		GameStats.OnChangedDelegates -= OnGameStatsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnShadowDistanceChanged(int optionsShadowDistance)
	{
		int shadowCustomResolution;
		switch (optionsShadowDistance)
		{
		case 0:
		case 1:
		case 2:
			shadowCustomResolution = 1024;
			break;
		case 3:
			shadowCustomResolution = 2048;
			break;
		default:
			shadowCustomResolution = 4096;
			break;
		}
		sunLight.shadowCustomResolution = shadowCustomResolution;
		moonLight.shadowCustomResolution = shadowCustomResolution;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGameStatsChanged(EnumGameStats _gameState, object _newValue)
	{
		switch (_gameState)
		{
		case EnumGameStats.BloodMoonDay:
			bloodmoonDay = GameStats.GetInt(EnumGameStats.BloodMoonDay);
			break;
		case EnumGameStats.DayLightLength:
			(duskTime, dawnTime) = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		Reset();
	}

	public static void Loaded(GameObject _obj)
	{
		_obj.SetActive(value: true);
		_obj.transform.parent = GameManager.Instance.transform;
		skyManager = _obj.GetComponent<SkyManager>();
		Reset();
	}

	public static void Cleanup()
	{
		if (skyManager != null)
		{
			UnityEngine.Object.DestroyImmediate(skyManager.gameObject);
		}
	}

	public static void SetSkyEnabled(bool _enabled)
	{
		if (_enabled)
		{
			SetFogDebug();
			mainCamera.backgroundColor = Color.black;
		}
		else
		{
			SetFogDebug(0f);
			mainCamera.backgroundColor = new Color(0.44f, 0.48f, 0.52f);
		}
		atmosphereSphere.gameObject.SetActive(_enabled);
		cloudsSphere.gameObject.SetActive(_enabled);
	}

	public static Color GetSkyColor()
	{
		return SkyColor;
	}

	public static void SetSkyColor(Color c)
	{
		SkyColor = c;
	}

	public static void SetGameTime(ulong _time)
	{
		dayCount = (float)_time / 24000f + 1f;
		timeOfDay = _time;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float TimeOfDay()
	{
		return (float)(timeOfDay % 24000) / 1000f;
	}

	public static float GetTimeOfDayAsMinutes()
	{
		return TimeOfDay() / 24f * (float)GamePrefs.GetInt(EnumGamePrefs.DayNightLength);
	}

	public static void SetCloudTextures(Texture _mainTex, Texture _blendTex)
	{
		cloudMainTex = _mainTex;
		cloudBlendTex = _blendTex;
	}

	public static void SetCloudTransition(float t)
	{
		cloudTransition = t;
	}

	public static Color GetFogColor()
	{
		return fogColor;
	}

	public static void SetFogColor(Color c)
	{
		if (fogDebugColor.a > 0f)
		{
			c = fogDebugColor;
		}
		fogColor = c;
	}

	public static float GetFogDensity()
	{
		return fogDensity;
	}

	public static void SetFogDensity(float density)
	{
		if (fogDebugDensity >= 0f)
		{
			density = fogDebugDensity;
		}
		fogDensity = density;
		float num = density - 0.65f;
		if (num < 0f)
		{
			num = 0f;
		}
		fogLightScale = 1f - num * 1.7f;
	}

	public static float GetFogStart()
	{
		return fogStart;
	}

	public static float GetFogEnd()
	{
		return fogEnd;
	}

	public static void SetFogFade(float start, float end)
	{
		float t = 1f;
		World world = GameManager.Instance.World;
		if (world != null)
		{
			t = (world.GetPrimaryPlayer().bPlayingSpawnIn ? 1f : 0.01f);
		}
		fogStart = Mathf.Lerp(fogStart, start, t);
		fogEnd = Mathf.Lerp(fogEnd, end, t);
		if (fogDebugDensity >= 0f)
		{
			if (fogDebugStart > -1000f)
			{
				fogStart = fogDebugStart;
			}
			if (fogDebugEnd > -1000f)
			{
				fogEnd = fogDebugEnd;
			}
		}
	}

	public static void SetFogDebug(float density = -1f, float start = float.MinValue, float end = float.MinValue)
	{
		fogDebugDensity = density;
		fogDebugStart = start;
		fogDebugEnd = end;
		SetFogDensity(0f);
	}

	public static void SetFogDebugColor(Color color = default(Color))
	{
		fogDebugColor = color;
		SetFogColor(Color.gray);
	}

	public static void Reset()
	{
		bNeedsReset = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearStatics()
	{
		random = null;
		fogLightScale = 1f;
		weatherLightScale = 1f;
		cloudTransition = 1f;
		cloudMainTexOld = null;
		cloudBlendTex = null;
		parent = null;
		sunLightT = null;
		sunLight = null;
		moonLightT = null;
		moonLight = null;
		moonSpriteT = null;
		mainCamera = null;
		moonSpriteMat = null;
		cloudsSphere = null;
		cloudsSphereMtrl = null;
		atmosphereSphere = null;
		atmosphereMtrl = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		random = GameRandomManager.Instance.CreateGameRandom();
		RenderSettings.fog = true;
		RenderSettings.fogMode = FogMode.Exponential;
		sunAxis = Vector3.forward;
		sunStartV = Quaternion.AngleAxis(20f, Vector3.up) * Vector3.left;
		moonStartV = Quaternion.AngleAxis(35f, Vector3.up) * Vector3.right;
		starAxis = Vector3.forward;
		if (parent == null)
		{
			parent = GameObject.Find("SkySystem(Clone)");
			if (parent == null)
			{
				return;
			}
		}
		if (sunLightT == null)
		{
			Transform transform = parent.transform.Find("SunLight");
			if (transform != null)
			{
				sunLightT = transform;
			}
		}
		if (sunLight == null && sunLightT != null)
		{
			sunLight = sunLightT.transform.GetComponent<Light>();
		}
		if (moonLightT == null)
		{
			moonLightT = parent.transform.Find("MoonLight");
			if ((bool)moonLightT)
			{
				moonLight = moonLightT.GetComponent<Light>();
			}
		}
		GetMaterialAndTransform("MoonSprite", ref moonSpriteT, ref moonSpriteMat);
		GetMaterialAndTransform("CloudsSphere", ref cloudsSphere, ref cloudsSphereMtrl);
		GetMaterialAndTransform("AtmosphereSphere", ref atmosphereSphere, ref atmosphereMtrl);
		MeshFilter component = moonSpriteT.GetComponent<MeshFilter>();
		if (component != null)
		{
			Mesh mesh = component.mesh;
			if (mesh != null)
			{
				mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000000f);
			}
		}
		if ((bool)cloudsSphereMtrl)
		{
			cloudsSphereMtrl.SetFloat("_CloudSpeed", cloudSpeed);
		}
		if (mainCamera == null)
		{
			GetMainCamera();
		}
		OnShadowDistanceChanged(GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowDistance));
		bloodmoonDay = GameStats.GetInt(EnumGameStats.BloodMoonDay);
		(duskTime, dawnTime) = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GetMaterialAndTransform(string objectName, ref Transform trans, ref Material mtrl)
	{
		Transform transform = parent.transform.Find(objectName);
		if (trans == null)
		{
			trans = transform;
		}
		if (mtrl == null)
		{
			MeshRenderer component = transform.GetComponent<MeshRenderer>();
			mtrl = component.material;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetMainCamera()
	{
		mainCamera = Camera.main;
	}

	public static bool IsDark()
	{
		float num = TimeOfDay();
		if (!(num < (float)dawnTime))
		{
			return num > (float)duskTime;
		}
		return true;
	}

	public static float GetDawnTime()
	{
		return dawnTime;
	}

	public static float GetDawnTimeAsMinutes()
	{
		return (float)dawnTime / 24f * (float)GamePrefs.GetInt(EnumGamePrefs.DayNightLength);
	}

	public static float GetDuskTime()
	{
		return duskTime;
	}

	public static float GetDuskTimeAsMinutes()
	{
		return (float)duskTime / 24f * (float)GamePrefs.GetInt(EnumGamePrefs.DayNightLength);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float CalcDayPercent()
	{
		float num;
		if (worldRotation < 0.5f)
		{
			num = Mathf.Pow(1f - Utils.FastAbs(0.25f - worldRotation) * 4f, 0.6f);
			num = num * 0.68f + 0.5f;
			if (num > 1f)
			{
				num = 1f;
			}
		}
		else
		{
			num = Mathf.Pow(1f - Utils.FastAbs(0.75f - worldRotation) * 4f, 0.6f);
			num = 0.5f - num * 0.68f;
			if (num < 0f)
			{
				num = 0f;
			}
		}
		return num;
	}

	public static void TriggerLightning(Vector3 _position)
	{
		if (!triggerLightning)
		{
			lightningFlashes = random.RandomRange(2, 6);
			float num = Time.time;
			for (int i = 0; i < lightningFlashes; i++)
			{
				float num2 = random.RandomRange(0.1f, 0.21f);
				num += num2;
				lightningEndTimes[i] = num;
			}
			lightningIndex = -1;
			lightningDir.x = random.RandomFloat * 360f;
			lightningDir.y = random.RandomRange(8, 20);
			triggerLightning = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateSunMoonAngles()
	{
		if (!sunLight || !moonLight)
		{
			return;
		}
		if (Time.time - worldRotationTime >= 0.2f || bUpdateSunMoonNow)
		{
			worldRotationTime = Time.time;
			bUpdateSunMoonNow = false;
			float num = TimeOfDay();
			if (num >= (float)dawnTime && num < (float)duskTime)
			{
				worldRotationTarget = (num - (float)dawnTime) / (float)(duskTime - dawnTime);
			}
			else
			{
				float num2 = 24 - duskTime;
				float num3 = num2 + (float)dawnTime;
				if (num < (float)dawnTime)
				{
					worldRotationTarget = (num2 + num) / num3;
				}
				else
				{
					worldRotationTarget = (num - (float)duskTime) / num3;
				}
				worldRotationTarget += 1f;
			}
			worldRotationTarget *= 0.5f;
			worldRotationTarget = Mathf.Clamp01(worldRotationTarget);
		}
		float num4 = worldRotationTarget - worldRotation;
		float num5 = worldRotationTarget;
		if (num4 < -0.5f)
		{
			num5 += 1f;
		}
		else if (num4 > 0.5f)
		{
			num5 -= 1f;
		}
		worldRotation = Mathf.Lerp(worldRotation, num5, 0.05f);
		if (worldRotation < 0f)
		{
			worldRotation += 1f;
		}
		else if (worldRotation >= 1f)
		{
			worldRotation -= 1f;
		}
		dayPercent = CalcDayPercent();
		float angle = worldRotation * 360f;
		sunDirV = Quaternion.AngleAxis(angle, sunAxis) * sunStartV;
		moonLightRot = Quaternion.LookRotation(Quaternion.AngleAxis(angle, sunAxis) * moonStartV);
		float num6 = worldRotation * 360f;
		if (sunIntensity >= 0.001f)
		{
			if (num6 < 14f)
			{
				num6 = 14f;
			}
			if (num6 > 166f)
			{
				num6 = 166f;
			}
			Vector3 eulerAngles = Quaternion.LookRotation(Quaternion.AngleAxis(num6, sunAxis) * sunStartV).eulerAngles;
			sunLightT.localEulerAngles = eulerAngles;
			sunLight.shadowStrength = 1f;
			sunLight.shadows = ((sunIntensity > 0f) ? LightShadows.Soft : LightShadows.None);
			moonLight.enabled = false;
		}
		else if (moonLightColor.grayscale > 0f)
		{
			if (num6 < 166f)
			{
				num6 = 166f;
			}
			if (num6 > 346f)
			{
				num6 = 346f;
			}
			Vector3 eulerAngles2 = Quaternion.LookRotation(Quaternion.AngleAxis(num6, sunAxis) * moonStartV).eulerAngles;
			moonLightT.localEulerAngles = eulerAngles2;
			float num7 = fogLightScale * moonBright;
			float t = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxBrightness) * 2f;
			num7 *= Utils.FastLerp(0.2f, 1f, t);
			moonLight.intensity = num7;
			moonLight.color = moonLightColor;
			moonLight.shadowStrength = 1f;
			moonLight.shadows = ((num7 > 0f) ? LightShadows.Soft : LightShadows.None);
			moonLight.enabled = true;
		}
		else
		{
			moonLight.enabled = false;
		}
		sunMoonDirV = sunDirV;
		if (sunIntensity < 0.001f)
		{
			sunMoonDirV = moonLightRot * Vector3.forward;
		}
		if (!GameManager.IsDedicatedServer && (bool)mainCamera)
		{
			Vector3 position = mainCamera.transform.position;
			if ((bool)moonSpriteT)
			{
				moonSpriteT.position = moonLightRot * Vector3.forward * -45000f;
				moonSpriteT.rotation = Quaternion.LookRotation(moonSpriteT.position, Vector3.up);
				moonSpriteT.position += position;
				float num8 = 6857.143f;
				if (IsBloodMoonVisible())
				{
					num8 *= 1.3f;
				}
				moonSpriteT.localScale = new Vector3(num8, num8, num8);
			}
			UpdateSunShaftSettings();
		}
		atmosphereSphere.Rotate(starAxis, worldRotation * 0.004f);
		if (bUpdateShaders && (bool)cloudsSphereMtrl)
		{
			cloudsSphereMtrl.SetVector("_SunDir", sunDirV);
			cloudsSphereMtrl.SetVector("_SunMoonDir", sunMoonDirV);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateSunShaftSettings()
	{
		Vector3 vector = GetSunDirection();
		if (vector.y <= 0.09f)
		{
			sunShaftSettings.sunShaftIntensity = 0.1f + GetFogDensity() * 0.5f;
			sunShaftSettings.sunColor = GetSunLightColor();
			sunShaftSettings.sunThreshold = new Color(0.87f, 0.74f, 0.65f, 1f);
		}
		else
		{
			vector = GetMoonDirection();
			sunShaftSettings.sunShaftIntensity = 0.06f + GetFogDensity() * 0.85f;
			sunShaftSettings.sunColor = GetMoonLightColor();
			sunShaftSettings.sunThreshold = new Color(0.8f, 0.6f, 0.6f, 1f);
		}
		sunShaftSettings.sunPosition = vector * -100000f;
	}

	public static SunShaftsEffect.SunSettings GetSunShaftSettings()
	{
		return sunShaftSettings;
	}

	public static float GetLuma(Color color)
	{
		return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool VerifyCamera()
	{
		if (GameManager.IsDedicatedServer)
		{
			return true;
		}
		if (mainCamera == null)
		{
			GetMainCamera();
			if (mainCamera == null)
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool VerifyValidMaterials()
	{
		if (atmosphereMtrl == null)
		{
			return false;
		}
		if (cloudsSphere == null)
		{
			return false;
		}
		if (atmosphereSphere == null)
		{
			return false;
		}
		if (moonLight == null)
		{
			return false;
		}
		if (sunLight == null)
		{
			return false;
		}
		if (moonSpriteT == null)
		{
			return false;
		}
		if (moonSpriteMat == null)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetIfNeeded()
	{
		if (bNeedsReset)
		{
			ClearStatics();
			Init();
			bNeedsReset = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateFogShader()
	{
		fogParams.x = GetFogStart();
		fogParams.y = GetFogEnd();
		fogParams.z = 1f;
		fogParams.w = Mathf.Pow(GetFogDensity(), 2f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateShaderGlobals()
	{
		Color color = GetFogColor();
		float num = GetFogDensity();
		num = (RenderSettings.fogDensity = num * (num * num));
		RenderSettings.fogColor = color;
		Shader.SetGlobalVector("_FogParams", new Vector4(num, fogParams.x, fogParams.y, 0f));
		Shader.SetGlobalColor("_FogColor", color.linear);
		Shader.SetGlobalVector("SunColor", sunLight.color);
		Shader.SetGlobalVector("FogColor", color);
		Shader.SetGlobalFloat("_HighResViewDistance", (float)GameUtils.GetViewDistance() * 16f);
		Shader.SetGlobalFloat("_DayPercent", dayPercent);
	}

	public void Update()
	{
		if (parent == null)
		{
			Init();
			if (parent == null)
			{
				return;
			}
		}
		sMaxSunIntensity = maxSunIntensity;
		if ((bool)mainCamera)
		{
			float num = fogDensity + 0.001f;
			num *= fogDensity * fogDensity;
			mainCamera.farClipPlane = Utils.FastClamp(6f / num + 400f, 200f, 2800f);
		}
		int num2 = Time.frameCount & 1;
		bUpdateShaders = num2 == 0 || triggerLightning;
		if (!VerifyCamera())
		{
			return;
		}
		ResetIfNeeded();
		UpdateSunMoonAngles();
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		float magnitude = (parent.transform.position - mainCamera.transform.position).magnitude;
		if (magnitude < 0.001f)
		{
			bMovingSky = false;
			targetSkyPosition = mainCamera.transform.position;
		}
		else if (!bMovingSky && magnitude > 5f)
		{
			bMovingSky = true;
			targetSkyPosition = mainCamera.transform.position;
		}
		if (bMovingSky)
		{
			parent.transform.position = Vector3.Lerp(parent.transform.position, targetSkyPosition, Mathf.Clamp01(Time.deltaTime * 10f));
		}
		UpdateFogShader();
		if (!bUpdateShaders || !VerifyValidMaterials())
		{
			return;
		}
		sSunFadeHeight = sunFadeHeight;
		UpdateShaderGlobals();
		bool num3 = IsBloodMoonVisible();
		int num4 = 0;
		moonSpriteColor = new Color(1f, 0.14f, 0.05f) * 1.5f;
		if (!num3)
		{
			num4 = (int)(dayCount + 5.5f) % 7;
			moonSpriteColor = Color.Lerp(Color.white, moonLightColor, 0.2f);
		}
		float num5 = moonPhases[num4];
		float f = num5 * MathF.PI;
		Vector3 vector = new Vector3(0f - Mathf.Sin(f), 0f, Mathf.Cos(f));
		moonSpriteMat.SetVector("_LightDir", vector);
		moonSpriteMat.SetColor("_Color", moonSpriteColor);
		moonBright = 1f - num5;
		if (moonBright < 0f)
		{
			moonBright = 0f - moonBright;
		}
		float b = ((Mathf.Pow(moonLightColor.grayscale, 0.45f) - 0.5f) * 0.5f + 0.5f) * moonBright;
		float value = Mathf.Max(sunIntensity, b);
		if (WeatherManager.currentWeather != null)
		{
			starIntensity = 1f - WeatherManager.currentWeather.CloudThickness() * 0.01f;
		}
		if (bUpdateShaders)
		{
			atmosphereMtrl.SetColor("_SkyColor", GetSkyColor());
			atmosphereMtrl.SetFloat("_Stars", starIntensity);
			atmosphereMtrl.SetVector("_SunDir", sunDirV);
			cloudsSphereMtrl.SetFloat("_CloudTransition", cloudTransition);
			cloudsSphereMtrl.SetFloat("_LightIntensity", value);
			cloudsSphereMtrl.SetColor("_SkyColor", GetSkyColor());
			cloudsSphereMtrl.SetColor("_SunColor", sunLight.color);
			cloudsSphereMtrl.SetVector("_SunDir", sunDirV);
			cloudsSphereMtrl.SetVector("_SunMoonDir", sunMoonDirV);
			cloudsSphereMtrl.SetColor("_MoonColor", moonSpriteColor);
		}
		if (cloudMainTex != cloudMainTexOld)
		{
			cloudsSphereMtrl.SetTexture("_CloudMainTex", cloudMainTex);
			cloudMainTexOld = cloudMainTex;
		}
		if (cloudBlendTex != cloudBlendTexOld)
		{
			cloudsSphereMtrl.SetTexture("_CloudBlendTex", cloudBlendTex);
			cloudBlendTexOld = cloudBlendTex;
		}
		if (triggerLightning)
		{
			float deltaTime = Time.deltaTime;
			Light light = moonLight;
			lightningIntensity -= 4f * deltaTime;
			if (lightningIndex < 0 || Time.time >= lightningEndTimes[lightningIndex])
			{
				lightningIndex++;
				lightningIntensity = 0.75f + random.RandomFloat * 0.25f;
				lightningDir.x += random.RandomRange(-20f, 20f);
				lightningDirVel.x = random.RandomRange(-50f, 50f);
				lightningDir.y += random.RandomRange(-8, 8);
			}
			if (lightningIndex >= lightningFlashes)
			{
				triggerLightning = false;
				cloudsSphereMtrl.SetFloat("_Lightning", 0f);
			}
			else
			{
				lightningIntensity = Utils.FastMax(0f, lightningIntensity);
				cloudsSphereMtrl.SetFloat("_Lightning", lightningIntensity);
				lightningDir += lightningDirVel * deltaTime;
				float f2 = lightningDir.x * (MathF.PI / 180f);
				Vector3 vector2 = default(Vector3);
				vector2.x = Mathf.Sin(f2);
				vector2.y = Mathf.Sin(lightningDir.y * (MathF.PI / 180f)) * 1.5f;
				vector2.z = Mathf.Cos(f2);
				cloudsSphereMtrl.SetVector("_LightningDir", vector2.normalized);
				Transform transform = GameManager.Instance.World.GetPrimaryPlayer().transform;
				Transform obj = light.transform;
				Vector3 vector3 = vector2 * 200f;
				vector3.y = 300f;
				obj.position = transform.position + vector3;
				obj.LookAt(transform);
				light.color = Color.white;
				light.intensity = lightningIntensity * 0.9f;
				light.shadows = LightShadows.Hard;
				light.shadowStrength = 1f;
				light.enabled = true;
			}
		}
		if (frustumShadows == null)
		{
			frustumShadows = mainCamera.GetComponent<NGSS_FrustumShadows_7DTD>();
		}
		if (frustumShadows != null)
		{
			frustumShadows.mainShadowsLight = ((!IsDark()) ? sunLight : moonLight);
		}
	}

	public static float GetSunAngle()
	{
		return sunDirV.y;
	}

	public static Vector3 GetSunDirection()
	{
		return sunDirV;
	}

	public static float GetSunPercent()
	{
		return 0f - sunDirV.y;
	}

	public static Vector3 GetSunLightDirection()
	{
		if (!(sunLight == null))
		{
			return sunLightT.forward;
		}
		return Vector3.down;
	}

	public static float GetSunIntensity()
	{
		return sunIntensity;
	}

	public static Color GetSunLightColor()
	{
		if ((bool)sunLight)
		{
			return sunLight.color;
		}
		return Color.black;
	}

	public static void SetSunColor(Color color)
	{
		if (sunLight != null)
		{
			sunLight.color = color;
		}
	}

	public static void SetSunIntensity(float _intensity)
	{
		sunIntensity = _intensity;
		float sunAngle = GetSunAngle();
		if (sunAngle >= 0f - sSunFadeHeight)
		{
			sunIntensity = (0f - sunAngle) * 10f * sunIntensity * (float)((sunAngle < 0f) ? 1 : 0);
		}
		sunIntensity = Mathf.Clamp(sunIntensity, 0f, sMaxSunIntensity);
		sunIntensity *= 1.5f;
		if ((bool)sunLight)
		{
			sunLight.intensity = sunIntensity * fogLightScale * weatherLightScale;
		}
	}

	public static void SetWeatherLightScale(float _scale)
	{
		weatherLightScale = _scale;
	}

	public static float GetMoonAmbientScale(float add, float mpy)
	{
		return Utils.FastLerp(add + moonBright * mpy, 1f, dayPercent * 3.030303f);
	}

	public static float GetMoonBrightness()
	{
		return moonLightColor.grayscale * moonBright;
	}

	public static Color GetMoonLightColor()
	{
		return moonLightColor;
	}

	public static Vector3 GetMoonDirection()
	{
		return moonLightRot * Vector3.forward;
	}

	public static void SetMoonLightColor(Color color)
	{
		moonLightColor = color;
	}

	public static float GetWorldRotation()
	{
		return worldRotation;
	}
}
