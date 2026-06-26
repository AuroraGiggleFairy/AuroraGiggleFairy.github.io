using UnityEngine;

public class CollisionParticleController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasHit;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string particleEffectName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string soundName;

	[PublicizedFrom(EAccessModifier.Private)]
	public int layerMask;

	public void Init(int _entityId, string _colliderSurfaceCategory, string _collisionSurfaceCategory, int _layerMask)
	{
		entityId = _entityId;
		particleEffectName = $"impact_{_colliderSurfaceCategory}_on_{_collisionSurfaceCategory}";
		soundName = $"{_colliderSurfaceCategory}hit{_collisionSurfaceCategory}";
		layerMask = _layerMask;
		Reset();
	}

	public void CheckCollision(Vector3 worldPos, Vector3 direction, float distance, int originEntityId = -1)
	{
		if (!hasHit && Physics.Raycast(new Ray(worldPos - Origin.position, direction), out var hitInfo, distance, layerMask))
		{
			Vector3 vector = hitInfo.point + Origin.position;
			float lightBrightness = GameManager.Instance.World.GetLightBrightness(World.worldToBlockPos(vector));
			GameManager.Instance.SpawnParticleEffectServer(new ParticleEffect(particleEffectName, vector, Quaternion.FromToRotation(Vector3.up, hitInfo.normal), lightBrightness, Color.white, soundName, null), (originEntityId == -1) ? entityId : originEntityId);
			hasHit = true;
		}
	}

	public void Reset()
	{
		hasHit = false;
	}
}
