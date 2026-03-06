using System.Collections.Generic;
using UnityEngine;

public abstract class AIDirectorHordeComponent : AIDirectorComponent
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPitstopSideMin = 40f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPitstopSideRange = 20f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPlayerClosestDist = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSinglePlayerSkipPer = 0.3f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public uint FindTargets(out Vector3 startPos, out Vector3 pitStop, out Vector3 endPos, List<AIDirectorPlayerState> outTargets)
	{
		startPos = Vector3.zero;
		pitStop = Vector3.zero;
		endPos = Vector3.zero;
		List<AIDirectorPlayerState> list = Director.GetComponent<AIDirectorPlayerManagementComponent>().trackedPlayers.list;
		int num = base.Random.RandomRange(0, list.Count);
		AIDirectorPlayerState aIDirectorPlayerState = list[num];
		for (int i = 1; i < list.Count; i++)
		{
			if (!aIDirectorPlayerState.Dead)
			{
				break;
			}
			num = (num + i) % list.Count;
			aIDirectorPlayerState = list[num];
		}
		if (aIDirectorPlayerState.Dead)
		{
			return 1u;
		}
		outTargets.Add(aIDirectorPlayerState);
		int num2 = 1;
		Vector3 pos = aIDirectorPlayerState.Player.position;
		for (int j = 0; j < list.Count; j++)
		{
			AIDirectorPlayerState aIDirectorPlayerState2 = list[j];
			if (aIDirectorPlayerState2 != aIDirectorPlayerState)
			{
				Vector3 vector = aIDirectorPlayerState2.Player.position - aIDirectorPlayerState.Player.position;
				vector.y = 0f;
				if (vector.sqrMagnitude <= 900f)
				{
					pos += aIDirectorPlayerState2.Player.position;
					num2++;
					outTargets.Add(aIDirectorPlayerState2);
				}
			}
		}
		if (num2 == 1 && base.Random.RandomFloat < 0.3f)
		{
			return 12u;
		}
		pos /= (float)num2;
		pos.y += 10f;
		if (!FindOnGroundPos(ref pos))
		{
			AIDirector.LogAI("FindWanderingTargets !playerPos");
			return 1u;
		}
		Vector2 randomOnUnitCircle = base.Random.RandomOnUnitCircle;
		World world = Director.World;
		float num3 = 92f;
		int num4 = 8;
		Vector3i vector3i = default(Vector3i);
		while (num3 > 0f)
		{
			Vector2 vector2 = randomOnUnitCircle * num3;
			startPos.x = pos.x + vector2.x;
			startPos.y = pos.y;
			startPos.z = pos.z + vector2.y;
			vector3i.x = Utils.Fastfloor(startPos.x);
			vector3i.z = Utils.Fastfloor(startPos.z);
			Chunk chunk = (Chunk)world.GetChunkFromWorldPos(vector3i.x, vector3i.z);
			if (chunk != null && world.GetChunkFromWorldPos(vector3i.x - 16, vector3i.z - 16) != null && world.GetChunkFromWorldPos(vector3i.x + 16, vector3i.z + 16) != null)
			{
				if (num4 > 0)
				{
					bool flag = false;
					for (int k = 0; k < list.Count; k++)
					{
						AIDirectorPlayerState aIDirectorPlayerState3 = list[k];
						if (!aIDirectorPlayerState3.Dead)
						{
							Vector3 vector3 = aIDirectorPlayerState3.Player.position - startPos;
							vector3.y = 0f;
							if (vector3.sqrMagnitude < 900f)
							{
								flag = true;
								break;
							}
						}
					}
					if (flag)
					{
						num3 = 92f;
						randomOnUnitCircle = base.Random.RandomOnUnitCircle;
						num4--;
						continue;
					}
				}
				if (!FindOnGroundPos(ref startPos))
				{
					AIDirector.LogAI("FindWanderingTargets !start");
					return 1u;
				}
				vector3i = World.worldToBlockPos(startPos);
				bool checkWater = num4 >= 3;
				if (chunk.CanMobsSpawnAtPos(World.toBlockXZ(vector3i.x), vector3i.y, World.toBlockXZ(vector3i.z), _ignoreCanMobsSpawnOn: false, checkWater))
				{
					break;
				}
				if (num4 <= 0)
				{
					AIDirector.LogAI("FindWanderingTargets !CanMobsSpawnAtPos");
					return 1u;
				}
				num3 = 92f;
				randomOnUnitCircle = base.Random.RandomOnUnitCircle;
				num4--;
			}
			else
			{
				num3 -= 16f;
			}
		}
		if (num3 < 50f)
		{
			AIDirector.LogAI("FindWanderingTargets start too close {0}", num3);
			return 1u;
		}
		Vector2 vector4 = Vector2.Perpendicular(randomOnUnitCircle);
		if (base.Random.RandomFloat < 0.5f)
		{
			vector4 *= -1f;
		}
		vector4 *= 40f + base.Random.RandomFloat * 20f;
		pitStop.x += pos.x + vector4.x;
		pitStop.z += pos.z + vector4.y;
		pitStop.y = startPos.y + 16f;
		if (!FindOnGroundPos(ref pitStop))
		{
			AIDirector.LogAI("FindWanderingTargets !pitStop");
			return 1u;
		}
		endPos.x = (pitStop.x - startPos.x) * 0.5f + pitStop.x;
		endPos.z = (pitStop.z - startPos.z) * 0.5f + pitStop.z;
		endPos.y = pitStop.y + 16f;
		if (!FindOnGroundPos(ref endPos))
		{
			AIDirector.LogAI("FindWanderingTargets !end");
			return 1u;
		}
		AIDirector.LogAIExtra("FindWanderingTargets at player '{0}', dist {1}", aIDirectorPlayerState.Player, vector4.magnitude);
		return 0u;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool FindOnGroundPos(ref Vector3 pos)
	{
		if (Director.World.GetWorldExtent(out var _minSize, out var _maxSize))
		{
			pos.x = Mathf.Clamp(pos.x, _minSize.x, _maxSize.x);
			pos.z = Mathf.Clamp(pos.z, _minSize.z, _maxSize.z);
		}
		int num = Utils.Fastfloor(pos.x);
		int num2 = Utils.Fastfloor(pos.z);
		int v = Utils.Fastfloor(pos.y);
		v = Utils.FastClamp(v, 0, 255);
		Chunk chunk = (Chunk)Director.World.GetChunkFromWorldPos(num, num2);
		if (chunk == null)
		{
			return false;
		}
		int x = World.toBlockXZ(num);
		int z = World.toBlockXZ(num2);
		while (chunk.GetBlockId(x, v, z) == 0)
		{
			if (--v < 0)
			{
				return false;
			}
		}
		v++;
		while (true)
		{
			if (chunk.GetBlockId(x, v, z) != 0)
			{
				if (++v >= 255)
				{
					return false;
				}
				continue;
			}
			if (chunk.GetBlockId(x, v + 1, z) == 0)
			{
				pos.x = num;
				pos.y = v;
				pos.z = num2;
				return true;
			}
			v += 2;
			if (v >= 255)
			{
				break;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool FindScoutStartPos(Vector3 endPos, out Vector3 startPos)
	{
		List<AIDirectorPlayerState> list = Director.GetComponent<AIDirectorPlayerManagementComponent>().trackedPlayers.list;
		startPos = Vector3.zero;
		if (!FindOnGroundPos(ref endPos))
		{
			AIDirector.LogAI("FindScoutStartPos !end");
			return false;
		}
		World world = Director.World;
		_ = base.Random.RandomOnUnitCircle;
		float num = 80f;
		int num2 = 15;
		Vector3i vector3i = default(Vector3i);
		while (true)
		{
			if (--num2 < 0)
			{
				num -= 16f;
				if (num < 40f)
				{
					AIDirector.LogAI("FindScoutStartPos !dist");
					return false;
				}
				num2 = 9;
			}
			Vector2 vector = base.Random.RandomOnUnitCircle * num;
			startPos.x = endPos.x + vector.x;
			startPos.y = endPos.y;
			startPos.z = endPos.z + vector.y;
			vector3i.x = Utils.Fastfloor(startPos.x);
			vector3i.z = Utils.Fastfloor(startPos.z);
			Chunk chunk = (Chunk)world.GetChunkFromWorldPos(vector3i.x, vector3i.z);
			if (chunk == null || world.GetChunkFromWorldPos(vector3i.x - 16, vector3i.z - 16) == null || world.GetChunkFromWorldPos(vector3i.x + 16, vector3i.z + 16) == null)
			{
				continue;
			}
			if (num2 > 0)
			{
				bool flag = false;
				for (int i = 0; i < list.Count; i++)
				{
					AIDirectorPlayerState aIDirectorPlayerState = list[i];
					if (!aIDirectorPlayerState.Dead)
					{
						Vector3 vector2 = aIDirectorPlayerState.Player.position - startPos;
						vector2.y *= 0.1f;
						if (vector2.sqrMagnitude < 900f)
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					continue;
				}
			}
			if (FindOnGroundPos(ref startPos))
			{
				vector3i = World.worldToBlockPos(startPos);
				bool checkWater = num2 >= 5;
				if (chunk.CanMobsSpawnAtPos(World.toBlockXZ(vector3i.x), vector3i.y, World.toBlockXZ(vector3i.z), _ignoreCanMobsSpawnOn: false, checkWater))
				{
					break;
				}
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AIDirectorHordeComponent()
	{
	}
}
