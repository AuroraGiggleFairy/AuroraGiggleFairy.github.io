using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionFillSafeZone : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum FillSafeZoneStates
	{
		FindClaim,
		HandleChunks,
		SetupChanges,
		AddSigns,
		Action,
		ResetChunks
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum FillTypes
	{
		Cube,
		Sphere,
		Cylinder,
		Pyramid
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public FillSafeZoneStates currentState;

	[PublicizedFrom(EAccessModifier.Protected)]
	public FillTypes FillType = FillTypes.Sphere;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string fillTypeNames = "Sphere";

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<FillTypes> FillTypeList = new List<FillTypes>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool destroyClaim;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string newName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string blockTags;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFillType = "fill_type";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBlock = "block";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDestroyClaim = "destroy_claim";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBlockTags = "block_tags";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropNewName = "new_name";

	[PublicizedFrom(EAccessModifier.Private)]
	public string blockName = "terrDirtTwitch";

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
	public HashSet<Vector3i> blockedPositions = new HashSet<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong chunkHash = new HashSetLong();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<EntityPlayer, Vector3> playerDictionary = new Dictionary<EntityPlayer, Vector3>();

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
		string[] array = fillTypeNames.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			FillTypes item = (FillTypes)Enum.Parse(typeof(FillTypes), array[i]);
			FillTypeList.Add(item);
		}
	}

	public override ActionCompleteStates OnPerformAction()
	{
		World world = GameManager.Instance.World;
		switch (currentState)
		{
		case FillSafeZoneStates.FindClaim:
		{
			if (FillTypeList.Count == 0)
			{
				return ActionCompleteStates.InCompleteRefund;
			}
			FillType = FillTypeList.RandomObject();
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
			if (destroyClaim)
			{
				world.SetBlockRPC(claimPos, BlockValue.Air);
			}
			currentState = FillSafeZoneStates.HandleChunks;
			break;
		}
		case FillSafeZoneStates.HandleChunks:
		{
			chunkList.Clear();
			int num13 = GameStats.GetInt(EnumGameStats.LandClaimSize) - 1;
			int num14 = GameStats.GetInt(EnumGameStats.LandClaimDeadZone) + num13;
			int num15 = num14 / 16 + 1;
			int num16 = num14 / 16 + 1;
			for (int num17 = -num15; num17 <= num15; num17++)
			{
				int x5 = claimPos.x + num17 * 16;
				for (int num18 = -num16; num18 <= num16; num18++)
				{
					int z5 = claimPos.z + num18 * 16;
					Chunk chunk6 = (Chunk)world.GetChunkFromWorldPos(new Vector3i(x5, claimPos.y, z5));
					if (chunk6 != null && !chunkList.Contains(chunk6))
					{
						chunkList.Add(chunk6);
					}
				}
			}
			List<Entity> entitiesInBounds = world.GetEntitiesInBounds(null, new Bounds(claimPos, Vector3.one * num13), _isAlive: true);
			for (int num19 = 0; num19 < entitiesInBounds.Count; num19++)
			{
				Entity entity = entitiesInBounds[num19];
				bool num20 = entity is EntityPlayer;
				Vector3i vector3i6 = new Vector3i(Utils.Fastfloor(entity.position.x), Utils.Fastfloor(entity.position.y), Utils.Fastfloor(entity.position.z));
				if (num20)
				{
					EntityPlayer key = entity as EntityPlayer;
					Vector3 value = vector3i6 + new Vector3(0.5f, 0.5f, 0.5f);
					playerDictionary.Add(key, value);
				}
				blockedPositions.Add(vector3i6);
				blockedPositions.Add(vector3i6 + Vector3i.up);
				if (num20)
				{
					blockedPositions.Add(vector3i6 + Vector3i.up * 2);
				}
				vector3i6 += Vector3i.right;
				blockedPositions.Add(vector3i6);
				blockedPositions.Add(vector3i6 + Vector3i.up);
				if (num20)
				{
					blockedPositions.Add(vector3i6 + Vector3i.up * 2);
				}
			}
			currentState = FillSafeZoneStates.SetupChanges;
			break;
		}
		case FillSafeZoneStates.SetupChanges:
		{
			blockChanges.Clear();
			IChunk _chunk = null;
			int num2 = halfClaimSize;
			BlockValue blockValue2 = Block.GetBlockValue(blockName);
			TeleportPlayers();
			FastTags<TagGroup.Global> other = ((blockTags != null) ? FastTags<TagGroup.Global>.Parse(blockTags) : FastTags<TagGroup.Global>.none);
			switch (FillType)
			{
			case FillTypes.Cube:
			{
				for (int num10 = -num2; num10 <= num2; num10++)
				{
					for (int num11 = -num2; num11 <= num2; num11++)
					{
						for (int num12 = -num2; num12 <= num2; num12++)
						{
							Vector3i vector3i5 = claimPos + new Vector3i(num12, num10, num11);
							if (!world.GetChunkFromWorldPos(vector3i5, ref _chunk))
							{
								continue;
							}
							int x4 = World.toBlockXZ(vector3i5.x);
							int y4 = World.toBlockY(vector3i5.y);
							int z4 = World.toBlockXZ(vector3i5.z);
							BlockValue blockNoDamage4 = _chunk.GetBlockNoDamage(x4, y4, z4);
							if ((blockNoDamage4.isair || blockNoDamage4.Block.Tags.Test_AnySet(other)) && !blockedPositions.Contains(vector3i5))
							{
								Chunk chunk5 = (Chunk)_chunk;
								if (!chunkList.Contains(chunk5))
								{
									chunkList.Add(chunk5);
								}
								blockChanges.Add(new BlockChangeInfo(chunk5.ClrIdx, vector3i5, blockValue2));
							}
						}
					}
				}
				break;
			}
			case FillTypes.Sphere:
			{
				for (int n = -num2; n <= num2; n++)
				{
					for (int num5 = -num2; num5 <= num2; num5++)
					{
						for (int num6 = -num2; num6 <= num2; num6++)
						{
							Vector3i vector3i3 = claimPos + new Vector3i(num6, n, num5);
							if (Vector3.SqrMagnitude(claimPos - vector3i3) >= (float)distanceSq || !world.GetChunkFromWorldPos(vector3i3, ref _chunk))
							{
								continue;
							}
							int x2 = World.toBlockXZ(vector3i3.x);
							int y2 = World.toBlockY(vector3i3.y);
							int z2 = World.toBlockXZ(vector3i3.z);
							BlockValue blockNoDamage2 = _chunk.GetBlockNoDamage(x2, y2, z2);
							if ((blockNoDamage2.isair || blockNoDamage2.Block.Tags.Test_AnySet(other)) && !blockedPositions.Contains(vector3i3))
							{
								Chunk chunk3 = (Chunk)_chunk;
								if (!chunkList.Contains(chunk3))
								{
									chunkList.Add(chunk3);
								}
								blockChanges.Add(new BlockChangeInfo(chunk3.ClrIdx, vector3i3, blockValue2));
							}
						}
					}
				}
				break;
			}
			case FillTypes.Cylinder:
			{
				for (int num7 = -num2; num7 <= num2; num7++)
				{
					for (int num8 = -num2; num8 <= num2; num8++)
					{
						for (int num9 = -num2; num9 <= num2; num9++)
						{
							Vector3i vector3i4 = claimPos + new Vector3i(num9, num7, num8);
							if (Vector3.SqrMagnitude(new Vector3i(claimPos.x, vector3i4.y, claimPos.z) - vector3i4) >= (float)distanceSq || !world.GetChunkFromWorldPos(vector3i4, ref _chunk))
							{
								continue;
							}
							int x3 = World.toBlockXZ(vector3i4.x);
							int y3 = World.toBlockY(vector3i4.y);
							int z3 = World.toBlockXZ(vector3i4.z);
							BlockValue blockNoDamage3 = _chunk.GetBlockNoDamage(x3, y3, z3);
							if ((blockNoDamage3.isair || blockNoDamage3.Block.Tags.Test_AnySet(other)) && !blockedPositions.Contains(vector3i4))
							{
								Chunk chunk4 = (Chunk)_chunk;
								if (!chunkList.Contains(chunk4))
								{
									chunkList.Add(chunk4);
								}
								blockChanges.Add(new BlockChangeInfo(chunk4.ClrIdx, vector3i4, blockValue2));
							}
						}
					}
				}
				break;
			}
			case FillTypes.Pyramid:
			{
				int num3 = 0;
				for (int k = -num2; k <= num2; k++)
				{
					int num4 = num2 - num3 / 2;
					for (int l = -num4; l <= num4; l++)
					{
						for (int m = -num4; m <= num4; m++)
						{
							Vector3i vector3i2 = claimPos + new Vector3i(m, k, l);
							if (!world.GetChunkFromWorldPos(vector3i2, ref _chunk))
							{
								continue;
							}
							int x = World.toBlockXZ(vector3i2.x);
							int y = World.toBlockY(vector3i2.y);
							int z = World.toBlockXZ(vector3i2.z);
							BlockValue blockNoDamage = _chunk.GetBlockNoDamage(x, y, z);
							if ((blockNoDamage.isair || blockNoDamage.Block.Tags.Test_AnySet(other)) && !blockedPositions.Contains(vector3i2))
							{
								Chunk chunk2 = (Chunk)_chunk;
								if (!chunkList.Contains(chunk2))
								{
									chunkList.Add(chunk2);
								}
								blockChanges.Add(new BlockChangeInfo(chunk2.ClrIdx, vector3i2, blockValue2));
							}
						}
					}
					num3++;
				}
				break;
			}
			}
			currentState = FillSafeZoneStates.AddSigns;
			break;
		}
		case FillSafeZoneStates.AddSigns:
		{
			int num = halfClaimSize + 1;
			BlockValue blockValue = Block.GetBlockValue("playerSignWood1x3");
			if (signPositions == null)
			{
				signPositions = new List<Vector3i>();
			}
			Vector3i vector3i = claimPos + new Vector3i(0, 0, num);
			vector3i.y = world.GetHeight(vector3i.x, vector3i.z) + 1;
			blockValue.rotation = 0;
			signPositions.Add(vector3i);
			blockChanges.Add(new BlockChangeInfo(0, vector3i, blockValue));
			vector3i = claimPos + new Vector3i(0, 0, -num);
			vector3i.y = world.GetHeight(vector3i.x, vector3i.z) + 1;
			blockValue.rotation = 2;
			signPositions.Add(vector3i);
			blockChanges.Add(new BlockChangeInfo(0, vector3i, blockValue));
			vector3i = claimPos + new Vector3i(num, 0, 0);
			vector3i.y = world.GetHeight(vector3i.x, vector3i.z) + 1;
			blockValue.rotation = 1;
			signPositions.Add(vector3i);
			blockChanges.Add(new BlockChangeInfo(0, vector3i, blockValue));
			vector3i = claimPos + new Vector3i(-num, 0, 0);
			vector3i.y = world.GetHeight(vector3i.x, vector3i.z) + 1;
			blockValue.rotation = 3;
			signPositions.Add(vector3i);
			blockChanges.Add(new BlockChangeInfo(0, vector3i, blockValue));
			currentState = FillSafeZoneStates.Action;
			break;
		}
		case FillSafeZoneStates.Action:
			GameManager.Instance.ChangeBlocks(null, blockChanges);
			currentState = FillSafeZoneStates.ResetChunks;
			break;
		case FillSafeZoneStates.ResetChunks:
		{
			for (int i = 0; i < chunkList.Count; i++)
			{
				Chunk chunk = chunkList[i];
				long item = WorldChunkCache.MakeChunkKey(chunk.X, chunk.Z);
				chunkHash.Add(item);
			}
			if (base.Owner.Target != null)
			{
				PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.Owner.Target.entityId);
				for (int j = 0; j < signPositions.Count; j++)
				{
					if (world.GetTileEntity(0, signPositions[j]) is TileEntitySign tileEntitySign)
					{
						tileEntitySign.SetText(ModifiedName, _syncData: true, playerDataFromEntityID?.PrimaryId);
					}
				}
			}
			world.m_ChunkManager.ResendChunksToClients(chunkHash);
			TeleportPlayers();
			return ActionCompleteStates.Complete;
		}
		}
		return ActionCompleteStates.InComplete;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TeleportPlayers()
	{
		foreach (EntityPlayer key in playerDictionary.Keys)
		{
			GameManager.Instance.StartCoroutine(TeleportEntity(key, playerDictionary[key], 0f));
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropFillType, ref fillTypeNames);
		properties.ParseBool(PropDestroyClaim, ref destroyClaim);
		properties.ParseString(PropBlock, ref blockName);
		properties.ParseString(PropNewName, ref newName);
		properties.ParseString(PropBlockTags, ref blockTags);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionFillSafeZone
		{
			fillTypeNames = fillTypeNames,
			FillTypeList = FillTypeList,
			blockName = blockName,
			destroyClaim = destroyClaim,
			newName = newName,
			blockTags = blockTags
		};
	}
}
