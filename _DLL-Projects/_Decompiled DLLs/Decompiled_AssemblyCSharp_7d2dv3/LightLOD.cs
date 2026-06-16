using System;
using Audio;
using UnityEngine;

public class LightLOD : MonoBehaviour
{
	public delegate void MaxIntensityEvent();

	public GameObject LitRootObject;

	public Transform[] RefIlluminatedMaterials;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Material[][] cachedMaterials;

	public Transform RefFlare;

	public float MaxDistance = 30f;

	public float DistanceScale = 1f;

	public float FlareBrightnessFactor = 1f;

	public bool bPlayerPlacedLight;

	public bool bSwitchedOn;

	public bool bToggleable = true;

	public bool bWorksUnderwater = true;

	public bool isHeld;

	public Light otherLight;

	public LightStateType lightStateStart;

	public string cyclingAudioClipPath;

	public float cyclingAudioOffset;

	public float cyclingAudioPlayEveryNCycles = 1f;

	public bool EmissiveFromLightColorOn;

	public Color EmissiveColor = Color.white;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color EmissiveColorOff = Color.black;

	public float StateRate = 1f;

	public float FluxDelay = 1f;

	public static float DebugViewDistance;

	public MaxIntensityEvent MaxIntensityChanged;

	public bool WaterLevelDirty;

	public float priority;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isCulled;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform selfT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform parentT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasInitialized;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BlockEntityData bed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInitialBlockDone;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Light myLight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lightIntensityMaster;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lightIntensity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lightRangeMaster;

	[NonSerialized]
	public float lightRange;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lightViewDistance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bRenderingOff;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LensFlare lensFlare;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float[] maxLightPowerValues;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 registeredPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float registeredRange;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LightStateType lightStateType;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LightState lightState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LightShadows shadowStateMaster;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float shadowStrengthMaster;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AudioPlayer audioPlayer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Handle cyclingAudioHandle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isUnderwater;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float waterLevelRecheckTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Color? lastEmissiveColor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float previousCycleTime;

	public LightStateType LightStateType
	{
		get
		{
			return lightStateType;
		}
		set
		{
			if (lightStateType != value && !GameManager.IsDedicatedServer)
			{
				if ((bool)lightState)
				{
					UnityEngine.Object.Destroy(lightState);
				}
				lightStateType = value;
				if (lightStateType != LightStateType.Static)
				{
					Type type = Type.GetType(lightStateType.ToStringCached());
					lightState = (LightState)base.gameObject.AddComponent(type);
				}
			}
		}
	}

	public float MaxIntensity
	{
		get
		{
			return lightIntensity;
		}
		set
		{
			lightIntensity = value;
			lightIntensityMaster = value;
			MaxIntensityChanged?.Invoke();
		}
	}

	public float LightAngle
	{
		set
		{
			if (myLight.type == LightType.Spot)
			{
				if (value > 160f)
				{
					value = 160f;
				}
				myLight.spotAngle = value;
			}
		}
	}

