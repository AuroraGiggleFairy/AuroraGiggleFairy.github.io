using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockLiquidv2 : Block
{
	public enum UpdateID
	{
		Sleep,
		Evaporate,
		Awake
	}

	public enum FlowDirection
	{
		None,
		North,
		East,
		South,
		West
	}

	public static Color Color = new Color32(0, 105, 148, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public static int fallSet = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int MAX_EMISSIONS = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ZERO_EMISSIONS = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ZERO_EVAPORATION = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int AUTO_GENERATED = 8;

	public static int blockUpdatesPerSecond = 16;

	public static int blockUpdates = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float blockUpdateTimer = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i[,] fallDirsSet = new Vector3i[8, 4]
	{
		{
			new Vector3i(-1, 0, 0),
			new Vector3i(0, 0, -1),
			new Vector3i(1, 0, 0),
			new Vector3i(0, 0, 1)
		},
		{
			new Vector3i(1, 0, 0),
			new Vector3i(0, 0, -1),
			new Vector3i(0, 0, 1),
			new Vector3i(-1, 0, 0)
		},
		{
			new Vector3i(0, 0, 1),
			new Vector3i(-1, 0, 0),
			new Vector3i(0, 0, -1),
			new Vector3i(1, 0, 0)
		},
		{
			new Vector3i(0, 0, -1),
			new Vector3i(0, 0, 1),
			new Vector3i(1, 0, 0),
			new Vector3i(-1, 0, 0)
		},
		{
			new Vector3i(-1, 0, 0),
			new Vector3i(1, 0, 0),
			new Vector3i(0, 0, -1),
			new Vector3i(0, 0, 1)
		},
		{
			new Vector3i(1, 0, 0),
			new Vector3i(-1, 0, 0),
			new Vector3i(0, 0, 1),
			new Vector3i(0, 0, -1)
		},
		{
			new Vector3i(0, 0, 1),
			new Vector3i(-1, 0, 0),
			new Vector3i(1, 0, 0),
			new Vector3i(0, 0, -1)
		},
		{
			new Vector3i(1, 0, 0),
			new Vector3i(-1, 0, 0),
			new Vector3i(0, 0, 1),
			new Vector3i(0, 0, -1)
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static float currentTime = 0f;

	public BlockLiquidv2()
	{
		IsRandomlyTick = false;
	}

	public override void LateInit()
	{
		base.LateInit();
	}

	public override int OnBlockDamaged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		_damagePoints = 0;
		return base.OnBlockDamaged(_world, _clrIdx, _blockPos, _blockValue, _damagePoints, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage, _recDepth);
	}

	public override bool IsHealthShownInUI(HitInfoDetails _hit, BlockValue _bv)
	{
		return _bv.damage > 0;
	}

	public override void OnNeighborBlockChange(WorldBase world, int _clrIdx, Vector3i _myBlockPos, BlockValue _myBlockValue, Vector3i _blockPosThatChanged, BlockValue _newNeighborBlockValue, BlockValue _oldNeighborBlockValue)
	{
		base.OnNeighborBlockChange(world, _clrIdx, _myBlockPos, _myBlockValue, _blockPosThatChanged, _newNeighborBlockValue, _oldNeighborBlockValue);
		if (_newNeighborBlockValue.isair)
		{
			if (_myBlockPos.x - _blockPosThatChanged.x > 0)
			{
				ChangeThis(world, _clrIdx, _myBlockValue, _myBlockPos, UpdateID.Awake, Emissions(_myBlockValue), 0, FlowDirection.West);
			}
			else if (_myBlockPos.x - _blockPosThatChanged.x < 0)
			{
				ChangeThis(world, _clrIdx, _myBlockValue, _myBlockPos, UpdateID.Awake, Emissions(_myBlockValue), 0, FlowDirection.East);
			}
			else if (_myBlockPos.z - _blockPosThatChanged.z < 0)
			{
				ChangeThis(world, _clrIdx, _myBlockValue, _myBlockPos, UpdateID.Awake, Emissions(_myBlockValue), 0, FlowDirection.North);
			}
			else if (_myBlockPos.z - _blockPosThatChanged.z > 0)
			{
				ChangeThis(world, _clrIdx, _myBlockValue, _myBlockPos, UpdateID.Awake, Emissions(_myBlockValue), 0, FlowDirection.South);
			}
			else
			{
				ChangeThis(world, _clrIdx, _myBlockValue, _myBlockPos, UpdateID.Awake, Emissions(_myBlockValue), Evap(_myBlockValue), FlowDirection.None);
			}
		}
		else if (_newNeighborBlockValue.type != blockID || _oldNeighborBlockValue.type != blockID || Evap(_newNeighborBlockValue) == Evap(_oldNeighborBlockValue))
		{
			if (_myBlockPos.y == _blockPosThatChanged.y && (_myBlockPos.x - _blockPosThatChanged.x > 0 || _myBlockPos.x - _blockPosThatChanged.x < 0 || _myBlockPos.z - _blockPosThatChanged.z < 0 || _myBlockPos.z - _blockPosThatChanged.z > 0))
			{
				ChangeThis(world, _clrIdx, _myBlockValue, _myBlockPos, UpdateID.Awake, Emissions(_myBlockValue), Evap(_myBlockValue), FlowDirection.None);
			}
			else
			{
				ChangeThis(world, _clrIdx, _myBlockValue, _myBlockPos, UpdateID.Awake, Emissions(_myBlockValue), Evap(_myBlockValue), FlowDirection.None);
			}
		}
	}

	public override bool IsMovementBlocked(IBlockAccess world, Vector3i _blockPos, BlockValue _blockValue, BlockFace crossingFace)
	{
		return false;
	}

	public override ulong GetTickRate()
	{
		return 1uL;
	}

	public static void UpdateTime()
	{
		currentTime = Time.time;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckUpdate()
	{
		if (currentTime > blockUpdateTimer + 0.5f)
		{
			blockUpdates = 0;
			blockUpdateTimer = currentTime;
		}
		blockUpdates++;
		if (blockUpdates > blockUpdatesPerSecond / 2)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ChangeThis(WorldBase _world, int _clusterIndex, BlockValue _blockValue, Vector3i _blockPos, UpdateID _blockState)
	{
		ChangeThis(_world, _clusterIndex, _blockValue, _blockPos, _blockState, Emissions(_blockValue), Evap(_blockValue), Flow(_blockValue));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ChangeThis(WorldBase _world, int _clusterIndex, BlockValue _blockValue, Vector3i _blockPos, UpdateID _blockState, int _emissions, int _evaporation)
	{
		ChangeThis(_world, _clusterIndex, _blockValue, _blockPos, _blockState, _emissions, 0, Flow(_blockValue));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ChangeThis(WorldBase _world, int _clusterIndex, BlockValue _blockValue, Vector3i _blockPos, UpdateID _blockState, int _emissions, int _evaporation, FlowDirection _flowDirection)
	{
		if (_blockValue.rotation != 8)
		{
			_emissions = MAX_EMISSIONS;
		}
		_emissions = Mathf.Clamp(_emissions, 0, MAX_EMISSIONS);
		BlockValue _blockValue2 = BlockValue.Air;
		_blockValue2.rawData = (uint)blockID;
		SetBlockState(ref _blockValue2, _blockState);
		_blockValue2.meta2 = (byte)_emissions;
		_blockValue2.rotation = 8;
		_blockValue2.damage = (int)(_evaporation + ((_flowDirection != FlowDirection.None) ? (50 + _flowDirection) : FlowDirection.None));
		_world.SetBlockRPC(_clusterIndex, _blockPos, _blockValue2);
		ulong ticks = 1000uL;
		switch (_blockState)
		{
		case UpdateID.Awake:
			ticks = 1uL;
			break;
		case UpdateID.Sleep:
			ticks = 60uL;
			break;
		}
		if (_world.GetWBT() != null)
		{
			_world.GetWBT().AddScheduledBlockUpdate(_clusterIndex, _blockPos, blockID, ticks);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ChangeToAir(WorldBase _world, int _clusterIndex, Vector3i _blockPos)
	{
		BlockValue air = BlockValue.Air;
		WaterSplashCubes.RemoveSplashAt(_blockPos.x, _blockPos.y, _blockPos.z);
		air.rawData = (uint)BlockValue.Air.type;
		air.damage = 0;
		_world.SetBlockRPC(_clusterIndex, _blockPos, air);
		if (_world.GetWBT() != null)
		{
			_world.GetWBT().AddScheduledBlockUpdate(_clusterIndex, _blockPos, blockID, GetTickRate());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public UpdateID State(BlockValue _blockValue)
	{
		return (UpdateID)_blockValue.meta;
	}

	public static int Emissions(BlockValue _blockValue)
	{
		if (_blockValue.rotation != 8)
		{
			return MAX_EMISSIONS;
		}
		return _blockValue.meta2;
	}

	public static int Evap(BlockValue _blockValue)
	{
		if (_blockValue.damage <= 45)
		{
			return _blockValue.damage;
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void IncEvap(ref BlockValue _blockValue)
	{
		if (_blockValue.damage > 45)
		{
			_blockValue.damage = 0;
		}
		_blockValue.damage++;
	}

	public static FlowDirection Flow(BlockValue _blockValue)
	{
		if (_blockValue.damage > 50)
		{
			return (FlowDirection)(_blockValue.damage - 50);
		}
		return FlowDirection.None;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i GetFlowDirection(FlowDirection _flowDirection)
	{
		Vector3i zero = Vector3i.zero;
		switch (_flowDirection)
		{
		case FlowDirection.North:
			zero.z = 1;
			break;
		case FlowDirection.East:
			zero.x = 1;
			break;
		case FlowDirection.South:
			zero.z = -1;
			break;
		case FlowDirection.West:
			zero.x = -1;
			break;
		}
		return zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsAir(BlockValue _blockValue)
	{
		return _blockValue.isair;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsPlant(BlockValue _blockValue)
	{
		Block block = _blockValue.Block;
		if (block == null)
		{
			Log.Error("BlockLiquidv2::IsPlant() - Couldn't find block with type [" + _blockValue.type + "].  Block is null.");
			return false;
		}
		return block.blockMaterial.IsPlant;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HasHoles(BlockValue _blockValue)
	{
		Block block = null;
		try
		{
			block = _blockValue.Block;
		}
		catch (Exception ex)
		{
			Log.Error("BlockLiquidv2::HasHoles() - Couldn't find block with type [" + _blockValue.type + "].  Exception is: " + ex.ToString());
			return false;
		}
		if (block == null)
		{
			Log.Error("BlockLiquidv2::HasHoles() - Couldn't find block with type [" + _blockValue.type + "].  Block is null.");
			return false;
		}
		try
		{
			bool flag = IsWater(_blockValue);
			int facesDrawnFullBitfield = block.shape.getFacesDrawnFullBitfield(_blockValue);
			bool flag2 = facesDrawnFullBitfield == 255 || facesDrawnFullBitfield == 63;
			return block.blockMaterial.IsPlant || (!block.shape.IsSolidCube && !flag && !flag2);
		}
		catch (Exception ex2)
		{
			Log.Error("BlockLiquidv2::HasHoles() - BlockValue type is [" + _blockValue.type + "].  Exception is: " + ex2.ToString());
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsWater(BlockValue _blockValue)
	{
		return _blockValue.type == blockID;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FlowDirection GetFlowDirection(Vector3i _dir)
	{
		if (_dir.x == -1)
		{
			return FlowDirection.West;
		}
		if (_dir.x == 1)
		{
			return FlowDirection.East;
		}
		if (_dir.z == 1)
		{
			return FlowDirection.North;
		}
		if (_dir.z == -1)
		{
			return FlowDirection.South;
		}
		return FlowDirection.None;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue GetBlock(WorldBase _world, Vector3i _blockPos)
	{
		BlockValue block = _world.GetBlock(_blockPos.x, _blockPos.y, _blockPos.z);
		if (block.rotation != 8)
		{
			block.meta2 = (byte)MAX_EMISSIONS;
		}
		return block;
	}

	public override void DoExchangeAction(WorldBase _world, int _clusterIndex, Vector3i _blockPos, BlockValue _blockValue, string _action, int _itemCount)
	{
		if (_blockValue.rotation != 8 || string.IsNullOrEmpty(_action) || _world == null || !_action.Contains("deplete"))
		{
			return;
		}
		int num = _action.LastIndexOf("deplete");
		if (num < 0)
		{
			return;
		}
		num += "deplete".Length;
		string text = _action.Substring(num);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		try
		{
			int num2 = int.Parse(text);
			num2 *= _itemCount;
			if (num2 <= 0)
			{
				return;
			}
			if (num2 > Emissions(_blockValue))
			{
				int num3 = Mathf.Clamp(num2 - Emissions(_blockValue) - 1, 0, MAX_EMISSIONS);
				if (num3 > 0)
				{
					Vector3i zero = Vector3i.zero;
					for (int i = 0; i < 4; i++)
					{
						if (num3 <= 0)
						{
							break;
						}
						zero = _blockPos + fallDirsSet[fallSet, i];
						BlockValue block = GetBlock(_world, zero);
						if (!IsWater(block))
						{
							continue;
						}
						int num4 = Emissions(block);
						if (num3 <= num4)
						{
							if (num4 == 0)
							{
								ChangeToAir(_world, _clusterIndex, zero);
								num3--;
							}
							else
							{
								ChangeThis(_world, _clusterIndex, block, zero, UpdateID.Awake, Mathf.Clamp(Emissions(block) - num3, 0, MAX_EMISSIONS), Evap(block), Flow(block));
							}
							break;
						}
						ChangeToAir(_world, _clusterIndex, zero);
						num3 -= Emissions(block);
					}
				}
				ChangeToAir(_world, _clusterIndex, _blockPos);
			}
			else
			{
				ChangeThis(_world, _clusterIndex, _blockValue, _blockPos, UpdateID.Awake, Mathf.Clamp(Emissions(_blockValue) - num2, 0, MAX_EMISSIONS), Evap(_blockValue), Flow(_blockValue));
			}
		}
		catch (Exception ex)
		{
			Log.Error("BlockLiquidv2::DoExchangeAction() - " + ex.ToString());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckDeepWater_Expensive(WorldBase world, Vector3i _blockPos)
	{
		int num = 0;
		Vector3i blockPos = default(Vector3i);
		blockPos.x = _blockPos.x;
		blockPos.y = _blockPos.y + 1;
		blockPos.z = _blockPos.z;
		BlockValue block = GetBlock(world, blockPos);
		while (IsWater(block) && num <= 6)
		{
			num++;
			blockPos.y++;
			block = GetBlock(world, blockPos);
		}
		return num >= 6;
	}

	public override bool UpdateTick(WorldBase world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		if (State(_blockValue) == UpdateID.Sleep)
		{
			return true;
		}
		fallSet++;
		if (fallSet >= 8)
		{
			fallSet = 0;
		}
		BlockValue air = BlockValue.Air;
		BlockValue air2 = BlockValue.Air;
		Vector3i zero = Vector3i.zero;
		Vector3i zero2 = Vector3i.zero;
		zero2.x = _blockPos.x;
		zero2.y = _blockPos.y - 1;
		zero2.z = _blockPos.z;
		air = GetBlock(world, zero2);
		if (HasHoles(air))
		{
			zero2.x = _blockPos.x;
			zero2.y = _blockPos.y - 2;
			zero2.z = _blockPos.z;
			air = GetBlock(world, zero2);
			if (IsAir(air) || IsPlant(air))
			{
				if (Emissions(_blockValue) > 0 && CheckUpdate())
				{
					if (!CheckDeepWater_Expensive(world, _blockPos))
					{
						ChangeThis(world, _clrIdx, _blockValue, _blockPos, UpdateID.Sleep, 0, 0, FlowDirection.None);
					}
					ChangeThis(world, _clrIdx, _blockValue, zero2, UpdateID.Awake, Emissions(_blockValue) - 1, 0, FlowDirection.None);
				}
				else
				{
					ChangeToAir(world, _clrIdx, _blockPos);
					ChangeThis(world, _clrIdx, _blockValue, zero2, UpdateID.Awake, Emissions(_blockValue), 0, FlowDirection.None);
				}
				return true;
			}
			zero2.x = _blockPos.x;
			zero2.y = _blockPos.y - 1;
			zero2.z = _blockPos.z;
			air = GetBlock(world, zero2);
		}
		if (IsAir(air) || IsPlant(air))
		{
			if (Emissions(_blockValue) > 0 && CheckUpdate())
			{
				if (!CheckDeepWater_Expensive(world, _blockPos))
				{
					ChangeThis(world, _clrIdx, _blockValue, _blockPos, UpdateID.Sleep, 0, 0, FlowDirection.None);
				}
				ChangeThis(world, _clrIdx, _blockValue, zero2, UpdateID.Awake, Emissions(_blockValue) - 1, 0, FlowDirection.None);
			}
			else
			{
				ChangeToAir(world, _clrIdx, _blockPos);
				ChangeThis(world, _clrIdx, _blockValue, zero2, UpdateID.Awake, Emissions(_blockValue), 0, FlowDirection.None);
			}
			return true;
		}
		if (!CheckUpdate())
		{
			return true;
		}
		BlockValue[] array = new BlockValue[4]
		{
			BlockValue.Air,
			BlockValue.Air,
			BlockValue.Air,
			BlockValue.Air
		};
		BlockValue[] array2 = new BlockValue[4]
		{
			BlockValue.Air,
			BlockValue.Air,
			BlockValue.Air,
			BlockValue.Air
		};
		bool[] array3 = new bool[4];
		bool[] array4 = new bool[4];
		for (int i = 0; i < 4; i++)
		{
			array3[i] = false;
		}
		for (int j = 0; j < 4; j++)
		{
			array4[j] = false;
		}
		Vector3i blockPos = default(Vector3i);
		for (int k = 0; k < 4; k++)
		{
			zero = _blockPos + fallDirsSet[fallSet, k];
			air2 = (array[k] = world.GetBlock(zero.x, zero.y, zero.z));
			array3[k] = true;
			if (!IsAir(air2) && !HasHoles(air2))
			{
				continue;
			}
			blockPos.x = zero.x;
			blockPos.y = zero.y - 1;
			blockPos.z = zero.z;
			air2 = (array2[k] = GetBlock(world, blockPos));
			array4[k] = true;
			if (!IsAir(air2) && !IsPlant(air2))
			{
				continue;
			}
			if (Emissions(_blockValue) > 0)
			{
				if (!CheckDeepWater_Expensive(world, _blockPos))
				{
					ChangeThis(world, _clrIdx, _blockValue, _blockPos, UpdateID.Sleep, 0, 0, GetFlowDirection(fallDirsSet[fallSet, k]));
				}
				ChangeThis(world, _clrIdx, _blockValue, zero, UpdateID.Awake, Emissions(_blockValue) - 1, 0, GetFlowDirection(fallDirsSet[fallSet, k]));
			}
			else
			{
				ChangeToAir(world, _clrIdx, _blockPos);
				ChangeThis(world, _clrIdx, _blockValue, zero, UpdateID.Awake, Emissions(_blockValue), 0, GetFlowDirection(fallDirsSet[fallSet, k]));
			}
			return true;
		}
		if (Emissions(_blockValue) > 0)
		{
			if (IsWater(air) && Emissions(air) < MAX_EMISSIONS)
			{
				if (!CheckDeepWater_Expensive(world, _blockPos))
				{
					ChangeThis(world, _clrIdx, _blockValue, _blockPos, UpdateID.Awake, Emissions(_blockValue) - 1, 0, FlowDirection.None);
				}
				ChangeThis(world, _clrIdx, air, zero2, UpdateID.Awake, Emissions(air) + 1, 0);
				return true;
			}
			for (int l = 0; l < 4; l++)
			{
				zero = _blockPos + fallDirsSet[fallSet, l];
				if (array3[l])
				{
					air2 = array[l];
				}
				else
				{
					air2 = (array[l] = GetBlock(world, zero));
					array3[l] = true;
				}
				if (IsAir(air2) || IsPlant(air2))
				{
					if (!CheckDeepWater_Expensive(world, _blockPos))
					{
						ChangeThis(world, _clrIdx, _blockValue, _blockPos, UpdateID.Awake, 0, 0, GetFlowDirection(fallDirsSet[fallSet, l]));
					}
					ChangeThis(world, _clrIdx, _blockValue, zero, UpdateID.Awake, Emissions(_blockValue) - 1, 0, GetFlowDirection(fallDirsSet[fallSet, l]));
					return true;
				}
				if (IsWater(air2) && Emissions(_blockValue) - Emissions(air2) - 1 > 0)
				{
					if (!CheckDeepWater_Expensive(world, _blockPos))
					{
						ChangeThis(world, _clrIdx, _blockValue, _blockPos, UpdateID.Awake, Emissions(_blockValue) - 1, 0, FlowDirection.None);
					}
					ChangeThis(world, _clrIdx, air2, zero, UpdateID.Awake, Emissions(air2) + 1, 0);
					return true;
				}
			}
		}
		if (IsWater(air) && Emissions(air) < MAX_EMISSIONS)
		{
			ChangeToAir(world, _clrIdx, _blockPos);
			ChangeThis(world, _clrIdx, air, zero2, UpdateID.Awake, Emissions(air) + 1, 0);
			return true;
		}
		Vector3i blockPos2 = default(Vector3i);
		for (int m = 0; m < 4; m++)
		{
			zero = _blockPos + fallDirsSet[fallSet, m];
			if (array4[m])
			{
				air2 = array2[m];
			}
			else
			{
				blockPos2.x = zero.x;
				blockPos2.y = zero.y - 1;
				blockPos2.z = zero.z;
				air2 = (array2[m] = GetBlock(world, blockPos2));
				array4[m] = true;
			}
			if (IsWater(air2) && Emissions(air2) < MAX_EMISSIONS)
			{
				ChangeToAir(world, _clrIdx, _blockPos);
				ChangeThis(world, _clrIdx, air2, zero + Vector3i.down, UpdateID.Awake, Emissions(air2) + 1, 0);
				return true;
			}
		}
		if (Emissions(_blockValue) < MAX_EMISSIONS)
		{
			zero = _blockPos;
			Vector3i blockPos3 = default(Vector3i);
			blockPos3.x = zero.x;
			blockPos3.y = zero.y + 1;
			blockPos3.z = zero.z;
			air2 = GetBlock(world, blockPos3);
			if (IsWater(air2))
			{
				if (Emissions(air2) == 0)
				{
					ChangeToAir(world, _clrIdx, _blockPos + Vector3i.up);
					ChangeThis(world, _clrIdx, _blockValue, _blockPos, UpdateID.Awake, Emissions(_blockValue) + 1, 0);
					return true;
				}
				ChangeThis(world, _clrIdx, air2, _blockPos + Vector3i.up, UpdateID.Awake, Emissions(air2) - 1, 0);
				ChangeThis(world, _clrIdx, _blockValue, _blockPos, UpdateID.Awake, Emissions(_blockValue) + 1, 0);
				return true;
			}
		}
		bool flag = false;
		Vector3i blockPos4 = Vector3i.zero;
		_ = BlockValue.Air;
		FlowDirection flowDirection = FlowDirection.None;
		for (int n = 0; n < 4; n++)
		{
			zero = _blockPos + fallDirsSet[fallSet, n];
			if (array3[n])
			{
				air2 = array[n];
			}
			else
			{
				air2 = (array[n] = GetBlock(world, zero));
				array3[n] = true;
			}
			if (IsAir(air2) || IsPlant(air2))
			{
				flag = true;
				blockPos4 = zero;
				flowDirection = GetFlowDirection(fallDirsSet[fallSet, n]);
				break;
			}
		}
		if (Emissions(_blockValue) < MAX_EMISSIONS - 1)
		{
			for (int num = 0; num < 4; num++)
			{
				zero = _blockPos + fallDirsSet[fallSet, num];
				if (array3[num])
				{
					air2 = array[num];
				}
				else
				{
					air2 = (array[num] = GetBlock(world, zero));
					array3[num] = true;
				}
				if (IsWater(air2) && Emissions(air2) >= 2 - (flag ? 1 : 0) + Emissions(_blockValue))
				{
					ChangeThis(world, _clrIdx, air2, zero, UpdateID.Awake, Emissions(air2) - 1, 0);
					if (flag)
					{
						ChangeThis(world, _clrIdx, _blockValue, blockPos4, UpdateID.Awake, 0, 0, flowDirection);
					}
					else
					{
						ChangeThis(world, _clrIdx, _blockValue, _blockPos, UpdateID.Awake, Emissions(_blockValue) + 1, 0);
					}
					return true;
				}
			}
		}
		if (Emissions(_blockValue) > 0)
		{
			for (int num2 = 0; num2 < 4; num2++)
			{
				zero = _blockPos + fallDirsSet[fallSet, num2];
				if (array3[num2])
				{
					air2 = array[num2];
				}
				else
				{
					air2 = (array[num2] = GetBlock(world, zero));
					array3[num2] = true;
				}
				if (HasHoles(air2))
				{
					zero = _blockPos + fallDirsSet[fallSet, num2] + fallDirsSet[fallSet, num2];
					air2 = GetBlock(world, zero);
					if (IsAir(air2) || IsPlant(air2))
					{
						ChangeThis(world, _clrIdx, _blockValue, _blockPos, UpdateID.Awake, Emissions(_blockValue) - 1, 0, GetFlowDirection(fallDirsSet[fallSet, num2]));
						ChangeThis(world, _clrIdx, air2, zero, UpdateID.Awake, 0, 0, GetFlowDirection(fallDirsSet[fallSet, num2]));
						return true;
					}
					if (IsWater(air2) && Emissions(_blockValue) > Emissions(air2) + 1)
					{
						ChangeThis(world, _clrIdx, _blockValue, _blockPos, UpdateID.Awake, Emissions(_blockValue) - 1, 0, GetFlowDirection(fallDirsSet[fallSet, num2]));
						ChangeThis(world, _clrIdx, air2, zero, UpdateID.Awake, Emissions(air2) + 1, 0);
						return true;
					}
				}
			}
		}
		if (flag)
		{
			if (State(_blockValue) == UpdateID.Evaporate)
			{
				WaterEvaporationManager.AddToEvapList(_blockPos);
				ChangeThis(world, _clrIdx, _blockValue, _blockPos, UpdateID.Sleep);
				return true;
			}
			WaterEvaporationManager.AddToRestList(_blockPos);
		}
		ChangeThis(world, _clrIdx, _blockValue, _blockPos, UpdateID.Sleep);
		return true;
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (_blockValue.type != blockID || world.IsRemote())
		{
			return;
		}
		if (_blockValue.rotation != 8)
		{
			int count = Count;
			ChangeThis(world, _chunk.ClrIdx, _blockValue, _blockPos, UpdateID.Awake, count, 0, FlowDirection.None);
			if (GameTimer.Instance.elapsedTicks > 0)
			{
				_blockValue.meta2 = (byte)MAX_EMISSIONS;
				_blockValue.rotation = 8;
				UpdateTick(world, _chunk.ClrIdx, _blockPos, _blockValue, _bRandomTick: false, 0uL, null);
			}
		}
		else
		{
			ChangeThis(world, _chunk.ClrIdx, _blockValue, _blockPos, UpdateID.Awake, _blockValue.meta2, 0, FlowDirection.None);
			if (GameTimer.Instance.elapsedTicks > 0)
			{
				UpdateTick(world, _chunk.ClrIdx, _blockPos, _blockValue, _bRandomTick: false, 0uL, null);
			}
		}
	}

	public static void WaterDataToPlaceholderBlock(WaterValue _data, out BlockValue _bv)
	{
		if ((float)_data.GetMass() > 195f)
		{
			_bv = default(BlockValue);
			_bv.type = 242;
			_bv.damage = 0;
			_bv.meta = 0;
			_bv.meta2 = (byte)MAX_EMISSIONS;
			_bv.rotation = 8;
		}
		else
		{
			_bv = BlockValue.Air;
		}
	}

	public static void WaterDataToBlockValue(WaterValue _data, out BlockValue _bv)
	{
		if ((float)_data.GetMass() > 195f)
		{
			_bv = default(BlockValue);
			_bv.type = 240;
			_bv.damage = 0;
			_bv.meta = 2;
			_bv.meta2 = (byte)MAX_EMISSIONS;
			_bv.rotation = 8;
		}
		else
		{
			_bv = BlockValue.Air;
		}
	}

	public static void SetBlockState(ref BlockValue _blockValue, UpdateID _blockState)
	{
		_blockValue.meta = (byte)_blockState;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetWaterRPC()
	{
	}
}
