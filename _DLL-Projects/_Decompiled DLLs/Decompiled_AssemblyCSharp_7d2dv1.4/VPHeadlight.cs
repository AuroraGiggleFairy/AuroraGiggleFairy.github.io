using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class VPHeadlight : VehiclePart
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRange = 50f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cModBrightPer = 0.58f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform headlightT;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Light> lights;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material headlightMat;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color headLightEmissive;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform modT;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material modMat;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color modMatEmissive;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform modOnT;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Light> modLights;

	[PublicizedFrom(EAccessModifier.Private)]
	public float bright;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color tailLightEmissive;

	[PublicizedFrom(EAccessModifier.Private)]
	public float tailLightIntensity = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOn;

	[PublicizedFrom(EAccessModifier.Private)]
	public float curIntensity;

	public override void InitPrefabConnections()
	{
		headlightT = GetTransform();
		if ((bool)headlightT)
		{
			GameObject gameObject = headlightT.gameObject;
			lights = new List<Light>();
			gameObject.GetComponentsInChildren(includeInactive: true, lights);
			for (int num = lights.Count - 1; num >= 0; num--)
			{
				if (lights[num].type != LightType.Spot)
				{
					lights.RemoveAt(num);
				}
			}
		}
		Transform transform = GetTransform("matT");
		if ((bool)transform)
		{
			MeshRenderer componentInChildren = transform.GetComponentInChildren<MeshRenderer>();
			if ((bool)componentInChildren)
			{
				headlightMat = componentInChildren.material;
			}
		}
		modT = GetTransform("modT");
		if ((bool)modT)
		{
			List<MeshRenderer> list = new List<MeshRenderer>();
			modT.GetComponentsInChildren(list);
			for (int i = 0; i < list.Count; i++)
			{
				MeshRenderer meshRenderer = list[i];
				if (meshRenderer.gameObject.CompareTag("LOD"))
				{
					if (!modMat)
					{
						modMat = meshRenderer.material;
					}
					else
					{
						meshRenderer.material = modMat;
					}
				}
			}
		}
		modOnT = GetTransform("modOnT");
		if ((bool)modOnT)
		{
			GameObject gameObject2 = modOnT.gameObject;
			modLights = new List<Light>();
			gameObject2.GetComponentsInChildren(includeInactive: true, modLights);
			for (int num2 = modLights.Count - 1; num2 >= 0; num2--)
			{
				if (modLights[num2].type != LightType.Spot)
				{
					modLights.RemoveAt(num2);
				}
			}
		}
		curIntensity = -1f;
	}

	public override void SetProperties(DynamicProperties _properties)
	{
		base.SetProperties(_properties);
		StringParsers.TryParseFloat(GetProperty("bright"), out bright);
		properties.ParseColorHex("matEmissive", ref headLightEmissive);
		properties.ParseColorHex("modMatEmissive", ref modMatEmissive);
		properties.ParseColorHex("tailEmissive", ref tailLightEmissive);
	}

	public override void SetMods()
	{
		base.SetMods();
		UpdateOn();
	}

	public override void HandleEvent(Event _event, VehiclePart _part, float _arg)
	{
		if (_event == Event.LightsOn)
		{
			SetOn(_arg != 0f);
			PlaySound();
		}
	}

	public override void Update(float _dt)
	{
		if (IsBroken())
		{
			SetOn(_isOn: false);
			return;
		}
		if (lights != null)
		{
			float v = (vehicle.EffectLightIntensity - 1f) * 0.5f + 1f;
			v = Utils.FastClamp(v, 0f, 10f);
			if (v != curIntensity)
			{
				curIntensity = v;
				float num = bright * v;
				float range = 50f * v;
				if (modInstalled && modLights != null)
				{
					num *= 0.58f;
					for (int num2 = modLights.Count - 1; num2 >= 0; num2--)
					{
						Light light = modLights[num2];
						light.intensity = num;
						light.range = range;
					}
				}
				for (int num3 = lights.Count - 1; num3 >= 0; num3--)
				{
					Light light2 = lights[num3];
					light2.intensity = num;
					light2.range = range;
				}
			}
		}
		SetTailLights();
	}

	public void SetTailLights()
	{
		float num = 0f;
		if (isOn)
		{
			num = 0.5f;
		}
		if (vehicle.CurrentIsBreak)
		{
			num = 1f;
		}
		if (num != tailLightIntensity)
		{
			tailLightIntensity = num;
			if ((bool)vehicle.mainEmissiveMat && tailLightEmissive.a > 0f)
			{
				Color value = tailLightEmissive;
				value.r *= num;
				value.g *= num;
				value.b *= num;
				vehicle.mainEmissiveMat.SetColor("_EmissionColor", value);
			}
		}
	}

	public bool IsOn()
	{
		return isOn;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetOn(bool _isOn)
	{
		if (_isOn != isOn)
		{
			isOn = _isOn;
			UpdateOn();
			SetTailLights();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateOn()
	{
		curIntensity = -1f;
		if ((bool)headlightT)
		{
			headlightT.gameObject.SetActive(isOn);
		}
		if ((bool)headlightMat)
		{
			Color value = (isOn ? headLightEmissive : Color.black);
			headlightMat.SetColor("_EmissionColor", value);
		}
		if (modInstalled)
		{
			if ((bool)modOnT)
			{
				modOnT.gameObject.SetActive(isOn);
			}
			if ((bool)modMat)
			{
				Color value2 = (isOn ? modMatEmissive : Color.black);
				modMat.SetColor("_EmissionColor", value2);
			}
		}
	}

	public float GetLightLevel()
	{
		if (!isOn)
		{
			return 0f;
		}
		return bright * 3f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlaySound()
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if ((bool)primaryPlayer && primaryPlayer.IsSpawned() && vehicle.entity != null && !vehicle.entity.isEntityRemote)
		{
			vehicle.entity.PlayOneShot("UseActions/flashlight_toggle");
		}
	}
}
