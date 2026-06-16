using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using InControl;
using UnityEngine;

public class GUIWindowScreenshotText : GUIWindow
{
	public static readonly string ID = "GUIWindowScreenshotText";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int PosX = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int PosY = 70;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Width = 700;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFirstTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public string noteInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool savePerks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool confirmed;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i lastResolution;

	[PublicizedFrom(EAccessModifier.Private)]
	public GUIStyle labelStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lineHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public int inputAreaHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public NGuiWdwDebugPanels nGuiWdwDebugPanels;

	public GUIWindowScreenshotText()
		: base(ID)
	{
		alwaysUsesMouseCursor = true;
	}

	public override void OnGUI()
	{
		base.OnGUI();
		Vector2i vector2i = new Vector2i(Screen.width, Screen.height);
		if (lastResolution != vector2i)
		{
			lastResolution = vector2i;
			labelStyle = new GUIStyle(GUI.skin.label)
			{
				wordWrap = true,
				fontStyle = FontStyle.Bold
			};
			lineHeight = 18;
			inputAreaHeight = lineHeight + 7;
		}
		float _targetScale;
		float _actualScale;
		Matrix4x4 matrix = GUIWindow.UiScaleMatrix(out _targetScale, out _actualScale);
		if (Event.current.type == EventType.KeyDown)
		{
			if (Event.current.keyCode == KeyCode.Escape)
			{
				CloseWindow();
			}
			else if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
			{
				DoScreenshot();
			}
		}
		float num = 20f;
		float num2 = 700f;
		float num3 = 0f;
		for (int i = 0; i < 2; i++)
		{
			float num4 = 70f;
			if (i == 1)
			{
				GUI.Box(new Rect(num, num4, num2, num3 - 70f + 5f), "");
			}
			if (GameManager.Instance.World != null)
			{
				GameUtils.WorldInfo worldInfo = GameManager.Instance.World.ChunkCache?.ChunkProvider?.WorldInfo;
				if (i == 1)
				{
					string text = "World: " + GamePrefs.GetString(EnumGamePrefs.GameWorld);
					Utils.DrawOutline(new Rect(num + 5f, num4, num2 - 10f, inputAreaHeight), text, labelStyle, Color.black, Color.white);
				}
				num4 += (float)lineHeight;
				if (!PrefabEditModeManager.Instance.IsActive() && worldInfo != null && worldInfo.RandomGeneratedWorld && worldInfo.DynamicProperties.Contains("Generation", "Seed"))
				{
					if (i == 1)
					{
						Utils.DrawOutline(new Rect(num + 5f, num4, num2 - 10f, inputAreaHeight), "World gen seed: " + worldInfo.DynamicProperties.GetString("Generation", "Seed"), labelStyle, Color.black, Color.white);
					}
					num4 += (float)lineHeight;
				}
				if (i == 1)
				{
					Utils.DrawOutline(new Rect(num + 5f, num4, num2 - 10f, inputAreaHeight), "Save name / deco seed: " + (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? GamePrefs.GetString(EnumGamePrefs.GameName) : GamePrefs.GetString(EnumGamePrefs.GameNameClient)) + " / " + GameManager.Instance.World.Seed, labelStyle, Color.black, Color.white);
				}
				num4 += (float)lineHeight;
				if (LocalPlayerUI.GetUIForPrimaryPlayer() != null)
				{
					EntityPlayer entityPlayer = LocalPlayerUI.GetUIForPrimaryPlayer().entityPlayer;
					if (entityPlayer != null)
					{
						PrefabInstance pOIAtPosition = entityPlayer.world.GetPOIAtPosition(entityPlayer.position);
						if (i == 1)
						{
							string text2 = $"Coordinates: {entityPlayer.position.x:F0} {entityPlayer.position.y:F0} {entityPlayer.position.z:F0}";
							if (pOIAtPosition != null)
							{
								text2 += $" / relative to POI: {pOIAtPosition.GetPositionRelativeToPoi(Vector3i.Floor(entityPlayer.position))}";
							}
							Utils.DrawOutline(new Rect(num + 5f, num4, num2 - 10f, inputAreaHeight), text2, labelStyle, Color.black, Color.white);
						}
						num4 += (float)lineHeight;
						if (pOIAtPosition != null)
						{
							string text3 = pOIAtPosition.prefab?.PrefabName ?? pOIAtPosition.name;
							string text4 = pOIAtPosition.prefab?.LocalizedEnglishName ?? "";
							if (i == 1)
							{
								Utils.DrawOutline(new Rect(num + 5f, num4, num2 - 10f, inputAreaHeight), "POI: " + text3 + " (" + text4 + ")", labelStyle, Color.black, Color.white);
							}
							num4 += (float)lineHeight;
						}
						pOIAtPosition = entityPlayer.world.GetPOIAtPosition(entityPlayer.position, FastTags<TagGroup.Poi>.none, DynamicPrefabDecorator.streetTileTag);
						if (pOIAtPosition != null)
						{
							string text5 = pOIAtPosition.prefab?.PrefabName ?? pOIAtPosition.name;
							string text6 = pOIAtPosition.prefab?.LocalizedEnglishName ?? "";
							if (i == 1)
							{
								Utils.DrawOutline(new Rect(num + 5f, num4, num2 - 10f, inputAreaHeight), "Tile: " + text5 + " (" + text6 + ")", labelStyle, Color.black, Color.white);
							}
							num4 += (float)lineHeight;
						}
						if (!confirmed)
						{
							if (i == 1)
							{
								savePerks = GUI.Toggle(new Rect(num + 5f, num4, num2 - 10f, lineHeight), savePerks, "Save Perks, Buffs and CVars");
							}
							num4 += (float)lineHeight;
						}
					}
				}
				num4 += (float)lineHeight;
			}
			if (!confirmed)
			{
				if (i == 1)
				{
					GUI.SetNextControlName("InputField");
					noteInput = GUI.TextField(new Rect(num + 5f, num4, num2 - 60f, inputAreaHeight), noteInput, 300);
					if (bFirstTime)
					{
						bFirstTime = false;
						GUI.FocusControl("InputField");
					}
					if (GUI.Button(new Rect(num + num2 - 50f, num4, 50f, inputAreaHeight), "Ok"))
					{
						DoScreenshot();
						return;
					}
				}
				num4 += (float)inputAreaHeight;
			}
			else
			{
				float num5 = labelStyle.CalcHeight(new GUIContent("Note: " + noteInput), num2 - 10f);
				if (i == 1)
				{
					Utils.DrawOutline(new Rect(num + 5f, num4, num2 - 10f, num5 + 4f), "Note: " + noteInput, labelStyle, Color.black, Color.white);
				}
				num4 += num5;
			}
			num3 = num4;
		}
		if (nGuiWdwDebugPanels == null)
		{
			nGuiWdwDebugPanels = UnityEngine.Object.FindAnyObjectByType<NGuiWdwDebugPanels>();
		}
		if (nGuiWdwDebugPanels != null)
		{
			nGuiWdwDebugPanels.showDebugPanel_FocusedBlock((int)num, (int)num3 + 10, forceFocusedBlock: true);
		}
		GUI.matrix = matrix;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CloseWindow()
	{
		windowManager.Close(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DoScreenshot()
	{
		confirmed = true;
		ThreadManager.StartCoroutine(screenshotCo(null));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator screenshotCo(string _filename)
	{
		yield return null;
		bool saved = true;
		yield return ThreadManager.CoroutineWrapperWithExceptionCallback(GameUtils.TakeScreenshotEnum(GameUtils.EScreenshotMode.Both, _filename), [PublicizedFrom(EAccessModifier.Internal)] (Exception _exception) =>
		{
			saved = false;
			Log.Exception(_exception);
		});
		if (saved && savePerks)
		{
			StoreAdditionalStats();
		}
		yield return null;
		CloseWindow();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StoreAdditionalStats()
	{
		string lastSavedScreenshotFilename = GameUtils.lastSavedScreenshotFilename;
		lastSavedScreenshotFilename = lastSavedScreenshotFilename.Substring(0, lastSavedScreenshotFilename.LastIndexOf('.'));
		if (GameManager.Instance.World != null && GameManager.Instance.World.GetPrimaryPlayer() != null)
		{
			StorePlayerStats(lastSavedScreenshotFilename);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StorePlayerStats(string _filenameBase)
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		_filenameBase += "_playerstats.csv";
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Level,{primaryPlayer.Progression.GetLevel()}");
		stringBuilder.AppendLine();
		writePlayerSkills(stringBuilder, primaryPlayer);
		writePlayerBuffs(stringBuilder, primaryPlayer);
		writePlayerCVars(stringBuilder, primaryPlayer);
		SdFile.WriteAllText(_filenameBase, stringBuilder.ToString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writePlayerSkills(StringBuilder _sb, EntityPlayerLocal _epl)
	{
		List<ProgressionValue> list = new List<ProgressionValue>();
		foreach (KeyValuePair<int, ProgressionValue> item in _epl.Progression.GetDict())
		{
			if (item.Value?.ProgressionClass?.Name != null)
			{
				list.Add(item.Value);
			}
		}
		list.Sort(ProgressionClass.ListSortOrderComparer.Instance);
		_sb.AppendLine("Skills");
		_sb.AppendLine("Name,Level,CalcLevel");
		foreach (ProgressionValue item2 in list)
		{
			ProgressionClass progressionClass = item2.ProgressionClass;
			if (progressionClass.IsAttribute && progressionClass.MaxLevel != 0)
			{
				_sb.AppendLine();
				_sb.AppendLine($"{progressionClass.Name},{item2.Level},{item2.CalculatedLevel(_epl)}");
			}
			else if (progressionClass.IsPerk)
			{
				_sb.AppendLine($" - {progressionClass.Name},{item2.Level},{item2.CalculatedLevel(_epl)}");
			}
		}
		_sb.AppendLine();
		_sb.AppendLine("Books");
		_sb.AppendLine("Name,Level,CalcLevel");
		foreach (ProgressionValue item3 in list)
		{
			ProgressionClass progressionClass2 = item3.ProgressionClass;
			if (progressionClass2.IsBook)
			{
				_sb.AppendLine($"{progressionClass2.Name},{item3.Level},{item3.CalculatedLevel(_epl)}");
			}
		}
		_sb.AppendLine();
		_sb.AppendLine("Crafting Skills");
		_sb.AppendLine("Name,Level,CalcLevel");
		foreach (ProgressionValue item4 in list)
		{
			ProgressionClass progressionClass3 = item4.ProgressionClass;
			if (progressionClass3.IsCrafting)
			{
				_sb.AppendLine($"{progressionClass3.Name},{item4.Level},{item4.CalculatedLevel(_epl)}");
			}
		}
		_sb.AppendLine();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writePlayerBuffs(StringBuilder _sb, EntityPlayerLocal _epl)
	{
		_sb.AppendLine("Buffs");
		_sb.AppendLine("Buff,FromName,FromId,Missing?");
		foreach (BuffValue activeBuff in _epl.Buffs.ActiveBuffs)
		{
			BuffClass buffClass = activeBuff.BuffClass;
			Entity entity = GameManager.Instance.World.GetEntity(activeBuff.InstigatorId);
			string text = $"none (id {activeBuff.InstigatorId})";
			_sb.AppendLine(activeBuff.BuffName + "," + (entity ? entity.GetDebugName() : text) + "," + (entity ? entity.entityId.ToString() : "") + "," + ((buffClass == null) ? "BuffClass missing" : ""));
		}
		_sb.AppendLine();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writePlayerCVars(StringBuilder _sb, EntityPlayerLocal _epl)
	{
		_sb.AppendLine("Buffs");
		_sb.AppendLine("Name,Value");
		foreach (var (arg, num2) in _epl.Buffs.EnumerateCustomVars())
		{
			if (num2 != 0f)
			{
				_sb.AppendLine($"{arg},{num2}");
			}
		}
		_sb.AppendLine();
	}

	public override void OnOpen()
	{
		confirmed = false;
		bFirstTime = true;
		noteInput = "";
		isInputActive = true;
		if (UIInput.selection != null)
		{
			UIInput.selection.isSelected = false;
		}
		InputManager.Enabled = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		isInputActive = false;
		InputManager.Enabled = true;
	}

	public static void Open(LocalPlayerUI _playerUi, bool _savePerks)
	{
		GUIWindowScreenshotText window = _playerUi.windowManager.GetWindow<GUIWindowScreenshotText>(ID);
		if (window != null)
		{
			window.savePerks = _savePerks;
			_playerUi.windowManager.Open(window, _bModal: false);
		}
	}
}
