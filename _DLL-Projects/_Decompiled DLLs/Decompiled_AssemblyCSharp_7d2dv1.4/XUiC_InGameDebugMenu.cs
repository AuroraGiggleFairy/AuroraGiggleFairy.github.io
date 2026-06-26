using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_InGameDebugMenu : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static int LastTeleportX = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int LastTeleportZ = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int SLIDER_MAX_DAYS = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int SLIDER_MAX_SPEED = 500;

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnSuicide;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnTeleport;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnReloadChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput teleportX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput teleportZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Slider sliderDay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Slider sliderTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Slider sliderSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleSaving;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton togglePhysics;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleTicking;

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
	public XUiC_ToggleButton toggleInvisibileMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<BiomeDefinition.BiomeType> cbxPlaytestBiome;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnPlaytest;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBackToEditor;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ulong> ticksFormatter = new CachedStringFormatter<ulong>([PublicizedFrom(EAccessModifier.Internal)] (ulong _i) => _i.ToString());

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

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "decorationsState":
			if (HasWorld)
			{
				bool flag = ChunkCluster0 != null && ChunkCluster0.ChunkProvider.IsDecorationsEnabled();
				value = (flag ? "On" : "Off");
			}
			else
			{
				value = "N/A";
			}
			return true;
		case "gameTicksName":
			value = Localization.Get("xuiDebugTicks");
			return true;
		case "gameTicks":
			value = ticksFormatter.Format(GameTimer.Instance.ticks);
			return true;
		default:
			return base.GetBindingValue(ref value, bindingName);
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		btnSuicide = GetChildById("btnSuicide");
		btnSuicide.GetChildById("clickable").OnPress += BtnSuicide_Controller_OnPress;
		teleportX = GetChildById("teleportX") as XUiC_TextInput;
		teleportZ = GetChildById("teleportZ") as XUiC_TextInput;
		teleportX.OnSubmitHandler += Teleport_OnSubmitHandler;
		teleportZ.OnSubmitHandler += Teleport_OnSubmitHandler;
		teleportX.SelectOnTab = teleportZ;
		teleportZ.SelectOnTab = teleportX;
		teleportX.Text = LastTeleportX.ToString();
		teleportZ.Text = LastTeleportZ.ToString();
		btnTeleport = GetChildById("btnTeleport");
		btnTeleport.GetChildById("clickable").OnPress += BtnTeleport_Controller_OnPress;
		((XUiV_Button)GetChildById("btnRecalcLight").GetChildById("clickable").ViewComponent).Controller.OnPress += BtnRecalcLight_Controller_OnPress;
		((XUiV_Button)GetChildById("btnRecalcStability").GetChildById("clickable").ViewComponent).Controller.OnPress += BtnRecalcStability_Controller_OnPress;
		btnReloadChunks = GetChildById("btnReloadChunks");
		btnReloadChunks.GetChildById("clickable").OnPress += BtnReloadChunks_Controller_OnPress;
		sliderDay = GetChildById("sliderDayRect").GetChildByType<XUiC_Slider>();
		sliderDay.Label = Localization.Get("xuiDebugDay");
		sliderDay.ValueFormatter = SliderDay_ValueFormatter;
		sliderDay.OnValueChanged += SliderDay_OnValueChanged;
		sliderTime = GetChildById("sliderTimeRect").GetChildByType<XUiC_Slider>();
		sliderTime.Label = Localization.Get("xuiDebugTime");
		sliderTime.ValueFormatter = SliderTime_ValueFormatter;
		sliderTime.OnValueChanged += SliderTime_OnValueChanged;
		sliderSpeed = GetChildById("sliderSpeedRect").GetChildByType<XUiC_Slider>();
		sliderSpeed.Label = Localization.Get("xuiDebugSpeed");
		sliderSpeed.ValueFormatter = SliderSpeed_ValueFormatter;
		sliderSpeed.OnValueChanged += SliderSpeed_OnValueChanged;
		toggleFlyMode = GetChildById("toggleFlyMode").GetChildByType<XUiC_ToggleButton>();
		toggleFlyMode.OnValueChanged += ToggleFlyMode_OnValueChanged;
		toggleGodMode = GetChildById("toggleGodMode").GetChildByType<XUiC_ToggleButton>();
		toggleGodMode.OnValueChanged += ToggleGodMode_OnValueChanged;
		toggleNoCollisionMode = GetChildById("toggleNoCollisionMode").GetChildByType<XUiC_ToggleButton>();
		toggleNoCollisionMode.OnValueChanged += ToggleNoCollisionMode_OnValueChanged;
		toggleInvisibileMode = GetChildById("toggleInvisibileMode").GetChildByType<XUiC_ToggleButton>();
		toggleInvisibileMode.OnValueChanged += ToggleInvisibileMode_OnValueChanged;
		toggleSaving = GetChildById("toggleSaving").GetChildByType<XUiC_ToggleButton>();
		toggleSaving.OnValueChanged += ToggleSaving_OnValueChanged;
		togglePhysics = GetChildById("togglePhysics").GetChildByType<XUiC_ToggleButton>();
		togglePhysics.OnValueChanged += TogglePhysics_OnValueChanged;
		toggleTicking = GetChildById("toggleTicking").GetChildByType<XUiC_ToggleButton>();
		toggleTicking.OnValueChanged += ToggleTicking_OnValueChanged;
		toggleWaterSim = GetChildById("toggleWaterSim").GetChildByType<XUiC_ToggleButton>();
		toggleWaterSim.OnValueChanged += ToggleWaterSim_OnValueChanged;
		toggleDebugShaders = GetChildById("toggleDebugShaders").GetChildByType<XUiC_ToggleButton>();
		toggleDebugShaders.OnValueChanged += ToggleDebugShaders_OnValueChanged;
		toggleLightPerformance = GetChildById("toggleLightPerformance").GetChildByType<XUiC_ToggleButton>();
		toggleLightPerformance.OnValueChanged += ToggleLightPerformance_OnValueChanged;
		toggleStabilityGlue = GetChildById("toggleStabilityGlue").GetChildByType<XUiC_ToggleButton>();
		toggleStabilityGlue.OnValueChanged += ToggleStabilityGlue_OnValueChanged;
		btnPlaytest = GetChildById("btnPlaytest") as XUiC_SimpleButton;
		btnPlaytest.OnPressed += BtnPlaytestOnPressed;
		btnBackToEditor = GetChildById("btnBackToEditor") as XUiC_SimpleButton;
		btnBackToEditor.OnPressed += BtnBackToEditorOnPressed;
		cbxPlaytestBiome = GetChildById("cbxPlaytestBiome") as XUiC_ComboBoxList<BiomeDefinition.BiomeType>;
		cbxPlaytestBiome.Elements.AddRange(new BiomeDefinition.BiomeType[5]
		{
			BiomeDefinition.BiomeType.Snow,
			BiomeDefinition.BiomeType.PineForest,
			BiomeDefinition.BiomeType.Desert,
			BiomeDefinition.BiomeType.Wasteland,
			BiomeDefinition.BiomeType.burnt_forest
		});
		cbxPlaytestBiome.Value = (BiomeDefinition.BiomeType)GamePrefs.GetInt(EnumGamePrefs.PlaytestBiome);
		cbxPlaytestBiome.OnValueChanged += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _sender, BiomeDefinition.BiomeType _value, BiomeDefinition.BiomeType _newValue) =>
		{
			GamePrefs.Set(EnumGamePrefs.PlaytestBiome, (int)_newValue);
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSuicide_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		if (base.xui.playerUI.entityPlayer != null)
		{
			base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
			base.xui.playerUI.entityPlayer.Kill(DamageResponse.New(new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Suicide), _fatal: true));
			GameManager.Instance.Pause(_bOn: false);
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
		if (base.xui.playerUI.entityPlayer != null && int.TryParse(teleportX.Text, out var result) && int.TryParse(teleportZ.Text, out var result2))
		{
			LastTeleportX = result;
			LastTeleportZ = result2;
			base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
			base.xui.playerUI.entityPlayer.Teleport(new Vector3(result, 240f, result2));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnViewStabilityGlue_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRecalcLight_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		if (ChunkCluster0 == null)
		{
			return;
		}
		lock (ChunkCluster0.GetSyncRoot())
		{
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
				lightProcessor.GenerateSunlight(item2, bSpreadLight: false);
			}
			foreach (Chunk item3 in chunkArrayCopySync)
			{
				lightProcessor.GenerateSunlight(item3, bSpreadLight: true);
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
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RecalcStability()
	{
		ChunkCluster chunkCluster = ChunkCluster0;
		if (chunkCluster == null)
		{
			return;
		}
		lock (chunkCluster.GetSyncRoot())
		{
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
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRecalcStability_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		RecalcStability();
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
	public string SliderDay_ValueFormatter(float _value)
	{
		return SliderDay_Value().ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SliderDay_OnValueChanged(XUiC_Slider _sender)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && HasWorld)
		{
			ulong num = GameManager.Instance.World.worldTime % 24000;
			GameManager.Instance.World.SetTimeJump(num + (ulong)((long)(SliderDay_Value() - 1) * 24000L), _isSeek: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int SliderDay_Value()
	{
		return (int)(Mathf.Clamp(sliderDay.Value, 0f, 0.99f) * 16f + 1f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string SliderTime_ValueFormatter(float _value)
	{
		return SliderTime_Value().ToCultureInvariantString("0.00");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SliderTime_OnValueChanged(XUiC_Slider _sender)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && HasWorld)
		{
			ulong num = GameManager.Instance.World.worldTime / 24000;
			ulong num2 = (ulong)(SliderTime_Value() * 1000f);
			GameManager.Instance.World.SetTimeJump(num2 + num * 24000, _isSeek: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float SliderTime_Value()
	{
		return Mathf.Clamp(sliderTime.Value, 0f, 0.99f) * 24f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string SliderSpeed_ValueFormatter(float _value)
	{
		return SliderSpeed_Value().ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SliderSpeed_OnValueChanged(XUiC_Slider _sender)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			int value = SliderSpeed_Value();
			GameStats.Set(EnumGameStats.TimeOfDayIncPerSec, value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int SliderSpeed_Value()
	{
		return (int)(Mathf.Clamp(sliderSpeed.Value, 0f, 0.99f) * 500f + 1f);
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
		if (base.xui.playerUI.entityPlayer != null)
		{
			base.xui.playerUI.entityPlayer.IsGodMode.Value = _newValue;
			base.xui.playerUI.entityPlayer.IsNoCollisionMode.Value = _newValue;
			base.xui.playerUI.entityPlayer.IsFlyMode.Value = _newValue;
			base.xui.playerUI.entityPlayer.bEntityAliveFlagsChanged = true;
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
	public void ToggleInvisibileMode_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (base.xui.playerUI.entityPlayer != null)
		{
			base.xui.playerUI.entityPlayer.IsSpectator = _newValue;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleSaving_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		GameManager.bSavingActive = _newValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TogglePhysics_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		GameManager.bPhysicsActive = _newValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleTicking_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		GameManager.bTickingActive = _newValue;
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
		if (GameManager.Instance.stabilityViewer != null)
		{
			GameManager.Instance.ClearStabilityViewer();
			return;
		}
		GameManager.Instance.CreateStabilityViewer();
		GameManager.Instance.stabilityViewer.StartSearch();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (GameManager.IsDedicatedServer)
		{
			btnSuicide.ViewComponent.IsVisible = false;
			teleportX.ViewComponent.IsVisible = false;
			teleportZ.ViewComponent.IsVisible = false;
			btnTeleport.ViewComponent.IsVisible = false;
			toggleFlyMode.ViewComponent.IsVisible = false;
			toggleGodMode.ViewComponent.IsVisible = false;
			toggleNoCollisionMode.ViewComponent.IsVisible = false;
			toggleInvisibileMode.ViewComponent.IsVisible = false;
		}
		if (GameManager.Instance.IsEditMode())
		{
			btnSuicide.ViewComponent.IsVisible = false;
		}
		if (!GameManager.Instance.IsEditMode())
		{
			btnReloadChunks.ViewComponent.IsVisible = false;
		}
		bool isServer = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		bool flag = GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty";
		bool flag2 = GameUtils.IsPlaytesting();
		btnPlaytest.ViewComponent.IsVisible = (flag || flag2) && isServer;
		btnPlaytest.Text = (flag2 ? Localization.Get("xuiDebugMenuPlaytestReset") : Localization.Get("xuiDebugMenuPlaytest"));
		btnBackToEditor.ViewComponent.IsVisible = (flag || flag2) && isServer;
		btnBackToEditor.Enabled = flag2;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			sliderDay.ViewComponent.IsVisible = false;
			sliderTime.ViewComponent.IsVisible = false;
			sliderSpeed.ViewComponent.IsVisible = false;
			toggleSaving.ViewComponent.IsVisible = false;
			togglePhysics.ViewComponent.IsVisible = false;
			toggleTicking.ViewComponent.IsVisible = false;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		sliderDay.Value = (HasWorld ? ((float)(GameManager.Instance.World.worldTime / 24000) / 16f) : 0f);
		sliderTime.Value = (HasWorld ? ((float)(GameManager.Instance.World.worldTime % 24000) / 24000f) : 0f);
		sliderSpeed.Value = (HasWorld ? ((float)(GameStats.GetInt(EnumGameStats.TimeOfDayIncPerSec) - 1) / 500f) : 0f);
		toggleFlyMode.Value = base.xui.playerUI.entityPlayer != null && base.xui.playerUI.entityPlayer.IsFlyMode.Value;
		toggleGodMode.Value = base.xui.playerUI.entityPlayer != null && base.xui.playerUI.entityPlayer.IsGodMode.Value;
		toggleNoCollisionMode.Value = base.xui.playerUI.entityPlayer != null && base.xui.playerUI.entityPlayer.IsNoCollisionMode.Value;
		toggleInvisibileMode.Value = base.xui.playerUI.entityPlayer != null && base.xui.playerUI.entityPlayer.IsSpectator;
		toggleSaving.Value = GameManager.bSavingActive;
		togglePhysics.Value = GameManager.bPhysicsActive;
		toggleTicking.Value = GameManager.bTickingActive;
		toggleWaterSim.Value = !WaterSimulationNative.Instance.IsPaused;
		toggleDebugShaders.Value = MeshDescription.bDebugStability;
		toggleLightPerformance.Value = LightViewer.IsEnabled;
		RefreshBindings(_forceAll: true);
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
}
