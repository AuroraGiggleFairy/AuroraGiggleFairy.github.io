using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabList : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabGroupList groupList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabFileList fileList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture prefabPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label noPreviewLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnLoad;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnProperties;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnLoadIntoPrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApplyLoadedPrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnWorldPlacePrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnWorldReplacePrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnWorldDeletePrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnWorldApplyPrefabChanges;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnWorldRevertPrefabChanges;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		prefabPreview = (XUiV_Texture)GetChildById("prefabPreview").ViewComponent;
		noPreviewLabel = (XUiV_Label)GetChildById("noPreview").ViewComponent;
		noPreviewLabel.IsVisible = false;
		groupList = GetChildById("groups") as XUiC_PrefabGroupList;
		fileList = GetChildById("files") as XUiC_PrefabFileList;
		btnLoad = GetChildById("btnLoad") as XUiC_SimpleButton;
		btnProperties = GetChildById("btnProperties") as XUiC_SimpleButton;
		btnSave = GetChildById("btnSave") as XUiC_SimpleButton;
		btnLoadIntoPrefab = GetChildById("btnLoadIntoPrefab") as XUiC_SimpleButton;
		btnApplyLoadedPrefab = GetChildById("btnApplyLoadedPrefab") as XUiC_SimpleButton;
		groupList.SelectionChanged += GroupListSelectionChanged;
		fileList.SelectionChanged += FileList_SelectionChanged;
		fileList.OnEntryDoubleClicked += FileList_OnEntryDoubleClicked;
		fileList.PageNumberChanged += FileListOnPageNumberChanged;
		btnLoad.OnPressed += BtnLoad_OnPressed;
		btnProperties.OnPressed += BtnPropertiesOnOnPressed;
		btnSave.OnPressed += BtnSave_OnPressed;
		btnLoadIntoPrefab.OnPressed += BtnLoadIntoPrefabOnOnPressed;
		btnApplyLoadedPrefab.OnPressed += BtnApplyLoadedPrefabOnOnPressed;
		if (GetChildById("btnCleanOtherPrefabs") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += BtnCleanOtherPrefabsOnOnPressed;
		}
		if (GetChildById("btnLoadIntoClipboard") is XUiC_SimpleButton xUiC_SimpleButton2)
		{
			xUiC_SimpleButton2.OnPressed += BtnLoadIntoClipboardOnOnPressed;
		}
		if (GetChildById("btnNew") is XUiC_SimpleButton xUiC_SimpleButton3)
		{
			xUiC_SimpleButton3.OnPressed += BtnNewOnOnPressed;
		}
		btnWorldPlacePrefab = GetChildById("btnWorldPlacePrefab") as XUiC_SimpleButton;
		if (btnWorldPlacePrefab != null)
		{
			btnWorldPlacePrefab.OnPressed += BtnWorldPlacePrefabOnPressed;
		}
		btnWorldReplacePrefab = GetChildById("btnWorldReplacePrefab") as XUiC_SimpleButton;
		if (btnWorldReplacePrefab != null)
		{
			btnWorldReplacePrefab.OnPressed += BtnWorldReplacePrefabOnPressed;
		}
		btnWorldDeletePrefab = GetChildById("btnWorldDeletePrefab") as XUiC_SimpleButton;
		if (btnWorldDeletePrefab != null)
		{
			btnWorldDeletePrefab.OnPressed += BtnWorldDeletePrefabOnPressed;
		}
		btnWorldApplyPrefabChanges = GetChildById("btnWorldApplyPrefabChanges") as XUiC_SimpleButton;
		if (btnWorldApplyPrefabChanges != null)
		{
			btnWorldApplyPrefabChanges.OnPressed += BtnWorldApplyPrefabChangesOnPressed;
		}
		btnWorldRevertPrefabChanges = GetChildById("btnWorldRevertPrefabChanges") as XUiC_SimpleButton;
		if (btnWorldRevertPrefabChanges != null)
		{
			btnWorldRevertPrefabChanges.OnPressed += BtnWorldRevertPrefabChangesOnPressed;
		}
		btnLoad.Enabled = false;
		btnProperties.Enabled = false;
		groupList.SelectedEntryIndex = 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GroupListSelectionChanged(XUiC_ListEntry<XUiC_PrefabGroupList.PrefabGroupEntry> _previousEntry, XUiC_ListEntry<XUiC_PrefabGroupList.PrefabGroupEntry> _newEntry)
	{
		string groupFilter = null;
		if (_newEntry != null)
		{
			groupFilter = _newEntry.GetEntry().filterString;
		}
		fileList.SetGroupFilter(groupFilter);
		if (fileList.EntryCount > 0)
		{
			fileList.SelectedEntryIndex = 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FileListOnPageNumberChanged(int _pageNumber)
	{
		fileList.SelectedEntryIndex = fileList.Page * fileList.PageLength;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FileList_SelectionChanged(XUiC_ListEntry<XUiC_PrefabFileList.PrefabFileEntry> _previousEntry, XUiC_ListEntry<XUiC_PrefabFileList.PrefabFileEntry> _newEntry)
	{
		btnLoad.Enabled = _newEntry != null;
		btnProperties.Enabled = _newEntry != null;
		updatePreview(_newEntry);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePreview(XUiC_ListEntry<XUiC_PrefabFileList.PrefabFileEntry> _newEntry)
	{
		if (prefabPreview.Texture != null)
		{
			Texture2D obj = (Texture2D)prefabPreview.Texture;
			prefabPreview.Texture = null;
			UnityEngine.Object.Destroy(obj);
		}
		if (_newEntry?.GetEntry() != null)
		{
			string path = _newEntry.GetEntry().location.FullPathNoExtension + ".jpg";
			if (SdFile.Exists(path))
			{
				Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGB24, mipChain: false);
				byte[] data = SdFile.ReadAllBytes(path);
				texture2D.LoadImage(data);
				prefabPreview.Texture = texture2D;
				noPreviewLabel.IsVisible = false;
				prefabPreview.IsVisible = true;
				return;
			}
		}
		noPreviewLabel.IsVisible = true;
		prefabPreview.IsVisible = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FileList_OnEntryDoubleClicked(XUiC_PrefabFileList.PrefabFileEntry _entry)
	{
		if (PrefabEditModeManager.Instance.IsActive())
		{
			BtnLoad_OnPressed(this, -1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLoad_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (fileList.SelectedEntry?.GetEntry() != null)
		{
			XUiC_SaveDirtyPrefab.Show(base.xui, loadPrefab);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadPrefab(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		base.xui.playerUI.windowManager.Open(XUiC_InGameMenuWindow.ID, _bModal: true);
		if (_action == XUiC_SaveDirtyPrefab.ESelectedAction.Cancel)
		{
			return;
		}
		PathAbstractions.AbstractedLocation location = fileList.SelectedEntry.GetEntry().location;
		bool flag = XUiC_LevelToolsHelpers.IsShowImposter();
		if (flag && PrefabEditModeManager.Instance.HasPrefabImposter(location))
		{
			PrefabEditModeManager.Instance.LoadImposterPrefab(location);
			return;
		}
		if (flag)
		{
			GameManager.ShowTooltip(GameManager.Instance.World.GetLocalPlayers()[0], string.Format(Localization.Get("xuiPrefabsPrefabHasNoImposter"), location.Name));
			XUiC_LevelToolsHelpers.SetShowImposter();
		}
		PrefabEditModeManager.Instance.LoadVoxelPrefab(location);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPropertiesOnOnPressed(XUiController _sender, int _mouseButton)
	{
		PathAbstractions.AbstractedLocation location = fileList.SelectedEntry.GetEntry().location;
		PathAbstractions.AbstractedLocation loadedPrefab = PrefabEditModeManager.Instance.LoadedPrefab;
		if (location == loadedPrefab)
		{
			XUiC_PrefabPropertiesEditor.Show(base.xui, XUiC_PrefabPropertiesEditor.EPropertiesFrom.LoadedPrefab, PathAbstractions.AbstractedLocation.None);
		}
		else
		{
			XUiC_PrefabPropertiesEditor.Show(base.xui, XUiC_PrefabPropertiesEditor.EPropertiesFrom.FileBrowserSelection, location);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_SaveDirtyPrefab.Show(base.xui, savePrefab, XUiC_SaveDirtyPrefab.EMode.ForceSave);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void savePrefab(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		base.xui.playerUI.windowManager.Open(XUiC_InGameMenuWindow.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLoadIntoPrefabOnOnPressed(XUiController _sender, int _mouseButton)
	{
		PathAbstractions.AbstractedLocation location = fileList.SelectedEntry.GetEntry().location;
		BlockToolSelection blockToolSelection = GameManager.Instance.GetActiveBlockTool() as BlockToolSelection;
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		Prefab prefab = new Prefab();
		prefab.Load(location, _applyMapping: true, _fixChildblocks: true, _allowMissingBlocks: true);
		dynamicPrefabDecorator.CreateNewPrefabAndActivate(location, blockToolSelection.SelectionStart, prefab);
		base.xui.playerUI.windowManager.Close(XUiC_InGameMenuWindow.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnApplyLoadedPrefabOnOnPressed(XUiController _sender, int _mouseButton)
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		PrefabInstance obj = ((dynamicPrefabDecorator?.ActivePrefab != null) ? dynamicPrefabDecorator.ActivePrefab : null);
		obj.CleanFromWorld(GameManager.Instance.World, _bRemoveEntities: true);
		obj.CopyIntoWorld(GameManager.Instance.World, _CopyEntities: true, _bOverwriteExistingBlocks: false, FastTags<TagGroup.Global>.none);
		GameManager.Instance.World.m_ChunkManager.RemoveAllChunksOnAllClients();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCleanOtherPrefabsOnOnPressed(XUiController _sender, int _mouseButton)
	{
		throw new NotImplementedException();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLoadIntoClipboardOnOnPressed(XUiController _sender, int _mouseButton)
	{
		PathAbstractions.AbstractedLocation location = fileList.SelectedEntry.GetEntry().location;
		BlockToolSelection obj = GameManager.Instance.GetActiveBlockTool() as BlockToolSelection;
		Prefab prefab = new Prefab();
		prefab.Load(location, _applyMapping: true, _fixChildblocks: true, _allowMissingBlocks: true);
		obj.LoadPrefabIntoClipboard(prefab);
		base.xui.playerUI.windowManager.Close(XUiC_InGameMenuWindow.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnNewOnOnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_SaveDirtyPrefab.Show(base.xui, newPrefab);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void newPrefab(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		if (_action == XUiC_SaveDirtyPrefab.ESelectedAction.Cancel)
		{
			base.xui.playerUI.windowManager.Open(XUiC_InGameMenuWindow.ID, _bModal: true);
		}
		else
		{
			base.xui.playerUI.windowManager.Open(XUiC_CreatePoi.ID, _bModal: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnWorldPlacePrefabOnPressed(XUiController _sender, int _mouseButton)
	{
		GameUtils.DirEightWay closestDirection = GameUtils.GetClosestDirection(base.xui.playerUI.entityPlayer.rotation.y, _limitTo90Degress: true);
		if (GameManager.Instance.GetActiveBlockTool() is BlockToolSelection blockToolSelection && GameManager.Instance.GetDynamicPrefabDecorator() != null)
		{
			PathAbstractions.AbstractedLocation location = fileList.SelectedEntry.GetEntry().location;
			Prefab prefab = new Prefab();
			prefab.Load(location);
			int num = MathUtils.Mod(closestDirection switch
			{
				GameUtils.DirEightWay.N => 2, 
				GameUtils.DirEightWay.E => 3, 
				GameUtils.DirEightWay.S => 0, 
				GameUtils.DirEightWay.W => 1, 
				_ => throw new ArgumentOutOfRangeException(), 
			} - prefab.rotationToFaceNorth, 4);
			Vector3i vector3i = closestDirection switch
			{
				GameUtils.DirEightWay.N => new Vector3i(-prefab.size.x / 2, 0, 0), 
				GameUtils.DirEightWay.E => new Vector3i(0, 0, -prefab.size.z / 2), 
				GameUtils.DirEightWay.S => new Vector3i(-prefab.size.x / 2, 0, 1 - prefab.size.z), 
				GameUtils.DirEightWay.W => new Vector3i(1 - prefab.size.x, 0, -prefab.size.z / 2), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			vector3i.y = prefab.yOffset;
			PrefabInstance prefabInstance = GameManager.Instance.GetDynamicPrefabDecorator().CreateNewPrefabAndActivate(prefab.location, blockToolSelection.SelectionStart + vector3i, prefab);
			while (num-- > 0)
			{
				prefabInstance.RotateAroundY();
			}
			prefabInstance.UpdateImposterView();
			base.xui.playerUI.windowManager.Close(XUiC_InGameMenuWindow.ID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnWorldReplacePrefabOnPressed(XUiController _sender, int _mouseButton)
	{
		if (GameManager.Instance.GetDynamicPrefabDecorator() != null)
		{
			PrefabInstance prefabInstance = GameManager.Instance.GetDynamicPrefabDecorator().RemoveActivePrefab(GameManager.Instance.World);
			PathAbstractions.AbstractedLocation location = fileList.SelectedEntry.GetEntry().location;
			Prefab prefab = new Prefab();
			prefab.Load(location);
			Vector3i position = prefabInstance.boundingBoxPosition + new Vector3i(0f, (float)prefab.yOffset - prefabInstance.yOffsetOfPrefab, 0f);
			PrefabInstance prefabInstance2 = GameManager.Instance.GetDynamicPrefabDecorator().CreateNewPrefabAndActivate(prefab.location, position, prefab);
			while (prefabInstance.rotation-- > 0)
			{
				prefabInstance2.RotateAroundY();
			}
			prefabInstance2.UpdateImposterView();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnWorldDeletePrefabOnPressed(XUiController _sender, int _mouseButton)
	{
		GameManager.Instance.GetDynamicPrefabDecorator().RemoveActivePrefab(GameManager.Instance.World);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnWorldApplyPrefabChangesOnPressed(XUiController _sender, int _mouseButton)
	{
		PrefabInstance prefabInstance = GameManager.Instance.GetDynamicPrefabDecorator()?.ActivePrefab;
		if (prefabInstance != null)
		{
			prefabInstance.CleanFromWorld(GameManager.Instance.World, _bRemoveEntities: true);
			prefabInstance.CopyIntoWorld(GameManager.Instance.World, _CopyEntities: true, _bOverwriteExistingBlocks: false, FastTags<TagGroup.Global>.none);
			prefabInstance.UpdateImposterView();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnWorldRevertPrefabChangesOnPressed(XUiController _sender, int _mouseButton)
	{
		PrefabInstance prefabInstance = GameManager.Instance.GetDynamicPrefabDecorator()?.ActivePrefab;
		if (prefabInstance != null)
		{
			prefabInstance.UpdateBoundingBoxPosAndScale(GameManager.Instance.GetDynamicPrefabDecorator().ActivePrefab.lastCopiedPrefabPosition, prefabInstance.prefab.size);
			prefabInstance.rotation = prefabInstance.lastCopiedRotation;
			prefabInstance.UpdateImposterView();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		PathAbstractions.AbstractedLocation location = PathAbstractions.AbstractedLocation.None;
		if (fileList.SelectedEntry != null)
		{
			location = fileList.SelectedEntry.GetEntry().location;
		}
		if (groupList.SelectedEntry != null)
		{
			string name = groupList.SelectedEntry.GetEntry().name;
			groupList.RebuildList();
			if (!groupList.SelectByName(name))
			{
				groupList.SelectedEntryIndex = 0;
			}
		}
		fileList.RebuildList();
		if (location.Type != PathAbstractions.EAbstractedLocationType.None)
		{
			fileList.SelectByLocation(location);
		}
		RefreshBindings();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		bool enabled = PrefabEditModeManager.Instance.VoxelPrefab != null;
		btnSave.Enabled = enabled;
		BlockToolSelection blockToolSelection = GameManager.Instance.GetActiveBlockTool() as BlockToolSelection;
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		PrefabInstance prefabInstance = dynamicPrefabDecorator?.ActivePrefab;
		btnLoadIntoPrefab.Enabled = fileList.SelectedEntry != null && blockToolSelection != null && blockToolSelection.SelectionActive && dynamicPrefabDecorator != null;
		btnApplyLoadedPrefab.Enabled = prefabInstance != null && !prefabInstance.IsBBInSyncWithPrefab();
		btnWorldPlacePrefab.Enabled = blockToolSelection != null && blockToolSelection.SelectionActive && fileList.SelectedEntry != null;
		btnWorldReplacePrefab.Enabled = prefabInstance != null && fileList.SelectedEntry != null;
		btnWorldDeletePrefab.Enabled = prefabInstance != null;
		btnWorldApplyPrefabChanges.Enabled = prefabInstance != null && !prefabInstance.IsBBInSyncWithPrefab();
		btnWorldRevertPrefabChanges.Enabled = prefabInstance != null && !prefabInstance.IsBBInSyncWithPrefab() && prefabInstance.bPrefabCopiedIntoWorld;
	}
}
