using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CreatePoi : XUiController
{
	[Preserve]
	public class PoiSizeInfo
	{
		public readonly int X;

		public readonly int Z;

		public readonly bool IsCustom;

		public readonly string Label;

		public readonly string DefaultFolder;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly Regex infoMatcher = new Regex("^(\\d+)x(\\d+)(?:\\/([^/]*))(?:\\/([^/]*))$", RegexOptions.Compiled | RegexOptions.Singleline);

		public Vector3i Size => new Vector3i(X, 1, Z);

		public PoiSizeInfo(int _x, int _z, string _label, string _defaultFolder, bool _isCustom = false)
		{
			X = _x;
			Z = _z;
			Label = _label;
			DefaultFolder = _defaultFolder;
			IsCustom = _isCustom;
		}

		public PoiSizeInfo(string _input)
		{
			if (_input.StartsWith("custom", StringComparison.OrdinalIgnoreCase))
			{
				IsCustom = true;
				int num = _input.IndexOf('/');
				if (num > 0)
				{
					DefaultFolder = _input.Substring(num + 1);
				}
				return;
			}
			Match match = infoMatcher.Match(_input);
			if (!match.Success)
			{
				throw new FormatException("PoiSizeInfo: Input ('" + _input + "') in invalid form. Needs to be \"<width>x<height>\" or \"<width>x<height>/<label>\" or \"Custom\".");
			}
			if (!StringParsers.TryParseSInt32(match.Groups[1].Value, out X))
			{
				throw new ArgumentException("PoiSizeInfo: Input ('" + _input + "') in invalid form, first part ('" + match.Groups[1].Value + "') is not a valid integer. Needs to be \"<width>x<height>\" or \"<width>x<height>/<label>\" or \"Custom\".");
			}
			if (!StringParsers.TryParseSInt32(match.Groups[2].Value, out Z))
			{
				throw new ArgumentException("PoiSizeInfo: Input ('" + _input + "') in invalid form, first part ('" + match.Groups[2].Value + "') is not a valid integer. Needs to be \"<width>x<height>\" or \"<width>x<height>/<label>\" or \"Custom\".");
			}
			if (match.Groups[3].Success)
			{
				Label = match.Groups[3].Value;
			}
			if (match.Groups[4].Success)
			{
				DefaultFolder = match.Groups[4].Value;
			}
		}

		public override string ToString()
		{
			if (!IsCustom)
			{
				return $"{X}x{Z} ({Label})";
			}
			return Localization.Get("xuiCustom");
		}
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<PoiSizeInfo> cmbSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSizeX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSizeZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtDepth;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<PathAbstractions.EAbstractedLocationType> cmbLocation;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<Mod> cmbMod;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabFolderList folderList;

	[PublicizedFrom(EAccessModifier.Private)]
	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public int customSizeX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int customSizeZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public int depth = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validName;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool nameExistsInLocation;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validFolder;

	public bool IsCustomSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cmbSize?.Value?.IsCustom == true;
		}
	}

	public Vector3i SelectedSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!IsCustomSize)
			{
				return new Vector3i(cmbSize.Value.Size.x, depth, cmbSize.Value.Size.z);
			}
			return new Vector3i(customSizeX, depth, customSizeZ);
		}
	}

	public PathAbstractions.EAbstractedLocationType SelectedLocation
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cmbLocation?.Value ?? PathAbstractions.EAbstractedLocationType.UserDataPath;
		}
	}

	public bool ValidCustomSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (customSizeX > 0 && customSizeX <= 300 && customSizeZ > 0)
			{
				return customSizeZ <= 300;
			}
			return false;
		}
	}

	public bool ValidSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (IsCustomSize)
			{
				return ValidCustomSize;
			}
			return true;
		}
	}

	public bool ValidDepth
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (depth > 0)
			{
				return depth <= 30;
			}
			return false;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		if (GetChildById("txtName") is XUiC_TextInput xUiC_TextInput)
		{
			txtName = xUiC_TextInput;
			txtName.OnChangeHandler += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, string _text, bool _) =>
			{
				name = _text;
				validateName();
			};
		}
		if (GetChildById("cmbSize") is XUiC_ComboBoxList<PoiSizeInfo> xUiC_ComboBoxList)
		{
			cmbSize = xUiC_ComboBoxList;
			cmbSize.OnValueChanged += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, PoiSizeInfo _, PoiSizeInfo _newSize) =>
			{
				autoSelectFolder(_newSize);
				IsDirty = true;
			};
		}
		if (GetChildById("txtSizeX") is XUiC_TextInput xUiC_TextInput2)
		{
			txtSizeX = xUiC_TextInput2;
			txtSizeX.OnChangeHandler += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, string _text, bool _) =>
			{
				sizeTextChanged(_text, _isZ: false);
			};
		}
		if (GetChildById("txtSizeZ") is XUiC_TextInput xUiC_TextInput3)
		{
			txtSizeZ = xUiC_TextInput3;
			txtSizeZ.OnChangeHandler += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, string _text, bool _) =>
			{
				sizeTextChanged(_text, _isZ: true);
			};
		}
		if (GetChildById("txtDepth") is XUiC_TextInput xUiC_TextInput4)
		{
			txtDepth = xUiC_TextInput4;
			txtDepth.OnChangeHandler += depthTextChanged;
		}
		if (GetChildById("cmbLocation") is XUiC_ComboBoxEnum<PathAbstractions.EAbstractedLocationType> xUiC_ComboBoxEnum)
		{
			cmbLocation = xUiC_ComboBoxEnum;
			cmbLocation.Elements.Remove(PathAbstractions.EAbstractedLocationType.GameData);
			cmbLocation.Value = PathAbstractions.EAbstractedLocationType.UserDataPath;
			cmbLocation.TriggerValueChangedEvent(PathAbstractions.EAbstractedLocationType.GameData);
			cmbLocation.OnValueChanged += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, PathAbstractions.EAbstractedLocationType _, PathAbstractions.EAbstractedLocationType _) =>
			{
				updateFolders();
				IsDirty = true;
			};
		}
		if (GetChildById("cmbMod") is XUiC_ComboBoxList<Mod> xUiC_ComboBoxList2)
		{
			cmbMod = xUiC_ComboBoxList2;
			cmbMod.OnValueChanged += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, Mod _, Mod _) =>
			{
				updateFolders();
				IsDirty = true;
			};
		}
		if (GetChildById("folders") is XUiC_PrefabFolderList xUiC_PrefabFolderList)
		{
			folderList = xUiC_PrefabFolderList;
			folderList.SelectionChanged += [PublicizedFrom(EAccessModifier.Private)] (XUiC_ListEntry<XUiC_PrefabFolderList.PrefabFolderEntry> _, XUiC_ListEntry<XUiC_PrefabFolderList.PrefabFolderEntry> _) =>
			{
				validateName();
			};
			folderList.SelectedEntryIndex = 0;
			validateName();
		}
		if (GetChildById("btnCancel") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += BtnBack_OnPressed;
		}
		if (GetChildById("btnOk") is XUiC_SimpleButton xUiC_SimpleButton2)
		{
			xUiC_SimpleButton2.OnPressed += BtnOk_OnPressed;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (txtDepth != null)
		{
			depthTextChanged(txtDepth, txtDepth.Text, _changeFromCode: false);
		}
		updateMods();
		updateFolders();
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sizeTextChanged(string _text, bool _isZ)
	{
		StringParsers.TryParseSInt32(_text, out var _result);
		if (_isZ)
		{
			customSizeZ = _result;
		}
		else
		{
			customSizeX = _result;
		}
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void depthTextChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		StringParsers.TryParseSInt32(_text, out depth);
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void autoSelectFolder(PoiSizeInfo _newValue)
	{
		if (folderList != null && !string.IsNullOrEmpty(_newValue.DefaultFolder))
		{
			folderList.SelectByName(_newValue.DefaultFolder);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void validateName()
	{
		bool flag = name != null && name.Length > 0 && !name.Contains(" ") && GameUtils.ValidateGameName(name);
		nameExistsInLocation = false;
		if (flag)
		{
			Regex nameMatch = new Regex("^" + name + "$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
			List<PathAbstractions.AbstractedLocation> availablePathsList = PathAbstractions.PrefabsSearchPaths.GetAvailablePathsList(nameMatch, null, null, _ignoreDuplicateNames: false, SelectedLocation);
			if (SelectedLocation == PathAbstractions.EAbstractedLocationType.Mods)
			{
				foreach (PathAbstractions.AbstractedLocation item in availablePathsList)
				{
					if (item.ContainingMod == cmbMod.Value)
					{
						nameExistsInLocation = true;
						break;
					}
				}
			}
			else
			{
				nameExistsInLocation = availablePathsList.Count > 0;
			}
		}
		validName = flag && !nameExistsInLocation;
		validFolder = !string.IsNullOrEmpty(folderList.SelectedEntry?.GetEntry()?.AbsolutePath);
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateMods()
	{
		List<Mod> loadedMods = ModManager.GetLoadedMods();
		if (cmbLocation != null)
		{
			if (loadedMods.Count == 0)
			{
				cmbLocation.Elements.Remove(PathAbstractions.EAbstractedLocationType.Mods);
			}
			else if (!cmbLocation.Elements.Contains(PathAbstractions.EAbstractedLocationType.Mods))
			{
				cmbLocation.Elements.Add(PathAbstractions.EAbstractedLocationType.Mods);
			}
		}
		if (cmbMod == null)
		{
			return;
		}
		cmbMod.Elements.Clear();
		loadedMods.Sort([PublicizedFrom(EAccessModifier.Internal)] (Mod _a, Mod _b) => string.Compare(_a.DisplayName, _b.DisplayName, StringComparison.OrdinalIgnoreCase));
		foreach (Mod item in loadedMods)
		{
			cmbMod.Elements.Add(item);
		}
		if (loadedMods.Count > 0)
		{
			cmbMod.SelectedIndex = 0;
			cmbMod.TriggerValueChangedEvent(null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateFolders()
	{
		if (folderList != null)
		{
			folderList.Mod = cmbMod?.Value;
			folderList.LocationType = cmbLocation?.Value ?? PathAbstractions.EAbstractedLocationType.UserDataPath;
		}
		cmbSize?.TriggerValueChangedEvent(null);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings(_forceAll: true);
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_InGameMenuWindow.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOk_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		if (XUiC_LevelToolsHelpers.IsShowImposter())
		{
			XUiC_LevelToolsHelpers.SetShowImposter();
		}
		PrefabEditModeManager.Instance.NewVoxelPrefab();
		Vector3i vector3i = -1 * (SelectedSize / 2);
		vector3i.y = 1;
		Vector3i pos = vector3i + SelectedSize - Vector3i.one;
		BlockValue blockValue = Block.GetBlockValue(Constants.cTerrainFillerBlockName);
		Block block = blockValue.Block;
		BlockPlacement.Result _bpResult = new BlockPlacement.Result(0, Vector3.zero, Vector3i.zero, blockValue);
		block.OnBlockPlaceBefore(GameManager.Instance.World, ref _bpResult, base.xui.playerUI.entityPlayer, GameManager.Instance.World.GetGameRandom());
		blockValue = _bpResult.blockValue;
		blockValue.rotation = blockValue.rotation;
		BlockTools.CubeRPC(GameManager.Instance, 0, vector3i, pos, blockValue, blockValue.Block.shape.IsTerrain() ? MarchingCubes.DensityTerrain : MarchingCubes.DensityAir, 0, TextureFullArray.Default);
		PrefabEditModeManager.Instance.SetGroundLevel(-SelectedSize.y);
		string subFolder = folderList.SelectedEntry?.GetEntry()?.RelativePath;
		PathAbstractions.AbstractedLocation? abstractedLocation = PathAbstractions.PrefabsSearchPaths.BuildLocation(SelectedLocation, subFolder, name, cmbMod?.Value);
		PrefabEditModeManager.Instance.VoxelPrefab.location = abstractedLocation.Value;
		if (PrefabEditModeManager.Instance.SaveVoxelPrefab())
		{
			GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, string.Format(Localization.Get("xuiPrefabsPrefabSaved"), PrefabEditModeManager.Instance.LoadedPrefab.Name));
		}
		else
		{
			GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, Localization.Get("xuiPrefabsPrefabSavingError"));
		}
		base.xui.playerUI.windowManager.Open(XUiC_InGameMenuWindow.ID, _bModal: true);
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		return base.ParseAttribute(_name, _value, _parent);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "size_custom":
			_value = IsCustomSize.ToString();
			return true;
		case "location":
			_value = SelectedLocation.ToStringCached();
			return true;
		case "valid_name":
			_value = validName.ToString();
			return true;
		case "name_exists":
			_value = nameExistsInLocation.ToString();
			return true;
		case "valid_custom_size":
			_value = ValidCustomSize.ToString();
			return true;
		case "valid_size":
			_value = ValidSize.ToString();
			return true;
		case "valid_depth":
			_value = ValidDepth.ToString();
			return true;
		case "valid_folder":
			_value = validFolder.ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}
}
