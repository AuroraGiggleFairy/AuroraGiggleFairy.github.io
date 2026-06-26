using UnityEngine;

public class BlockToolTerrainSmoothing : IBlockTool
{
	[PublicizedFrom(EAccessModifier.Private)]
	public BlockTools.Brush brush;

	[PublicizedFrom(EAccessModifier.Private)]
	public NguiWdwTerrainEditor parentWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemInventoryData lastData;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isButtonDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockMaskRules<sbyte?, char> blockMaskRuleset;

	public BlockToolTerrainSmoothing(BlockTools.Brush _brush, NguiWdwTerrainEditor _parentWindow)
	{
		brush = _brush;
		parentWindow = _parentWindow;
		isButtonDown = false;
		SetUp22DegreeRules();
	}

	public void CheckSpecialKeys(Event ev, PlayerActionsLocal playerActions)
	{
	}

	public void CheckKeys(ItemInventoryData _data, WorldRayHitInfo _hitInfo, PlayerActionsLocal playerActions)
	{
		if (_data != null && _data.actionData != null && _data.actionData[0] != null)
		{
			parentWindow.lastPosition = new Vector3i(_hitInfo.hit.pos);
			parentWindow.lastDirection = _hitInfo.ray.direction;
			if (Input.GetMouseButton(0) && isButtonDown)
			{
				SmoothTerrain();
			}
			else if (!Input.GetMouseButton(0) && isButtonDown)
			{
				isButtonDown = false;
			}
		}
	}

	public bool ConsumeScrollWheel(ItemInventoryData _data, float _scrollWheelInput, PlayerActionsLocal _playerInput)
	{
		return false;
	}

	public string GetDebugOutput()
	{
		return "";
	}

