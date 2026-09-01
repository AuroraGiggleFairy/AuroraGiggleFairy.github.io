using System.Collections.Generic;
using UnityEngine;
using XMLData.Item;

namespace FullautoLauncher.Scripts.ProjectileManager;

public abstract class ProjectileItemGroupAbs<T> : IProjectileItemGroup where T : ParameterHolderAbs
{
	protected readonly Queue<T> queue_pool_projectile = new Queue<T>();

	protected readonly Queue<Transform> queue_pool_sticky = new Queue<Transform>();

	protected readonly Dictionary<int, HashSet<T>> dict_fired_projectiles = new Dictionary<int, HashSet<T>>();

	protected readonly ItemClass item;

	protected readonly int maxPoolCount = 1000;

	protected readonly int maxStickyCount = 500;

	private int nextID;

	private List<T> list_remove = new List<T>();

	private int NextID => nextID++;

	public ProjectileItemGroupAbs(ItemClass item)
	{
		this.item = item;
	}

	protected abstract T Create(ProjectileParams par);

	private Transform CreateStickyTransform()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		Transform val = item.CloneModel(GameManager.Instance.World, new ItemValue(((ItemData)item).Id, false), Vector3.zero, CustomProjectileManager.CustomProjectileParent, (MeshPurpose)0, default(TextureFullArray));
		Utils.SetLayerRecursively(((Component)val).gameObject, 13);
		((Component)val).gameObject.AddComponent<ProjectileMoveScript>().SetState((State)2);
		return val;
	}

	public Transform GetStickyTransform()
	{
		if (queue_pool_sticky.Count == 0)
		{
			return CreateStickyTransform();
		}
		return queue_pool_sticky.Dequeue();
	}

	public void PoolStickyTransform(Transform sticky)
	{
		if (!((Object)(object)sticky == (Object)null) && ((Component)sticky).gameObject.activeSelf)
		{
			((Component)sticky).gameObject.SetActive(false);
			sticky.parent = CustomProjectileManager.CustomProjectileParent;
			queue_pool_sticky.Enqueue(sticky);
		}
	}

	public virtual void Cleanup()
	{
		foreach (Transform item in queue_pool_sticky)
		{
			if ((Object)(object)item != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)item).gameObject);
			}
		}
		queue_pool_sticky.Clear();
	}

	public void Pool(int count)
	{
		int num = Mathf.Min(maxPoolCount - queue_pool_projectile.Count, count - queue_pool_projectile.Count);
		for (int i = 0; i < num; i++)
		{
			queue_pool_projectile.Enqueue(Create(new ProjectileParams(NextID)));
		}
		if (item.IsSticky)
		{
			int num2 = Mathf.Min(maxStickyCount - queue_pool_sticky.Count, count - queue_pool_sticky.Count);
			for (int j = 0; j < num2; j++)
			{
				Transform val = CreateStickyTransform();
				queue_pool_sticky.Enqueue(val);
			}
		}
	}

	public virtual void Pool(T par)
	{
		if (maxPoolCount > queue_pool_projectile.Count)
		{
			par.Params.bOnIdealPosition = false;
			queue_pool_projectile.Enqueue(par);
		}
	}

	public ProjectileParams Fire(int entityID, ProjectileParams.ItemInfo info, Vector3 _idealStartPosition, Vector3 _realStartPosition, Vector3 _flyDirection, Entity _firingEntity, int _hmOverride = 0, float _radius = 0f)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		T val = ((queue_pool_projectile.Count == 0) ? Create(new ProjectileParams(NextID)) : queue_pool_projectile.Dequeue());
		if (!dict_fired_projectiles.TryGetValue(entityID, out var value))
		{
			value = new HashSet<T>();
			dict_fired_projectiles.Add(entityID, value);
		}
		value.Add(val);
		val.Params.Fire(info, _idealStartPosition, _realStartPosition, _flyDirection, _firingEntity, _hmOverride, _radius);
		val.Fire();
		return val.Params;
	}

	public abstract void Update();

	public void FixedUpdate()
	{
		if ((Object)(object)GameManager.Instance == (Object)null || GameManager.Instance.IsPaused() || GameManager.Instance.World == null)
		{
			return;
		}
		foreach (KeyValuePair<int, HashSet<T>> dict_fired_projectile in dict_fired_projectiles)
		{
			Entity entity = ((WorldBase)GameManager.Instance.World).GetEntity(dict_fired_projectile.Key);
			EntityAlive val = (EntityAlive)(object)((entity is EntityAlive) ? entity : null);
			list_remove.Clear();
			foreach (T item in dict_fired_projectile.Value)
			{
				if (item.Params.UpdatePosition())
				{
					list_remove.Add(item);
				}
				else
				{
					item.UpdatePosition();
				}
			}
			if ((Object)(object)val != (Object)null)
			{
				int num = 0;
				if ((Object)(object)((Entity)val).emodel != (Object)null)
				{
					num = val.GetModelLayer();
					val.SetModelLayer(2, false, (string[])null);
				}
				foreach (T item2 in dict_fired_projectile.Value)
				{
					if (item2.Params.CheckCollision(val))
					{
						list_remove.Add(item2);
					}
				}
				if ((Object)(object)((Entity)val).emodel != (Object)null)
				{
					val.SetModelLayer(num, false, (string[])null);
				}
			}
			foreach (T item3 in list_remove)
			{
				dict_fired_projectile.Value.Remove(item3);
				Pool(item3);
			}
			list_remove.Clear();
		}
	}
}
