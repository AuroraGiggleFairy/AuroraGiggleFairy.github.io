using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// A doom entity keeps the player in view when other bodies are the only thing
	/// in between. The check walks the open blocks, so a hallway still counts.
	/// A block that blocks sight still hides the player. Distance, facing, and
	/// stealth stay as they are. The shot itself is unchanged.
	/// </summary>
	[HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.CanEntityBeSeen), new System.Type[] { typeof(Entity), typeof(bool) })]
	internal static class Patch_SeePlayerPastBodies
	{
		private static readonly FastTags<TagGroup.Global> DemonTag = FastTags<TagGroup.Global>.Parse("demon");

		private static void Postfix(EntityAlive __instance, Entity _other, bool checkViewCone, ref bool __result)
		{
			if (__result || __instance == null || !(_other is EntityPlayer player) || player.IsDead())
			{
				return;
			}

			EntityClass kind = __instance.entityClass >= 0 ? EntityClass.list[__instance.entityClass] : null;
			if (kind == null || !kind.Tags.Test_AnySet(DemonTag))
			{
				return;
			}

			if (!InLook(__instance, player, checkViewCone) || WorldBlocks(__instance, player))
			{
				return;
			}

			__result = true;
		}

		private static bool InLook(EntityAlive demon, EntityPlayer player, bool checkViewCone)
		{
			Vector3 head = demon.getHeadPosition();
			Vector3 playerHead = player.getHeadPosition();
			float reach = demon.GetSeeDistance() * player.DetectUsScale(demon);
			if ((playerHead - head).sqrMagnitude > reach * reach)
			{
				return false;
			}

			return !checkViewCone || demon.IsInViewCone(playerHead);
		}

		private static bool WorldBlocks(EntityAlive demon, EntityPlayer player)
		{
			World world = demon.world;
			if (world == null)
			{
				return true;
			}

			Vector3 from = demon.position + new Vector3(0f, 0.6f, 0f);
			Vector3 to = player.position + new Vector3(0f, 0.6f, 0f);
			if ((to - from).sqrMagnitude < 0.09f)
			{
				return false;
			}

			int x = Utils.Fastfloor(from.x);
			int y = Utils.Fastfloor(from.y);
			int z = Utils.Fastfloor(from.z);
			int xEnd = Utils.Fastfloor(to.x);
			int yEnd = Utils.Fastfloor(to.y);
			int zEnd = Utils.Fastfloor(to.z);
			int stepX = x < xEnd ? 1 : (x > xEnd ? -1 : 0);
			int stepY = y < yEnd ? 1 : (y > yEnd ? -1 : 0);
			int stepZ = z < zEnd ? 1 : (z > zEnd ? -1 : 0);
			Vector3 dir = to - from;
			float tMaxX = Boundary(from.x, dir.x);
			float tMaxY = Boundary(from.y, dir.y);
			float tMaxZ = Boundary(from.z, dir.z);
			float tDeltaX = stepX == 0 ? float.MaxValue : Mathf.Abs(1f / dir.x);
			float tDeltaY = stepY == 0 ? float.MaxValue : Mathf.Abs(1f / dir.y);
			float tDeltaZ = stepZ == 0 ? float.MaxValue : Mathf.Abs(1f / dir.z);

			for (int i = 0; i < 128; i++)
			{
				if (x == xEnd && y == yEnd && z == zEnd)
				{
					return false;
				}

				if (tMaxX < tMaxY && tMaxX < tMaxZ)
				{
					x += stepX;
					tMaxX += tDeltaX;
				}
				else if (tMaxY < tMaxZ)
				{
					y += stepY;
					tMaxY += tDeltaY;
				}
				else
				{
					z += stepZ;
					tMaxZ += tDeltaZ;
				}

				if (SightBlock(world, x, y, z))
				{
					return true;
				}
			}

			return false;
		}

		private static float Boundary(float origin, float delta)
		{
			if (Mathf.Abs(delta) < 0.0001f)
			{
				return float.MaxValue;
			}

			float edge = delta > 0f ? Mathf.Floor(origin) + 1f : Mathf.Floor(origin);
			return (edge - origin) / delta;
		}

		private static bool SightBlock(World world, int x, int y, int z)
		{
			if (y < 0 || y > 255)
			{
				return true;
			}

			BlockValue block = world.GetBlock(x, y, z);
			return block.Block != null && block.Block.IsCollideSight;
		}
	}
}