	public bool ExecuteAttackAction(ItemInventoryData _data, bool _bReleased, PlayerActionsLocal playerActions)
	{
		isButtonDown = true;
		lastData = _data;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SmoothTerrain()
	{
		ItemInventoryData itemInventoryData = lastData;
		if (!(Time.time - itemInventoryData.actionData[0].lastUseTime > Constants.cBuildIntervall) || !itemInventoryData.hitInfo.bHitValid || !GameUtils.IsBlockOrTerrain(itemInventoryData.hitInfo.tag))
		{
			return;
		}
		Vector3i blockPos = itemInventoryData.hitInfo.hit.blockPos;
		BlockValue block = itemInventoryData.world.GetBlock(blockPos);
		Vector3i[] cubesInBrush = brush.GetCubesInBrush();
		for (int i = 0; i < cubesInBrush.Length; i++)
		{
			Vector3i vector3i = blockPos + cubesInBrush[i];
			if (HasValidNeighbor(vector3i, 0, itemInventoryData))
			{
				BlockValue block2 = itemInventoryData.world.GetBlock(vector3i);
				bool flag = block2.Equals(BlockValue.Air);
				sbyte averageDensity = GetAverageDensity(vector3i, itemInventoryData);
				if (averageDensity <= -1 && flag)
				{
					itemInventoryData.world.SetBlockRPC(vector3i, block, averageDensity);
				}
				else if (averageDensity >= 0 && !flag)
				{
					itemInventoryData.world.SetBlockRPC(vector3i, BlockValue.Air, averageDensity);
				}
				else
				{
					itemInventoryData.world.SetBlockRPC(0, vector3i, block2, averageDensity);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SnapTerrain22()
	{
		ItemInventoryData itemInventoryData = lastData;
		if (!(Time.time - itemInventoryData.actionData[0].lastUseTime > Constants.cBuildIntervall) || !itemInventoryData.hitInfo.bHitValid || !GameUtils.IsBlockOrTerrain(itemInventoryData.hitInfo.tag))
		{
			return;
		}
		Vector3i blockPos = itemInventoryData.hitInfo.hit.blockPos;
		BlockValue block = itemInventoryData.world.GetBlock(blockPos);
		Vector3i[] cubesInBrush = brush.GetCubesInBrush();
		for (int i = 0; i < cubesInBrush.Length; i++)
		{
			Vector3i vector3i = blockPos + cubesInBrush[i];
			if (HasValidNeighbor(vector3i, 0, itemInventoryData))
			{
				bool flag = itemInventoryData.world.GetBlock(cubesInBrush[i]).Equals(BlockValue.Air);
				sbyte outputBasedOnRuleset = GetOutputBasedOnRuleset22(vector3i, itemInventoryData);
				if (outputBasedOnRuleset <= -1 && flag)
				{
					itemInventoryData.world.SetBlockRPC(vector3i, block, outputBasedOnRuleset);
				}
				else if (outputBasedOnRuleset >= 0 && !flag)
				{
					itemInventoryData.world.SetBlockRPC(vector3i, BlockValue.Air, outputBasedOnRuleset);
				}
				else
				{
					itemInventoryData.world.SetBlockRPC(vector3i, outputBasedOnRuleset);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetUp22DegreeRules()
	{
		blockMaskRuleset = new BlockMaskRules<sbyte?, char>('*');
		blockMaskRuleset.AddRule(new BlockRule<sbyte?, char>((sbyte)(-1), new char[27]
		{
			'*', 'A', '*', '*', 'A', '*', '*', 'A', '*', '*',
			'B', '*', '*', '*', '*', '*', 'A', '*', '*', 'B',
			'*', '*', 'B', '*', '*', 'B', '*'
		}));
		blockMaskRuleset.AddRule(new BlockRule<sbyte?, char>((sbyte)(-1), new char[27]
		{
			'*', 'A', '*', '*', 'A', '*', '*', 'A', '*', '*',
			'B', '*', '*', '*', '*', '*', 'A', '*', '*', 'B',
			'*', '*', 'B', '*', '*', 'B', '*'
		}));
		blockMaskRuleset.AddRule(new BlockRule<sbyte?, char>((sbyte)(-1), new char[27]
		{
			'*', '*', '*', '*', '*', '*', '*', '*', '*', '*',
			'*', '*', 'H', 'A', 'H', '*', '*', '*', '*', '*',
			'*', '*', '*', '*', '*', '*', '*'
		}));
		blockMaskRuleset.AddRule(new BlockRule<sbyte?, char>((sbyte)sbyte.MinValue, new char[27]
		{
			'*', '*', '*', '*', '*', '*', '*', '*', '*', '*',
			'*', '*', 'B', 'A', 'B', '*', '*', '*', '*', '*',
			'*', '*', '*', '*', '*', '*', '*'
		}));
		blockMaskRuleset.AddRule(new BlockRule<sbyte?, char>((sbyte)sbyte.MinValue, new char[27]
		{
			'*', '*', '*', '*', '*', '*', '*', '*', '*', '*',
			'*', '*', 'B', 'H', 'B', '*', 'H', '*', '*', '*',
			'*', '*', '*', '*', '*', '*', '*'
		}));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sbyte GetOutputBasedOnRuleset22(Vector3i _pos, ItemInventoryData _data)
	{
		char[] array = new char[27];
		int num = 0;
		for (int num2 = 1; num2 >= -1; num2--)
		{
			for (int num3 = 1; num3 >= -1; num3--)
			{
				for (int num4 = 1; num4 >= -1; num4--)
				{
					int density = _data.world.GetDensity(0, _pos.x + num4, _pos.y + num2, _pos.z + num3);
					if (density >= 0)
					{
						array[num] = 'A';
					}
					else if (density > -32)
					{
						array[num] = 'H';
					}
					else
					{
						array[num] = 'B';
					}
					num++;
				}
			}
		}
		sbyte? output = blockMaskRuleset.GetOutput(array);
		if (!output.HasValue)
		{
			sbyte density2 = _data.world.GetDensity(0, _pos.x, _pos.y, _pos.z);
			if (density2 >= 0)
			{
				return sbyte.MaxValue;
			}
			if (density2 > -32)
			{
				return -1;
			}
			return sbyte.MinValue;
		}
		return output.Value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SnapTerrain45()
	{
		ItemInventoryData itemInventoryData = lastData;
		if (!(Time.time - itemInventoryData.actionData[0].lastUseTime > Constants.cBuildIntervall) || !itemInventoryData.hitInfo.bHitValid || !GameUtils.IsBlockOrTerrain(itemInventoryData.hitInfo.tag))
		{
			return;
		}
		Vector3i blockPos = itemInventoryData.hitInfo.hit.blockPos;
		BlockValue block = itemInventoryData.world.GetBlock(blockPos);
		Vector3i[] cubesInBrush = brush.GetCubesInBrush();
		for (int i = 0; i < cubesInBrush.Length; i++)
		{
			Vector3i vector3i = blockPos + cubesInBrush[i];
			if (HasValidNeighbor(vector3i, 0, itemInventoryData))
			{
				bool flag = itemInventoryData.world.GetBlock(vector3i).Equals(BlockValue.Air);
				sbyte b = GetAverageDensity(vector3i, itemInventoryData);
				if (AirNeighborCount(vector3i, itemInventoryData) > 3 && !flag)
				{
					b = sbyte.MinValue;
				}
				if (b <= -1 && flag)
				{
					itemInventoryData.world.SetBlockRPC(vector3i, block, b);
				}
				else if (b >= 0 && !flag)
				{
					itemInventoryData.world.SetBlockRPC(vector3i, BlockValue.Air, b);
				}
				else
				{
					itemInventoryData.world.SetBlockRPC(vector3i, b);
				}
			}
		}
	}

	public bool ExecuteUseAction(ItemInventoryData _data, bool _bReleased, PlayerActionsLocal playerActions)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int AirNeighborCount(Vector3i _pos, ItemInventoryData _data)
	{
		int num = 0;
		for (int i = _pos.x - 1; i <= _pos.x + 1; i++)
		{
			for (int j = _pos.z - 1; j <= _pos.z + 1; j++)
			{
				if ((i != 0 || j != 0) && _data.world.GetBlock(i, _pos.y, j).Equals(BlockValue.Air))
				{
					num++;
				}
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public sbyte GetAverageLockedDensity(Vector3i _pos, ItemInventoryData _data)
	{
		Vector3i[] cubesInBrush = brush.GetCubesInBrush();
		int num = _data.world.GetDensity(0, _pos.x, _pos.y, _pos.z);
		for (int i = 0; i < cubesInBrush.Length; i++)
		{
			Vector3i blockPos = _pos + cubesInBrush[i];
			num += _data.world.GetDensity(0, blockPos);
		}
		return (sbyte)(num / (cubesInBrush.Length + 1));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public sbyte GetAverageDensity(Vector3i _pos, ItemInventoryData _data, int _clrIdx = 0)
	{
		float num = 1f;
		float num2 = 0f + (float)_data.world.GetDensity(_clrIdx, _pos.x, _pos.y, _pos.z);
		num += 1f;
		float num3 = num2 + (float)_data.world.GetDensity(_clrIdx, _pos.x - 1, _pos.y, _pos.z);
		num += 1f;
		float num4 = num3 + (float)_data.world.GetDensity(_clrIdx, _pos.x + 1, _pos.y, _pos.z);
		num += 1f;
		float num5 = num4 + (float)_data.world.GetDensity(_clrIdx, _pos.x, _pos.y, _pos.z - 1);
		num += 1f;
		return (sbyte)((num5 + (float)_data.world.GetDensity(_clrIdx, _pos.x, _pos.y, _pos.z + 1)) / num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sbyte[] GetLocalDensityMap(Vector3i blockTargetPos, ItemInventoryData data, Vector3i[] localArea)
	{
		sbyte[] array = new sbyte[localArea.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = data.world.GetDensity(0, localArea[i].x, localArea[i].y, localArea[i].z);
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool HasValidNeighbor(Vector3i _pos, sbyte _denThreshold, ItemInventoryData _data)
	{
		if (_denThreshold < 0)
		{
			if (_data.world.GetDensity(0, _pos.x - 1, _pos.y, _pos.z) <= _denThreshold)
			{
				return true;
			}
			if (_data.world.GetDensity(0, _pos.x + 1, _pos.y, _pos.z) <= _denThreshold)
			{
				return true;
			}
			if (_data.world.GetDensity(0, _pos.x, _pos.y - 1, _pos.z) <= _denThreshold)
			{
				return true;
			}
			if (_data.world.GetDensity(0, _pos.x, _pos.y + 1, _pos.z) <= _denThreshold)
			{
				return true;
			}
			if (_data.world.GetDensity(0, _pos.x, _pos.y, _pos.z - 1) <= _denThreshold)
			{
				return true;
			}
			if (_data.world.GetDensity(0, _pos.x, _pos.y, _pos.z + 1) <= _denThreshold)
			{
				return true;
			}
			return false;
		}
		if (_denThreshold >= 0)
		{
			if (_data.world.GetDensity(0, _pos.x - 1, _pos.y, _pos.z) >= _denThreshold)
			{
				return true;
			}
			if (_data.world.GetDensity(0, _pos.x + 1, _pos.y, _pos.z) >= _denThreshold)
			{
				return true;
			}
			if (_data.world.GetDensity(0, _pos.x, _pos.y - 1, _pos.z) >= _denThreshold)
			{
				return true;
			}
			if (_data.world.GetDensity(0, _pos.x, _pos.y + 1, _pos.z) >= _denThreshold)
			{
				return true;
			}
			if (_data.world.GetDensity(0, _pos.x, _pos.y, _pos.z - 1) >= _denThreshold)
			{
				return true;
			}
			if (_data.world.GetDensity(0, _pos.x, _pos.y, _pos.z + 1) >= _denThreshold)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public override string ToString()
	{
		return "Smooth Terrain";
	}
}
