using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAddPart : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string partName;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject goToInstantiate;

	[PublicizedFrom(EAccessModifier.Private)]
	public string itemPropertyName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string parentTransformPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 localPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 localRot;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool colorTint;

	public override void Execute(MinEventParams _params)
	{
		if (_params.Self == null)
		{
			return;
		}
		string propertyOverride = _params.ItemValue.GetPropertyOverride(itemPropertyName, "");
		if (goToInstantiate == null && propertyOverride == "")
		{
			return;
		}
		if (propertyOverride != "")
		{
			goToInstantiate = DataLoader.LoadAsset<GameObject>(propertyOverride);
		}
		Transform transform = _params.Self.RootTransform;
		if (!string.IsNullOrEmpty(parentTransformPath))
		{
			if (!parentTransformPath.EqualsCaseInsensitive("#HeldItemRoot") || !(_params.Self.emodel != null))
			{
				transform = ((!(_params.Self is EntityPlayerLocal entityPlayerLocal) || !entityPlayerLocal.emodel.IsFPV || !entityPlayerLocal.vp_FPCamera.Locked3rdPerson) ? GameUtils.FindDeepChildActive(_params.Self.RootTransform, parentTransformPath) : GameUtils.FindDeepChildActive(entityPlayerLocal.vp_FPCamera.Transform, parentTransformPath));
			}
			else
			{
				transform = _params.Self.inventory.models[_params.Self.inventory.holdingItemIdx];
				if (transform == null)
				{
					transform = GameUtils.FindDeepChildActive(_params.Self.RootTransform, parentTransformPath);
				}
			}
		}
		if (transform == null)
		{
			return;
		}
		if (int.Parse(_params.ItemValue.GetPropertyOverride(ItemClass.PropMatEmission, "0")) > 0)
		{
			Renderer[] componentsInChildren = transform.GetComponentsInChildren<Renderer>();
			for (int num = componentsInChildren.Length - 1; num >= 0; num--)
			{
				componentsInChildren[num].material.EnableKeyword("_EMISSION");
			}
		}
		string text = string.Format("part!" + goToInstantiate.name);
		Transform transform2 = GameUtils.FindDeepChild(transform, text);
		if (transform2 == null)
		{
			GameObject gameObject = Object.Instantiate(goToInstantiate);
			if (gameObject == null)
			{
				return;
			}
			transform2 = gameObject.transform;
			gameObject.name = text;
			Utils.SetLayerRecursively(gameObject, transform.gameObject.layer);
			transform2.SetParent(transform, worldPositionStays: false);
			transform2.SetLocalPositionAndRotation(localPos, Quaternion.Euler(localRot.x, localRot.y, localRot.z));
		}
		if (colorTint && transform2 != null)
		{
			UpdateLightOnAllMaterials updateLightOnAllMaterials = transform2.gameObject.AddMissingComponent<UpdateLightOnAllMaterials>();
			string text2 = _params.ItemValue.ItemClass.Properties.GetString(Block.PropTintColor);
			if (text2.Length > 0)
			{
				updateLightOnAllMaterials.SetTintColorForItem(Block.StringToVector3(_params.ItemValue.GetPropertyOverride(Block.PropTintColor, text2)));
			}
			else
			{
				updateLightOnAllMaterials.SetTintColorForItem(Block.StringToVector3(_params.ItemValue.GetPropertyOverride(Block.PropTintColor, "255,255,255")));
			}
		}
		_params.Self.AddPart(partName, transform2);
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (base.CanExecute(_eventType, _params) && _params.Self != null)
		{
			if (!(goToInstantiate != null))
			{
				return itemPropertyName != null;
			}
			return true;
		}
		return false;
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			switch (_attribute.Name.LocalName)
			{
			case "part":
				partName = _attribute.Value;
				return true;
			case "prefab":
				itemPropertyName = _attribute.Value;
				if (!itemPropertyName.StartsWith("property?"))
				{
					goToInstantiate = DataLoader.LoadAsset<GameObject>(_attribute.Value);
				}
				else
				{
					itemPropertyName = itemPropertyName.Replace("property?", "");
				}
				return true;
			case "parentTransform":
				parentTransformPath = _attribute.Value;
				return true;
			case "localPos":
				localPos = StringParsers.ParseVector3(_attribute.Value);
				return true;
			case "localRot":
				localRot = StringParsers.ParseVector3(_attribute.Value);
				return true;
			case "colorTint":
				colorTint = StringParsers.ParseBool(_attribute.Value);
				return true;
			}
		}
		return flag;
	}
}
