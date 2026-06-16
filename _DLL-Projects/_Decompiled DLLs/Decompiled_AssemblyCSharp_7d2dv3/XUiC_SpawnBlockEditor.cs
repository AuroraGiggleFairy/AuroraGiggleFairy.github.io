using UnityEngine.Scripting;

[Preserve]
public class XUiC_SpawnBlockEditor : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_StringList list;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		list = GetChildByType<XUiC_StringList>();
		if (GetChildById("btnOk") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += BtnOk_OnPressed;
		}
		if (GetChildById("btnCancel") is XUiC_SimpleButton xUiC_SimpleButton2)
		{
			xUiC_SimpleButton2.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				xui.playerUI.windowManager.Close(base.WindowGroup);
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOk_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_StringList.Entry selectedEntryData = list.SelectedEntryData;
		if (selectedEntryData != null)
		{
			BlockValue block = GameManager.Instance.World.GetBlock(blockPos);
			block.meta = (byte)selectedEntryData.Tag;
			GameManager.Instance.World.SetBlockRPC(blockPos, block);
			xui.playerUI.windowManager.Close(base.WindowGroup);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setBlock(Vector3i _blockPos, BlockValue _blockValue, BlockSpawnEntity _block)
	{
		string[] spawnClasses = _block.spawnClasses;
		blockPos = _blockPos;
		list.ClearList();
		for (byte b = 0; b < spawnClasses.Length; b++)
		{
			list.AddEntry(spawnClasses[b], b);
		}
		list.SortList();
		list.RebuildList();
		byte meta = _blockValue.meta;
		if (meta < spawnClasses.Length)
		{
			list.SelectByString(spawnClasses[meta]);
		}
	}

	public static void Open(LocalPlayerUI _playerUi, Vector3i _blockPos, BlockValue _blockValue, BlockSpawnEntity _block)
	{
		XUiC_SpawnBlockEditor childByType = _playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_SpawnBlockEditor>();
		_playerUi.windowManager.Open(ID, _bModal: true);
		childByType.setBlock(_blockPos, _blockValue, _block);
	}
}
