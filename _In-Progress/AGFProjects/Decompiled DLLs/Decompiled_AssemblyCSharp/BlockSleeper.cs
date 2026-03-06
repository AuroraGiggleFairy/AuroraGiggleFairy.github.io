using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockSleeper : Block
{
	public enum eMode
	{
		Normal,
		Bandit,
		Infested
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string PropPose = "Pose";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string PropLookIdentity = "LookIdentity";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string PropExcludeWalkType = "ExcludeWalkType";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string PropSpawnGroup = "SpawnGroup";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string PropSpawnMode = "SpawnMode";

	public int pose;

	public Vector3 look;

	public string spawnGroup;

	public eMode spawnMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> excludedWalkTypes;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[1]
	{
		new BlockActivationCommand("open", "dummy", _enabled: true)
	};

	public BlockSleeper()
	{
		IsSleeperBlock = true;
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		base.Properties.ParseInt(PropPose, ref pose);
		look = Vector3.forward;
		base.Properties.ParseVec(PropLookIdentity, ref look);
		string text = base.Properties.GetString(PropExcludeWalkType);
		if (text.Length > 0)
		{
			string[] array = text.Split(',');
			excludedWalkTypes = new List<int>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == "Crawler")
				{
					excludedWalkTypes.Add(21);
					continue;
				}
				Log.Warning("Block {0}, invalid ExcludeWalkType {1}", GetBlockName(), array[i]);
			}
		}
		base.Properties.ParseString(PropSpawnGroup, ref spawnGroup);
		base.Properties.ParseEnum(PropSpawnMode, ref spawnMode);
	}

	public override bool CanPlaceBlockAt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
	{
		if (_world.IsEditor())
		{
			return true;
		}
		return base.CanPlaceBlockAt(_world, _clrIdx, _blockPos, _blockValue, _bOmitCollideCheck);
	}

	public float GetSleeperRotation(BlockValue _blockValue)
	{
		return _blockValue.rotation switch
		{
			1 => 90f, 
			2 => 180f, 
			3 => 270f, 
			24 => 45f, 
			25 => 135f, 
			26 => 225f, 
			27 => 315f, 
			_ => 0f, 
		};
	}

	public bool ExcludesWalkType(int _walkType)
	{
		if (excludedWalkTypes != null)
		{
			return excludedWalkTypes.Contains(_walkType);
		}
		return false;
	}

	public override bool IsTileEntitySavedInPrefab()
	{
		return false;
	}
}
