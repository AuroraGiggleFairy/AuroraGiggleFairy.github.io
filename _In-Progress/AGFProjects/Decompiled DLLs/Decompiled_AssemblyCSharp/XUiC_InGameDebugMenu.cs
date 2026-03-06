using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_InGameDebugMenu : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static int lastTeleportX;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int lastTeleportZ;

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton togglePhysics;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleWaterSim;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleDebugShaders;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleLightPerformance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleStabilityGlue;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleFlyMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleGodMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleNoCollisionMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleInvisibleMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showCamPositions;

	public ChunkCluster ChunkCluster0
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!(GameManager.Instance == null))
			{
				if (GameManager.Instance.World != null)
				{
					return GameManager.Instance.World.ChunkClusters[0];
				}
				return null;
			}
			return null;
		}
	}

	public bool HasWorld
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameManager.Instance.World != null;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (!(_bindingName == "decorationsState"))
		{
			if (_bindingName == "cam_positions_open")
			{
				_value = showCamPositions.ToString();
				return true;
			}
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		if (HasWorld)
		{
			bool flag = ChunkCluster0 != null && ChunkCluster0.ChunkProvider.IsDecorationsEnabled();
			_value = (flag ? "On" : "Off");
		}
		else
		{
			_value = "N/A";
		}
		return true;
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		if (GetChildById("btnSuicide") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += BtnSuicide_Controller_OnPress;
		}
		XUiC_TextInput xUiC_TextInput = GetChildById("teleportX") as XUiC_TextInput;
		XUiC_TextInput xUiC_TextInput2 = GetChildById("teleportZ") as XUiC_TextInput;
		if (xUiC_TextInput != null)
		{
			xUiC_TextInput.OnChangeHandler += TeleportX_OnChangeHandler;
			xUiC_TextInput.OnSubmitHandler += Teleport_OnSubmitHandler;
			xUiC_TextInput.SelectOnTab = xUiC_TextInput2;
			xUiC_TextInput.Text = lastTeleportX.ToString();
		}
		if (xUiC_TextInput2 != null)
		{
			xUiC_TextInput2.OnChangeHandler += TeleportZ_OnChangeHandler;
			xUiC_TextInput2.OnSubmitHandler += Teleport_OnSubmitHandler;
			xUiC_TextInput2.SelectOnTab = xUiC_TextInput;
			xUiC_TextInput2.Text = lastTeleportZ.ToString();
		}
		if (GetChildById("btnTeleport") is XUiC_SimpleButton xUiC_SimpleButton2)
		{
			xUiC_SimpleButton2.OnPressed += BtnTeleport_Controller_OnPress;
		}
		if (GetChildById("btnRecalcLight") is XUiC_SimpleButton xUiC_SimpleButton3)
		{
			xUiC_SimpleButton3.OnPressed += BtnRecalcLight_Controller_OnPress;
		}
		if (GetChildById("btnRecalcStability") is XUiC_SimpleButton xUiC_SimpleButton4)
		{
			xUiC_SimpleButton4.OnPressed += BtnRecalcStability_Controller_OnPress;
		}
		if (GetChildById("btnReloadChunks") is XUiC_SimpleButton xUiC_SimpleButton5)
		{
			xUiC_SimpleButton5.OnPressed += BtnReloadChunks_Controller_OnPress;
		}
		toggleFlyMode = GetChildById("toggleFlyMode")?.GetChildByType<XUiC_ToggleButton>();
		if (toggleFlyMode != null)
		{
			toggleFlyMode.OnValueChanged += ToggleFlyMode_OnValueChanged;
		}
		toggleGodMode = GetChildById("toggleGodMode")?.GetChildByType<XUiC_ToggleButton>();
		if (toggleGodMode != null)
		{
			toggleGodMode.OnValueChanged += ToggleGodMode_OnValueChanged;
		}
		toggleNoCollisionMode = GetChildById("toggleNoCollisionMode")?.GetChildByType<XUiC_ToggleButton>();
		if (toggleNoCollisionMode != null)
		{
			toggleNoCollisionMode.OnValueChanged += ToggleNoCollisionMode_OnValueChanged;
		}
		toggleInvisibleMode = GetChildById("toggleInvisibleMode")?.GetChildByType<XUiC_ToggleButton>();
		if (toggleInvisibleMode != null)
		{
			toggleInvisibleMode.OnValueChanged += toggleInvisibleModeOnValueChanged;
		}
		togglePhysics = GetChildById("togglePhysics")?.GetChildByType<XUiC_ToggleButton>();
		if (togglePhysics != null)
		{
			togglePhysics.OnValueChanged += TogglePhysics_OnValueChanged;
		}
		toggleWaterSim = GetChildById("toggleWaterSim")?.GetChildByType<XUiC_ToggleButton>();
		if (toggleWaterSim != null)
		{
			toggleWaterSim.OnValueChanged += ToggleWaterSim_OnValueChanged;
		}
		toggleDebugShaders = GetChildById("toggleDebugShaders")?.GetChildByType<XUiC_ToggleButton>();
		if (toggleDebugShaders != null)
		{
			toggleDebugShaders.OnValueChanged += ToggleDebugShaders_OnValueChanged;
		}
		toggleLightPerformance = GetChildById("toggleLightPerformance")?.GetChildByType<XUiC_ToggleButton>();
		if (toggleLightPerformance != null)
		{
			toggleLightPerformance.OnValueChanged += ToggleLightPerformance_OnValueChanged;
		}
		toggleStabilityGlue = GetChildById("toggleStabilityGlue")?.GetChildByType<XUiC_ToggleButton>();
		if (toggleStabilityGlue != null)
		{
			toggleStabilityGlue.OnValueChanged += ToggleStabilityGlue_OnValueChanged;
		}
		if (GetChildById("btnPlaytest") is XUiC_SimpleButton xUiC_SimpleButton6)
		{
			xUiC_SimpleButton6.OnPressed += BtnPlaytestOnPressed;
		}
		if (GetChildById("btnBackToEditor") is XUiC_SimpleButton xUiC_SimpleButton7)
		{
			xUiC_SimpleButton7.OnPressed += BtnBackToEditorOnPressed;
		}
		XUiC_ToggleButton xUiC_ToggleButton = GetChildById("toggleCamPositions")?.GetChildByType<XUiC_ToggleButton>();
		if (xUiC_ToggleButton != null)
		{
			xUiC_ToggleButton.OnValueChanged += [PublicizedFrom(EAccessModifier.Private)] (XUiC_ToggleButton _, bool _value) =>
			{
				showCamPositions = _value;
				RefreshBindings();
			};
		}
		if (GetChildById("cbxPlaytestBiome") is XUiC_ComboBoxList<BiomeDefinition.BiomeType> xUiC_ComboBoxList)
		{
			xUiC_ComboBoxList.Elements.AddRange(new BiomeDefinition.BiomeType[5]
			{
				BiomeDefinition.BiomeType.Snow,
				BiomeDefinition.BiomeType.PineForest,
				BiomeDefinition.BiomeType.Desert,
				BiomeDefinition.BiomeType.Wasteland,
				BiomeDefinition.BiomeType.burnt_forest
			});
			xUiC_ComboBoxList.Value = (BiomeDefinition.BiomeType)GamePrefs.GetInt(EnumGamePrefs.PlaytestBiome);
			xUiC_ComboBoxList.OnValueChanged += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _, BiomeDefinition.BiomeType _, BiomeDefinition.BiomeType _newValue) =>
			{
				GamePrefs.Set(EnumGamePrefs.PlaytestBiome, (int)_newValue);
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSuicide_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		if (!(base.xui.playerUI.entityPlayer == null))
		{
			base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
			base.xui.playerUI.entityPlayer.Kill(DamageResponse.New(new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Suicide), _fatal: true));
			GameManager.Instance.Pause(_bOn: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void TeleportX_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!int.TryParse(_text, out lastTeleportX))
		{
			lastTeleportX = int.MinValue;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void TeleportZ_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!int.TryParse(_text, out lastTeleportZ))
		{
			lastTeleportZ = int.MinValue;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Teleport_OnSubmitHandler(XUiController _sender, string _text)
	{
		BtnTeleport_Controller_OnPress(_sender, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnTeleport_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		if (!(base.xui.playerUI.entityPlayer == null) && lastTeleportX != int.MinValue && lastTeleportZ != int.MinValue)
		{
			base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
			base.xui.playerUI.entityPlayer.Teleport(new Vector3(lastTeleportX, 240f, lastTeleportZ));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRecalcLight_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		if (ChunkCluster0 == null)
		{
			return;
		}
		LightProcessor lightProcessor = new LightProcessor(GameManager.Instance.World);
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		List<Chunk> chunkArrayCopySync = ChunkCluster0.GetChunkArrayCopySync();
		foreach (Chunk item in chunkArrayCopySync)
		{
			item.ResetLights(0);
			item.RefreshSunlight();
		}
		foreach (Chunk item2 in chunkArrayCopySync)
		{
			lightProcessor.GenerateSunlight(item2, _isSpread: false);
		}
		foreach (Chunk item3 in chunkArrayCopySync)
		{
			lightProcessor.GenerateSunlight(item3, _isSpread: true);
		}
		foreach (Chunk item4 in chunkArrayCopySync)
		{
			lightProcessor.LightChunk(item4);
		}
		stopwatch.Stop();
		foreach (Chunk item5 in chunkArrayCopySync)
		{
			item5.NeedsRegeneration = true;
		}
		Log.Out("#" + chunkArrayCopySync.Count + " chunks needed " + stopwatch.ElapsedMilliseconds + "ms");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRecalcStability_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		ChunkCluster chunkCluster = ChunkCluster0;
		if (chunkCluster == null)
		{
			return;
		}
		StabilityInitializer stabilityInitializer = new StabilityInitializer(GameManager.Instance.World);
		MicroStopwatch microStopwatch = new MicroStopwatch();
		foreach (Chunk item in chunkCluster.GetChunkArray())
		{
			item.ResetStabilityToBottomMost();
		}
		Log.Out("RecalcStability reset in {0}ms", microStopwatch.ElapsedMilliseconds);
		foreach (Chunk item2 in chunkCluster.GetChunkArray())
		{
			stabilityInitializer.DistributeStability(item2);
			item2.NeedsRegeneration = true;
		}
		Log.Out("RecalcStability #{0} in {1}ms", chunkCluster.GetChunkArray().Count, microStopwatch.ElapsedMilliseconds);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnReloadChunks_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		if (ChunkCluster0 != null)
		{
			GameManager.Instance.World.m_ChunkManager.ReloadAllChunks();
			ChunkCluster0.ChunkProvider.ReloadAllChunks();
			GameManager.Instance.World.UnloadEntities(GameManager.Instance.World.Entities.list);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleFlyMode_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (base.xui.playerUI.entityPlayer != null)
		{
			base.xui.playerUI.entityPlayer.IsFlyMode.Value = _newValue;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleGodMode_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (!(entityPlayer == null))
		{
			entityPlayer.MoveController?.toggleGodMode?.Invoke();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleNoCollisionMode_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (base.xui.playerUI.entityPlayer != null)
		{
			base.xui.playerUI.entityPlayer.IsNoCollisionMode.Value = _newValue;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toggleInvisibleModeOnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (base.xui.playerUI.entityPlayer != null)
		{
			base.xui.playerUI.entityPlayer.IsSpectator = _newValue;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TogglePhysics_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		GameManager.bPhysicsActive = _newValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleWaterSim_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		WaterSimulationNative.Instance.SetPaused(!_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleDebugShaders_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		MeshDescription.SetDebugStabilityShader(_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleLightPerformance_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		LightViewer.SetEnabled(_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleStabilityGlue_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (_newValue)
		{
			if (GameManager.Instance.stabilityViewer == null)
			{
				GameManager.Instance.CreateStabilityViewer();
				GameManager.Instance.stabilityViewer.StartSearch();
			}
		}
		else if (GameManager.Instance.stabilityViewer != null)
		{
			GameManager.Instance.ClearStabilityViewer();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPlaytestOnPressed(XUiController _sender, int _mouseButton)
	{
		if (PrefabEditModeManager.Instance.IsActive() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			XUiC_SaveDirtyPrefab.Show(base.xui, startPlaytest);
		}
		else
		{
			startPlaytest(XUiC_SaveDirtyPrefab.ESelectedAction.DontSave);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startPlaytest(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		if (_action != XUiC_SaveDirtyPrefab.ESelectedAction.Cancel)
		{
			GameUtils.StartPlaytesting();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBackToEditorOnPressed(XUiController _sender, int _mouseButton)
	{
		GameUtils.StartSinglePrefabEditing();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (toggleFlyMode != null)
		{
			toggleFlyMode.Value = entityPlayer != null && entityPlayer.IsFlyMode.Value;
		}
		if (toggleGodMode != null)
		{
			toggleGodMode.Value = entityPlayer != null && entityPlayer.IsGodMode.Value;
		}
		if (toggleNoCollisionMode != null)
		{
			toggleNoCollisionMode.Value = entityPlayer != null && entityPlayer.IsNoCollisionMode.Value;
		}
		if (toggleInvisibleMode != null)
		{
			toggleInvisibleMode.Value = entityPlayer != null && entityPlayer.IsSpectator;
		}
		if (togglePhysics != null)
		{
			togglePhysics.Value = GameManager.bPhysicsActive;
		}
		if (toggleWaterSim != null)
		{
			toggleWaterSim.Value = !WaterSimulationNative.Instance.IsPaused;
		}
		if (toggleDebugShaders != null)
		{
			toggleDebugShaders.Value = MeshDescription.bDebugStability;
		}
		if (toggleLightPerformance != null)
		{
			toggleLightPerformance.Value = LightViewer.IsEnabled;
		}
		RefreshBindings(_forceAll: true);
	}
}
