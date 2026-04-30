using System;
using UnityEngine;

public class AutoTurretController : MonoBehaviour, IPowerSystemCamera
{
	public AutoTurretYawLerp YawController;

	public AutoTurretPitchLerp PitchController;

	public AutoTurretFireController FireController;

	public Transform Laser;

	public Transform Cone;

	public Material ConeMaterial;

	public Color ConeColor;

	public bool IsOn;

	public bool IsUserAccessing;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPoweredRangedTrap tileEntity;

	public bool IsTurning
	{
		get
		{
			if (!YawController.IsTurning)
			{
				return PitchController.IsTurning;
			}
			return true;
		}
	}

	public TileEntityPoweredRangedTrap TileEntity
	{
		get
		{
			return tileEntity;
		}
		set
		{
			tileEntity = value;
			FireController.TileEntity = value;
		}
	}

	public void OnDestroy()
	{
		Cleanup();
		if (ConeMaterial != null)
		{
			UnityEngine.Object.Destroy(ConeMaterial);
		}
	}

	public void Init(DynamicProperties _properties)
	{
		IsOn = false;
		FireController.Cone = Cone;
		FireController.Laser = Laser;
		FireController.Init(_properties, this);
		PitchController.Init(_properties);
		YawController.Init(_properties);
		if (Cone != null)
		{
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
		}
		WireManager.Instance.AddPulseObject(Cone.gameObject);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (FireController.IsOn && !IsOn)
		{
			FireController.OnPoweredOff();
		}
		FireController.IsOn = IsOn;
		if (IsOn)
		{
			YawController.UpdateYaw();
			PitchController.UpdatePitch();
		}
	}

	public void SetConeVisible(bool visible)
	{
		if (Cone != null)
		{
			Cone.gameObject.SetActive(visible);
		}
	}

	public void SetLaserVisible(bool visible)
	{
		if (Laser != null)
		{
			Laser.gameObject.SetActive(visible);
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
		return Cone;
	}

	public void SetUserAccessing(bool userAccessing)
	{
		IsUserAccessing = userAccessing;
	}

	public void Cleanup()
	{
		if (Cone != null && WireManager.HasInstance)
		{
			WireManager.Instance.RemovePulseObject(Cone.gameObject);
		}
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
		return Laser != null;
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
		if (Laser != null)
		{
			Laser.gameObject.SetActive(_active);
		}
	}

	public bool GetLaserActive()
	{
		if (Laser != null)
		{
			return Laser.gameObject.activeSelf;
		}
		return false;
	}
}
