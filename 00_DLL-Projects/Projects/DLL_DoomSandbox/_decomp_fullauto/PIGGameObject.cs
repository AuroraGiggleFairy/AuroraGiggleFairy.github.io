using System.Collections.Generic;
using FullautoLauncher.Scripts.ProjectileManager;
using UnityEngine;
using XMLData.Item;

public class PIGGameObject : ProjectileItemGroupAbs<PHGameObject>
{
	public PIGGameObject(ItemClass item)
		: base(item)
	{
	}

	public override void Cleanup()
	{
		base.Cleanup();
		foreach (PHGameObject item in queue_pool_projectile)
		{
			item.Dispose();
		}
		queue_pool_projectile.Clear();
		foreach (HashSet<PHGameObject> value in dict_fired_projectiles.Values)
		{
			foreach (PHGameObject item2 in value)
			{
				item2.Dispose();
			}
		}
		dict_fired_projectiles.Clear();
	}

	public override void Pool(PHGameObject par)
	{
		base.Pool(par);
		((Component)par.Transform).gameObject.SetActive(false);
		par.Transform.parent = CustomProjectileManager.CustomProjectileParent;
	}

	public override void Update()
	{
	}

	protected override PHGameObject Create(ProjectileParams par)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		Transform transform = item.CloneModel(GameManager.Instance.World, new ItemValue(((ItemData)item).Id, false), Vector3.zero, CustomProjectileManager.CustomProjectileParent, (MeshPurpose)0, default(TextureFullArray));
		return new PHGameObject(transform, par);
	}
}