	public Light GetLight()
	{
		if (!myLight)
		{
			if (!otherLight)
			{
				myLight = GetComponent<Light>();
			}
			else
			{
				myLight = otherLight;
			}
			if ((bool)myLight)
			{
				shadowStateMaster = myLight.shadows;
				shadowStrengthMaster = myLight.shadowStrength;
			}
		}
		return myLight;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		Init();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		if (hasInitialized)
		{
			return;
		}
		hasInitialized = true;
		selfT = base.transform;
		Light light = GetLight();
		if ((bool)light)
		{
			if (GameManager.IsDedicatedServer && registeredRange == 0f)
			{
				SetRange(light.range);
			}
			lightIntensityMaster = light.intensity;
			lightRangeMaster = light.range;
			CalcViewDistance();
			if (lightStateType != LightStateType.Static)
			{
				Type type = Type.GetType(lightStateType.ToStringCached());
				lightState = base.gameObject.GetComponent(type) as LightState;
				if (!lightState)
				{
					lightState = (LightState)base.gameObject.AddComponent(type);
				}
			}
		}
		if ((bool)RefFlare)
		{
			lensFlare = RefFlare.GetComponent<LensFlare>();
		}
		else
		{
			lensFlare = GetComponent<LensFlare>();
		}
		if (RefIlluminatedMaterials != null)
		{
			maxLightPowerValues = new float[RefIlluminatedMaterials.Length];
			cachedMaterials = new Material[RefIlluminatedMaterials.Length][];
			for (int i = 0; i < RefIlluminatedMaterials.Length; i++)
			{
				Transform transform = RefIlluminatedMaterials[i];
				if (!transform)
				{
					continue;
				}
				Renderer component = transform.GetComponent<Renderer>();
				if (!(component != null))
				{
					continue;
				}
				Material material = component.material;
				if (material != null)
				{
					if (material.HasProperty("_LightPower"))
					{
						maxLightPowerValues[i] = Utils.FastMax(material.GetFloat("_LightPower"), 1f);
					}
					else
					{
						maxLightPowerValues[i] = 1f;
					}
				}
				cachedMaterials[i] = component.materials;
			}
		}
		if (lightStateStart != LightStateType.Static)
		{
			LightStateType = lightStateStart;
		}
		if (!GameManager.IsDedicatedServer)
		{
			audioPlayer = GetComponent<AudioPlayer>();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		Init();
		if (GameManager.Instance.World != null)
		{
			lightRange = lightRangeMaster;
			lightIntensity = lightIntensityMaster;
			if ((bool)myLight)
			{
				myLight.enabled = false;
			}
			isInitialBlockDone = false;
			parentT = selfT.parent;
			LightManager.LightChanged(selfT.position + Origin.position);
			if (GameLightManager.Instance != null)
			{
				GameLightManager.Instance.AddLight(this);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckInitialBlock()
	{
		if (isInitialBlockDone || !parentT || !selfT)
		{
			return;
		}
		if (bed == null)
		{
			isInitialBlockDone = true;
			if (!isHeld && registeredRange == 0f && (bool)myLight)
			{
				SetRange(lightRange);
			}
			return;
		}
		World world = GameManager.Instance.World;
		BlockValue block = world.GetBlock(bed.pos);
		Block block2 = block.Block;
		if (block2.isMultiBlock && block.ischild)
		{
			Vector3i parentPos = block2.multiBlockPos.GetParentPos(bed.pos, block);
			block = world.GetBlock(parentPos);
		}
		if (bToggleable)
		{
			SwitchOnOff((block.meta & 2) != 0);
		}
		if (registeredRange == 0f && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && (bool)myLight)
		{
			SetRange(lightRange);
		}
		isInitialBlockDone = true;
	}

	public void SetEmissiveColor(bool _on)
	{
		SetEmissiveColor(_on ? 1f : 0f);
	}

	public void SetEmissiveColor(float _newV)
	{
		if (_newV <= 0f)
		{
			SetEmissiveColor(EmissiveColorOff);
			return;
		}
		Color color;
		if (!EmissiveFromLightColorOn)
		{
			color = EmissiveColor;
		}
		else
		{
			Light light = GetLight();
			if (!light)
			{
				return;
			}
			color = light.color;
		}
		if (_newV >= 1f)
		{
			SetEmissiveColor(color);
		}
		Color.RGBToHSV(color, out var H, out var S, out var _);
		Color emissiveColor = Color.HSVToRGB(H, S, _newV);
		SetEmissiveColor(emissiveColor);
	}

	public void SetEmissiveColor(Color _color)
	{
		if (RefIlluminatedMaterials == null || (lastEmissiveColor.HasValue && _color == lastEmissiveColor.Value))
		{
			return;
		}
		lastEmissiveColor = _color;
		bool flag = _color != EmissiveColorOff;
		for (int i = 0; i < cachedMaterials.Length; i++)
		{
			Material[] array = cachedMaterials[i];
			if (array == null)
			{
				continue;
			}
			foreach (Material material in array)
			{
				if ((bool)material)
				{
					material.SetColor(EmissionColorId, _color);
					if (flag)
					{
						material.EnableKeyword("_EMISSION");
					}
					else
					{
						material.DisableKeyword("_EMISSION");
					}
				}
			}
		}
	}

	public void SetRange(float _range)
	{
		Light light = GetLight();
		lightRange = _range;
		lightRangeMaster = _range;
		light.range = _range;
		CalcViewDistance();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && base.enabled && light.enabled)
		{
			UnregisterFromLightManager();
			registeredPos = light.transform.position + Origin.position;
			registeredRange = LightManager.RegisterLight(light);
		}
	}

	public void TestRegistration()
	{
		if ((bool)myLight)
		{
			SetRange(lightRange);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcViewDistance()
	{
		lightViewDistance = Utils.FastMax(MaxDistance, lightRangeMaster * 1.5f);
	}

	public void SetBlockEntityData(BlockEntityData _bed)
	{
		bed = _bed;
	}

	public void SwitchOnOff(bool _isOn, bool _ignoreToggle = false)
	{
		if ((_ignoreToggle || bToggleable) && _isOn != bSwitchedOn)
		{
			bSwitchedOn = _isOn;
		}
		if (bSwitchedOn)
		{
			base.enabled = true;
		}
	}

	public void SetCulled(bool _culled)
	{
		isCulled = _culled;
		if ((bool)myLight)
		{
			myLight.enabled = !_culled;
		}
	}

	public void OnDisable()
	{
		if (!string.IsNullOrEmpty(cyclingAudioClipPath) && cyclingAudioHandle != null)
		{
			cyclingAudioHandle.SetVolume(0f);
		}
		if (GameManager.Instance.World != null)
		{
			LightManager.LightChanged(selfT.position + Origin.position);
			if (GameLightManager.Instance != null)
			{
				GameLightManager.Instance.RemoveLight(this);
			}
		}
		UnregisterFromLightManager();
		parentT = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UnregisterFromLightManager()
	{
		if (registeredRange > 0f)
		{
			LightManager.UnRegisterLight(registeredPos, registeredRange);
			registeredRange = 0f;
		}
	}

	public void FrameUpdate(Vector3 _cameraPos)
	{
		bool flag = lightStateType != LightStateType.Static;
		priority = (flag ? 1 : 0);
		if (bRenderingOff || !myLight)
		{
			return;
		}
		CheckInitialBlock();
		bool flag2 = bSwitchedOn && (!flag || lightState.CanBeOn);
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		if (!isCulled)
		{
			num = (selfT.position - _cameraPos).sqrMagnitude * DistanceScale;
			num2 = Mathf.Sqrt(num) - lightRange;
			if (num2 < 0f)
			{
				num2 = 0f;
			}
		}
		float num3 = lightViewDistance;
		if (bPlayerPlacedLight)
		{
			num3 *= 1.2f;
		}
		if (DebugViewDistance > 0f)
		{
			num3 = Utils.FastMax(DebugViewDistance, lightRange + 0.01f);
		}
		float num4 = num3 * num3;
		float num5 = num / num4;
		float num6 = num3 - lightRange;
		if (flag2)
		{
			flag2 = flag2 && num2 < num6;
			if (lightStateType != LightStateType.Blinking)
			{
				flag2 &= num5 < (flag ? lightState.LODThreshold : 1f);
			}
		}
		if (flag2 && !bWorksUnderwater)
		{
			bool flag3 = false;
			if (waterLevelRecheckTime != float.MaxValue)
			{
				if (Time.time >= waterLevelRecheckTime)
				{
					waterLevelRecheckTime = float.MaxValue;
					if (WaterLevelDirty)
					{
						waterLevelRecheckTime = Time.time + 0.5f;
					}
					WaterLevelDirty = false;
					flag3 = bed != null;
				}
			}
			else if (WaterLevelDirty)
			{
				WaterLevelDirty = false;
				flag3 = bed != null;
				if (flag3)
				{
					waterLevelRecheckTime = Time.time + 0.5f;
				}
			}
			if (flag3)
			{
				World world = GameManager.Instance.World;
				bool flag4 = world.IsWater(bed.pos.x, bed.pos.y + 1, bed.pos.z);
				if (!flag4 && world.IsWater(bed.pos))
				{
					flag4 = 0.6f + (float)bed.pos.y > base.transform.position.y;
				}
				if (isUnderwater != flag4)
				{
					isUnderwater = flag4;
				}
			}
			flag2 &= !isUnderwater;
		}
		if (flag2)
		{
			if (bPlayerPlacedLight)
			{
				if (num5 >= 0.64000005f)
				{
					myLight.shadows = LightShadows.None;
				}
				else if (num5 >= 0.0625f)
				{
					if (shadowStateMaster == LightShadows.Soft)
					{
						myLight.shadows = LightShadows.Hard;
					}
					myLight.shadowStrength = (1f - Utils.FastClamp01((num5 - 0.36f) / 0.28000003f)) * shadowStrengthMaster;
				}
				else
				{
					myLight.shadows = shadowStateMaster;
					myLight.shadowStrength = shadowStrengthMaster;
				}
			}
			float num7 = num2 / num6;
			float num8 = 1f - num7 * num7;
			myLight.intensity = lightIntensity * num8 * (flag ? lightState.Intensity : 1f);
			myLight.range = lightRange * 0.5f + lightRange * 0.5f * num8;
			SetEmissiveColor(flag ? lightState.Emissive : 1f);
			if (num8 < 0.9f && myLight.enabled)
			{
				priority = 1f;
			}
			if (flag && lightState.AudioFrequency > 0f && !string.IsNullOrEmpty(cyclingAudioClipPath))
			{
				float num9 = cyclingAudioPlayEveryNCycles * (1f / lightState.AudioFrequency);
				float num10 = (Time.time + cyclingAudioOffset) % num9;
				if (num10 < previousCycleTime)
				{
					cyclingAudioHandle = Manager.Play(selfT.position + Origin.position, cyclingAudioClipPath, -1, wantHandle: true);
				}
				previousCycleTime = num10;
			}
			if (bed != null)
			{
				Block block = bed.blockValue.Block;
				if (block.RadiusEffects != null)
				{
					GameManager.Instance.World.GetPrimaryPlayer().BlockRadiusEffectsApply(block, selfT.position + Origin.position);
				}
			}
		}
		else
		{
			SetEmissiveColor(_on: false);
		}
		myLight.enabled = flag2;
		if ((bool)LitRootObject)
		{
			LitRootObject.SetActive(flag2);
		}
		if ((bool)audioPlayer)
		{
			audioPlayer.enabled = flag2;
		}
		if ((bool)lensFlare)
		{
			if (flag2 && num < 10f * num4)
			{
				float num11 = (1f - num / (num4 * 10f)) * lightIntensity * 0.33f * FlareBrightnessFactor;
				if (num11 > 1f)
				{
					num11 = 1f;
				}
				if (lightRange < 4f)
				{
					num11 *= lightRange * 0.25f;
				}
				lensFlare.brightness = num11;
				lensFlare.color = myLight.color;
				lensFlare.enabled = true;
			}
			else
			{
				lensFlare.enabled = false;
			}
		}
		base.enabled = bSwitchedOn;
	}

	public void SetRenderingOn()
	{
		bRenderingOff = false;
	}

	public void SetRenderingOff()
	{
		bRenderingOff = true;
	}
}
