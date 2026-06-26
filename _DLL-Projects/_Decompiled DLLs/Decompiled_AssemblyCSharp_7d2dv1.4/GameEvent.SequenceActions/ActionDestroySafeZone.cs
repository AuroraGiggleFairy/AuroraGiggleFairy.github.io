using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionDestroySafeZone : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum DestroySafeZoneStates
	{
		FindClaim,
		HandleChunks,
		SetupChanges,
		AddSigns,
		Action,
		ResetChunks,
		ClientResets
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum DestructionTypes
	{
		Cube,
		Sphere,
		Cylinder,
		LandClaimOnly
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public DestroySafeZoneStates currentState;

	[PublicizedFrom(EAccessModifier.Protected)]
	public DestructionTypes DestructionType = DestructionTypes.Sphere;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string destroyTypeNames = "Sphere";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string newName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<DestructionTypes> DestroyTypeList = new List<DestructionTypes>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDestructionType = "destruction_type";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropNewName = "new_name";

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i claimPos = Vector3i.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockChangeInfo> blockChanges = new List<BlockChangeInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Chunk> chunkList = new List<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int claimSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public int halfClaimSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public int distanceSq;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delay = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong chunkHash = new HashSetLong();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3i> signPositions;

	public string ModifiedName
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return GetTextWithElements(newName);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string ParseTextElement(string element)
	{
		if (element == "viewer")
		{
			if (base.Owner.ExtraData.Length <= 12)
			{
				return base.Owner.ExtraData;
			}
			return base.Owner.ExtraData.Insert(12, "\n");
		}
		return element;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
		base.OnInit();
		string[] array = destroyTypeNames.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			DestructionTypes item = (DestructionTypes)Enum.Parse(typeof(DestructionTypes), array[i]);
			DestroyTypeList.Add(item);
		}
	}

	public override ActionCompleteStates OnPerformAction()
	{
		World world = GameManager.Instance.World;
		switch (currentState)
		{
		case DestroySafeZoneStates.FindClaim:
		{
			if (DestroyTypeList.Count == 0)
			{
				return ActionCompleteStates.InCompleteRefund;
			}
			DestructionType = DestroyTypeList.RandomObject();
			if (base.Owner.TargetPosition == Vector3.zero)
			{
				return ActionCompleteStates.InCompleteRefund;
			}
			claimSize = Mathf.Min(GameStats.GetInt(EnumGameStats.LandClaimSize), 41);
			halfClaimSize = (claimSize - 1) / 2;
			distanceSq = halfClaimSize * halfClaimSize;
			claimPos = new Vector3i(base.Owner.TargetPosition);
			BlockValue block = world.GetBlock(claimPos);
			if (!(block.Block is BlockLandClaim) || !BlockLandClaim.IsPrimary(block))
			{
				return ActionCompleteStates.InCompleteRefund;
			}
			world.SetBlockRPC(claimPos, BlockValue.Air);
			currentState = DestroySafeZoneStates.HandleChunks;
			break;
		}
		case DestroySafeZoneStates.HandleChunks:
		{
			chunkList.Clear();
			int num8 = GameStats.GetInt(EnumGameStats.LandClaimSize) - 1;
			int num9 = GameStats.GetInt(EnumGameStats.LandClaimDeadZone) + num8;
			int num10 = num9 / 16 + 1;
			int num11 = num9 / 16 + 1;
			for (int num12 = -num10; num12 <= num10; num12++)
			{
				int x4 = claimPos.x + num12 * 16;
				for (int num13 = -num11; num13 <= num11; num13++)
				{
					int z4 = claimPos.z + num13 * 16;
					Chunk chunk5 = (Chunk)world.GetChunkFromWorldPos(new Vector3i(x4, claimPos.y, z4));
					if (chunk5 != null && !chunkList.Contains(chunk5))
					{
						chunkList.Add(chunk5);
						chunk5.StopStabilityCalculation = true;
					}
				}
			}
			List<Entity> entitiesInBounds = world.GetEntitiesInBounds(null, new Bounds(claimPos, Vector3.one * num8), _isAlive: true);
			for (int num14 = 0; num14 < entitiesInBounds.Count; num14++)
			{
				if (entitiesInBounds[num14] is EntityAlive entityAlive)
				{
					entityAlive.Buffs.AddBuff("buffTwitchDontBreakLeg");
				}
			}
			currentState = DestroySafeZoneStates.SetupChanges;
			break;
		}
		case DestroySafeZoneStates.SetupChanges:
		{
			blockChanges.Clear();
			IChunk _chunk = null;
			int num = halfClaimSize;
			switch (DestructionType)
			{
			case DestructionTypes.Cube:
			{
				for (int l = -num; l <= num; l++)
				{
					for (int m = -num; m <= num; m++)
					{
						for (int n = -num; n <= num; n++)
						{
							Vector3i vector3i2 = claimPos + new Vector3i(n, l, m);
							if (!world.GetChunkFromWorldPos(vector3i2, ref _chunk))
							{
								continue;
							}
							int x2 = World.toBlockXZ(vector3i2.x);
							int y2 = World.toBlockY(vector3i2.y);
							int z2 = World.toBlockXZ(vector3i2.z);
							if (!_chunk.IsAir(x2, y2, z2) && _chunk.GetBlock(x2, y2, z2).Block.blockMaterial.CanDestroy)
							{
								Chunk chunk2 = (Chunk)_chunk;
								if (!chunkList.Contains(chunk2))
								{
									chunkList.Add(chunk2);
									chunk2.StopStabilityCalculation = true;
								}
								if (chunk2.GetTileEntity(World.toBlock(vector3i2)).TryGetSelfOrFeature<ITileEntityLootable>(out var _typedTe2))
								{
									_typedTe2.SetEmpty();
									_typedTe2.SetModified();
								}
								blockChanges.Add(new BlockChangeInfo(chunk2.ClrIdx, vector3i2, BlockValue.Air));
							}
						}
					}
				}
				break;
			}
			case DestructionTypes.Sphere:
			{
				for (int num2 = -num; num2 <= num; num2++)
				{
					for (int num3 = -num; num3 <= num; num3++)
					{
						for (int num4 = -num; num4 <= num; num4++)
						{
							Vector3i vector3i3 = claimPos + new Vector3i(num4, num2, num3);
							if (Vector3.SqrMagnitude(claimPos - vector3i3) >= (float)distanceSq || !world.GetChunkFromWorldPos(vector3i3, ref _chunk))
							{
								continue;
							}
							int x3 = World.toBlockXZ(vector3i3.x);
							int y3 = World.toBlockY(vector3i3.y);
							int z3 = World.toBlockXZ(vector3i3.z);
							if (!_chunk.IsAir(x3, y3, z3) && _chunk.GetBlock(x3, y3, z3).Block.blockMaterial.CanDestroy)
							{
								Chunk chunk3 = (Chunk)_chunk;
								if (!chunkList.Contains(chunk3))
								{
									chunkList.Add(chunk3);
									chunk3.StopStabilityCalculation = true;
								}
								if (chunk3.GetTileEntity(World.toBlock(vector3i3)).TryGetSelfOrFeature<ITileEntityLootable>(out var _typedTe3))
								{
									_typedTe3.SetEmpty();
									_typedTe3.SetModified();
								}
								blockChanges.Add(new BlockChangeInfo(chunk3.ClrIdx, vector3i3, BlockValue.Air));
							}
						}
					}
				}
				break;
			}
			case DestructionTypes.Cylinder:
			{
				for (int i = -num; i <= num; i++)
				{
					for (int j = -num; j <= num; j++)
					{
						for (int k = -num; k <= num; k++)
						{
							Vector3i vector3i = claimPos + new Vector3i(k, i, j);
							if (Vector3.SqrMagnitude(new Vector3i(claimPos.x, vector3i.y, claimPos.z) - vector3i) >= (float)distanceSq || !world.GetChunkFromWorldPos(vector3i, ref _chunk))
							{
								continue;
							}
							int x = World.toBlockXZ(vector3i.x);
							int y = World.toBlockY(vector3i.y);
							int z = World.toBlockXZ(vector3i.z);
							if (!_chunk.IsAir(x, y, z) && _chunk.GetBlock(x, y, z).Block.blockMaterial.CanDestroy)
							{
								Chunk chunk = (Chunk)_chunk;
								if (!chunkList.Contains(chunk))
								{
									chunkList.Add(chunk);
									chunk.StopStabilityCalculation = true;
								}
								if (chunk.GetTileEntity(World.toBlock(vector3i)).TryGetSelfOrFeature<ITileEntityLootable>(out var _typedTe))
								{
									_typedTe.SetEmpty();
									_typedTe.SetModified();
								}
								blockChanges.Add(new BlockChangeInfo(chunk.ClrIdx, vector3i, BlockValue.Air));
							}
						}
					}
				}
				break;
			}
			case DestructionTypes.LandClaimOnly:
				blockChanges.Add(new BlockChangeInfo(0, claimPos, BlockValue.Air));
				break;
			}
			if (DestructionType == DestructionTypes.LandClaimOnly)
			{
				currentState = DestroySafeZoneStates.Action;
			}
			else
			{
				currentState = DestroySafeZoneStates.AddSigns;
			}
			break;
		}
		case DestroySafeZoneStates.AddSigns:
		{
			int num7 = halfClaimSize + 1;
			BlockValue blockValue = Block.GetBlockValue("playerSignWood1x3");
			if (signPositions == null)
			{
				signPositions = new List<Vector3i>();
			}
			Vector3i vector3i4 = claimPos + new Vector3i(0, 0, num7);
			vector3i4.y = world.GetHeight(vector3i4.x, vector3i4.z) + 1;
			blockValue.rotation = 0;
			signPositions.Add(vector3i4);
			blockChanges.Add(new BlockChangeInfo(0, vector3i4, blockValue));
			vector3i4 = claimPos + new Vector3i(0, 0, -num7);
			vector3i4.y = world.GetHeight(vector3i4.x, vector3i4.z) + 1;
			blockValue.rotation = 2;
			signPositions.Add(vector3i4);
			blockChanges.Add(new BlockChangeInfo(0, vector3i4, blockValue));
			vector3i4 = claimPos + new Vector3i(num7, 0, 0);
			vector3i4.y = world.GetHeight(vector3i4.x, vector3i4.z) + 1;
			blockValue.rotation = 1;
			signPositions.Add(vector3i4);
			blockChanges.Add(new BlockChangeInfo(0, vector3i4, blockValue));
			vector3i4 = claimPos + new Vector3i(-num7, 0, 0);
			vector3i4.y = world.GetHeight(vector3i4.x, vector3i4.z) + 1;
			blockValue.rotation = 3;
			signPositions.Add(vector3i4);
			blockChanges.Add(new BlockChangeInfo(0, vector3i4, blockValue));
			currentState = DestroySafeZoneStates.Action;
			break;
		}
		case DestroySafeZoneStates.Action:
			GameManager.Instance.ChangeBlocks(null, blockChanges);
			currentState = DestroySafeZoneStates.ResetChunks;
			break;
		case DestroySafeZoneStates.ResetChunks:
		{
			for (int num5 = 0; num5 < chunkList.Count; num5++)
			{
				Chunk chunk4 = chunkList[num5];
				long item = WorldChunkCache.MakeChunkKey(chunk4.X, chunk4.Z);
				chunk4.StopStabilityCalculation = false;
				chunkHash.Add(item);
			}
			if (signPositions != null && base.Owner.Target != null)
			{
				PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.Owner.Target.entityId);
				for (int num6 = 0; num6 < signPositions.Count; num6++)
				{
					if (world.GetTileEntity(0, signPositions[num6]) is TileEntitySign tileEntitySign)
					{
						tileEntitySign.SetText(ModifiedName, _syncData: true, playerDataFromEntityID?.PrimaryId);
					}
				}
			}
			world.m_ChunkManager.ResendChunksToClients(chunkHash);
			currentState = DestroySafeZoneStates.ClientResets;
			break;
		}
		case DestroySafeZoneStates.ClientResets:
			if (delay > 0f)
			{
				delay -= Time.deltaTime;
				break;
			}
			world.m_ChunkManager.ResendChunksToClients(chunkHash);
			return ActionCompleteStates.Complete;
		}
		return ActionCompleteStates.InComplete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropDestructionType, ref destroyTypeNames);
		properties.ParseString(PropNewName, ref newName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionDestroySafeZone
		{
			destroyTypeNames = destroyTypeNames,
			DestroyTypeList = DestroyTypeList,
			newName = newName
		};
	}
}
