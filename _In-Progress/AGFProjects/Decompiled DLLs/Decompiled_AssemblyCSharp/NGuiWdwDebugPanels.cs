using System;
using System.Collections.Generic;
using System.Text;
using DynamicMusic;
using GamePath;
using UnityEngine;

public class NGuiWdwDebugPanels : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum EDebugDataType
	{
		Off,
		General
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EPerformanceDisplayType
	{
		Off,
		Fps,
		FpsAndHeat,
		FpsAndFpsGraph,
		FpsAndNetGraphs
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EGuiState
	{
		CalcSize,
		Draw,
		Count
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class PanelDefinition
	{
		public string Name;

		public string ButtonCaption;

		public Func<int, int, int> GuiHandler;

		public bool Enabled;

		public bool Active;

		public PanelDefinition(string _name, string _buttonCaption, Func<int, int, int> _guiHandler, string _enabledPanels, bool _enabled = true)
		{
			Name = _name;
			ButtonCaption = _buttonCaption;
			GuiHandler = _guiHandler;
			Enabled = _enabled;
			Active = _enabledPanels.Contains("," + _buttonCaption + ",");
		}
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EDebugDataType debugData;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EPerformanceDisplayType performanceType;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly GUIStyle guiStyleDebug = new GUIStyle();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static GUIStyle guiStyleToggleBox;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static GUIStyle guiStyleTooltipLabel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static GUIStyle guiStyleLabelRightAligned;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GUIFPS guiFPS;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public NetworkMonitor networkMonitorCh0;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public NetworkMonitor networkMonitorCh1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal playerEntity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LocalPlayerUI playerUI;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i lastResolution;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GUIStyle boxStyle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int boxAreaHeight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int boxAreaWidth;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cLineHeight = 16;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cHeaderLabelWidth = 200;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cHeaderLabelHeight = 25;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lastPlayerPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastPlayerTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float playerSpeed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static string filterCVar = "";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PanelDefinition> Panels = new List<PanelDefinition>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		playerUI = GetComponentInParent<LocalPlayerUI>();
		if (playerUI.IsCleanCopy || LocalPlayerUI.CreatingCleanCopy)
		{
			return;
		}
		guiStyleDebug.fontSize = 12;
		guiStyleDebug.fontStyle = FontStyle.Bold;
		debugData = EDebugDataType.Off;
		guiFPS = base.transform.GetComponentInChildren<GUIFPS>();
		NGuiAction nGuiAction = new NGuiAction("Show Debug Data", PlayerActionsGlobal.Instance.ShowDebugData);
		nGuiAction.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			debugData = debugData.CycleEnum();
		});
		nGuiAction.SetIsEnabledDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled));
		NGuiAction nGuiAction2 = new NGuiAction("ShowFPS", PlayerActionsGlobal.Instance.ShowFPS);
		nGuiAction2.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			performanceType = performanceType.CycleEnum(EPerformanceDisplayType.Off, (!GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled)) ? EPerformanceDisplayType.Fps : EPerformanceDisplayType.FpsAndNetGraphs);
			guiFPS.Enabled = performanceType != EPerformanceDisplayType.Off;
			guiFPS.ShowGraph = performanceType == EPerformanceDisplayType.FpsAndFpsGraph;
			networkMonitorCh0.Enabled = performanceType == EPerformanceDisplayType.FpsAndNetGraphs;
			networkMonitorCh1.Enabled = performanceType == EPerformanceDisplayType.FpsAndNetGraphs;
		});
		playerUI.windowManager.AddGlobalAction(nGuiAction);
		playerUI.windowManager.AddGlobalAction(nGuiAction2);
		GameManager.Instance.OnWorldChanged += HandleWorldChanged;
		GameObject gameObject = GameObject.Find("NetworkMonitor");
		networkMonitorCh0 = new NetworkMonitor(0, gameObject.transform.Find("Ch0").transform);
		networkMonitorCh1 = new NetworkMonitor(1, gameObject.transform.Find("Ch1").transform);
		string text = GamePrefs.GetString(EnumGamePrefs.DebugPanelsEnabled);
		if (text == null || text == "-")
		{
			text = ",Ge,Fo,Pr,";
			if (!GameManager.Instance.IsEditMode())
			{
				text += "Ply,";
			}
			if (!GameManager.Instance.IsEditMode())
			{
				text += "Sp,";
			}
			if (GameManager.Instance.IsEditMode())
			{
				text += "Se,";
			}
		}
		Panels.Add(new PanelDefinition("Player", "Ply", showDebugPanel_Player, text));
		Panels.Add(new PanelDefinition("General", "Ge", showDebugPanel_General, text));
		Panels.Add(new PanelDefinition("Spawning", "Sp", showDebugPanel_Spawning, text, SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer));
		Panels.Add(new PanelDefinition("Chunk", "Ch", showDebugPanel_Chunk, text));
		Panels.Add(new PanelDefinition("Cache", "Ca", showDebugPanel_Cache, text));
		Panels.Add(new PanelDefinition("Focused Block", "Fo", showDebugPanel_FocusedBlock, text));
		Panels.Add(new PanelDefinition("Network", "Ne", showDebugPanel_Network, text));
		Panels.Add(new PanelDefinition("Selection", "Se", showDebugPanel_Selection, text));
		Panels.Add(new PanelDefinition("Prefab", "Pr", showDebugPanel_Prefab, text));
		Panels.Add(new PanelDefinition("Stealth", "St", showDebugPanel_Stealth, text));
		Panels.Add(new PanelDefinition("Player Extended - Buffs and CVars", "Plx", showDebugPanel_PlayerEffectInfo, text));
		Panels.Add(new PanelDefinition("Texture", "Te", showDebugPanel_Texture, text));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		playerEntity = playerUI.entityPlayer;
		playerUI.OnEntityPlayerLocalAssigned += HandleEntityPlayerLocalAssigned;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		playerUI.OnEntityPlayerLocalAssigned -= HandleEntityPlayerLocalAssigned;
		string text = ",";
		foreach (PanelDefinition panel in Panels)
		{
			if (panel.Active)
			{
				text = text + panel.ButtonCaption + ",";
			}
		}
		GamePrefs.Set(EnumGamePrefs.DebugPanelsEnabled, text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnWorldChanged -= HandleWorldChanged;
		}
		networkMonitorCh0.Cleanup();
		networkMonitorCh1.Cleanup();
	}

	public void ToggleDisplay()
	{
		if (debugData == EDebugDataType.Off)
		{
			debugData = EDebugDataType.General;
		}
		else
		{
			debugData = EDebugDataType.Off;
		}
	}

	public void ShowGeneralData()
	{
		debugData = EDebugDataType.General;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleWorldChanged(World _world)
	{
		debugData = EDebugDataType.Off;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleEntityPlayerLocalAssigned(EntityPlayerLocal _entity)
	{
		playerEntity = _entity;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnGUI()
	{
		if (!GameManager.Instance.gameStateManager.IsGameStarted() || !playerUI.windowManager.IsHUDEnabled())
		{
			return;
		}
		if (guiStyleToggleBox == null)
		{
			guiStyleToggleBox = new GUIStyle(GUI.skin.toggle)
			{
				wordWrap = false,
				padding = new RectOffset(17, 0, 3, 0)
			};
			guiStyleTooltipLabel = new GUIStyle(GUI.skin.label)
			{
				wordWrap = false,
				clipping = TextClipping.Overflow
			};
			guiStyleLabelRightAligned = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.UpperRight
			};
		}
		if (GameStats.GetInt(EnumGameStats.GameState) == 1)
		{
			GUI.color = Color.white;
			if (debugData != EDebugDataType.Off)
			{
				panelManager();
			}
			if (performanceType == EPerformanceDisplayType.FpsAndHeat)
			{
				debugShowHeatValue();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (GameStats.GetInt(EnumGameStats.GameState) != 0 && !GameManager.IsDedicatedServer)
		{
			networkMonitorCh0.Update();
			networkMonitorCh1.Update();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void debugShowChunkCache()
	{
		float num = (float)Screen.width / 2f;
		float middleY = (float)Screen.height / 2f;
		for (int i = 0; i < GameManager.Instance.World.ChunkClusters.Count; i++)
		{
			GameManager.Instance.World.ChunkClusters[i]?.DebugOnGUI(num + (float)(100 * i), middleY, 8f);
		}
		GameManager.Instance.World.m_ChunkManager.DebugOnGUI(num, middleY, 8);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void debugShowHeatValue()
	{
		if (!GameStats.GetBool(EnumGameStats.ZombieHordeMeter) || GameManager.Instance == null || GameManager.Instance.World?.aiDirector == null)
		{
			return;
		}
		Vector2i vector2i = new Vector2i(Screen.width, Screen.height);
		if (lastResolution != vector2i)
		{
			lastResolution = vector2i;
			boxStyle = new GUIStyle(GUI.skin.box);
			boxStyle.alignment = TextAnchor.MiddleLeft;
			int num = 13;
			if (vector2i.y > 1200)
			{
				num = vector2i.y / 90;
			}
			boxStyle.fontSize = num;
			boxAreaHeight = num + 10;
			boxAreaWidth = num * 22;
		}
		Vector3i position = World.worldToBlockPos(playerEntity.GetPosition());
		int num2 = World.toChunkXZ(position.x);
		int num3 = World.toChunkXZ(position.z);
		AIDirectorChunkEventComponent component = GameManager.Instance.World.aiDirector.GetComponent<AIDirectorChunkEventComponent>();
		AIDirectorChunkData chunkDataFromPosition = component.GetChunkDataFromPosition(position, _createIfNeeded: false);
		string text = $"Heat act {component.GetActiveCount()}";
		float num4 = 0f;
		if (chunkDataFromPosition != null)
		{
			num4 = chunkDataFromPosition.ActivityLevel;
			text += $", ch {num2} {num3}, {num4:F2}%, {chunkDataFromPosition.EventCount} evs";
			if (chunkDataFromPosition.cooldownDelay > 0f)
			{
				text += $", {chunkDataFromPosition.cooldownDelay} cd";
			}
		}
		Color color = ((num4 >= 90f) ? new Color(1f, 0.5f, 0.5f) : ((!(num4 >= 50f)) ? Color.green : Color.yellow));
		GUI.color = color;
		float y = (float)(Screen.height / 2 + 48) + 18f * GamePrefs.GetFloat(EnumGamePrefs.OptionsUiFpsScaling);
		Rect position2 = new Rect(14f, y, boxAreaWidth, boxAreaHeight);
		GUI.Box(position2, text, boxStyle);
		if (chunkDataFromPosition != null)
		{
			GUI.color = new Color(0.9f, 0.9f, 0.9f);
			int num5 = Utils.FastMin(10, chunkDataFromPosition.EventCount);
			for (int i = 0; i < num5; i++)
			{
				position2.y += boxAreaHeight + 1;
				AIDirectorChunkEvent aIDirectorChunkEvent = chunkDataFromPosition.GetEvent(i);
				GUI.Box(position2, $"{i + 1} {aIDirectorChunkEvent.EventType} ({aIDirectorChunkEvent.Position}) {aIDirectorChunkEvent.Value:F2} {aIDirectorChunkEvent.Duration}", boxStyle);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_EnablePanels(int x, int y)
	{
		int num = 6;
		int num2 = ((Panels.Count != 0) ? ((Panels.Count - 1) / num + 1) : 0);
		GUI.Box(new Rect(x, y - 1, 250f, 21 * num2 + 4), "");
		x += 5;
		int num3 = x;
		GUI.color = Color.yellow;
		for (int i = 0; i < Panels.Count; i++)
		{
			PanelDefinition panelDefinition = Panels[i];
			if (!panelDefinition.Enabled)
			{
				GUI.enabled = false;
			}
			panelDefinition.Active = GUI.Toggle(new Rect(x, y + 1, 38f, 20f), panelDefinition.Active, new GUIContent(panelDefinition.ButtonCaption, panelDefinition.Name), guiStyleToggleBox);
			GUI.enabled = true;
			x += 40;
			if (i % num == 5)
			{
				y += 21;
				x = num3;
			}
		}
		if (Panels.Count % num != 0)
		{
			y += 21;
		}
		GUI.color = Color.white;
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PanelBoxWithHeader(EGuiState _guiState, int _x, ref int _y, int _boxWidth, int _boxHeight, string _boxCaption)
	{
		if (_guiState == EGuiState.Draw)
		{
			GUI.Box(new Rect(_x, _y, _boxWidth, _boxHeight), "");
			HeaderLabel(_guiState, _x, _y, _boxCaption);
		}
		_y += 21;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HeaderLabel(EGuiState _guiState, int _x, int _y, string _text, int _labelWidth = 200, int _labelHeight = 25)
	{
		if (_guiState == EGuiState.Draw)
		{
			GUI.color = Color.yellow;
			GUI.Label(new Rect(_x + 5, _y, _labelWidth, _labelHeight), _text);
			GUI.color = Color.white;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LabelWithOutline(int _x, int _y, string _text, int _labelWidth = 200, int _labelHeight = 25, int _xOffset = 5)
	{
		Utils.DrawOutline(new Rect(_x + _xOffset, _y, _labelWidth, _labelHeight), _text, guiStyleDebug, Color.black, Color.white);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GreenWithOutline(int _x, int _y, string _text, int _labelWidth = 200, int _labelHeight = 25, int _xOffset = 5)
	{
		Utils.DrawOutline(new Rect(_x + _xOffset, _y, _labelWidth, _labelHeight), _text, guiStyleDebug, Color.black, Color.green);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BlueWithOutline(int _x, int _y, string _text, int _labelWidth = 200, int _labelHeight = 25, int _xOffset = 5)
	{
		Utils.DrawOutline(new Rect(_x + _xOffset, _y, _labelWidth, _labelHeight), _text, guiStyleDebug, Color.black, Color.cyan);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FakeTextField(int _x, int _y, int _width, int _height, string _text)
	{
		GUI.Box(new Rect(_x, _y, _width, _height), "", GUI.skin.textField);
		GUI.Label(new Rect(_x + 3, _y + 3, _width, _height), _text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_Player(int x, int y)
	{
		int num = 0;
		int num2 = y;
		int boxWidth = 340;
		for (EGuiState eGuiState = EGuiState.CalcSize; eGuiState < EGuiState.Count; eGuiState++)
		{
			y = num2;
			PanelBoxWithHeader(eGuiState, x, ref y, boxWidth, num - num2 + 5, "Player");
			EntityPlayer entityPlayer = playerEntity;
			if (entityPlayer == null)
			{
				return y;
			}
			if (eGuiState == EGuiState.Draw)
			{
				float num3 = Time.time - lastPlayerTime;
				if (num3 >= 0.5f)
				{
					playerSpeed = (entityPlayer.position - lastPlayerPos).magnitude / num3;
					lastPlayerPos = entityPlayer.position;
					lastPlayerTime = Time.time;
				}
				LabelWithOutline(x, y, $"X/Y/Z: {entityPlayer.position.x:F1}/{entityPlayer.position.y:F1}/{entityPlayer.position.z:F1}, Speed {playerSpeed:F3}");
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, $"Rot: {entityPlayer.rotation.x:F1}/{entityPlayer.rotation.y:F1}/{entityPlayer.rotation.z:F1}");
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				string text = string.Empty;
				string text2 = string.Empty;
				BiomeDefinition biomeStandingOn = entityPlayer.biomeStandingOn;
				if (biomeStandingOn != null)
				{
					text = biomeStandingOn.m_sBiomeName;
					IBiomeProvider biomeProvider = entityPlayer.world.ChunkCache.ChunkProvider.GetBiomeProvider();
					if (biomeProvider != null)
					{
						Vector3i blockPosition = entityPlayer.GetBlockPosition();
						int subBiomeIdxAt = biomeProvider.GetSubBiomeIdxAt(biomeStandingOn, blockPosition.x, blockPosition.y, blockPosition.z);
						if (subBiomeIdxAt >= 0)
						{
							text2 = $", sub {subBiomeIdxAt}";
						}
					}
				}
				LabelWithOutline(x, y, "Biome: " + text + text2);
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				PrefabInstance pOIAtPosition = entityPlayer.world.GetPOIAtPosition(entityPlayer.position, _checkTags: false);
				string text3 = ((pOIAtPosition == null) ? string.Empty : $"{pOIAtPosition.name}, {pOIAtPosition.boundingBoxPosition}, r {pOIAtPosition.rotation}, sl {pOIAtPosition.sleeperVolumes.Count}, tr {pOIAtPosition.triggerVolumes.Count}");
				LabelWithOutline(x, y, "POI: " + text3);
				y += 16;
				string text4 = ((pOIAtPosition == null) ? string.Empty : pOIAtPosition.GetPositionRelativeToPoi(Vector3i.Floor(entityPlayer.position)).ToString());
				LabelWithOutline(x, y, "X/Y/Z in prefab: " + text4);
				y += 16;
			}
			else
			{
				y += 32;
			}
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, $"DM Threat Lvl: {playerEntity.ThreatLevel.Category.ToStringCached()} : {playerEntity.ThreatLevel.Numeric:0.##}" + $", Zeds: {ThreatLevelUtility.Zombies}, Targeting: {ThreatLevelUtility.Targeting}");
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, string.Format("TemperatureF: Outside {0:0.0}, Core {1:0.0}, Absorb {2:0.0}", entityPlayer.PlayerStats.GetOutsideTemperature(), entityPlayer.Buffs.GetCustomVar("_coretemp"), entityPlayer.Buffs.GetCustomVar("_degreesabsorbed")));
			}
			y += 16;
			num = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_PlayerEffectInfo(int x, int y)
	{
		int result = y;
		EntityAlive entityAlive = playerEntity;
		if (InputUtils.ShiftKeyPressed)
		{
			Ray ray = playerEntity.GetLookRay();
			if (GameManager.Instance.IsEditMode() && GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 4)
			{
				ray = playerEntity.cameraTransform.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
				ray.origin += Origin.position;
			}
			ray.origin += ray.direction.normalized * 0.1f;
			float distance = Utils.FastMax(Utils.FastMax(Constants.cDigAndBuildDistance, Constants.cCollectItemDistance), 30f);
			int hitMask = 69;
			if (Voxel.Raycast(GameManager.Instance.World, ray, distance, -555528213, hitMask, 0f))
			{
				Transform hitRootTransform = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);
				if (hitRootTransform != null)
				{
					entityAlive = hitRootTransform.gameObject.GetComponent<Entity>() as EntityAlive;
				}
			}
		}
		if (entityAlive == null)
		{
			return result;
		}
		int num = entityAlive.Buffs.ActiveBuffs.Count + Mathf.Min(25, entityAlive.Buffs.CountCustomVars()) * 16;
		num += 96;
		num += 15;
		int num2 = 440;
		float num3 = Utils.FastClamp((float)Screen.height / 1080f * GameOptionsManager.GetActiveUiScale(), 0.4f, 2f);
		x = (int)((float)Screen.width / num3) - (num2 + 16);
		y = 64;
		PanelBoxWithHeader(EGuiState.Draw, x, ref y, num2, num, entityAlive.EntityName + " Buffs (" + entityAlive.Buffs.ActiveBuffs.Count + ")");
		for (int i = 0; i < entityAlive.Buffs.ActiveBuffs.Count; i++)
		{
			BuffValue buffValue = entityAlive.Buffs.ActiveBuffs[i];
			BuffClass buffClass = buffValue.BuffClass;
			GUI.color = buffClass?.IconColor ?? Color.magenta;
			Entity entity = GameManager.Instance.World.GetEntity(buffValue.InstigatorId);
			string text = $"none (id {buffValue.InstigatorId})";
			string text2 = buffValue.BuffName + " : From " + (entity ? entity.GetDebugName() : text) + " " + (entity ? entity.entityId.ToString() : "");
			if (buffClass == null)
			{
				text2 += " : BuffClass Missing";
			}
			LabelWithOutline(x, y, text2);
			GUI.color = Color.white;
			y += 16;
		}
		y += 21;
		HeaderLabel(EGuiState.Draw, x, y, entityAlive.EntityName + " CVars (" + entityAlive.Buffs.CountCustomVars() + ")");
		GUI.Label(new Rect(x + 150, y, 50f, 25f), "Filter:", guiStyleLabelRightAligned);
		if (Cursor.visible)
		{
			filterCVar = GUI.TextField(new Rect(x + 205, y, 200f, 25f), filterCVar);
		}
		else
		{
			FakeTextField(x + 205, y, 200, 25, filterCVar);
		}
		y += 21;
		int num4 = y;
		int num5 = 1;
		num = -1;
		foreach (var (arg, num7) in entityAlive.Buffs.EnumerateCustomVars(filterCVar))
		{
			if (num7 == 0f)
			{
				continue;
			}
			LabelWithOutline(x, y, $"{arg} : {num7}");
			if (num5 % 25 == 0)
			{
				x += 220;
				if (num == -1)
				{
					num = y + 16 + 5;
				}
				y = num4;
			}
			else
			{
				y += 16;
			}
			num5++;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_DynamicMusicInfo(int x, int y)
	{
		int num = 0;
		int num2 = y;
		int boxWidth = 220;
		for (EGuiState eGuiState = EGuiState.CalcSize; eGuiState < EGuiState.Count; eGuiState++)
		{
			y = num2;
			PanelBoxWithHeader(eGuiState, x, ref y, boxWidth, num - num2 + 5, "Dynamic Music");
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, "SomeStringData");
			}
			y += 16;
			num = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_Spawning(int x, int y)
	{
		int count = GameManager.Instance.World.Last4Spawned.Count;
		int boxHeight = 21 + count * 16 + 5;
		PanelBoxWithHeader(EGuiState.Draw, x, ref y, 325, boxHeight, "Spawning");
		for (int num = count - 1; num >= 0; num--)
		{
			SSpawnedEntity sSpawnedEntity = GameManager.Instance.World.Last4Spawned[num];
			LabelWithOutline(x, y, $"{sSpawnedEntity.name}:{sSpawnedEntity.pos} - {sSpawnedEntity.distanceToLocalPlayer:F1}m", 300);
			y += 16;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_Chunk(int x, int y)
	{
		EntityPlayer entityPlayer = playerEntity;
		if (entityPlayer == null)
		{
			return y;
		}
		int x2 = entityPlayer.chunkPosAddedEntityTo.x;
		int z = entityPlayer.chunkPosAddedEntityTo.z;
		Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkSync(x2, z);
		if (chunk == null)
		{
			return y;
		}
		Vector3i vector3i = Chunk.ToAreaMasterChunkPos(chunk.ToWorldPos(Vector3i.zero));
		Chunk chunk2 = (Chunk)GameManager.Instance.World.GetChunkSync(vector3i.x, vector3i.z);
		int num = 0;
		int num2 = y;
		int boxWidth = 550;
		for (EGuiState eGuiState = EGuiState.CalcSize; eGuiState < EGuiState.Count; eGuiState++)
		{
			y = num2;
			PanelBoxWithHeader(eGuiState, x, ref y, boxWidth, num - num2 + 5, "Chunk");
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, RegionFileManager.DebugUtil.GetLocationString(chunk.X, chunk.Z));
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				string text = "";
				int num3 = 0;
				ChunkAreaBiomeSpawnData chunkAreaBiomeSpawnData = chunk2?.GetChunkBiomeSpawnData();
				if (chunkAreaBiomeSpawnData != null)
				{
					text = chunkAreaBiomeSpawnData.poiTags.ToString();
					num3 = chunkAreaBiomeSpawnData.groupsEnabledFlags;
				}
				LabelWithOutline(x, y, $"AreaMaster: {vector3i.x}/{vector3i.z} {text} {num3:x}");
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				ChunkAreaBiomeSpawnData chunkAreaBiomeSpawnData2 = chunk2?.GetChunkBiomeSpawnData();
				LabelWithOutline(x, y, ((chunkAreaBiomeSpawnData2 != null) ? chunkAreaBiomeSpawnData2.ToString() : string.Empty) ?? "");
			}
			y += 16;
			string text2 = "Tris sum: " + chunk.GetTris();
			int num4 = 0;
			int num5 = 0;
			while (eGuiState == EGuiState.Draw && num5 < MeshDescription.meshes.Length)
			{
				text2 = text2 + " [" + num5 + "]: " + chunk.GetTrisInMesh(num5);
				num4 += chunk.GetSizeOfMesh(num5);
				num5++;
			}
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, $"{text2} Size: {num4 / 1024}kB", 300);
			}
			y += 16;
			num = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_Cache(int x, int y)
	{
		int num = 0;
		int num2 = y;
		int boxWidth = 550;
		for (EGuiState eGuiState = EGuiState.CalcSize; eGuiState < EGuiState.Count; eGuiState++)
		{
			y = num2;
			PanelBoxWithHeader(eGuiState, x, ref y, boxWidth, num - num2 + 5, "Cache");
			if (eGuiState == EGuiState.Draw)
			{
				int displayedChunkGameObjectsCount = GameManager.Instance.World.m_ChunkManager.GetDisplayedChunkGameObjectsCount();
				int count = GameManager.Instance.World.m_ChunkManager.GetFreeChunkGameObjects().Count;
				LabelWithOutline(x, y, $"CGO: {ChunkGameObject.InstanceCount} Displayed: {displayedChunkGameObjectsCount} Free: {count}");
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, MemoryPools.GetDebugInfo());
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, MemoryPools.GetDebugInfoEx());
			}
			y += 16;
			num = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_General(int x, int y)
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return y;
		}
		int num = 0;
		int num2 = y;
		int boxWidth = 220;
		for (EGuiState eGuiState = EGuiState.CalcSize; eGuiState < EGuiState.Count; eGuiState++)
		{
			y = num2;
			PanelBoxWithHeader(eGuiState, x, ref y, boxWidth, num - num2 + 5, "General");
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, "Seed='" + (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? GamePrefs.GetString(EnumGamePrefs.GameName) : GamePrefs.GetString(EnumGamePrefs.GameNameClient)) + "' '" + GamePrefs.GetString(EnumGamePrefs.GameWorld) + "'");
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, $"Time scale: {Time.timeScale}");
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				int entityAliveCount = world.GetEntityAliveCount(EntityFlags.Animal, EntityFlags.Animal);
				int entityAliveCount2 = world.GetEntityAliveCount(EntityFlags.Bandit, EntityFlags.Bandit);
				int entityAliveCount3 = world.GetEntityAliveCount(EntityFlags.Zombie, EntityFlags.Zombie);
				LabelWithOutline(x, y, $"World Ent: {world.Entities.Count} ({Entity.InstanceCount}) An: {entityAliveCount} Ban: {entityAliveCount2} Zom: {entityAliveCount3}");
			}
			y += 16;
			PathFinderThread instance = PathFinderThread.Instance;
			if (instance != null)
			{
				if (eGuiState == EGuiState.Draw)
				{
					LabelWithOutline(x, y, $"Paths: q {instance.GetQueueCount()}, finish {instance.GetFinishedCount()}");
				}
				y += 16;
			}
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, $"Memory used: {GC.GetTotalMemory(forceFullCollection: false) / 1048576}MB");
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, $"Active threads: {ThreadManager.ActiveThreads.Count} tasks: {ThreadManager.QueuedCount}");
			}
			y += 16;
			if (!world.IsRemote())
			{
				if (eGuiState == EGuiState.Draw)
				{
					LabelWithOutline(x, y, "Ticked blocks: " + world.GetWBT().GetCount());
				}
				y += 16;
			}
			num = y;
		}
		return y + 10;
	}

	public int showDebugPanel_FocusedBlock(int x, int y)
	{
		return showDebugPanel_FocusedBlock(x, y, false);
	}

	public int showDebugPanel_FocusedBlock(int x, int y, bool forceFocusedBlock = false)
	{
		EntityPlayer entityPlayer = playerEntity;
		if (entityPlayer == null)
		{
			return y;
		}
		if (entityPlayer == null || entityPlayer.inventory.holdingItemData == null || !entityPlayer.inventory.holdingItemData.hitInfo.bHitValid)
		{
			return y;
		}
		WorldRayHitInfo hitInfo = entityPlayer.inventory.holdingItemData.hitInfo;
		Vector3i vector3i = ((InputUtils.ShiftKeyPressed || forceFocusedBlock) ? hitInfo.hit.blockPos : hitInfo.lastBlockPos);
		BlockFace blockFace = hitInfo.hit.blockFace;
		if (vector3i.y < 0 || vector3i.y >= 256)
		{
			return y;
		}
		ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[hitInfo.hit.clrIdx];
		if (chunkCluster == null)
		{
			return y;
		}
		Chunk chunk = (Chunk)chunkCluster.GetChunkFromWorldPos(vector3i);
		if (chunk == null)
		{
			return y;
		}
		Vector3i vector3i2 = World.toBlock(vector3i);
		int x2 = vector3i2.x;
		int y2 = vector3i2.y;
		int z = vector3i2.z;
		BlockValue block = chunk.GetBlock(vector3i2);
		Block block2 = block.Block;
		BlockShape shape = block2.shape;
		BlockFace rotatedBlockFace = shape.GetRotatedBlockFace(block, blockFace);
		string[] array = new string[1];
		int[] array2 = new int[1];
		for (int i = 0; i < 1; i++)
		{
			array2[i] = GameManager.Instance.World.ChunkClusters[0].GetBlockFaceTexture(vector3i, rotatedBlockFace, i);
			if (array2[i] == 0)
			{
				array2[i] = GameUtils.FindPaintIdForBlockFace(block, rotatedBlockFace, out var _name, i);
				array[i] = _name;
			}
			else
			{
				array[i] = ((array2[i] < 0 || array2[i] >= BlockTextureData.list.Length) ? string.Empty : (BlockTextureData.list[array2[i]]?.Name ?? "N/A"));
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int j = 0; j < 1; j++)
		{
			bool flag = false;
			if (array2[0] >= 0 && array2[0] < BlockTextureData.list.Length && block2.MeshIndex == 0)
			{
				int num = BlockTextureData.list[array2[0]]?.TextureID ?? 0;
				num = ((num == 0) ? block2.GetSideTextureId(block, rotatedBlockFace, 0) : num);
				flag = MeshDescription.meshes[0].textureAtlas.uvMapping[num].bGlobalUV;
				if (rotatedBlockFace != BlockFace.None)
				{
					switch (block2.GetUVMode((int)rotatedBlockFace, j))
					{
					case Block.UVMode.Global:
						flag = true;
						break;
					case Block.UVMode.Local:
						flag = false;
						break;
					}
				}
			}
			if (j > 0)
			{
				stringBuilder.Append(",");
			}
			stringBuilder.Append(flag ? "G" : "L");
		}
		int num2 = 0;
		int num3 = y;
		int boxWidth = 260;
		for (EGuiState eGuiState = EGuiState.CalcSize; eGuiState < EGuiState.Count; eGuiState++)
		{
			y = num3;
			PanelBoxWithHeader(eGuiState, x, ref y, boxWidth, num2 - num3 + 5, "Focused Block");
			if (eGuiState == EGuiState.Draw)
			{
				int y3 = y;
				Vector3i vector3i3 = vector3i;
				LabelWithOutline(x, y3, "Pos: " + vector3i3.ToString());
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				if (block.isair && entityPlayer.inventory.holdingItemData is ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData && !itemBlockInventoryData.itemValue.ToBlockValue().Block.shape.IsTerrain())
				{
					BlockValue blockValue = itemBlockInventoryData.itemValue.ToBlockValue();
					blockValue.rotation = itemBlockInventoryData.rotation;
					int y4 = y;
					BlockValue blockValue2 = blockValue;
					LabelWithOutline(x, y4, "Data: " + blockValue2.ToString());
				}
				else
				{
					int y5 = y;
					BlockValue blockValue2 = block;
					LabelWithOutline(x, y5, "Data: " + blockValue2.ToString());
				}
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, "Name: " + block2.GetBlockName() + " (W=" + blockFace.ToStringCached() + "->B=" + rotatedBlockFace.ToStringCached() + ")");
			}
			y += 16;
			for (int k = 0; k < 1; k++)
			{
				if (eGuiState == EGuiState.Draw)
				{
					LabelWithOutline(x, y, $"Paint Id: {array2[k]} ({array[k]})");
				}
				y += 16;
			}
			if (eGuiState == EGuiState.Draw)
			{
				if (shape is BlockShapeModelEntity blockShapeModelEntity)
				{
					BlueWithOutline(x, y, "Prefab: " + blockShapeModelEntity.modelName);
					if (Input.GetKeyDown(KeyCode.RightControl))
					{
						GUIUtility.systemCopyBuffer = blockShapeModelEntity.modelName;
					}
				}
				else
				{
					GreenWithOutline(x, y, $"Shape: {shape.GetName()} V={shape.GetVertexCount()} T={shape.GetTriangleCount()} UV: {stringBuilder.ToString()}");
					if (Input.GetKeyDown(KeyCode.RightControl))
					{
						string oldValue = "@:Shapes/";
						string oldValue2 = ".fbx";
						GUIUtility.systemCopyBuffer = shape.GetName().Replace(oldValue, "").Replace(oldValue2, "");
					}
				}
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, "Light: emit=" + block2.GetLightValue(block) + " opac=" + block2.lightOpacity + " sun=" + chunk.GetLight(x2, y2, z, Chunk.LIGHT_TYPE.SUN) + " blk=" + chunk.GetLight(x2, y2, z, Chunk.LIGHT_TYPE.BLOCK));
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, "Stability: " + chunk.GetStability(x2, y2, z) + " Density: " + chunk.GetDensity(x2, y2, z).ToString("0.00") + " ");
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, "Height: " + chunk.GetHeight(x2, z) + " Terrain: " + chunk.GetTerrainHeight(x2, z) + " Deco: " + chunk.GetDecoAllowedAt(x2, z).ToStringFriendlyCached());
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				string text = "Normal: " + GameManager.Instance.World.GetTerrainNormalAt(vector3i.x, vector3i.z).ToCultureInvariantString();
				int mass = chunk.GetWater(x2, y2, z).GetMass();
				if (mass > 0)
				{
					text = text + " Water: " + mass;
				}
				LabelWithOutline(x, y, text);
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				if (block2.HasTileEntity)
				{
					TileEntity tileEntity = chunk.GetTileEntity(vector3i2);
					if (!(tileEntity is TileEntitySecureDoor) && tileEntity.TryGetSelfOrFeature<ITileEntityLootable>(out var _typedTe))
					{
						LabelWithOutline(x, y, "LootStage: " + entityPlayer.GetLootStage(_typedTe.LootStageMod, _typedTe.LootStageBonus));
					}
					else
					{
						LabelWithOutline(x, y, "LootStage: " + entityPlayer.GetLootStage(0f, 0f));
					}
				}
				else
				{
					LabelWithOutline(x, y, "LootStage: " + entityPlayer.GetLootStage(0f, 0f));
				}
			}
			y += 16;
			num2 = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_Network(int x, int y)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() == 0)
		{
			return y;
		}
		int num = 0;
		int num2 = y;
		int boxWidth = 220;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		for (EGuiState eGuiState = EGuiState.CalcSize; eGuiState < EGuiState.Count; eGuiState++)
		{
			y = num2;
			PanelBoxWithHeader(eGuiState, x, ref y, boxWidth, num - num2 + 5, "Network");
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (eGuiState == EGuiState.Draw)
				{
					LabelWithOutline(x, y, "Clients: " + SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount());
				}
				y += 16;
				if (eGuiState == EGuiState.Draw)
				{
					string arg = string.Format("{0}{1}B", (num3 > 1024) ? ((float)num3 / 1024f).ToCultureInvariantString("0.0") : num3.ToString(), (num3 > 1024) ? "k" : "");
					LabelWithOutline(x, y, string.Format("   total sent: #{1,3}  {0}", arg, num5));
				}
				y += 16;
				if (eGuiState == EGuiState.Draw)
				{
					string arg2 = string.Format("{0}{1}B", (num4 > 1024) ? ((float)num4 / 1024f).ToCultureInvariantString("0.0") : num4.ToString(), (num4 > 1024) ? "k" : "");
					LabelWithOutline(x, y, string.Format("   total recv: #{1,3}  {0}", arg2, num6));
				}
				y += 16;
				int _bytesPerSecondSent = 0;
				int _bytesPerSecondReceived = 0;
				int _packagesPerSecondSent = 0;
				int _packagesPerSecondReceived = 0;
				foreach (ClientInfo item in SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List)
				{
					if (eGuiState == EGuiState.Draw)
					{
						LabelWithOutline(x, y, $"Client {item.InternalId.CombinedString,2}");
					}
					y += 16;
					if (eGuiState == EGuiState.CalcSize)
					{
						item.netConnection[0].GetStats().GetStats(0.5f, out _bytesPerSecondSent, out _packagesPerSecondSent, out _bytesPerSecondReceived, out _packagesPerSecondReceived);
						num3 += _bytesPerSecondSent;
						num4 += _bytesPerSecondReceived;
						num5 += _packagesPerSecondSent;
						num6 += _packagesPerSecondReceived;
					}
					if (eGuiState == EGuiState.Draw)
					{
						item.netConnection[0].GetStats().GetStats(0.5f, out _bytesPerSecondSent, out _packagesPerSecondSent, out _bytesPerSecondReceived, out _packagesPerSecondReceived);
						string arg3 = string.Format("{0}{1}B", (_bytesPerSecondSent > 1024) ? ((float)_bytesPerSecondSent / 1024f).ToCultureInvariantString("0.0") : _bytesPerSecondSent.ToString(), (_bytesPerSecondSent > 1024) ? "k" : "");
						LabelWithOutline(x, y, string.Format("   stream0 sent: #{1,3}  {0}", arg3, _packagesPerSecondSent));
					}
					y += 16;
					if (eGuiState == EGuiState.Draw)
					{
						string arg4 = string.Format("{0}{1}B", (_bytesPerSecondReceived > 1024) ? ((float)_bytesPerSecondSent / 1024f).ToCultureInvariantString("0.0") : _bytesPerSecondReceived.ToString(), (_bytesPerSecondReceived > 1024) ? "k" : "");
						LabelWithOutline(x, y, string.Format("   stream0 rcvd: #{1,3}  {0}", arg4, _packagesPerSecondReceived));
					}
					y += 16;
					if (eGuiState == EGuiState.CalcSize)
					{
						item.netConnection[1].GetStats().GetStats(0.5f, out _bytesPerSecondSent, out _packagesPerSecondSent, out _bytesPerSecondReceived, out _packagesPerSecondReceived);
						num3 += _bytesPerSecondSent;
						num4 += _bytesPerSecondReceived;
						num5 += _packagesPerSecondSent;
						num6 += _packagesPerSecondReceived;
					}
					if (eGuiState == EGuiState.Draw)
					{
						item.netConnection[1].GetStats().GetStats(0.5f, out _bytesPerSecondSent, out _packagesPerSecondSent, out _bytesPerSecondReceived, out _packagesPerSecondReceived);
						string arg5 = string.Format("{0}{1}B", (_bytesPerSecondSent > 1024) ? ((float)_bytesPerSecondSent / 1024f).ToCultureInvariantString("0.0") : _bytesPerSecondSent.ToString(), (_bytesPerSecondSent > 1024) ? "k" : "");
						LabelWithOutline(x, y, string.Format("   stream1 sent: #{1,3}  {0}", arg5, _packagesPerSecondSent));
					}
					y += 16;
					if (eGuiState == EGuiState.Draw)
					{
						string arg6 = string.Format("{0}{1}B", (_bytesPerSecondReceived > 1024) ? ((float)_bytesPerSecondReceived / 1024f).ToCultureInvariantString("0.0") : _bytesPerSecondReceived.ToString(), (_bytesPerSecondReceived > 1024) ? "k" : "");
						LabelWithOutline(x, y, string.Format("   stream1 rcvd: #{1,3}  {0}", arg6, _packagesPerSecondReceived));
					}
					y += 16;
				}
			}
			num = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_Selection(int x, int y)
	{
		if (GameManager.Instance.GetActiveBlockTool() == null)
		{
			return y;
		}
		int num = 0;
		int num2 = y;
		int boxWidth = 220;
		for (EGuiState eGuiState = EGuiState.CalcSize; eGuiState < EGuiState.Count; eGuiState++)
		{
			y = num2;
			PanelBoxWithHeader(eGuiState, x, ref y, boxWidth, num - num2 + 5, "Selection");
			if (eGuiState == EGuiState.Draw)
			{
				string debugOutput = GameManager.Instance.GetActiveBlockTool().GetDebugOutput();
				LabelWithOutline(x, y, debugOutput);
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				if (XUiC_WoPropsSleeperVolume.GetSelectedVolumeStats(out var _stats))
				{
					LabelWithOutline(x, y, "Sleeper Volume");
					y += 16;
					LabelWithOutline(x, y, $"Index: {_stats.index}");
					y += 16;
					LabelWithOutline(x, y, $"Pos: {_stats.pos}");
					y += 16;
					LabelWithOutline(x, y, $"Size: {_stats.size}");
					y += 16;
					LabelWithOutline(x, y, "Group: " + _stats.groupName);
					y += 16;
					LabelWithOutline(x, y, $"Priority: {_stats.isPriority}   QuestExc: {_stats.isQuestExclude}");
					y += 16;
					LabelWithOutline(x, y, $"Sleepers: {_stats.sleeperCount}   MinMax: {_stats.spawnCountMin}-{_stats.spawnCountMax}");
					y += 16;
				}
			}
			else
			{
				y += 112;
			}
			num = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_Prefab(int x, int y)
	{
		PrefabInstance prefabInstance = GameManager.Instance.GetDynamicPrefabDecorator()?.ActivePrefab;
		if (prefabInstance == null)
		{
			return y;
		}
		Prefab.BlockStatistics blockStatistics = prefabInstance.prefab.GetBlockStatistics();
		int num = 0;
		int num2 = y;
		int boxWidth = 220;
		for (EGuiState eGuiState = EGuiState.CalcSize; eGuiState < EGuiState.Count; eGuiState++)
		{
			y = num2;
			PanelBoxWithHeader(eGuiState, x, ref y, boxWidth, num - num2 + 5, "Prefab");
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, "Name: " + prefabInstance.prefab.PrefabName);
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				int y2 = y;
				Vector3i boundingBoxPosition = prefabInstance.boundingBoxPosition;
				LabelWithOutline(x, y2, "Pos: " + boundingBoxPosition.ToString());
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				Vector3i vector3i = prefabInstance.prefab.size;
				SelectionBox selectionBox = SelectionBoxManager.Instance.Selection?.box;
				if (selectionBox != null)
				{
					vector3i = selectionBox.GetScale();
				}
				int y3 = y;
				Vector3i boundingBoxPosition = vector3i;
				LabelWithOutline(x, y3, "Size: " + boundingBoxPosition.ToString());
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, "Rot: " + prefabInstance.rotation, 70);
				LabelWithOutline(x, y, "RotToNorth: " + prefabInstance.prefab.rotationToFaceNorth, 130, 25, 75);
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, $"BEnts: {blockStatistics.cntBlockEntities} BMods: {blockStatistics.cntBlockModels} Wdws: {blockStatistics.cntWindows}");
			}
			y += 16;
			num = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_Stealth(int x, int y)
	{
		int num = 0;
		int num2 = y;
		int boxWidth = 220;
		for (EGuiState eGuiState = EGuiState.CalcSize; eGuiState < EGuiState.Count; eGuiState++)
		{
			y = num2;
			PanelBoxWithHeader(eGuiState, x, ref y, boxWidth, num - num2 + 5, "Stealth");
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			if (eGuiState == EGuiState.Draw)
			{
				float selfLight;
				float stealthLightLevel = LightManager.GetStealthLightLevel(primaryPlayer, out selfLight);
				LabelWithOutline(x, y, $"Player light: {(int)(stealthLightLevel * 100f)} + {(int)(selfLight * 100f)}");
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, $"Light: {primaryPlayer.Stealth.lightLevel}");
			}
			y += 16;
			if (eGuiState == EGuiState.Draw)
			{
				LabelWithOutline(x, y, $"Noise: {primaryPlayer.Stealth.noiseVolume}");
			}
			y += 16;
			num = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_Texture(int x, int y)
	{
		int num = 0;
		int num2 = y;
		int boxWidth = 220;
		for (EGuiState eGuiState = EGuiState.CalcSize; eGuiState < EGuiState.Count; eGuiState++)
		{
			y = num2;
			PanelBoxWithHeader(eGuiState, x, ref y, boxWidth, num - num2 + 5, "Texture");
			bool streamingMipmapsActive = QualitySettings.streamingMipmapsActive;
			if (streamingMipmapsActive)
			{
				if (eGuiState == EGuiState.Draw)
				{
					LabelWithOutline(x, y, $"Streaming mipmaps enabled: {streamingMipmapsActive}");
				}
				y += 16;
				if (eGuiState == EGuiState.Draw)
				{
					LabelWithOutline(x, y, $"Streaming budget: {QualitySettings.streamingMipmapsMemoryBudget} MB");
				}
				y += 16;
				if (eGuiState == EGuiState.Draw)
				{
					LabelWithOutline(x, y, $"Memory desired: {(double)Texture.desiredTextureMemory * 9.5367431640625E-07:F2} MB");
				}
				y += 16;
				if (eGuiState == EGuiState.Draw)
				{
					LabelWithOutline(x, y, $"Memory target: {(double)Texture.targetTextureMemory * 9.5367431640625E-07:F2} MB");
				}
				y += 16;
				if (eGuiState == EGuiState.Draw)
				{
					LabelWithOutline(x, y, $"Memory current: {(double)Texture.currentTextureMemory * 9.5367431640625E-07:F2} MB");
				}
				y += 16;
				if (eGuiState == EGuiState.Draw)
				{
					LabelWithOutline(x, y, $"Non-streamed memory: {(double)Texture.nonStreamingTextureMemory * 9.5367431640625E-07:F2} MB");
				}
				y += 16;
			}
			num = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void panelManager()
	{
		float num = (float)Screen.height / 1080f * GameOptionsManager.GetActiveUiScale();
		float num2 = Utils.FastClamp(num, 0.4f, 2f);
		Matrix4x4 matrix = GUI.matrix;
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(num2, num2, 1f));
		int num3 = (GameManager.Instance.IsEditMode() ? 365 : 247);
		num3 += 55;
		float num4 = num / num2;
		num3 = (int)((float)num3 * num4);
		num3 = showDebugPanel_EnablePanels(18, num3);
		for (int i = 0; i < Panels.Count; i++)
		{
			PanelDefinition panelDefinition = Panels[i];
			if (panelDefinition.Enabled && panelDefinition.Active)
			{
				num3 = panelDefinition.GuiHandler(18, num3);
			}
		}
		if (!string.IsNullOrEmpty(GUI.tooltip))
		{
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.y = (float)Screen.height - mousePosition.y;
			mousePosition /= num2;
			mousePosition.y -= 20f;
			GUI.color = Color.white;
			GUI.Label(new Rect(mousePosition.x, mousePosition.y, 100f, 20f), GUI.tooltip ?? "", guiStyleTooltipLabel);
		}
		GUI.matrix = matrix;
	}

	public void SetActivePanels(params string[] panelCaptions)
	{
		foreach (PanelDefinition panel in Panels)
		{
			bool active = false;
			foreach (string text in panelCaptions)
			{
				if (panel.ButtonCaption == text)
				{
					active = true;
					break;
				}
			}
			panel.Active = active;
		}
	}
}
