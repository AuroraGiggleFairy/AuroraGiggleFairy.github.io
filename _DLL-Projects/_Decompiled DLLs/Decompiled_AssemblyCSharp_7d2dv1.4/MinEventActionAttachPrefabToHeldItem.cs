using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAttachPrefabToHeldItem : MinEventActionBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject goToInstantiate;

	[PublicizedFrom(EAccessModifier.Private)]
	public string itemPropertyName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string prefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public string parent_transform = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 local_offset = new Vector3(0f, 0f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 local_rotation = new Vector3(0f, 0f, 0f);

	public override void Execute(MinEventParams _params)
	{
		Transform transform = null;
		if (parent_transform != "")
		{
			transform = GameUtils.FindDeepChild(_params.Transform, parent_transform);
		}
		else if (_params.Transform != null)
		{
			transform = _params.Transform;
		}
		else if (_params.Self != null)
		{
			transform = GameUtils.FindDeepChildActive(_params.Self.RootTransform, "InactiveItems");
		}
		if (transform == null)
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
		string text = string.Format("tempMod_" + goToInstantiate.name);
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
			Utils.SetLayerRecursively(gameObject, transform.gameObject.layer, Utils.ExcludeLayerZoom);
			transform2.parent = transform;
			transform2.localPosition = local_offset;
			transform2.localRotation = Quaternion.Euler(local_rotation.x, local_rotation.y, local_rotation.z);
		}
		if (transform2 != null)
		{
			UpdateLightOnAllMaterials updateLightOnAllMaterials = transform2.gameObject.AddMissingComponent<UpdateLightOnAllMaterials>();
			updateLightOnAllMaterials.SetTintColorForItem(Vector3.one);
			if (_params.ItemValue.ItemClass.Properties.Values.ContainsKey(Block.PropTintColor))
			{
				updateLightOnAllMaterials.SetTintColorForItem(Block.StringToVector3(_params.ItemValue.GetPropertyOverride(Block.PropTintColor, _params.ItemValue.ItemClass.Properties.Values[Block.PropTintColor])));
			}
			else
			{
				updateLightOnAllMaterials.SetTintColorForItem(Block.StringToVector3(_params.ItemValue.GetPropertyOverride(Block.PropTintColor, "255,255,255")));
			}
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (base.CanExecute(_eventType, _params) && (_params.Self != null || _params.Transform != null))
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
			case "parent_transform":
				parent_transform = _attribute.Value;
				return true;
			case "local_offset":
				local_offset = StringParsers.ParseVector3(_attribute.Value);
				return true;
			case "local_rotation":
				local_rotation = StringParsers.ParseVector3(_attribute.Value);
				return true;
			}
		}
		return flag;
	}
}
