using System;
using UnityEngine;

public class SpotlightController : MonoBehaviour, IPowerSystemCamera
{
	public AutoTurretYawLerp YawController;

	public AutoTurretPitchLerp PitchController;

	public LightLOD LightScript;

	public Transform Cone;

	public Material ConeMaterial;

	public Color ConeColor;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float degreesPerSecond = 11.25f;

	public bool IsOn;

	public TileEntityPowered TileEntity;

	public bool IsUserAccessing;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool initialized;

	public void Init(DynamicProperties _properties)
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		if (!(Cone != null))
		{
			return;
		}
		MeshRenderer component = Cone.GetComponent<MeshRenderer>();
		if (component != null)
		{
			if (component.material != null)
			{
				ConeMaterial = component.material;
				ConeColor = ConeMaterial.GetColor("_Color");
			}
			else if (component.sharedMaterial != null)
			{
				ConeMaterial = component.sharedMaterial;
				ConeColor = ConeMaterial.GetColor("_Color");
			}
		}
		Cone.gameObject.SetActive(value: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (TileEntity == null)
		{
			return;
		}
		if (!TileEntity.IsPowered || IsUserAccessing)
		{
			if (IsUserAccessing)
			{
				YawController.Yaw = TileEntity.CenteredYaw;
				YawController.UpdateYaw();
				PitchController.Pitch = TileEntity.CenteredPitch;
				PitchController.UpdatePitch();
			}
			else if (!TileEntity.IsPowered)
			{
				if (YawController.Yaw != TileEntity.CenteredYaw)
				{
					YawController.Yaw = TileEntity.CenteredYaw;
					YawController.SetYaw();
				}
				if (PitchController.Pitch != TileEntity.CenteredPitch)
				{
					PitchController.Pitch = TileEntity.CenteredPitch;
					PitchController.SetPitch();
				}
			}
			return;
		}
		if (TileEntity.IsPowered)
		{
			if (YawController.Yaw != TileEntity.CenteredYaw)
			{
				YawController.Yaw = Mathf.Lerp(YawController.Yaw, TileEntity.CenteredYaw, Time.deltaTime * degreesPerSecond);
				YawController.UpdateYaw();
			}
			if (PitchController.Pitch != TileEntity.CenteredPitch)
			{
				PitchController.Pitch = Mathf.Lerp(PitchController.Pitch, TileEntity.CenteredPitch, Time.deltaTime * degreesPerSecond);
				PitchController.UpdatePitch();
			}
		}
		IsOn &= TileEntity.IsPowered;
		if (LightScript.bSwitchedOn != IsOn)
		{
			UpdateEmissionColor(IsOn);
			LightScript.bSwitchedOn = IsOn;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEmissionColor(bool isPowered)
	{
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
		if (componentsInChildren == null)
		{
			return;
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].material != componentsInChildren[i].sharedMaterial)
			{
				componentsInChildren[i].material = new Material(componentsInChildren[i].sharedMaterial);
			}
			if (isPowered)
			{
				componentsInChildren[i].material.SetColor("_EmissionColor", Color.white);
			}
			else
			{
				componentsInChildren[i].material.SetColor("_EmissionColor", Color.black);
			}
			componentsInChildren[i].sharedMaterial = componentsInChildren[i].material;
		}
	}

	public void SetPitch(float pitch)
	{
		TileEntity.CenteredPitch = pitch;
	}

	public void SetYaw(float yaw)
	{
		TileEntity.CenteredYaw = yaw;
	}

	public float GetPitch()
	{
		return TileEntity.CenteredPitch;
	}

	public float GetYaw()
	{
		return TileEntity.CenteredYaw;
	}

	public Transform GetCameraTransform()
	{
		return null;
	}

	public void SetUserAccessing(bool userAccessing)
	{
		IsUserAccessing = userAccessing;
	}

	public void SetConeColor(Color _color)
	{
		if (ConeMaterial != null)
		{
			ConeMaterial.SetColor("_Color", _color);
		}
	}

	public Color GetOriginalConeColor()
	{
		return ConeColor;
	}

	public void SetConeActive(bool _active)
	{
		if (Cone != null)
		{
			Cone.gameObject.SetActive(_active);
		}
	}

	public bool GetConeActive()
	{
		if (Cone != null)
		{
			return Cone.gameObject.activeSelf;
		}
		return false;
	}

	public bool HasCone()
	{
		return Cone != null;
	}

	public bool HasLaser()
	{
		return false;
	}

	public void SetLaserColor(Color _color)
	{
	}

	public Color GetOriginalLaserColor()
	{
		return Color.black;
	}

	public void SetLaserActive(bool _active)
	{
	}

	public bool GetLaserActive()
	{
		return false;
	}
}
