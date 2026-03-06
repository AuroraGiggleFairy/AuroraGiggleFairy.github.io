using UnityEngine.Scripting;
using XMLData;

[Preserve]
public class XUiC_DoorEditor : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDowngrade;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnUpgrade;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> cbxColorPresetList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOpenClose;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOk;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk chunk;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte initialColorIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public int initialDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bAcceptChanges;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		btnDowngrade = GetChildById("btnDowngrade").GetChildByType<XUiC_SimpleButton>();
		btnDowngrade.OnPressed += BtnDowngrade_OnPressed;
		btnUpgrade = GetChildById("btnUpgrade").GetChildByType<XUiC_SimpleButton>();
		btnUpgrade.OnPressed += BtnUpgrade_OnPressed;
		cbxColorPresetList = (XUiC_ComboBoxList<string>)GetChildById("cbxPresets");
		cbxColorPresetList.OnValueChanged += CbxColorPresetList_OnValueChanged;
		foreach (string key in ColorMappingData.Instance.IDFromName.Keys)
		{
			cbxColorPresetList.Elements.Add(key);
		}
		btnOpenClose = GetChildById("btnOpenClose").GetChildByType<XUiC_SimpleButton>();
		btnOpenClose.OnPressed += BtnOpenClose_OnPressed;
		btnCancel = GetChildById("btnCancel").GetChildByType<XUiC_SimpleButton>();
		btnCancel.OnPressed += BtnCancel_OnPressed;
		btnOk = GetChildById("btnOk").GetChildByType<XUiC_SimpleButton>();
		btnOk.OnPressed += BtnOk_OnPressed;
	}

	public static void Open(LocalPlayerUI _playerUi, TileEntitySecureDoor _te, Vector3i _blockPos, World _world, int _cIdx)
	{
		XUiC_DoorEditor childByType = _playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_DoorEditor>();
		childByType.world = _world;
		ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
		childByType.chunk = (Chunk)chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), _blockPos.y, World.toChunkXZ(_blockPos.z));
		BlockEntityData blockEntity = childByType.chunk.GetBlockEntity(_blockPos);
		childByType.blockPos = _blockPos;
		childByType.initialColorIdx = blockEntity.blockValue.meta2;
		childByType.initialDamage = blockEntity.blockValue.damage;
		childByType.cbxColorPresetList.Value = ColorMappingData.Instance.NameFromID[blockEntity.blockValue.meta2];
		childByType.bAcceptChanges = false;
		_playerUi.windowManager.Open(ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxColorPresetList_OnValueChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		if (ColorMappingData.Instance.IDFromName.TryGetValue(_newValue, out var value) && ColorMappingData.Instance.ColorFromID.ContainsKey(value))
		{
			UpdateDoorColor(value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDowngrade_OnPressed(XUiController _sender, int _mouseButton)
	{
		UpdateDoorHealth(_upgrade: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnUpgrade_OnPressed(XUiController _sender, int _mouseButton)
	{
		UpdateDoorHealth(_upgrade: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDoorHealth(bool _upgrade)
	{
		BlockEntityData blockEntity = chunk.GetBlockEntity(blockPos);
		Block block = blockEntity.blockValue.Block;
		if (!(block.shape is BlockShapeModelEntity blockShapeModelEntity))
		{
			Log.Warning($"block {block} does not have shape field. Cannot change damage state.");
			return;
		}
		int num = (_upgrade ? ((int)blockShapeModelEntity.GetNextDamageStateUpHealth(blockEntity.blockValue)) : ((int)blockShapeModelEntity.GetNextDamageStateDownHealth(blockEntity.blockValue)));
		blockEntity.blockValue.damage = block.MaxDamage - num;
		blockShapeModelEntity.UpdateDamageState(blockEntity.blockValue, blockEntity.blockValue, blockEntity, bPlayEffects: false);
		UpdateDoorColor(blockEntity.blockValue.meta2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDoorColor(int _colorIdx)
	{
		chunk.GetBlockEntity(blockPos).blockValue.meta2 = (byte)_colorIdx;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetDoorDamage()
	{
		BlockEntityData blockEntity = chunk.GetBlockEntity(blockPos);
		Block block = blockEntity.blockValue.Block;
		if (!(block.shape is BlockShapeModelEntity blockShapeModelEntity))
		{
			Log.Warning($"block {block} does not have shape field. Cannot change damage state.");
			return;
		}
		blockEntity.blockValue.damage = initialDamage;
		blockShapeModelEntity.UpdateDamageState(blockEntity.blockValue, blockEntity.blockValue, blockEntity, bPlayEffects: false);
		UpdateDoorColor(blockEntity.blockValue.meta2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOpenClose_OnPressed(XUiController _sender, int _mouseButton)
	{
		BlockEntityData blockEntity = chunk.GetBlockEntity(blockPos);
		blockEntity.blockValue.Block.OnBlockActivated("close", world, 0, blockPos, blockEntity.blockValue, world.GetPrimaryPlayer());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOk_OnPressed(XUiController _sender, int _mouseButton)
	{
		bAcceptChanges = true;
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	public override void OnClose()
	{
		base.OnClose();
		if (bAcceptChanges)
		{
			BlockEntityData blockEntity = chunk.GetBlockEntity(blockPos);
			world.SetBlockRPC(blockPos, blockEntity.blockValue);
		}
		else
		{
			ResetDoorDamage();
			UpdateDoorColor(initialColorIdx);
		}
	}
}
