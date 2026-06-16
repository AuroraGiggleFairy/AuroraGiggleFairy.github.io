using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EModelDrone : EModelBase
{
	public override void Init(World _world, Entity _entity, EModelInstanceAssets _assets)
	{
		entity = _entity;
		assets = _assets;
		EntityClass ec = EntityClass.list[entity.entityClass];
		modelTransformParent = EModelBase.FindModel(base.transform);
		createModel(_world, ec);
		if (GameManager.IsDedicatedServer && !entity.RootMotion)
		{
			avatarController = base.transform.gameObject.AddComponent<AvatarControllerDummy>();
			Animator[] componentsInChildren = base.transform.GetComponentsInChildren<Animator>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
		else
		{
			createAvatarController(ec);
			if (GameManager.IsDedicatedServer && avatarController != null && entity.RootMotion)
			{
				avatarController.SetVisible(_b: true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createAvatarController(EntityClass _ec)
	{
		avatarController = base.gameObject.AddComponent<AvatarControllerDummy>();
	}
}
