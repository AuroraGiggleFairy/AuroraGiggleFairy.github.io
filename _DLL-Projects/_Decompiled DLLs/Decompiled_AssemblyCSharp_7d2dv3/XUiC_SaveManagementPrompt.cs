using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SaveManagementPrompt : XUiController
{
	public enum SaveManagementMode
	{
		Copy,
		Manage
	}

	public delegate void SaveManagementActionCopy(UserDataManagement.Result _result, string _targetSaveName, string _worldName, UserDataStorageType _expectedWorldStorage);

	public delegate void SaveManagementActionMove(UserDataManagement.Result _result, string _targetSaveName, string _worldName, UserDataStorageType _expectedWorldStorage);

	public delegate void SaveManagementActionApply();

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[XuiBindComponent("btnConfirm", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnConfirm;

	[XuiBindComponent("txtSaveName", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtSaveName;

	[XuiBindComponent("cbxStorageType", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<UserDataStorageType> cbxStorageType;

	[XuiBindComponent("cbxIsArchived", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxBool cbxIsArchived;

	[XuiBindComponent("cbxSaveDataLimit", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<SaveDataLimitType> cbxSaveDataLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar dataManagementBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveManagementMode mode;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRemoteSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMove;

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveInfoProvider.SaveEntryInfo saveInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveInfoProvider.WorldEntryInfo worldInfo;

	[XuiXmlBinding("showStorageType")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool ShowStorageType
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlBinding("canChangeStorageType")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool CanChangeStorageType
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlBinding("applyValid")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool ActionIsValid
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlBinding("canCopy")]
	public bool CanCopy
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !isRemoteSave;
		}
	}

	[XuiXmlBinding("showNameInput")]
	public bool ShowNameInput
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !isRemoteSave;
		}
	}

	[XuiXmlBinding("showSaveDataLimit")]
	public bool ShowSaveDataLimit
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return worldInfo?.WorldSize.HasValue ?? false;
		}
	}

	[XuiXmlBinding("showIsArchived")]
	public bool ShowIsArchived
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return PlatformManager.MultiPlatform.UserDataRoaming.IsSupported;
		}
	}

	[XuiXmlBinding("canChangeIsArchived")]
	public bool CanChangeIsArchived
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cbxStorageType?.Value.UsesDataLimit() ?? false;
		}
	}

	[XuiXmlBinding("confirmTextKey")]
	public string ConfirmTextKey
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return mode switch
			{
				SaveManagementMode.Manage => isMove ? "xuiDmMoveSave" : "btnApply", 
				SaveManagementMode.Copy => "xuiDmCopySave", 
				_ => throw new Exception("Unhandled SaveManagementMode"), 
			};
		}
	}

	[XuiXmlBinding("titleTextKey")]
	public string TitleTextKey
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return mode switch
			{
				SaveManagementMode.Manage => "xuiDmManageSave", 
				SaveManagementMode.Copy => "xuiDmCopySave", 
				_ => throw new Exception("Unhandled SaveManagementMode"), 
			};
		}
	}

	public event SaveManagementActionCopy OnCopyConfirmed;

	public event SaveManagementActionMove OnMoveConfirmed;

	public event SaveManagementActionApply OnApplyConfirmed;

	public override void Init()
	{
		base.Init();
		txtSaveName.UIInput.onValidate = GameUtils.ValidateGameNameInput;
	}

	public override void OnClose()
	{
		base.OnClose();
		if (dataManagementBar != null)
		{
			dataManagementBar.SetDeleteHovered(_hovered: false);
			dataManagementBar.SetArchivePreviewRegion(XUiC_DataManagementBar.BarRegion.None);
			dataManagementBar.SetPendingBytes(0L);
			dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cleanLinks()
	{
		saveInfo = null;
		worldInfo = null;
		dataManagementBar = null;
		this.OnCopyConfirmed = null;
		this.OnMoveConfirmed = null;
		this.OnApplyConfirmed = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (XUiUtils.HotkeysAllowedFor(viewComponent) && xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
		{
			ThreadManager.RunTaskAfterFrames(Cancel);
		}
		handleDirtyUpdateDefault();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void configureSaveDataLimitAllowedValues(UserDataStorageType _storageType)
	{
		SaveDataLimitType value = cbxSaveDataLimit.Value;
		cbxSaveDataLimit.Min = null;
		if (_storageType.UsesDataLimit())
		{
			if (value == SaveDataLimitType.Unlimited)
			{
				cbxSaveDataLimit.Value = SaveDataLimitType.VeryLong;
			}
			cbxSaveDataLimit.Min = SaveDataLimitType.Short;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validateSizeLimit(out long _pendingSaveBytes)
	{
		if (worldInfo.WorldSize.HasValue)
		{
			SaveDataLimitType value = cbxSaveDataLimit.Value;
			if (value == SaveDataLimitType.Unlimited)
			{
				_pendingSaveBytes = saveInfo.SizeInfo.BytesOnDisk;
				return true;
			}
			long num = value.CalculateTotalSize(worldInfo.WorldSize.Value);
			if (saveInfo.SizeInfo.BytesOnDisk > num)
			{
				_pendingSaveBytes = saveInfo.SizeInfo.BytesOnDisk;
				return false;
			}
			_pendingSaveBytes = num;
			return true;
		}
		_pendingSaveBytes = Math.Max(saveInfo.SizeInfo.BytesOnDisk, saveInfo.SizeInfo.BytesReserved);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void validate()
	{
		long _pendingSaveBytes;
		bool flag = validateSizeLimit(out _pendingSaveBytes);
		long num = (cbxIsArchived.Value ? saveInfo.SizeInfo.BytesOnDisk : _pendingSaveBytes);
		if (worldInfo.ShouldBeMovedWithSave(cbxStorageType.Value))
		{
			num += worldInfo.WorldDataSize;
		}
		bool flag2 = !cbxStorageType.Value.UsesDataLimit() || num <= SaveInfoProvider.Instance.TotalAvailableBytes;
		cbxSaveDataLimit.TextColor = (flag ? Color.white : Color.red);
		bool num2 = isRemoteSave || validateNameAvailable(txtSaveName.Text, cbxStorageType.Value);
		bool flag3 = !isRemoteSave && !string.Equals(txtSaveName.Text, saveInfo.Name, StringComparison.Ordinal);
		bool flag4 = cbxStorageType.Value != saveInfo.StorageType;
		isMove = flag3 || flag4;
		bool flag5 = num2 || (mode == SaveManagementMode.Manage && !isMove);
		txtSaveName.ActiveTextColor = (flag5 ? Color.white : Color.red);
		ActionIsValid = flag && flag2 && flag5;
		updateDataBarPreview(num, flag4);
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDataBarPreview(long _pendingBytes, bool _storageChanged)
	{
		if (dataManagementBar == null)
		{
			return;
		}
		bool flag = cbxStorageType.Value.UsesDataLimit();
		bool flag2 = saveInfo.StorageType.UsesDataLimit();
		if (flag && (mode == SaveManagementMode.Copy || !flag2))
		{
			dataManagementBar.SetDeleteHovered(_hovered: false);
			dataManagementBar.SetArchivePreviewRegion(XUiC_DataManagementBar.BarRegion.None);
			dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
			dataManagementBar.SetPendingBytes(_pendingBytes);
		}
		else if (mode == SaveManagementMode.Manage && flag2 && _storageChanged && !flag)
		{
			dataManagementBar.SetPendingBytes(0L);
			dataManagementBar.SetArchivePreviewRegion(XUiC_DataManagementBar.BarRegion.None);
			dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
			dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Secondary);
			dataManagementBar.SetDeleteHovered(_hovered: true);
		}
		else if (flag && flag2 && !_storageChanged)
		{
			long num = _pendingBytes - saveInfo.SizeInfo.ReportedSize;
			dataManagementBar.SetDeleteHovered(_hovered: false);
			if (num < 0)
			{
				long offset = saveInfo.BarStartOffset + _pendingBytes;
				XUiC_DataManagementBar.BarRegion archivePreviewRegion = new XUiC_DataManagementBar.BarRegion(offset, -num);
				dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
				dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Secondary);
				dataManagementBar.SetArchivePreviewRegion(archivePreviewRegion);
			}
			else if (num > 0)
			{
				dataManagementBar.SetArchivePreviewRegion(XUiC_DataManagementBar.BarRegion.None);
				dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
				dataManagementBar.SetPendingBytes(num);
			}
			else
			{
				dataManagementBar.SetArchivePreviewRegion(XUiC_DataManagementBar.BarRegion.None);
				dataManagementBar.SetPendingBytes(0L);
				dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
			}
		}
		else
		{
			dataManagementBar.SetDeleteHovered(_hovered: false);
			dataManagementBar.SetArchivePreviewRegion(XUiC_DataManagementBar.BarRegion.None);
			dataManagementBar.SetPendingBytes(0L);
			dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validateNameAvailable(string _name, UserDataStorageType _storageType)
	{
		if (string.IsNullOrEmpty(_name))
		{
			return false;
		}
		return !SdDirectory.Exists(GameIO.GetSaveGameDir(worldInfo.Name, _name, _storageType));
	}

	[XuiBindEvent("OnChangeHandler", "txtSaveName")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtSaveName_OnChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		validate();
	}

	[XuiBindEvent("OnValueChanged", "cbxStorageType")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxStorageType_OnValueChanged(XUiController _sender, UserDataStorageType _oldValue, UserDataStorageType _newValue)
	{
		configureSaveDataLimitAllowedValues(_newValue);
		cbxIsArchived.Value = _newValue.UsesDataLimit() && cbxIsArchived.Value;
		validate();
	}

	[XuiBindEvent("OnValueChanged", "cbxIsArchived")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxIsArchived_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		validate();
	}

	[XuiBindEvent("OnValueChanged", "cbxSaveDataLimit")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxSaveDataLimit_OnValueChanged(XUiController _sender, SaveDataLimitType _oldValue, SaveDataLimitType _newValue)
	{
		validate();
	}

	public void Cancel()
	{
		xui.playerUI.windowManager.Close(windowGroup);
		cleanLinks();
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		Cancel();
	}

	[XuiBindEvent("OnPress", "btnConfirm")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirm_OnPressed(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.windowManager.Close(windowGroup);
		bool value = cbxIsArchived.Value;
		long reservedBytes = (worldInfo.WorldSize.HasValue ? cbxSaveDataLimit.Value.CalculateTotalSize(worldInfo.WorldSize.Value) : saveInfo.SizeInfo.BytesReserved);
		switch (mode)
		{
		case SaveManagementMode.Copy:
			performCopy(txtSaveName.Text, cbxStorageType.Value, value, reservedBytes);
			break;
		case SaveManagementMode.Manage:
			if (isMove)
			{
				performMove(txtSaveName.Text, cbxStorageType.Value, value, reservedBytes);
			}
			else
			{
				performApply(value, reservedBytes);
			}
			break;
		}
		cleanLinks();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void performCopy(string _targetName, UserDataStorageType _storageType, bool _isArchived, long _reservedBytes)
	{
		UserDataManagement.GameSaveCopy gameSaveCopy;
		try
		{
			gameSaveCopy = new UserDataManagement.GameSaveCopy(saveInfo, _targetName, _storageType);
		}
		catch (Exception ex)
		{
			Log.Error("Failed to create save copy operation: " + ex.Message);
			return;
		}
		UserDataManagement.SaveCopyResultInfo saveCopyResultInfo = gameSaveCopy.PerformCopy();
		if (saveCopyResultInfo.Result == UserDataManagement.Result.Success)
		{
			UserDataManagement.SetSaveArchived(saveCopyResultInfo.NewSaveDir, _isArchived);
			if (!isRemoteSave)
			{
				UserDataManagement.SetSaveDataLimit(saveCopyResultInfo.NewSaveDir, _reservedBytes);
			}
		}
		UserDataStorageType expectedWorldStorage = (gameSaveCopy.WorldRequiresMoving ? _storageType : worldInfo.Location.StorageType);
		this.OnCopyConfirmed?.Invoke(saveCopyResultInfo.Result, _targetName, worldInfo.Name, expectedWorldStorage);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void performMove(string _targetName, UserDataStorageType _storageType, bool _isArchived, long _reservedBytes)
	{
		UserDataManagement.GameSaveMove gameSaveMove;
		try
		{
			gameSaveMove = new UserDataManagement.GameSaveMove(saveInfo, _targetName, _storageType);
		}
		catch (Exception ex)
		{
			Log.Error("Failed to create save move operation: " + ex.Message);
			return;
		}
		UserDataManagement.SaveMoveResultInfo saveMoveResultInfo = gameSaveMove.PerformMove();
		if (saveMoveResultInfo.Result == UserDataManagement.Result.Success)
		{
			UserDataManagement.SetSaveArchived(saveMoveResultInfo.NewSaveDir, _isArchived);
			if (!isRemoteSave)
			{
				UserDataManagement.SetSaveDataLimit(saveMoveResultInfo.NewSaveDir, _reservedBytes);
			}
		}
		UserDataStorageType expectedWorldStorage = (gameSaveMove.WorldRequiresMoving ? _storageType : worldInfo.Location.StorageType);
		this.OnMoveConfirmed?.Invoke(saveMoveResultInfo.Result, _targetName, worldInfo.Name, expectedWorldStorage);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void performApply(bool _isArchived, long _reservedBytes)
	{
		UserDataManagement.SetSaveArchived(saveInfo.SaveDir, _isArchived);
		if (!isRemoteSave)
		{
			UserDataManagement.SetSaveDataLimit(saveInfo.SaveDir, _reservedBytes);
		}
		SaveInfoProvider.Instance.SetDirty();
		this.OnApplyConfirmed?.Invoke();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setValues(SaveManagementMode _mode, SaveInfoProvider.SaveEntryInfo _saveInfo, SaveInfoProvider.WorldEntryInfo _worldInfo)
	{
		mode = _mode;
		saveInfo = _saveInfo;
		worldInfo = _worldInfo;
		isRemoteSave = _worldInfo.Type == SaveInfoProvider.RemoteWorldsType;
		ShowStorageType = PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional;
		CanChangeStorageType = true;
		txtSaveName.Text = ((_mode == SaveManagementMode.Copy) ? (_saveInfo.Name + " - Copy") : (_saveInfo.Name ?? ""));
		cbxIsArchived.Value = _saveInfo.StorageType.UsesDataLimit() && _saveInfo.SizeInfo.IsArchived;
		cbxSaveDataLimit.Min = null;
		if (_worldInfo.WorldSize.HasValue)
		{
			SaveDataLimitType saveDataLimitType;
			if (!_saveInfo.StorageType.UsesDataLimit() && _saveInfo.SizeInfo.BytesReserved <= 0)
			{
				cbxSaveDataLimit.Value = SaveDataLimitType.Unlimited;
			}
			else if (SaveDataLimitExtensions.TryCalculateSaveDataLimitType(_saveInfo.SizeInfo.BytesReserved, _worldInfo.WorldSize.Value, out saveDataLimitType))
			{
				cbxSaveDataLimit.Value = saveDataLimitType;
			}
			else
			{
				cbxSaveDataLimit.Value = SaveDataLimitType.VeryLong;
			}
			cbxStorageType.Value = _saveInfo.StorageType;
		}
		else if (_saveInfo.SizeInfo.BytesReserved > 0)
		{
			cbxStorageType.Value = _saveInfo.StorageType;
			CanChangeStorageType = true;
		}
		else
		{
			cbxStorageType.Value = UserDataStorageType.DeviceLocal;
			CanChangeStorageType = false;
		}
		configureSaveDataLimitAllowedValues(cbxStorageType.Value);
		validate();
	}

	public static void Show(XUi _xui, XUiC_DataManagementBar _dataManagementBar, SaveManagementMode _mode, SaveInfoProvider.SaveEntryInfo _saveInfo, SaveInfoProvider.WorldEntryInfo _worldInfo, SaveManagementActionCopy _onCopied, SaveManagementActionMove _onMoved, SaveManagementActionApply _onApplied)
	{
		XUiC_SaveManagementPrompt childByType = _xui.GetChildByType<XUiC_SaveManagementPrompt>();
		childByType.dataManagementBar = _dataManagementBar;
		childByType.OnCopyConfirmed = _onCopied;
		childByType.OnMoveConfirmed = _onMoved;
		childByType.OnApplyConfirmed = _onApplied;
		childByType.setValues(_mode, _saveInfo, _worldInfo);
		_xui.playerUI.windowManager.Open(childByType.windowGroup, _bModal: false);
	}
}
