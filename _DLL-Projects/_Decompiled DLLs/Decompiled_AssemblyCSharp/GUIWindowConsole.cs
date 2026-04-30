using System;
using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.UI;

public class GUIWindowConsole : GUIWindowUGUI
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct ConsoleLine(string _text, string _stackTrace, LogType _type)
	{
		public string text = _text;

		public LogType type = _type;

		public string stackTrace = _stackTrace;

		public Color GetLogColor()
		{
			switch (type)
			{
			case LogType.Log:
				return Color.white;
			case LogType.Warning:
				return Color.yellow;
			case LogType.Error:
			case LogType.Assert:
			case LogType.Exception:
				return Color.red;
			default:
				return Color.white;
			}
		}
	}

	public static string ID = typeof(GUIWindowConsole).Name;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool scrolledToBottom = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<ConsoleLine> linesToAdd = new Queue<ConsoleLine>(301);

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<Text> displayedLines = new Queue<Text>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int maxConsoleLines = 300;

	[PublicizedFrom(EAccessModifier.Private)]
	public Stack<Text> linePool = new Stack<Text>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> lastCommands = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastCommandsIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFirstTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bUpdateCursor;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bShouldReopenGebugMenu;

	[PublicizedFrom(EAccessModifier.Private)]
	public GUIWindowConsoleComponents components;

	[PublicizedFrom(EAccessModifier.Private)]
	public ScrollRect scrollRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform contentRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public InputField commandField;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] lineSeparators = new string[3] { "\r\n", "\r", "\n" };

	public override string UIPrefabPath => "GUI/Prefabs/ConsoleWindow";

	public GUIWindowConsole()
		: base(ID)
	{
		Log.LogCallbacks += LogCallback;
		alwaysUsesMouseCursor = true;
		components = canvas.GetComponent<GUIWindowConsoleComponents>();
		scrollRect = components.scrollRect;
		contentRect = components.contentRect;
		commandField = components.commandField;
		commandField.onSubmit.AddListener(EnterCommand);
		commandField.shouldActivateOnSelect = !TouchScreenKeyboard.isSupported;
		components.closeButton.onClick.AddListener(CloseConsole);
		components.openLogsButton.onClick.AddListener([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			GameIO.OpenExplorer(Application.consoleLogPath);
		});
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			components.openLogsButton.gameObject.SetActive(value: false);
		}
		for (int num = 0; num < 5; num++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(components.consoleLinePrefab);
			gameObject.SetActive(value: false);
			gameObject.transform.SetParent(contentRect, worldPositionStays: false);
			linePool.Push(gameObject.GetComponent<Text>());
		}
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += Input_OnLastInputStyleChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Input_OnLastInputStyleChanged(PlayerInputManager.InputStyle _inputStyle)
	{
		if (_inputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			components.controllerPrompts.SetActive(value: false);
			return;
		}
		components.controllerPrompts.SetActive(value: true);
		components.RefreshButtonPrompts();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Text AllocText()
	{
		if (linePool.TryPop(out var result))
		{
			result.gameObject.SetActive(value: true);
			return result;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(components.consoleLinePrefab);
		gameObject.transform.SetParent(contentRect, worldPositionStays: false);
		return gameObject.GetComponent<Text>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FreeText(Text _text)
	{
		_text.gameObject.SetActive(value: false);
		linePool.Push(_text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddDisplayedLine(ConsoleLine _line)
	{
		StringSpan.SeparatorSplitAnyEnumerator enumerator = ((StringSpan)_line.text).GetSplitAnyEnumerator(lineSeparators, StringSplitOptions.RemoveEmptyEntries).GetEnumerator();
		while (enumerator.MoveNext())
		{
			StringSpan current = enumerator.Current;
			Text text = ((displayedLines.Count != 300) ? AllocText() : displayedLines.Dequeue());
			if (current.Length > 500)
			{
				text.text = SpanUtils.Concat(current.Slice(0, 500), "...");
			}
			else
			{
				text.text = current.ToString();
			}
			text.color = _line.GetLogColor();
			text.transform.SetAsLastSibling();
			displayedLines.Enqueue(text);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearDisplayedLines()
	{
		Text result;
		while (displayedLines.TryDequeue(out result))
		{
			FreeText(result);
		}
	}

	public void Shutdown()
	{
		Log.LogCallbacks -= LogCallback;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogCallback(string _msg, string _trace, LogType _type)
	{
		switch (_type)
		{
		case LogType.Assert:
			openConsole(_msg);
			break;
		case LogType.Exception:
			openConsole(_msg);
			break;
		}
		internalAddLine(new ConsoleLine(_msg, _trace, _type));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openConsole(string _logString)
	{
		if (!Submission.Enabled && !_logString.StartsWith("Can't send RPC") && !_logString.StartsWith("You are trying to load data from"))
		{
			windowManager.OpenIfNotOpen(ID, _bModal: false);
		}
	}

	public void AddLines(string[] _lines)
	{
		for (int i = 0; i < _lines.Length; i++)
		{
			AddLine(_lines[i]);
		}
	}

	public void AddLines(List<string> _lines)
	{
		for (int i = 0; i < _lines.Count; i++)
		{
			AddLine(_lines[i]);
		}
	}

	public void AddLine(string _line)
	{
		internalAddLine(new ConsoleLine(_line, string.Empty, LogType.Log));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void internalAddLine(ConsoleLine consoleLine)
	{
		lock (linesToAdd)
		{
			linesToAdd.Enqueue(consoleLine);
			while (linesToAdd.Count > 300)
			{
				linesToAdd.Dequeue();
			}
		}
	}

	public override void Update()
	{
		base.Update();
		scrolledToBottom = scrollRect.verticalNormalizedPosition < 0.1f;
		bool flag = false;
		lock (linesToAdd)
		{
			if (linesToAdd.Count > 0)
			{
				flag = true;
				foreach (ConsoleLine item in linesToAdd)
				{
					AddDisplayedLine(item);
				}
				linesToAdd.Clear();
			}
		}
		if (flag && scrolledToBottom)
		{
			Canvas.ForceUpdateCanvases();
			scrollRect.verticalNormalizedPosition = 0f;
		}
		if (bFirstTime)
		{
			if (!TouchScreenKeyboard.isSupported)
			{
				commandField.Select();
				commandField.ActivateInputField();
			}
			scrollRect.verticalNormalizedPosition = 0f;
			bFirstTime = false;
		}
		if (bUpdateCursor)
		{
			commandField.MoveTextEnd(shift: false);
			bUpdateCursor = false;
		}
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			PreviousCommand();
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			NextCommand();
		}
		else if (Input.GetKeyDown(KeyCode.PageUp))
		{
			float num = CalculateNormalizedPageSize();
			scrollRect.verticalNormalizedPosition = Math.Min(scrollRect.verticalNormalizedPosition + num, 1f);
		}
		else if (Input.GetKeyDown(KeyCode.PageDown))
		{
			float num2 = CalculateNormalizedPageSize();
			scrollRect.verticalNormalizedPosition = Math.Max(scrollRect.verticalNormalizedPosition - num2, 0f);
		}
		PlayerActionsGUI playerActionsGUI = playerUI.playerInput?.GUIActions;
		if (playerActionsGUI != null)
		{
			if (playerActionsGUI.Submit.WasPressed)
			{
				EnterCommand(commandField.text);
			}
			else if (playerActionsGUI.DPad_Up.WasPressed && playerActionsGUI.DPad_Up.LastDeviceClass != InputDeviceClass.Keyboard)
			{
				PreviousCommand();
			}
			else if (playerActionsGUI.DPad_Down.WasPressed && playerActionsGUI.DPad_Down.LastDeviceClass != InputDeviceClass.Keyboard)
			{
				NextCommand();
			}
			else if (playerActionsGUI.DPad_Left.WasPressed && playerActionsGUI.DPad_Down.LastDeviceClass != InputDeviceClass.Keyboard)
			{
				PlatformManager.NativePlatform.VirtualKeyboard?.Open("Enter Command", commandField.text, OnTextReceived);
			}
			else if (playerUI.playerInput.PermanentActions.Cancel.WasReleased || PlayerActionsGlobal.Instance.Console.WasPressed)
			{
				CloseConsole();
			}
			float y = playerActionsGUI.Camera.Vector.y;
			if (y != 0f)
			{
				float num3 = CalculateNormalizedPageSize();
				scrollRect.verticalNormalizedPosition = Math.Max(scrollRect.verticalNormalizedPosition + num3 * y * 0.05f, 0f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalculateNormalizedPageSize()
	{
		float height = scrollRect.viewport.rect.height;
		float num = scrollRect.content.rect.height - height;
		if (num > height)
		{
			return Math.Max(height / num, 0.01f);
		}
		return 1f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTextReceived(bool _success, string _text)
	{
		if (_success)
		{
			commandField.text = _text;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CloseConsole()
	{
		windowManager.Close(this);
		commandField.text = string.Empty;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnterCommand(string _command)
	{
		if (_command.Length <= 0)
		{
			return;
		}
		if (_command == "clear")
		{
			Clear();
		}
		else
		{
			scrollRect.verticalNormalizedPosition = 0f;
			internalAddLine(new ConsoleLine("> " + _command, string.Empty, LogType.Log));
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				AddLines(SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(_command, null));
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup(_command));
			}
		}
		if (lastCommands.Count == 0 || !lastCommands[lastCommands.Count - 1].Equals(_command))
		{
			if (lastCommands.Contains(_command))
			{
				lastCommands.Remove(_command);
			}
			lastCommands.Add(_command);
		}
		lastCommandsIdx = lastCommands.Count;
		commandField.text = "";
		if (!TouchScreenKeyboard.isSupported)
		{
			commandField.Select();
			commandField.ActivateInputField();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PreviousCommand()
	{
		if (lastCommands.Count > 0)
		{
			lastCommandsIdx = Mathf.Max(0, lastCommandsIdx - 1);
			commandField.text = lastCommands[lastCommandsIdx];
			if (!TouchScreenKeyboard.isSupported)
			{
				commandField.Select();
				commandField.ActivateInputField();
			}
			bUpdateCursor = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NextCommand()
	{
		if (lastCommands.Count <= 0)
		{
			return;
		}
		lastCommandsIdx = Mathf.Min(lastCommands.Count, lastCommandsIdx + 1);
		if (lastCommandsIdx < lastCommands.Count)
		{
			commandField.text = lastCommands[lastCommandsIdx];
			bUpdateCursor = true;
			if (!TouchScreenKeyboard.isSupported)
			{
				commandField.Select();
				commandField.ActivateInputField();
			}
		}
		else
		{
			commandField.text = string.Empty;
		}
	}

	public void Clear()
	{
		ClearDisplayedLines();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) && windowManager.IsWindowOpen(XUiC_InGameDebugMenu.ID))
		{
			bShouldReopenGebugMenu = true;
			windowManager.Close(XUiC_InGameDebugMenu.ID);
		}
		else
		{
			bShouldReopenGebugMenu = false;
		}
		commandField.text = string.Empty;
		bFirstTime = true;
		isInputActive = true;
		if (UIInput.selection != null)
		{
			UIInput.selection.isSelected = false;
		}
	}

	public override void OnClose()
	{
		scrollRect.verticalNormalizedPosition = 0f;
		base.OnClose();
		if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) && bShouldReopenGebugMenu)
		{
			windowManager.Open(XUiC_InGameDebugMenu.ID, _bModal: false);
		}
		bShouldReopenGebugMenu = false;
		isInputActive = false;
	}
}
