using System;
using UnityEngine;

public class LightLOD : MonoBehaviour
{
	public delegate void MaxIntensityEvent();

	public GameObject LitRootObject;

	public Transform[] RefIlluminatedMaterials;

	public Transform RefFlare;

	public float MaxDistance = 30f;

	public float DistanceScale = 1f;

	public float FlareBrightnessFactor = 1f;

	public bool bPlayerPlacedLight;

	public bool bSwitchedOn;

	public bool bToggleable = true;

	public bool isHeld;

	public Light otherLight;

	public bool EmissiveFromLightColorOn;

	public Color EmissiveColor = Color.white;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Color EmissiveColorOff = Color.black;

	public float StateRate = 1f;

	public float FluxDelay = 1f;

	public static float DebugViewDistance;

	public MaxIntensityEvent MaxIntensityChanged;

	public LightStateType lightStateStart;

	public bool lightStateEnabled;

	public float priority;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform selfT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform parentT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasInitialized;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos = Vector3i.min;

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
	[PublicizedFrom(EAccessModifier.Private)]
	public float lightRange;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lightViewDistance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float distSqRatio;

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
	public AudioSource audioSource;

	public LightStateType LightStateType
	{
		get
		{
			return lightStateType;
		}
		set
		{
			if (lightStateType == value || GameManager.IsDedicatedServer)
			{
				return;
			}
			if (lightStateType != LightStateType.Static)
			{
				UnityEngine.Object.Destroy(lightState);
			}
			lightStateType = value;
			if (value == LightStateType.Static)
			{
				lightState = null;
				lightStateEnabled = false;
				return;
			}
			Type type = Type.GetType(lightStateType.ToStringCached());
			lightState = (LightState)base.gameObject.GetComponent(type);
			if (!lightState)
			{
				lightState = (LightState)base.gameObject.AddComponent(type);
			}
			lightStateEnabled = true;
			if (!bSwitchedOn)
			{
				lightState.enabled = false;
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
			}
		}
		if (lightStateStart != LightStateType.Static)
		{
			LightStateType = lightStateStart;
		}
		if (!GameManager.IsDedicatedServer)
		{
			audioSource = GetComponent<AudioSource>();
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
		if (blockPos.x == int.MinValue)
		{
			isInitialBlockDone = true;
			if (!isHeld && registeredRange == 0f && (bool)myLight)
			{
				SetRange(lightRange);
			}
			return;
		}
		World world = GameManager.Instance.World;
		BlockValue block = world.GetBlock(blockPos);
		Block block2 = block.Block;
		if (block2.isMultiBlock && block.ischild)
		{
			Vector3i parentPos = block2.multiBlockPos.GetParentPos(blockPos, block);
			block = world.GetBlock(parentPos);
		}
		if (bToggleable)
		{
			SetOnOff((block.meta & 2) != 0);
		}
		if (registeredRange == 0f && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && (bool)myLight)
		{
			SetRange(lightRange);
		}
		isInitialBlockDone = true;
	}

	public void SetEmissiveColor(bool _on)
	{
		if (RefIlluminatedMaterials == null)
		{
			return;
		}
		Light light = GetLight();
		if (!light)
		{
			return;
		}
		Color color = light.color;
		for (int i = 0; i < RefIlluminatedMaterials.Length; i++)
		{
			Transform transform = RefIlluminatedMaterials[i];
			if (!transform)
			{
				continue;
			}
			Renderer component = transform.GetComponent<Renderer>();
			if (!component)
			{
				continue;
			}
			Material[] materials = component.materials;
			if (materials == null)
			{
				continue;
			}
			if (_on)
			{
				Color value = (EmissiveFromLightColorOn ? color : EmissiveColor);
				foreach (Material material in materials)
				{
					if ((bool)material)
					{
						material.SetColor("_EmissionColor", value);
						material.EnableKeyword("_EMISSION");
					}
				}
				continue;
			}
			foreach (Material material2 in materials)
			{
				if ((bool)material2)
				{
					material2.SetColor("_EmissionColor", EmissiveColorOff);
					material2.DisableKeyword("_EMISSION");
				}
			}
		}
	}

	public void SetEmissiveColorCurrent(Color _color)
	{
		if (RefIlluminatedMaterials == null)
		{
			return;
		}
		Light light = GetLight();
		if (!light)
		{
			return;
		}
		Color color = light.color;
		for (int i = 0; i < RefIlluminatedMaterials.Length; i++)
		{
			Transform transform = RefIlluminatedMaterials[i];
			if (!transform)
			{
				continue;
			}
			Renderer component = transform.GetComponent<Renderer>();
			if (!component)
			{
				continue;
			}
			Material[] materials = component.materials;
			if (materials == null)
			{
				continue;
			}
			if (!EmissiveFromLightColorOn)
			{
				_ = EmissiveColor;
			}
			foreach (Material material in materials)
			{
				if ((bool)material)
				{
					material.SetColor("_EmissionColor", _color);
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

	public void SwitchOnOff(bool _isOn, Vector3i _blockPos)
	{
		blockPos = _blockPos;
		if (bToggleable)
		{
			SetOnOff(_isOn);
		}
	}

	public void SetOnOff(bool _isOn)
	{
		Light light = GetLight();
		bSwitchedOn = _isOn;
		if ((bool)light)
		{
			light.enabled = _isOn;
		}
		if ((bool)LitRootObject)
		{
			LitRootObject.SetActive(_isOn);
		}
		if ((bool)lensFlare)
		{
			lensFlare.enabled = _isOn;
		}
		SetEmissiveColor(bSwitchedOn);
		base.enabled = _isOn;
		if (lightState != null)
		{
			lightState.enabled = _isOn;
		}
		if ((bool)audioSource)
		{
			audioSource.enabled = _isOn;
		}
	}

	public void SwitchLightByState(bool _isOn)
	{
		Light light = GetLight();
		if ((bool)light)
		{
			light.enabled = _isOn;
		}
		SetEmissiveColor(_isOn);
		GameLightManager.Instance.MakeLightAPriority(this);
	}

	public void OnDisable()
	{
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

	public void FrameUpdate(Vector3 cameraPos)
	{
		priority = 0f;
		if (bRenderingOff || lightStateEnabled)
		{
			return;
		}
		CheckInitialBlock();
		if (!bSwitchedOn)
		{
			return;
		}
		Light light = myLight;
		if (!light)
		{
			return;
		}
		float num = (selfT.position - cameraPos).sqrMagnitude * DistanceScale;
		float num2 = Mathf.Sqrt(num) - lightRange;
		if (num2 < 0f)
		{
			num2 = 0f;
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
		distSqRatio = num / num4;
		if (bToggleable)
		{
			LightStateCheck();
		}
		float num5 = num3 - lightRange;
		if (num2 < num5)
		{
			priority = 1f;
			if (bPlayerPlacedLight)
			{
				if (distSqRatio >= 0.64000005f)
				{
					light.shadows = LightShadows.None;
				}
				else if (distSqRatio >= 0.0625f)
				{
					if (shadowStateMaster == LightShadows.Soft)
					{
						light.shadows = LightShadows.Hard;
					}
					light.shadowStrength = (1f - Utils.FastClamp01((distSqRatio - 0.36f) / 0.28000003f)) * shadowStrengthMaster;
				}
				else
				{
					light.shadows = shadowStateMaster;
					light.shadowStrength = shadowStrengthMaster;
				}
			}
			float num6 = num2 / num5;
			float num7 = 1f - num6 * num6;
			light.intensity = lightIntensity * num7;
			light.range = lightRange * 0.5f + lightRange * 0.5f * num7;
			light.enabled = true;
		}
		else
		{
			light.enabled = false;
		}
		if (!(lensFlare != null))
		{
			return;
		}
		if (num < 10f * num4)
		{
			float num8 = (1f - num / (num4 * 10f)) * lightIntensity * 0.33f * FlareBrightnessFactor;
			if (num8 > 1f)
			{
				num8 = 1f;
			}
			if (lightRange < 4f)
			{
				num8 *= lightRange * 0.25f;
			}
			lensFlare.brightness = num8;
			lensFlare.color = light.color;
			lensFlare.enabled = true;
		}
		else
		{
			lensFlare.enabled = false;
		}
	}

	public void SetRenderingOn()
	{
		bRenderingOff = false;
	}

	public void SetRenderingOff()
	{
		bRenderingOff = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LightStateCheck()
	{
		if (!(lightState == null) && distSqRatio < lightState.LODThreshold && !(lightState is Blinking) && !lightState.enabled)
		{
			lightState.enabled = true;
		}
	}
}
