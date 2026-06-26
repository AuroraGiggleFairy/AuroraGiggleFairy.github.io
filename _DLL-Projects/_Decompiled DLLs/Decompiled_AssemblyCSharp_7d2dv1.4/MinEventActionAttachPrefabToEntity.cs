using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAttachPrefabToEntity : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject goToInstantiate;

	[PublicizedFrom(EAccessModifier.Private)]
	public string prefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public string parent_transform_path;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 local_offset = new Vector3(0f, 0f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 local_rotation = new Vector3(0f, 0f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 local_scale = new Vector3(1f, 1f, 1f);

	public override void Execute(MinEventParams _params)
	{
		if (_params.Self == null)
		{
			return;
		}
		Transform transform = _params.Self.RootTransform;
		if (parent_transform_path != null && parent_transform_path != "")
		{
			transform = GameUtils.FindDeepChildActive(_params.Self.RootTransform, parent_transform_path);
		}
		if (transform == null)
		{
			return;
		}
		string text = string.Format("tempPrefab_" + goToInstantiate.name);
		Transform transform2 = GameUtils.FindDeepChild(transform, text);
		if (transform2 == null)
		{
			GameObject gameObject = Object.Instantiate(goToInstantiate);
			if (!(gameObject == null))
			{
				transform2 = gameObject.transform;
				gameObject.name = text;
				Utils.SetLayerRecursively(gameObject, transform.gameObject.layer);
				transform2.parent = transform;
				transform2.localPosition = local_offset;
				transform2.localRotation = Quaternion.Euler(local_rotation.x, local_rotation.y, local_rotation.z);
				transform2.localScale = local_scale;
			}
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (base.CanExecute(_eventType, _params) && _params.Self != null)
		{
			return goToInstantiate != null;
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
				goToInstantiate = DataLoader.LoadAsset<GameObject>(_attribute.Value);
				return true;
			case "parent_transform":
				parent_transform_path = _attribute.Value;
				return true;
			case "local_offset":
				local_offset = StringParsers.ParseVector3(_attribute.Value);
				return true;
			case "local_rotation":
				local_rotation = StringParsers.ParseVector3(_attribute.Value);
				return true;
			case "local_scale":
				local_scale = StringParsers.ParseVector3(_attribute.Value);
				return true;
			}
		}
		return flag;
	}
}
