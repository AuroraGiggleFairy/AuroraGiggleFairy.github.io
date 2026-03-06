using System.Collections.Generic;
using UnityEngine;

public static class ProjectileManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static int nextId;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DictionaryKeyValueList<int, Transform> projectiles;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<int> keysToRemove;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform blockHitParent;

	[PublicizedFrom(EAccessModifier.Private)]
	public static void init()
	{
		nextId = 0;
		projectiles = new DictionaryKeyValueList<int, Transform>();
		keysToRemove = new List<int>();
		blockHitParent = new GameObject("WorldProjectileContainer").transform;
		Origin.Add(blockHitParent, 0);
	}

	public static void Update()
	{
		if (projectiles == null)
		{
			init();
		}
		for (int i = 0; i < projectiles.keyList.Count; i++)
		{
			int num = projectiles.keyList[i];
			Transform transform = projectiles.dict[num];
			if (transform == null)
			{
				keysToRemove.Add(num);
			}
			else if (transform.GetComponent<ProjectileMoveScript>() != null && !GameManager.Instance.World.IsChunkAreaLoaded(Mathf.CeilToInt(transform.position.x + Origin.position.x), Mathf.CeilToInt(transform.position.y + Origin.position.y), Mathf.CeilToInt(transform.position.z + Origin.position.z)))
			{
				keysToRemove.Add(num);
			}
		}
		for (int j = 0; j < keysToRemove.Count; j++)
		{
			RemoveProjectile(keysToRemove[j]);
		}
		keysToRemove.Clear();
	}

	public static void Cleanup()
	{
		keysToRemove = new List<int>();
		if (projectiles != null)
		{
			for (int i = 0; i < projectiles.keyList.Count; i++)
			{
				keysToRemove.Add(projectiles.keyList[i]);
			}
			for (int j = 0; j < keysToRemove.Count; j++)
			{
				RemoveProjectile(keysToRemove[j]);
			}
		}
	}

	public static int AddProjectileItem(Transform _transform = null, int _pId = -1, Vector3 _position = default(Vector3), Vector3 _movementLastFrame = default(Vector3), int _itemValueType = -1)
	{
		if (projectiles == null)
		{
			init();
		}
		Update();
		if (_pId == -1)
		{
			_pId = nextId;
			nextId++;
		}
		if (_transform == null && _itemValueType != -1)
		{
			_transform = instantiateProjectile(_pId, _itemValueType);
		}
		float num = ItemClass.GetForId(_itemValueType).StickyOffset;
		Renderer renderer = _transform.GetComponent<Renderer>();
		if (renderer == null)
		{
			LODGroup component = _transform.GetComponent<LODGroup>();
			if (component != null)
			{
				LOD[] lODs = component.GetLODs();
				for (int i = 0; i < component.lodCount; i++)
				{
					Renderer[] renderers = lODs[i].renderers;
					if (i == 0)
					{
						renderer = renderers[i];
					}
					for (int j = 0; j < renderers.Length; j++)
					{
						renderers[j].material = DataLoader.LoadAsset<Material>(ItemClass.GetForId(_itemValueType).StickyMaterial);
					}
				}
			}
			else
			{
				renderer = _transform.GetComponentInChildren<Renderer>();
			}
		}
		else if (ItemClass.GetForId(_itemValueType).StickyMaterial != null)
		{
			renderer.material = DataLoader.LoadAsset<Material>(ItemClass.GetForId(_itemValueType).StickyMaterial);
		}
		Mesh mesh = _transform.GetComponent<Mesh>();
		MeshFilter meshFilter = _transform.GetComponent<MeshFilter>();
		if (meshFilter == null)
		{
			meshFilter = _transform.GetComponentInChildren<MeshFilter>();
		}
		if (mesh == null)
		{
			SkinnedMeshRenderer componentInChildren = _transform.GetComponentInChildren<SkinnedMeshRenderer>();
			if (componentInChildren != null)
			{
				mesh = componentInChildren.sharedMesh;
				renderer = componentInChildren;
			}
		}
		if (mesh == null && meshFilter != null)
		{
			mesh = meshFilter.mesh;
		}
		if (renderer != null || (meshFilter != null && meshFilter.mesh != null))
		{
			Bounds bounds = renderer.bounds;
			if (meshFilter != null && meshFilter.mesh != null)
			{
				bounds = meshFilter.mesh.bounds;
			}
			if (mesh != null)
			{
				bounds = mesh.bounds;
			}
			if (!Voxel.voxelRayHitInfo.tag.StartsWith("E_"))
			{
				CapsuleCollider capsuleCollider = _transform.gameObject.AddComponent<CapsuleCollider>();
				capsuleCollider.center = bounds.center;
				float num2 = Mathf.Max(Mathf.Max(bounds.size.x, bounds.size.y), bounds.size.z);
				if (ItemClass.GetForId(_itemValueType).StickyColliderUp == -1)
				{
					if (num2 == bounds.size.x)
					{
						capsuleCollider.direction = 2;
					}
					else if (num2 == bounds.size.y)
					{
						capsuleCollider.direction = 1;
					}
					else if (num2 == bounds.size.z)
					{
						capsuleCollider.direction = 2;
					}
				}
				else
				{
					capsuleCollider.direction = ItemClass.GetForId(_itemValueType).StickyColliderUp;
				}
				capsuleCollider.height = ((ItemClass.GetForId(_itemValueType).StickyColliderLength > 0f) ? ItemClass.GetForId(_itemValueType).StickyColliderLength : num2);
				capsuleCollider.radius = ((ItemClass.GetForId(_itemValueType).StickyColliderRadius > 0f) ? ItemClass.GetForId(_itemValueType).StickyColliderRadius : 0.05f);
				if (num == 0f)
				{
					num = capsuleCollider.height / 2f;
				}
			}
		}
		if (Voxel.voxelRayHitInfo.tag.StartsWith("E_"))
		{
			_transform.parent = Voxel.voxelRayHitInfo.transform;
		}
		else
		{
			_transform.parent = blockHitParent;
		}
		_transform.localScale = Vector3.one * 0.75f;
		ProjectileMoveScript component2;
		ThrownWeaponMoveScript component3;
		if ((component2 = _transform.GetComponent<ProjectileMoveScript>()) != null)
		{
			component2.ProjectileID = _pId;
			_transform.name = $"temp_Projectile[item:{component2.itemProjectile.GetLocalizedItemName()},owner:{GameManager.Instance.World.GetEntity(component2.ProjectileOwnerID).name},projectileId:{component2.ProjectileID.ToString()}";
			component2.FinalPosition = _position - _transform.forward * num;
			_transform.position = component2.FinalPosition - Origin.position;
		}
		else if ((component3 = _transform.GetComponent<ThrownWeaponMoveScript>()) != null)
		{
			component3.ProjectileID = _pId;
			_transform.name = $"temp_Projectile[item:{component3.itemWeapon.GetLocalizedItemName()},owner:{GameManager.Instance.World.GetEntity(component3.ProjectileOwnerID).name},projectileId:{component3.ProjectileID.ToString()}";
			component3.FinalPosition = _position - component3.velocity.normalized * num;
			_transform.position = component3.FinalPosition - Origin.position;
		}
		Transform transform = _transform.Find("Trail");
		if (transform != null)
		{
			transform.gameObject.SetActive(value: false);
		}
		_transform.tag = "Item";
		_transform.gameObject.layer = 13;
		projectiles.Add(_pId, _transform);
		return _pId;
	}

	public static void RemoveProjectile(int _id)
	{
		if (projectiles == null)
		{
			init();
		}
		else if (projectiles.keyList.Contains(_id) && projectiles.dict[_id] != null)
		{
			Object.Destroy(projectiles.dict[_id].gameObject);
			projectiles.Remove(_id);
		}
	}

	public static Transform GetProjectile(int _id)
	{
		Transform result = null;
		if (projectiles.keyList.Contains(_id) && projectiles.dict[_id] != null)
		{
			result = projectiles.dict[_id];
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform instantiateProjectile(int _id, int _itemValueType)
	{
		ItemClass forId = ItemClass.GetForId(_itemValueType);
		if (forId == null)
		{
			return null;
		}
		ItemValue itemValue = new ItemValue(forId.Id);
		Transform transform = forId.CloneModel(GameManager.Instance.World, itemValue, Vector3.zero, null);
		Utils.SetLayerRecursively(transform.gameObject, 13);
		transform.gameObject.SetActive(value: true);
		return transform;
	}
}
