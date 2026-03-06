using System.Xml.Linq;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAttachParticleEffectToEntity : MinEventActionTargetedBase
{
	public const string cParticlePrefix = "Ptl_";

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject goToInstantiate;

	[PublicizedFrom(EAccessModifier.Private)]
	public string parent_transform_path;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 local_offset;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 local_rotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> usePassedInTransformTag = FastTags<TagGroup.Global>.Parse("usePassedInTransform");

	[PublicizedFrom(EAccessModifier.Private)]
	public bool setShapeMesh;

	[PublicizedFrom(EAccessModifier.Private)]
	public string soundName;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOneshot;

	public override void Execute(MinEventParams _params)
	{
		if (_params.Self == null)
		{
			return;
		}
		Transform transform = null;
		float num = 0f;
		bool flag = setShapeMesh;
		if (_params.Tags.Test_AnySet(usePassedInTransformTag))
		{
			transform = _params.Transform;
		}
		else if (parent_transform_path == null)
		{
			transform = _params.Self.emodel.meshTransform;
		}
		else if (parent_transform_path == "LOD0")
		{
			transform = _params.Self.emodel.meshTransform;
		}
		else if (parent_transform_path == ".item")
		{
			Transform rightHandTransform = _params.Self.emodel.GetRightHandTransform();
			if ((bool)rightHandTransform && rightHandTransform.childCount > 0)
			{
				transform = rightHandTransform.GetChild(0);
			}
		}
		else if (parent_transform_path == ".body")
		{
			if (_params.Self.emodel is EModelSDCS eModelSDCS)
			{
				Transform parent = _params.Self.transform.Find("Graphics/Model");
				if (setShapeMesh && !eModelSDCS.IsFPV)
				{
					transform = GameUtils.FindDeepChildActive(parent, "body");
					if (transform == null)
					{
						transform = GameUtils.FindDeepChildActive(parent, "torso");
					}
					if (transform == null)
					{
						transform = GameUtils.FindDeepChild(parent, "body");
					}
				}
				else
				{
					transform = GameUtils.FindDeepChild(parent, "Spine1");
					flag = false;
				}
			}
			else
			{
				transform = _params.Self.emodel.GetPelvisTransform();
				if ((_params.Self.entityFlags & EntityFlags.Animal) != EntityFlags.None)
				{
					local_rotation.x += 90f;
					num = 1f;
				}
			}
		}
		else if (parent_transform_path == ".head")
		{
			transform = _params.Self.emodel.GetHeadTransform();
		}
		else
		{
			Transform transform2 = GameUtils.FindDeepChild(_params.Self.transform, parent_transform_path);
			if ((bool)transform2)
			{
				Transform parent2 = transform2.parent;
				if (!parent2 || !setShapeMesh || !parent2.gameObject.CompareTag("Item"))
				{
					transform = transform2;
				}
			}
		}
		if (!transform)
		{
			return;
		}
		string text = "Ptl_" + goToInstantiate.name;
		Transform transform3 = transform.Find(text);
		if (!transform3)
		{
			GameObject gameObject = Object.Instantiate(goToInstantiate);
			if (!gameObject)
			{
				return;
			}
			transform3 = gameObject.transform;
			gameObject.name = text;
			Utils.SetLayerRecursively(gameObject, transform.gameObject.layer);
			transform3.SetParent(transform, worldPositionStays: false);
			transform3.SetLocalPositionAndRotation(local_offset, Quaternion.Euler(local_rotation.x, local_rotation.y, local_rotation.z));
			if (num > 0f)
			{
				transform3.localScale = Vector3.one * num;
			}
			if (!isOneshot)
			{
				_params.Self.AddParticle(text, transform3);
			}
			AudioPlayer component = transform3.GetComponent<AudioPlayer>();
			if ((bool)component)
			{
				component.duration = 100000f;
			}
			ParticleSystem[] componentsInChildren = transform3.GetComponentsInChildren<ParticleSystem>();
			if (componentsInChildren != null)
			{
				foreach (ParticleSystem particleSystem in componentsInChildren)
				{
					particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
					ParticleSystem.ShapeModule shape = particleSystem.shape;
					ParticleSystemShapeType shapeType = shape.shapeType;
					if (shapeType == ParticleSystemShapeType.SkinnedMeshRenderer || shapeType == ParticleSystemShapeType.Mesh)
					{
						SkinnedMeshRenderer componentInChildren = transform.GetComponentInChildren<SkinnedMeshRenderer>();
						if ((bool)componentInChildren && flag)
						{
							shape.skinnedMeshRenderer = componentInChildren;
						}
						else
						{
							MeshRenderer componentInChildren2 = transform.GetComponentInChildren<MeshRenderer>();
							if ((bool)componentInChildren2 && flag)
							{
								shape.meshRenderer = componentInChildren2;
								shape.shapeType = ParticleSystemShapeType.MeshRenderer;
							}
							else
							{
								shape.shapeType = ParticleSystemShapeType.Sphere;
								if (flag)
								{
									Log.Warning("AttachParticleEffectToEntity {0}, {1} no renderer!", _params.Self, text);
								}
							}
						}
					}
					if (flag)
					{
						EntityPlayerLocal entityPlayerLocal = _params.Self as EntityPlayerLocal;
						if ((bool)entityPlayerLocal && entityPlayerLocal.bFirstPersonView)
						{
							shape.position += new Vector3(0f, 0f, 0.3f);
						}
					}
					if (!isOneshot)
					{
						ParticleSystem.MainModule main = particleSystem.main;
						main.duration = 900000f;
					}
					particleSystem.Play();
				}
			}
		}
		if (soundName != null)
		{
			EntityPlayerLocal entityPlayerLocal2 = _params.Self as EntityPlayerLocal;
			if ((bool)entityPlayerLocal2)
			{
				Manager.PlayInsidePlayerHead(soundName, entityPlayerLocal2.entityId, 0f, false, false);
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
			case "particle":
				goToInstantiate = LoadManager.LoadAssetFromAddressables<GameObject>("ParticleEffects/" + _attribute.Value + ".prefab", null, null, false, true).Asset;
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
			case "oneshot":
				isOneshot = true;
				return true;
			case "shape_mesh":
				setShapeMesh = StringParsers.ParseBool(_attribute.Value);
				return true;
			case "sound":
				soundName = _attribute.Value;
				return true;
			}
		}
		return flag;
	}
}
