using UnityEngine;

public sealed class vp_Layer
{
	public static class Mask
	{
		public const int BulletBlockers = -538750981;

		public const int ExternalBlockers = 1084850176;

		public const int CameraBlockers = 1082195968;

		public const int PhysicsBlockers = 2260992;

		public const int IgnoreWalkThru = -738197525;
	}

	public static readonly vp_Layer instance;

	public const int Default = 0;

	public const int TransparentFX = 1;

	public const int IgnoreRaycast = 2;

	public const int Water = 4;

	public const int Ragdoll = 22;

	public const int PlayerDamageCollider = 23;

	public const int IgnoreBullets = 24;

	public const int Enemy = 25;

	public const int Pickup = 26;

	public const int Trigger = 27;

	public const int MovableObject = 28;

	public const int Debris = 29;

	public const int LocalPlayer = 30;

	public const int Weapon = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	static vp_Layer()
	{
		instance = new vp_Layer();
		Physics.IgnoreLayerCollision(30, 29);
		Physics.IgnoreLayerCollision(29, 29);
		Physics.IgnoreLayerCollision(22, 23);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Layer()
	{
	}

	public static void Set(GameObject obj, int layer, bool recursive = false)
	{
		if (layer < 0 || layer > 31)
		{
			Debug.LogError("vp_Layer: Attempted to set layer id out of range [0-31].");
			return;
		}
		obj.layer = layer;
		if (!recursive)
		{
			return;
		}
		foreach (Transform item in obj.transform)
		{
			Set(item.gameObject, layer, recursive: true);
		}
	}

	public static bool IsInMask(int layer, int layerMask)
	{
		return (layerMask & (1 << layer)) == 0;
	}
}
