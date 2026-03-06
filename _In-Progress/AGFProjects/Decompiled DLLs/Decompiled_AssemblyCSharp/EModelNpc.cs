using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EModelNpc : EModelBase
{
	public override void Init(World _world, Entity _entity)
	{
		entity = _entity;
		EntityClass entityClass = EntityClass.list[entity.entityClass];
		ragdollChance = entityClass.RagdollOnDeathChance;
		bHasRagdoll = entityClass.HasRagdoll;
		modelTransformParent = EModelBase.FindModel(base.transform);
		createModel(_world, entityClass);
		setupColliders(entity.transform);
		bool bIsMale = entityClass.bIsMale;
		if (GameManager.IsDedicatedServer && !entity.RootMotion)
		{
			avatarController = base.transform.gameObject.AddComponent<AvatarControllerDummy>();
			Animator[] componentsInChildren = base.transform.GetComponentsInChildren<Animator>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			if (modelTransformParent != null)
			{
				SwitchModelAndView(_bFPV: false, bIsMale);
			}
		}
		else
		{
			createAvatarController(entityClass);
			if (modelTransformParent != null)
			{
				SwitchModelAndView(_bFPV: false, bIsMale);
			}
			if (GameManager.IsDedicatedServer && avatarController != null && entity.RootMotion)
			{
				avatarController.SetVisible(_b: true);
			}
		}
		LookAtInit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupColliders(Transform bodyRoot)
	{
		Transform transform = bodyRoot.FindInChilds("Position");
		if (transform == null)
		{
			transform = bodyRoot.FindInChilds("Origin");
		}
		transform.tag = "E_BP_BipedRoot";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createAvatarController(EntityClass _ec)
	{
		Type type = Type.GetType(_ec.Properties.Values[EntityClass.PropAvatarController]);
		avatarController = base.transform.gameObject.AddComponent(type) as AvatarController;
		(entity as EntityAlive).ReassignEquipmentTransforms();
	}
}
