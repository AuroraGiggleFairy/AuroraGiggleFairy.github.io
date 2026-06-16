using System.Collections.Generic;
using UnityEngine;

public class PhysicsBodyInstance
{
	public EnumColliderMode Mode;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform modelRoot;

	[PublicizedFrom(EAccessModifier.Private)]
	public PhysicsBodyLayout layout;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<IBodyColliderInstance> colliders = new List<IBodyColliderInstance>();

	public PhysicsBodyInstance(Transform _modelRoot, PhysicsBodyLayout _layout, EnumColliderMode _initialMode)
	{
		modelRoot = _modelRoot;
		layout = _layout;
		Mode = _initialMode;
		BindColliders();
		for (int i = 0; i < colliders.Count; i++)
		{
			colliders[i].ColliderMode = _initialMode;
		}
	}

	public void BindColliders()
	{
		colliders.Clear();
		for (int i = 0; i < layout.Colliders.Count; i++)
		{
			PhysicsBodyColliderConfiguration bodyConfig = layout.Colliders[i];
			bindCollider(bodyConfig);
		}
	}

	public void SetColliderMode(EnumColliderType colliderTypes, EnumColliderMode _mode)
	{
		Mode = _mode;
		for (int i = 0; i < colliders.Count; i++)
		{
			IBodyColliderInstance bodyColliderInstance = colliders[i];
			if ((bodyColliderInstance.Config.Type & colliderTypes) != EnumColliderType.None)
			{
				bodyColliderInstance.ColliderMode = _mode;
			}
		}
	}

	public Transform GetTransformForColliderTag(string tag)
	{
		for (int i = 0; i < colliders.Count; i++)
		{
			IBodyColliderInstance bodyColliderInstance = colliders[i];
			if (bodyColliderInstance.Transform != null && bodyColliderInstance.Config.Tag == tag)
			{
				return bodyColliderInstance.Transform;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void bindCollider(PhysicsBodyColliderConfiguration _bodyConfig)
	{
		Transform transform = modelRoot.Find(_bodyConfig.Path);
		if ((bool)transform)
		{
			BoxCollider component;
			CapsuleCollider component2;
			SphereCollider component3;
			if ((component = transform.GetComponent<BoxCollider>()) != null)
			{
				colliders.Add(new PhysicsBodyBoxCollider(component, _bodyConfig));
			}
			else if ((component2 = transform.GetComponent<CapsuleCollider>()) != null)
			{
				colliders.Add(new PhysicsBodyCapsuleCollider(component2, _bodyConfig));
			}
			else if ((component3 = transform.GetComponent<SphereCollider>()) != null)
			{
				colliders.Add(new PhysicsBodySphereCollider(component3, _bodyConfig));
			}
			else
			{
				colliders.Add(new PhysicsBodyNullCollider(transform, _bodyConfig));
			}
			transform.gameObject.AddMissingComponent<RootTransformRefEntity>();
			CharacterJoint component4 = transform.GetComponent<CharacterJoint>();
			if ((bool)component4)
			{
				component4.enablePreprocessing = false;
				component4.enableProjection = true;
			}
		}
		else
		{
			Entity componentInParent = modelRoot.GetComponentInParent<Entity>();
			Log.Warning("PhysicsBodies {0}, {1}, path not found {2}", (componentInParent != null) ? componentInParent.GetDebugName() : modelRoot.name, _bodyConfig.Tag, _bodyConfig.Path);
		}
	}
}
