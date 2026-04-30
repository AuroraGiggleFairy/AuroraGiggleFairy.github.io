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
	public GUIStyle checkboxStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public GUIStyle textfieldStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public GUIStyle buttonStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fontSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lineHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public int inputAreaHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public NGuiWdwDebugPanels nGuiWdwDebugPanels;

	public GUIWindowScreenshotText()
		: base(ID, new Rect(20f, 70f, Screen.width / 3, 1f))
	{
		alwaysUsesMouseCursor = true;
	}

	public override void OnGUI(bool _inputActive)
	{
		base.OnGUI(_inputActive);
		Vector2i vector2i = new Vector2i(Screen.width, Screen.height);
		if (lastResolution != vector2i)
		{
			lastResolution = vector2i;
			labelStyle = new GUIStyle(GUI.skin.label);
			checkboxStyle = new GUIStyle(GUI.skin.toggle);
			textfieldStyle = new GUIStyle(GUI.skin.textField);
			buttonStyle = new GUIStyle(GUI.skin.button);
			labelStyle.wordWrap = true;
			labelStyle.fontStyle = FontStyle.Bold;
			fontSize = vector2i.y / 54;
			lineHeight = fontSize + 3;
			inputAreaHeight = fontSize + 10;
			labelStyle.fontSize = fontSize;
			checkboxStyle.fontSize = fontSize;
			textfieldStyle.fontSize = fontSize;
			buttonStyle.fontSize = fontSize;
		}
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
		float xMin = base.windowRect.xMin;
		float yMin = base.windowRect.yMin;
		float width = base.windowRect.width;
		float num = 0f;
		for (int i = 0; i < 2; i++)
		{
			float num2 = yMin;
			if (i == 1)
			{
				GUI.Box(new Rect(xMin, num2, width, num - yMin + 5f), "");
			}
			if (GameManager.Instance.World != null)
			{
				GameUtils.WorldInfo worldInfo = GameManager.Instance.World?.ChunkCache?.ChunkProvider?.WorldInfo;
				if (i == 1)
				{
					string text = "World: " + GamePrefs.GetString(EnumGamePrefs.GameWorld);
					Utils.DrawOutline(new Rect(xMin + 5f, num2, width - 10f, inputAreaHeight), text, labelStyle, Color.black, Color.white);
				}
				num2 += (float)lineHeight;
				if (!PrefabEditModeManager.Instance.IsActive() && worldInfo != null && worldInfo.RandomGeneratedWorld && worldInfo.DynamicProperties.Contains("Generation.Seed"))
				{
					if (i == 1)
					{
						Utils.DrawOutline(new Rect(xMin + 5f, num2, width - 10f, inputAreaHeight), "World gen seed: " + worldInfo.DynamicProperties.GetStringValue("Generation.Seed"), labelStyle, Color.black, Color.white);
					}
					num2 += (float)lineHeight;
				}
				if (i == 1)
				{
					Utils.DrawOutline(new Rect(xMin + 5f, num2, width - 10f, inputAreaHeight), "Save name / deco seed: " + (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? GamePrefs.GetString(EnumGamePrefs.GameName) : GamePrefs.GetString(EnumGamePrefs.GameNameClient)), labelStyle, Color.black, Color.white);
				}
				num2 += (float)lineHeight;
				if (LocalPlayerUI.GetUIForPrimaryPlayer() != null)
				{
					EntityPlayer entityPlayer = LocalPlayerUI.GetUIForPrimaryPlayer().entityPlayer;
					if (entityPlayer != null)
					{
						PrefabInstance prefab = entityPlayer.prefab;
						if (i == 1)
						{
							string text2 = $"Coordinates: {entityPlayer.position.x:F0} {entityPlayer.position.y:F0} {entityPlayer.position.z:F0}";
							if (prefab != null)
							{
								text2 += $" / relative to POI: {prefab.GetPositionRelativeToPoi(Vector3i.Floor(entityPlayer.position))}";
							}
							Utils.DrawOutline(new Rect(xMin + 5f, num2, width - 10f, inputAreaHeight), text2, labelStyle, Color.black, Color.white);
						}
						num2 += (float)lineHeight;
						if (prefab != null)
						{
							string text3 = prefab.prefab?.PrefabName ?? prefab.name;
							string text4 = prefab.prefab?.LocalizedEnglishName ?? "";
							if (i == 1)
							{
								Utils.DrawOutline(new Rect(xMin + 5f, num2, width - 10f, inputAreaHeight), "POI: " + text3 + " (" + text4 + ")", labelStyle, Color.black, Color.white);
							}
							num2 += (float)lineHeight;
						}
						if (!confirmed)
						{
							if (i == 1)
							{
								savePerks = GUI.Toggle(new Rect(xMin + 5f, num2, width - 10f, lineHeight), savePerks, "Save Perks, Buffs and CVars", checkboxStyle);
							}
							num2 += (float)lineHeight;
						}
					}
				}
				num2 += (float)lineHeight;
			}
			if (!confirmed)
			{
				if (i == 1)
				{
					GUI.SetNextControlName("InputField");
					noteInput = GUI.TextField(new Rect(xMin + 5f, num2, width - 60f, inputAreaHeight), noteInput, 300, textfieldStyle);
					if (bFirstTime)
					{
						bFirstTime = false;
						GUI.FocusControl("InputField");
					}
					if (GUI.Button(new Rect(xMin + width - 50f, num2, 50f, inputAreaHeight), "Ok", buttonStyle))
					{
						DoScreenshot();
						return;
					}
				}
				num2 += (float)inputAreaHeight;
			}
			else
			{
				float num3 = labelStyle.CalcHeight(new GUIContent("Note: " + noteInput), width - 10f);
				if (i == 1)
				{
					Utils.DrawOutline(new Rect(xMin + 5f, num2, width - 10f, num3 + 4f), "Note: " + noteInput, labelStyle, Color.black, Color.white);
				}
				num2 += num3;
			}
			num = num2;
		}
		if (nGuiWdwDebugPanels == null)
		{
			nGuiWdwDebugPanels = UnityEngine.Object.FindObjectOfType<NGuiWdwDebugPanels>();
		}
		if (nGuiWdwDebugPanels != null)
		{
			nGuiWdwDebugPanels.showDebugPanel_FocusedBlock((int)xMin, (int)num + 10, forceFocusedBlock: true);
		}
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
		}
		_playerUi.windowManager.Open(ID, _bModal: false);
	}
}
