using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LightEditor : XUiController
{
	public struct LightValues
	{
		public LightType type;

		public LightShadows shadows;

		public float range;

		public float intensity;

		public Color color;

		public float spotAngle;

		public Color emissiveColor;

		public LightStateType stateType;

		public float stateRate;

		public float stateDelay;

		public bool IsEqual(LightValues other)
		{
			if (type != other.type || shadows != other.shadows || range != other.range || intensity != other.intensity || color != other.color || spotAngle != other.spotAngle)
			{
				return false;
			}
			if (emissiveColor != other.emissiveColor)
			{
				return false;
			}
			if (stateType != other.stateType || stateRate != other.stateRate || stateDelay != other.stateDelay)
			{
				return false;
			}
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, Color> ColorPresets = new Dictionary<string, Color>
	{
		{
			Localization.Get("xuiLightPropColorPresetCustom"),
			new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetCandle"),
			new Color32(byte.MaxValue, 147, 41, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColor40WattWhite"),
			new Color32(byte.MaxValue, 197, 143, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPreset100WattWhite"),
			new Color32(byte.MaxValue, 214, 170, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetHalogen"),
			new Color32(byte.MaxValue, 241, 224, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetCarbonArc"),
			new Color32(byte.MaxValue, 250, 244, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetHighNoonSun"),
			new Color32(byte.MaxValue, byte.MaxValue, 251, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetFluorescent"),
			new Color32(244, byte.MaxValue, 250, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetWarmFluorescent"),
			new Color32(byte.MaxValue, 244, 229, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetCoolFluorescent"),
			new Color32(212, 235, byte.MaxValue, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetFullSpectrum"),
			new Color32(byte.MaxValue, 244, 242, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetMercury"),
			new Color32(216, 247, byte.MaxValue, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetSodium"),
			new Color32(byte.MaxValue, 209, 178, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetHighPressureSodium"),
			new Color32(byte.MaxValue, 183, 76, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetHalide"),
			new Color32(242, 252, byte.MaxValue, byte.MaxValue)
		}
	};

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<LightType> cbxLightType;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> cbxColorPresetList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<LightShadows> cbxLightShadow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<LightStateType> cbxLightState;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxIntensity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRate;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ColorPicker colorPicker;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel panelAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel panelRate;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel panelDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOk;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnCopy;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnPaste;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnRestore;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtColorR;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtColorG;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtColorB;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtHex;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityLight tileEntityLight;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldChangePreset = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public LightValues startValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public LightValues defaultValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool hasCopiedValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public static LightValues copyValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string copyColorPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public WireFrameSphere rangeGizmo;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk chunk;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockLight block;

	public LightLOD lightLOD;

	[PublicizedFrom(EAccessModifier.Private)]
	public Light light;

	public static void Open(LocalPlayerUI _playerUi, TileEntityLight _te, Vector3i _blockPos, World _world, int _cIdx, BlockLight _block)
	{
		XUiC_LightEditor childByType = _playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_LightEditor>();
		childByType.tileEntityLight = _te;
		childByType.blockPos = _blockPos;
		childByType.world = _world;
		ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
		childByType.chunk = (Chunk)chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), _blockPos.y, World.toChunkXZ(_blockPos.z));
		Transform transform = childByType.chunk.GetBlockEntity(_blockPos).transform.Find("MainLight");
		if (transform != null)
		{
			childByType.lightLOD = transform.GetComponent<LightLOD>();
			if (childByType.lightLOD != null)
			{
				childByType.light = childByType.lightLOD.GetLight();
			}
		}
		childByType.OpenAllValues();
		childByType.block = _block;
		_playerUi.windowManager.Open(ID, _bModal: true);
		if (childByType.light != null && childByType.light.type == LightType.Point)
		{
			childByType.panelAngle.IsVisible = false;
			childByType.rangeGizmo = UnityEngine.Object.Instantiate(Resources.Load<WireFrameSphere>("Prefabs/prefabSphereWF"), childByType.light.transform);
			childByType.rangeGizmo.center = childByType.light.gameObject.transform.position;
			childByType.rangeGizmo.name = "Range Gizmo";
			childByType.rangeGizmo.newRadius = childByType.light.range;
		}
		if (childByType.lightLOD != null)
		{
			if (childByType.lightLOD.LightStateType == LightStateType.Static || childByType.lightLOD.LightStateType == LightStateType.Fluctuating)
			{
				childByType.panelRate.IsVisible = false;
			}
			if (childByType.lightLOD.LightStateType != LightStateType.Fluctuating)
			{
				childByType.panelDelay.IsVisible = false;
			}
		}
		if (!hasCopiedValues)
		{
			childByType.btnPaste.ViewComponent.IsVisible = false;
		}
		childByType.CopyUIValues(out childByType.startValues);
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		cbxLightType = (XUiC_ComboBoxList<LightType>)GetChildById("cbxLightType");
		cbxLightType.Elements.Add(LightType.Point);
		cbxLightType.Elements.Add(LightType.Spot);
		cbxLightType.OnValueChanged += CbxLightType_OnValueChanged;
		cbxLightShadow = (XUiC_ComboBoxEnum<LightShadows>)GetChildById("cbxLightShadow");
		cbxLightShadow.OnValueChanged += CbxLightShadow_OnValueChanged;
		cbxColorPresetList = (XUiC_ComboBoxList<string>)GetChildById("cbxPresets");
		cbxLightState = (XUiC_ComboBoxEnum<LightStateType>)GetChildById("cbxLightState");
		cbxLightState.OnValueChanged += CbxLightState_OnValueChanged;
		foreach (KeyValuePair<string, Color> colorPreset in ColorPresets)
		{
			cbxColorPresetList.Elements.Add(colorPreset.Key);
		}
		cbxColorPresetList.OnValueChanged += CbxColorPresetList_OnValueChanged;
		colorPicker = (XUiC_ColorPicker)GetChildById("lightColor");
		colorPicker.OnSelectedColorChanged += ColorPicker_OnSelectedColorChanged;
		panelAngle = (XUiV_Panel)GetChildById("panelAngle").ViewComponent;
		panelRate = (XUiV_Panel)GetChildById("panelRate").ViewComponent;
		panelDelay = (XUiV_Panel)GetChildById("panelDelay").ViewComponent;
		cbxRange = (XUiC_ComboBoxFloat)GetChildById("cbxRange");
		cbxRange.OnValueChanged += CbxRange_OnValueChanged;
		cbxAngle = (XUiC_ComboBoxFloat)GetChildById("cbxAngle");
		cbxAngle.OnValueChanged += CbxAngle_OnValueChanged;
		cbxRate = (XUiC_ComboBoxFloat)GetChildById("cbxRate");
		cbxRate.OnValueChanged += CbxRate_OnValueChanged;
		cbxDelay = (XUiC_ComboBoxFloat)GetChildById("cbxDelay");
		cbxDelay.OnValueChanged += CbxDelay_OnValueChanged;
		cbxIntensity = (XUiC_ComboBoxFloat)GetChildById("cbxIntensity");
		cbxIntensity.OnValueChanged += CbxIntensity_OnValueChanged;
		btnOk = GetChildById("btnOk").GetChildByType<XUiC_SimpleButton>();
		btnOk.OnPressed += BtnSave_OnPressed;
		btnCancel = GetChildById("btnCancel").GetChildByType<XUiC_SimpleButton>();
		btnCancel.OnPressed += BtnCancel_OnPressed;
		btnOnOff = GetChildById("btnOnOff").GetChildByType<XUiC_SimpleButton>();
		btnOnOff.OnPressed += BtnOnOff_OnPressed;
		btnRestore = GetChildById("btnRestoreDefaults").GetChildByType<XUiC_SimpleButton>();
		btnRestore.OnPressed += BtnRestore_OnPressed;
		btnCopy = GetChildById("btnCopy").GetChildByType<XUiC_SimpleButton>();
		btnCopy.OnPressed += BtnCopy_OnPressed;
		btnPaste = GetChildById("btnPaste").GetChildByType<XUiC_SimpleButton>();
		btnPaste.OnPressed += BtnPaste_OnPressed;
		txtColorR = GetChildById("txtColorR") as XUiC_TextInput;
		txtColorR.OnChangeHandler += TxtColorR_OnChangeHandler;
		txtColorG = GetChildById("txtColorG") as XUiC_TextInput;
		txtColorG.OnChangeHandler += TxtColorG_OnChangeHandler;
		txtColorB = GetChildById("txtColorB") as XUiC_TextInput;
		txtColorB.OnChangeHandler += TxtColorB_OnChangeHandler;
		txtHex = GetChildById("txtHex") as XUiC_TextInput;
		UIInput uIInput = txtHex.UIInput;
		uIInput.onValidate = (UIInput.OnValidate)Delegate.Combine(uIInput.onValidate, new UIInput.OnValidate(GameUtils.ValidateHexInput));
		txtHex.OnChangeHandler += TxtHex_OnChangeHandler;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPaste_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (hasCopiedValues)
		{
			double num = (cbxIntensity.Value = copyValues.intensity);
			SetIntensity((float)num);
			num = (cbxRange.Value = copyValues.range);
			SetRange((float)num);
			cbxLightType.Value = copyValues.type;
			SetType(copyValues.type);
			string preset = (cbxColorPresetList.Value = copyColorPreset);
			SetPreset(preset);
			cbxLightState.Value = copyValues.stateType;
			SetState(copyValues.stateType);
			cbxLightShadow.Value = copyValues.shadows;
			SetShadow(copyValues.shadows);
			num = (cbxAngle.Value = copyValues.spotAngle);
			SetAngle((float)num);
			num = (cbxRate.Value = copyValues.stateRate);
			SetRate((float)num);
			num = (cbxDelay.Value = copyValues.stateDelay);
			SetDelay((float)num);
			colorPicker.SelectedColor = copyValues.color;
			SetHex(copyValues.color);
			SetRGB(copyValues.color);
			panelAngle.IsVisible = copyValues.type != LightType.Point;
			if (rangeGizmo != null)
			{
				rangeGizmo.gameObject.GetComponent<LineRenderer>().enabled = copyValues.type == LightType.Point;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCopy_OnPressed(XUiController _sender, int _mouseButton)
	{
		CopyUIValues(out copyValues);
		hasCopiedValues = true;
		copyColorPreset = cbxColorPresetList.Value;
		btnPaste.ViewComponent.IsVisible = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CopyUIValues(out LightValues values)
	{
		values = default(LightValues);
		values.type = cbxLightType.Value;
		values.shadows = cbxLightShadow.Value;
		values.intensity = (float)cbxIntensity.Value;
		values.range = (float)cbxRange.Value;
		values.color = colorPicker.SelectedColor;
		values.spotAngle = (float)cbxAngle.Value;
		if ((bool)lightLOD)
		{
			values.emissiveColor = lightLOD.EmissiveColor;
		}
		values.stateType = cbxLightState.Value;
		values.stateRate = (float)cbxRate.Value;
		values.stateDelay = (float)cbxDelay.Value;
	}

	public void AssignRate(float _rate)
	{
		cbxRate.Value = _rate;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOnOff_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (lightLOD != null)
		{
			lightLOD.SwitchOnOff(!lightLOD.bSwitchedOn, _ignoreToggle: true);
			SetEmissiveColor(light.color);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtHex_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (_text.Length >= 6 && isOpen)
		{
			NumberStyles style = NumberStyles.HexNumber;
			byte r = (byte)int.Parse(_text.Substring(0, 2), style);
			byte g = (byte)int.Parse(_text.Substring(2, 2), style);
			byte b = (byte)int.Parse(_text.Substring(4, 2), style);
			Color32 color = new Color32(r, g, b, byte.MaxValue);
			XUiC_ColorPicker xUiC_ColorPicker = colorPicker;
			Color selectedColor = (light.color = color);
			xUiC_ColorPicker.SelectedColor = selectedColor;
			SetRGB(color);
			SetEmissiveColor(color);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtColorB_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!isOpen || !int.TryParse(_text, out var result))
		{
			return;
		}
		result = Mathf.Clamp(result, 0, 255);
		if (!_text.Equals(result.ToString()))
		{
			txtColorB.Text = result.ToString();
		}
		if (light != null)
		{
			Color color = new Color(light.color.r, light.color.g, (float)result / 255f);
			light.color = color;
			SetEmissiveColor(color);
			colorPicker.SelectedColor = color;
			SetHex(color);
			if (shouldChangePreset)
			{
				cbxColorPresetList.Value = ColorPresets.Keys.First();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtColorG_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!isOpen || !int.TryParse(_text, out var result))
		{
			return;
		}
		result = Mathf.Clamp(result, 0, 255);
		if (!_text.Equals(result.ToString()))
		{
			txtColorG.Text = result.ToString();
		}
		if (light != null)
		{
			Color color = new Color(light.color.r, (float)result / 255f, light.color.b);
			light.color = color;
			SetEmissiveColor(color);
			colorPicker.SelectedColor = color;
			SetHex(color);
			if (shouldChangePreset)
			{
				cbxColorPresetList.Value = ColorPresets.Keys.First();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtColorR_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!isOpen || !int.TryParse(_text, out var result))
		{
			return;
		}
		result = Mathf.Clamp(result, 0, 255);
		if (!_text.Equals(result.ToString()))
		{
			txtColorR.Text = result.ToString();
		}
		if (light != null)
		{
			Color color = new Color((float)result / 255f, light.color.g, light.color.b);
			light.color = color;
			SetEmissiveColor(color);
			colorPicker.SelectedColor = color;
			SetHex(color);
			if (shouldChangePreset)
			{
				cbxColorPresetList.Value = ColorPresets.Keys.First();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxLightType_OnValueChanged(XUiController _sender, LightType _oldValue, LightType _newValue)
	{
		SetType(_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxColorPresetList_OnValueChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		SetPreset(_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxRange_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		SetRange((float)_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxIntensity_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		SetIntensity((float)_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxAngle_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		SetAngle((float)_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxRate_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		SetRate((float)_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxLightState_OnValueChanged(XUiController _sender, LightStateType _oldValue, LightStateType _newValue)
	{
		SetState(_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxLightShadow_OnValueChanged(XUiController _sender, LightShadows _oldValue, LightShadows _newValue)
	{
		SetShadow(_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxDelay_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRestore_OnPressed(XUiController _sender, int _mouseButton)
	{
		if ((bool)light && (bool)lightLOD)
		{
			LoadDefaultValues();
			cbxLightType.Value = defaultValues.type;
			cbxLightShadow.Value = defaultValues.shadows;
			cbxRange.Value = defaultValues.range;
			cbxIntensity.Value = defaultValues.intensity;
			colorPicker.SelectedColor = defaultValues.color;
			cbxAngle.Value = defaultValues.spotAngle;
			cbxLightState.Value = defaultValues.stateType;
			cbxRate.Value = defaultValues.stateRate;
			cbxDelay.Value = defaultValues.stateDelay;
			panelRate.IsVisible = false;
			panelDelay.IsVisible = false;
			SetRGB(defaultValues.color);
			SetHex(defaultValues.color);
			if (rangeGizmo != null)
			{
				rangeGizmo.newRadius = defaultValues.range;
			}
			SetLightFromValues(defaultValues);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		LoadDefaultValues();
		CopyUIValues(out startValues);
		if (defaultValues.IsEqual(startValues))
		{
			if (tileEntityLight != null)
			{
				chunk.RemoveTileEntity(world, tileEntityLight);
			}
		}
		else
		{
			if (tileEntityLight == null && world.GetBlock(blockPos).Block is BlockLight blockLight)
			{
				TileEntity tileEntity = chunk.GetTileEntity(World.toBlock(blockPos));
				if (tileEntity == null)
				{
					tileEntity = blockLight.CreateTileEntity(chunk);
					tileEntity.localChunkPos = World.toBlock(blockPos);
					chunk.AddTileEntity(tileEntity);
				}
				tileEntityLight = tileEntity as TileEntityLight;
			}
			tileEntityLight.LightType = startValues.type;
			tileEntityLight.LightShadows = startValues.shadows;
			tileEntityLight.LightRange = startValues.range;
			tileEntityLight.LightIntensity = startValues.intensity;
			tileEntityLight.LightColor = startValues.color;
			tileEntityLight.LightAngle = startValues.spotAngle;
			tileEntityLight.LightState = startValues.stateType;
			tileEntityLight.Rate = startValues.stateRate;
			tileEntityLight.Delay = startValues.stateDelay;
			tileEntityLight.SetModified();
		}
		base.xui.playerUI.windowManager.Close("lightProperties");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenLightType()
	{
		if (light != null)
		{
			cbxLightType.Value = light.type;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenLightShadows()
	{
		if (light != null)
		{
			cbxLightShadow.Value = light.shadows;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenRange()
	{
		if (light != null)
		{
			cbxRange.Value = light.range;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenIntensity()
	{
		if (lightLOD != null)
		{
			cbxIntensity.Value = lightLOD.MaxIntensity;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenSelectedColor()
	{
		if (light != null)
		{
			colorPicker.SelectedColor = light.color;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenAngle()
	{
		if (light != null && light.type == LightType.Spot)
		{
			cbxAngle.Value = light.spotAngle;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenColorRGB()
	{
		if (light != null)
		{
			Color32 color = light.color;
			txtColorR.Text = color.r.ToString();
			txtColorG.Text = color.g.ToString();
			txtColorB.Text = color.b.ToString();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenHex()
	{
		if (light != null)
		{
			SetHex(light.color);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenState()
	{
		if (lightLOD != null)
		{
			cbxLightState.Value = lightLOD.LightStateType;
			if (lightLOD.LightStateType == LightStateType.Static || lightLOD.LightStateType == LightStateType.Fluctuating)
			{
				panelRate.IsVisible = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenRate()
	{
		if (lightLOD != null)
		{
			cbxRate.Value = lightLOD.StateRate;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenDelay()
	{
		if (lightLOD != null)
		{
			cbxDelay.Value = lightLOD.FluxDelay;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenAllValues()
	{
		if (light != null && lightLOD != null)
		{
			startValues = MakeValues(lightLOD);
		}
		OpenLightType();
		OpenLightShadows();
		OpenRange();
		OpenAngle();
		OpenIntensity();
		OpenSelectedColor();
		OpenColorRGB();
		OpenHex();
		OpenState();
		OpenRate();
		OpenDelay();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoadDefaultValues()
	{
		Transform transform = DataLoader.LoadAsset<GameObject>(block.Properties.Values["Model"]).transform.Find("MainLight");
		if (!transform)
		{
			Log.Error("MainLight missing for {0}", block.Properties.Values["Model"]);
		}
		else
		{
			LightLOD component = transform.GetComponent<LightLOD>();
			defaultValues = MakeValues(component);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public LightValues MakeValues(LightLOD _lightLOD)
	{
		Light light = _lightLOD.GetLight();
		LightValues result = default(LightValues);
		result.type = light.type;
		result.shadows = light.shadows;
		result.range = light.range;
		result.intensity = light.intensity;
		result.color = light.color;
		result.spotAngle = light.spotAngle;
		result.emissiveColor = _lightLOD.EmissiveColor;
		result.stateType = LightStateType.Static;
		result.stateRate = 1f;
		result.stateDelay = 1f;
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetType(LightType _newType)
	{
		if (light != null)
		{
			light.type = _newType;
		}
		if (_newType == LightType.Point)
		{
			panelAngle.IsVisible = false;
			if (rangeGizmo != null)
			{
				rangeGizmo.gameObject.GetComponent<LineRenderer>().enabled = true;
			}
		}
		else
		{
			panelAngle.IsVisible = true;
			if (rangeGizmo != null)
			{
				rangeGizmo.gameObject.GetComponent<LineRenderer>().enabled = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetPreset(string _newPreset)
	{
		if (_newPreset.ToString() != "Custom" && light != null)
		{
			light.color = ColorPresets[_newPreset];
			SetEmissiveColor(light.color);
			shouldChangePreset = false;
			SetRGB(light.color);
			SetHex(light.color);
			shouldChangePreset = true;
			colorPicker.SelectedColor = ColorPresets[_newPreset];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRange(float _newRange)
	{
		if (light != null && lightLOD != null)
		{
			lightLOD.SetRange(_newRange);
			if (rangeGizmo != null)
			{
				rangeGizmo.newRadius = _newRange;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetIntensity(float _newIntensity)
	{
		if (lightLOD != null)
		{
			lightLOD.MaxIntensity = _newIntensity;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetAngle(float _newAngle)
	{
		if (light != null)
		{
			light.spotAngle = _newAngle;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRate(float _newRate)
	{
		if (lightLOD != null)
		{
			lightLOD.StateRate = _newRate;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetState(LightStateType _newState)
	{
		if (lightLOD != null)
		{
			lightLOD.LightStateType = _newState;
			panelRate.IsVisible = _newState == LightStateType.Pulsing || _newState == LightStateType.Blinking;
			if (panelRate.IsVisible)
			{
				cbxRate.Value = lightLOD.StateRate;
			}
			panelDelay.IsVisible = _newState == LightStateType.Fluctuating;
			if (panelDelay.IsVisible)
			{
				cbxDelay.Value = lightLOD.FluxDelay;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetShadow(LightShadows _newShadow)
	{
		if (light != null)
		{
			light.shadows = _newShadow;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetDelay(float _newDelay)
	{
		if (lightLOD != null)
		{
			lightLOD.FluxDelay = _newDelay;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRGB(Color32 color)
	{
		txtColorR.Text = color.r.ToString();
		txtColorG.Text = color.g.ToString();
		txtColorB.Text = color.b.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetHex(Color32 color)
	{
		string text = ((color.r < 16) ? ("0" + color.r.ToString("X")) : color.r.ToString("X"));
		string text2 = ((color.g < 16) ? ("0" + color.g.ToString("X")) : color.g.ToString("X"));
		string text3 = ((color.b < 16) ? ("0" + color.b.ToString("X")) : color.b.ToString("X"));
		txtHex.Text = text + text2 + text3;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetLightFromValues(LightValues values)
	{
		if ((bool)light && (bool)lightLOD)
		{
			light.type = values.type;
			light.shadows = values.shadows;
			lightLOD.SetRange(values.range);
			lightLOD.MaxIntensity = values.intensity;
			light.color = values.color;
			light.spotAngle = values.spotAngle;
			SetEmissiveColor(values.emissiveColor);
			lightLOD.LightStateType = values.stateType;
			lightLOD.StateRate = values.stateRate;
			lightLOD.FluxDelay = values.stateDelay;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetEmissiveColor(Color _color)
	{
		if ((bool)lightLOD)
		{
			lightLOD.SetEmissiveColor(_color);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ColorPicker_OnSelectedColorChanged(Color color)
	{
		if (light != null)
		{
			light.color = colorPicker.SelectedColor;
			cbxColorPresetList.Value = ColorPresets.Keys.First();
			SetRGB(light.color);
			SetHex(light.color);
		}
		SetEmissiveColor(color);
	}

	public override void OnClose()
	{
		Log.Out("CLOSE " + startValues.stateType);
		SetLightFromValues(startValues);
		base.OnClose();
		if (rangeGizmo != null)
		{
			rangeGizmo.KillWF();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		isOpen = true;
	}
}
