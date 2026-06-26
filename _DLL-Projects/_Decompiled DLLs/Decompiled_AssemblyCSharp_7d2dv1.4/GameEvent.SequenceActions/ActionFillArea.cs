using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionFillArea : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum FillSafeZoneStates
	{
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
		Cylinder
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
	public string excludeBlockTags;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool replaceAll;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool maxDensity;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFillType = "fill_type";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBlock = "block";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDestroyClaim = "destroy_claim";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBlockTags = "block_tags";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropExcludeBlockTags = "exclude_block_tags";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropReplaceAll = "replace_all";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropNewName = "new_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMaxDensity = "max_density";

	[PublicizedFrom(EAccessModifier.Private)]
	public string blockName = "terrDirtTwitch";

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i claimPos = Vector3i.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockChangeInfo> blockChanges = new List<BlockChangeInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Chunk> chunkList = new List<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int claimSize = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public int halfClaimSize = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public int distanceSq = 25;

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
		case FillSafeZoneStates.HandleChunks:
		{
			FillType = FillTypeList.RandomObject();
			if (base.Owner.TargetPosition != Vector3.zero)
			{
				claimPos = new Vector3i(base.Owner.TargetPosition);
			}
			else
			{
				if (!(base.Owner.Target.position != Vector3.zero))
				{
					return ActionCompleteStates.InCompleteRefund;
				}
				claimPos = new Vector3i(base.Owner.Target.position);
			}
			chunkList.Clear();
			int num8 = claimSize / 16 + 1;
			int num9 = claimSize / 16 + 1;
			for (int num10 = -num8; num10 <= num8; num10++)
			{
				int x4 = claimPos.x + num10 * 16;
				for (int num11 = -num9; num11 <= num9; num11++)
				{
					int z4 = claimPos.z + num11 * 16;
					Chunk chunk5 = (Chunk)world.GetChunkFromWorldPos(new Vector3i(x4, claimPos.y, z4));
					if (chunk5 != null && !chunkList.Contains(chunk5))
					{
						chunkList.Add(chunk5);
					}
				}
			}
			List<Entity> entitiesInBounds = world.GetEntitiesInBounds(null, new Bounds(claimPos, Vector3.one * claimSize), _isAlive: true);
			for (int num12 = 0; num12 < entitiesInBounds.Count; num12++)
			{
				Entity entity = entitiesInBounds[num12];
				bool num13 = entity is EntityPlayer;
				Vector3i vector3i5 = new Vector3i(Utils.Fastfloor(entity.position.x), Utils.Fastfloor(entity.position.y), Utils.Fastfloor(entity.position.z));
				if (num13)
				{
					EntityPlayer key = entity as EntityPlayer;
					Vector3 value = vector3i5 + new Vector3(0.5f, 0.5f, 0.5f);
					playerDictionary.Add(key, value);
				}
				blockedPositions.Add(vector3i5);
				blockedPositions.Add(vector3i5 + Vector3i.up);
				if (num13)
				{
					blockedPositions.Add(vector3i5 + Vector3i.up * 2);
				}
				vector3i5 += Vector3i.right;
				blockedPositions.Add(vector3i5);
				blockedPositions.Add(vector3i5 + Vector3i.up);
				if (num13)
				{
					blockedPositions.Add(vector3i5 + Vector3i.up * 2);
				}
			}
			currentState = FillSafeZoneStates.SetupChanges;
			break;
		}
		case FillSafeZoneStates.SetupChanges:
		{
			blockChanges.Clear();
			IChunk _chunk = null;
			int num = halfClaimSize;
			BlockValue blockValue = Block.GetBlockValue(blockName);
			TeleportPlayers();
			FastTags<TagGroup.Global> other = ((blockTags != null) ? FastTags<TagGroup.Global>.Parse(blockTags) : FastTags<TagGroup.Global>.none);
			FastTags<TagGroup.Global> other2 = ((excludeBlockTags != null) ? FastTags<TagGroup.Global>.Parse(excludeBlockTags) : FastTags<TagGroup.Global>.none);
			switch (FillType)
			{
			case FillTypes.Cube:
			{
				for (int n = -num; n <= num; n++)
				{
					for (int num2 = -num; num2 <= num; num2++)
					{
						for (int num3 = -num; num3 <= num; num3++)
						{
							Vector3i vector3i2 = claimPos + new Vector3i(num3, n, num2);
							if (!world.GetChunkFromWorldPos(vector3i2, ref _chunk))
							{
								continue;
							}
							int x2 = World.toBlockXZ(vector3i2.x);
							int y2 = World.toBlockY(vector3i2.y);
							int z2 = World.toBlockXZ(vector3i2.z);
							BlockValue blockNoDamage2 = _chunk.GetBlockNoDamage(x2, y2, z2);
							if ((blockNoDamage2.isair || blockNoDamage2.Block.Tags.Test_AnySet(other) || replaceAll) && !blockNoDamage2.Block.Tags.Test_AnySet(other2) && world.GetTraderAreaAt(vector3i2) == null && !blockedPositions.Contains(vector3i2))
							{
								Chunk chunk3 = (Chunk)_chunk;
								if (!chunkList.Contains(chunk3))
								{
									chunkList.Add(chunk3);
								}
								if (maxDensity)
								{
									blockChanges.Add(new BlockChangeInfo(chunk3.ClrIdx, vector3i2, blockValue, sbyte.MaxValue));
								}
								else
								{
									blockChanges.Add(new BlockChangeInfo(chunk3.ClrIdx, vector3i2, blockValue));
								}
							}
						}
					}
				}
				break;
			}
			case FillTypes.Sphere:
			{
				for (int num4 = -num; num4 <= num; num4++)
				{
					for (int num5 = -num; num5 <= num; num5++)
					{
						for (int num6 = -num; num6 <= num; num6++)
						{
							Vector3i vector3i3 = claimPos + new Vector3i(num6, num4, num5);
							if (Vector3.SqrMagnitude(claimPos - vector3i3) >= (float)distanceSq || !world.GetChunkFromWorldPos(vector3i3, ref _chunk))
							{
								continue;
							}
							int x3 = World.toBlockXZ(vector3i3.x);
							int y3 = World.toBlockY(vector3i3.y);
							int z3 = World.toBlockXZ(vector3i3.z);
							BlockValue blockNoDamage3 = _chunk.GetBlockNoDamage(x3, y3, z3);
							if ((blockNoDamage3.isair || blockNoDamage3.Block.Tags.Test_AnySet(other) || replaceAll) && !blockNoDamage3.Block.Tags.Test_AnySet(other2) && world.GetTraderAreaAt(vector3i3) == null && !blockedPositions.Contains(vector3i3))
							{
								Chunk chunk4 = (Chunk)_chunk;
								if (!chunkList.Contains(chunk4))
								{
									chunkList.Add(chunk4);
								}
								if (maxDensity)
								{
									blockChanges.Add(new BlockChangeInfo(chunk4.ClrIdx, vector3i3, blockValue, sbyte.MaxValue));
								}
								else
								{
									blockChanges.Add(new BlockChangeInfo(chunk4.ClrIdx, vector3i3, blockValue));
								}
							}
						}
					}
				}
				break;
			}
			case FillTypes.Cylinder:
			{
				for (int k = -num; k <= num; k++)
				{
					for (int l = -num; l <= num; l++)
					{
						for (int m = -num; m <= num; m++)
						{
							Vector3i vector3i = claimPos + new Vector3i(m, k, l);
							if (Vector3.SqrMagnitude(new Vector3i(claimPos.x, vector3i.y, claimPos.z) - vector3i) >= (float)distanceSq || !world.GetChunkFromWorldPos(vector3i, ref _chunk))
							{
								continue;
							}
							int x = World.toBlockXZ(vector3i.x);
							int y = World.toBlockY(vector3i.y);
							int z = World.toBlockXZ(vector3i.z);
							BlockValue blockNoDamage = _chunk.GetBlockNoDamage(x, y, z);
							if ((blockNoDamage.isair || blockNoDamage.Block.Tags.Test_AnySet(other) || replaceAll) && !blockNoDamage.Block.Tags.Test_AnySet(other2) && world.GetTraderAreaAt(vector3i) == null && !blockedPositions.Contains(vector3i))
							{
								Chunk chunk2 = (Chunk)_chunk;
								if (!chunkList.Contains(chunk2))
								{
									chunkList.Add(chunk2);
								}
								if (maxDensity)
								{
									blockChanges.Add(new BlockChangeInfo(chunk2.ClrIdx, vector3i, blockValue, sbyte.MaxValue));
								}
								else
								{
									blockChanges.Add(new BlockChangeInfo(chunk2.ClrIdx, vector3i, blockValue));
								}
							}
						}
					}
				}
				break;
			}
			}
			currentState = FillSafeZoneStates.AddSigns;
			break;
		}
		case FillSafeZoneStates.AddSigns:
		{
			if (newName == "")
			{
				currentState = FillSafeZoneStates.Action;
				break;
			}
			int num7 = halfClaimSize + 1;
			BlockValue blockValue2 = Block.GetBlockValue("playerSignWood1x3");
			if (signPositions == null)
			{
				signPositions = new List<Vector3i>();
			}
			Vector3i vector3i4 = claimPos + new Vector3i(0, 0, num7);
			vector3i4.y = world.GetHeight(vector3i4.x, vector3i4.z) + 1;
			blockValue2.rotation = 0;
			if (world.GetTraderAreaAt(vector3i4) == null)
			{
				signPositions.Add(vector3i4);
				blockChanges.Add(new BlockChangeInfo(0, vector3i4, blockValue2));
			}
			vector3i4 = claimPos + new Vector3i(0, 0, -num7);
			vector3i4.y = world.GetHeight(vector3i4.x, vector3i4.z) + 1;
			blockValue2.rotation = 2;
			if (world.GetTraderAreaAt(vector3i4) == null)
			{
				signPositions.Add(vector3i4);
				blockChanges.Add(new BlockChangeInfo(0, vector3i4, blockValue2));
			}
			vector3i4 = claimPos + new Vector3i(num7, 0, 0);
			vector3i4.y = world.GetHeight(vector3i4.x, vector3i4.z) + 1;
			blockValue2.rotation = 1;
			if (world.GetTraderAreaAt(vector3i4) == null)
			{
				signPositions.Add(vector3i4);
				blockChanges.Add(new BlockChangeInfo(0, vector3i4, blockValue2));
			}
			vector3i4 = claimPos + new Vector3i(-num7, 0, 0);
			vector3i4.y = world.GetHeight(vector3i4.x, vector3i4.z) + 1;
			blockValue2.rotation = 3;
			if (world.GetTraderAreaAt(vector3i4) == null)
			{
				signPositions.Add(vector3i4);
				blockChanges.Add(new BlockChangeInfo(0, vector3i4, blockValue2));
			}
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
			if (base.Owner.Target != null && newName != "")
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
		properties.ParseString(PropExcludeBlockTags, ref excludeBlockTags);
		properties.ParseBool(PropReplaceAll, ref replaceAll);
		properties.ParseBool(PropMaxDensity, ref maxDensity);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionFillArea
		{
			fillTypeNames = fillTypeNames,
			FillTypeList = FillTypeList,
			blockName = blockName,
			destroyClaim = destroyClaim,
			newName = newName,
			blockTags = blockTags,
			excludeBlockTags = excludeBlockTags,
			replaceAll = replaceAll,
			maxDensity = maxDensity
		};
	}
}
