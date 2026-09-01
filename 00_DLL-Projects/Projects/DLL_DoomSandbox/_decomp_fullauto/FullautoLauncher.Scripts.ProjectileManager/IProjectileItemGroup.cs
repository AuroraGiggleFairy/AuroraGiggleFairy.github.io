using UnityEngine;

namespace FullautoLauncher.Scripts.ProjectileManager;

public interface IProjectileItemGroup
{
	void Pool(int count);

	ProjectileParams Fire(int entityID, ProjectileParams.ItemInfo info, Vector3 _idealStartPosition, Vector3 _realStartPosition, Vector3 _flyDirection, Entity _firingEntity, int _hmOverride = 0, float _radius = 0f);

	void Update();

	void FixedUpdate();

	void Cleanup();

	Transform GetStickyTransform();

	void PoolStickyTransform(Transform stockTransform);
}
