using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class VehiclePart
{
	public enum Event
	{
		Broken,
		LightsOn,
		FuelEmpty,
		FuelRemove
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public DynamicProperties properties;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vehicle vehicle;

	public string tag;

	public bool modInstalled;

	public List<IKController.Target> ikTargets;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Renderer> renderers = new List<Renderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Material> materials = new List<Material>();

	public virtual void InitPrefabConnections()
	{
	}

	public void InitIKTarget(AvatarIKGoal ikGoal, Transform parentT)
	{
		string text = IKController.IKNames[(int)ikGoal];
		string property = GetProperty(text + "Position");
		if (!string.IsNullOrEmpty(property))
		{
			Vector3 vector = StringParsers.ParseVector3(property);
			Vector3 vector2 = StringParsers.ParseVector3(GetProperty(text + "Rotation"));
			IKController.Target item = default(IKController.Target);
			item.avatarGoal = ikGoal;
			item.transform = null;
			if ((bool)parentT)
			{
				Transform transform = (item.transform = new GameObject(text).transform);
				transform.SetParent(parentT, worldPositionStays: false);
				transform.localPosition = vector;
				transform.localEulerAngles = vector2;
				item.position = Vector3.zero;
				item.rotation = Vector3.zero;
			}
			else
			{
				item.position = vector;
				item.rotation = vector2;
			}
			if (ikTargets == null)
			{
				ikTargets = new List<IKController.Target>();
			}
			ikTargets.Add(item);
		}
	}

	public virtual void SetProperties(DynamicProperties _properties)
	{
		if (_properties == null)
		{
			Log.Warning("VehiclePart SetProperties null");
		}
		properties = _properties;
	}

	public string GetProperty(string _key)
	{
		if (properties == null)
		{
			Log.Warning("VehiclePart GetProperty null");
			return string.Empty;
		}
		return properties.GetString(_key);
	}

	public virtual void SetMods()
	{
		modInstalled = false;
		string property = GetProperty("mod");
		if (property.Length <= 0)
		{
			return;
		}
		int bit = FastTags<TagGroup.Global>.GetBit(property);
		modInstalled = vehicle.ModTags.Test_Bit(bit);
		Transform transform = GetTransform("modT");
		if ((bool)transform)
		{
			string property2 = GetProperty("modRot");
			if (property2.Length > 0)
			{
				Vector3 localEulerAngles = Vector3.zero;
				if (modInstalled)
				{
					localEulerAngles = StringParsers.ParseVector3(property2);
				}
				transform.localEulerAngles = localEulerAngles;
			}
			else
			{
				transform.gameObject.SetActive(modInstalled);
			}
		}
		SetTransformActive("modHideT", !modInstalled);
		SetPhysicsTransformActive("modRBT", modInstalled);
	}

	public void SetVehicle(Vehicle _v)
	{
		vehicle = _v;
	}

	public void SetTag(string _tag)
	{
		tag = _tag;
	}

	public Transform GetTransform()
	{
		return GetTransform("transform");
	}

	public Transform GetTransform(string _property)
	{
		Transform meshTransform = vehicle.GetMeshTransform();
		if ((bool)meshTransform)
		{
			string property = GetProperty(_property);
			if (property.Length > 0)
			{
				return meshTransform.Find(property);
			}
		}
		return null;
	}

	public void SetTransformActive(string _property, bool _active)
	{
		Transform meshTransform = vehicle.GetMeshTransform();
		if (!meshTransform)
		{
			return;
		}
		string property = GetProperty(_property);
		if (property.Length > 0)
		{
			meshTransform = meshTransform.Find(property);
			if ((bool)meshTransform)
			{
				meshTransform.gameObject.SetActive(_active);
				return;
			}
			Log.Warning("Vehicle SetTransformActive missing {0}", property);
		}
	}

	public void SetPhysicsTransformActive(string _property, bool _active)
	{
		Transform physicsTransform = vehicle.entity.PhysicsTransform;
		if (!physicsTransform)
		{
			return;
		}
		string property = GetProperty(_property);
		if (property.Length > 0)
		{
			physicsTransform = physicsTransform.Find(property);
			if ((bool)physicsTransform)
			{
				physicsTransform.gameObject.SetActive(_active);
				return;
			}
			Log.Warning("Vehicle SetPhysicsTransformActive missing {0}", property);
		}
	}

	public virtual bool IsBroken()
	{
		return vehicle.GetHealth() <= 0;
	}

	public float GetHealthPercentage()
	{
		return vehicle.GetHealthPercent();
	}

	public bool IsRequired()
	{
		return false;
	}

	public void SetColors(Color _color)
	{
		Transform transform = GetTransform("paint");
		if (!transform)
		{
			return;
		}
		transform.GetComponentsInChildren(includeInactive: true, renderers);
		if (renderers.Count <= 0)
		{
			return;
		}
		Material material = renderers[0].material;
		vehicle.mainEmissiveMat = material;
		material.color = _color;
		for (int i = 1; i < renderers.Count; i++)
		{
			Renderer renderer = renderers[i];
			if (renderer.CompareTag("LOD"))
			{
				renderer.GetSharedMaterials(materials);
				if (materials.Count == 1)
				{
					renderer.material = material;
				}
				materials.Clear();
			}
		}
		renderers.Clear();
	}

	public virtual void Update(float _dt)
	{
	}

	public virtual void HandleEvent(Vehicle.Event _event, float _arg)
	{
	}

	public virtual void HandleEvent(Event _event, VehiclePart _fromPart, float arg)
	{
	}
}
