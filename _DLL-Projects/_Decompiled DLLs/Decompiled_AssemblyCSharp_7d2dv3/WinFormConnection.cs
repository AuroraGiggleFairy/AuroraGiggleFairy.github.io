using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using UnityEngine;

public class WinFormConnection : Form, IConsoleConnection
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string iconPath = GameIO.GetGameDir("Data") + "/7dtd_icon.ico";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<LogType> enabledLogLevels = new HashSet<LogType>
	{
		LogType.Log,
		LogType.Warning,
		LogType.Error,
		LogType.Exception,
		LogType.Assert
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public RichTextBox consoleOutputBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public TextBox commandInputBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool forceClose;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shutdownRequested;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int lineLimit = 500;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly System.Drawing.Color logColorCommandReply = System.Drawing.Color.LightCyan;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly System.Drawing.Color logColorNormal = System.Drawing.Color.LimeGreen;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly System.Drawing.Color logColorWarning = System.Drawing.Color.Yellow;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly System.Drawing.Color logColorError = System.Drawing.Color.Red;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<string, System.Drawing.Color> addLineDelegate;

	public WinFormConnection(WinFormInstance _owner)
	{
		initialize();
		ModEvents.GameStartDone.RegisterHandler(OnGameStartDone);
		ModEvents.GameAwake.RegisterHandler(OnGameAwake);
	}

	public void CloseTerminal()
	{
		if (base.InvokeRequired)
		{
			BeginInvoke(new Action(CloseTerminal));
			return;
		}
		forceClose = true;
		Close();
		System.Windows.Forms.Application.Exit();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnClosing(CancelEventArgs _e)
	{
		base.OnClosing(_e);
		if (forceClose)
		{
			return;
		}
		_e.Cancel = true;
		if (!shutdownRequested && MessageBox.Show("Really shut down the 7 Days to Die server?", "Shutdown", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
		{
			shutdownRequested = true;
			Log.Out("Shutdown game from Terminal Window");
			ThreadManager.AddSingleTaskMainThread("Shutdown", [PublicizedFrom(EAccessModifier.Internal)] (object _) =>
			{
				UnityEngine.Application.Quit();
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnClosed(EventArgs _e)
	{
		base.OnClosed(_e);
		ModEvents.GameStartDone.UnregisterHandler(OnGameStartDone);
		ModEvents.GameAwake.UnregisterHandler(OnGameAwake);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initialize()
	{
		SuspendLayout();
		base.ClientSize = new Size(1000, 600);
		Text = "Starting - 7 Days to Die Dedicated Server Console";
		base.Icon = new Icon(iconPath);
		BackColor = System.Drawing.Color.Black;
		ForeColor = logColorNormal;
		consoleOutputBox = new RichTextBox
		{
			Dock = DockStyle.Fill,
			Multiline = true,
			ScrollBars = RichTextBoxScrollBars.Both,
			Font = new System.Drawing.Font(FontFamily.GenericMonospace, 10f),
			ReadOnly = true,
			BackColor = BackColor,
			ForeColor = ForeColor,
			BorderStyle = BorderStyle.None
		};
		base.Controls.Add(consoleOutputBox);
		_ = consoleOutputBox.Handle;
		commandInputBox = new TextBox
		{
			Dock = DockStyle.Bottom,
			Multiline = false,
			Text = "",
			Font = new System.Drawing.Font(FontFamily.GenericMonospace, 12f),
			Enabled = false,
			AutoCompleteMode = AutoCompleteMode.Append,
			AutoCompleteSource = AutoCompleteSource.CustomSource,
			BackColor = System.Drawing.Color.LightGray,
			ForeColor = System.Drawing.Color.Black,
			BorderStyle = BorderStyle.FixedSingle
		};
		commandInputBox.KeyDown += CommandInputBoxOnKeyDown;
		commandInputBox.AutoCompleteCustomSource = new AutoCompleteStringCollection();
		base.Controls.Add(commandInputBox);
		CreateHandle();
		ResumeLayout();
		PerformLayout();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGameAwake(ref ModEvents.SGameAwakeData _data)
	{
		Text = $"{GamePrefs.GetString(EnumGamePrefs.ServerName)} - Port {GamePrefs.GetInt(EnumGamePrefs.ServerPort)} - Loading - 7 Days to Die Dedicated Server Console";
		foreach (IConsoleCommand command in SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommands())
		{
			commandInputBox.AutoCompleteCustomSource.AddRange(command.GetCommands());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGameStartDone(ref ModEvents.SGameStartDoneData _data)
	{
		Text = $"{GamePrefs.GetString(EnumGamePrefs.ServerName)} - Port {GamePrefs.GetInt(EnumGamePrefs.ServerPort)} - Running - 7 Days to Die Dedicated Server Console";
		commandInputBox.Enabled = true;
		commandInputBox.Clear();
		commandInputBox.Focus();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CommandInputBoxOnKeyDown(object _sender, KeyEventArgs _keyEventArgs)
	{
		if (_keyEventArgs.KeyCode == Keys.Return)
		{
			execCommand();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void execCommand()
	{
		if (commandInputBox.Enabled && commandInputBox.Text.Length > 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteAsync(commandInputBox.Text, this);
			commandInputBox.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddLine(string _text, System.Drawing.Color _color)
	{
		if (consoleOutputBox.InvokeRequired)
		{
			if (addLineDelegate == null)
			{
				addLineDelegate = AddLine;
			}
			BeginInvoke(addLineDelegate, _text, _color);
			return;
		}
		consoleOutputBox.SelectionStart = consoleOutputBox.TextLength;
		consoleOutputBox.SelectionLength = 0;
		consoleOutputBox.SelectionColor = _color;
		consoleOutputBox.AppendText(_text + "\n");
		consoleOutputBox.SelectionColor = consoleOutputBox.ForeColor;
		if (consoleOutputBox.Lines.Length > 1000)
		{
			int num = consoleOutputBox.Lines.Length - 500;
			string[] array = new string[500];
			for (int i = 0; i < 500; i++)
			{
				array[i] = consoleOutputBox.Lines[i + num];
			}
			consoleOutputBox.Lines = array;
		}
		consoleOutputBox.SelectionStart = consoleOutputBox.TextLength;
		consoleOutputBox.ScrollToCaret();
	}

	public void SendLine(string _line)
	{
		AddLine(_line, logColorCommandReply);
	}

	public void SendLines(List<string> _output)
	{
		foreach (string item in _output)
		{
			SendLine(item);
		}
	}

	public void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime)
	{
		if (IsLogLevelEnabled(_type))
		{
			System.Drawing.Color color = logColorNormal;
			switch (_type)
			{
			case LogType.Warning:
				color = logColorWarning;
				break;
			case LogType.Error:
			case LogType.Assert:
			case LogType.Exception:
				color = logColorError;
				break;
			}
			AddLine(_formattedMessage, color);
		}
	}

	public void EnableLogLevel(LogType _type, bool _enable)
	{
		if (_enable)
		{
			enabledLogLevels.Add(_type);
		}
		else
		{
			enabledLogLevels.Remove(_type);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsLogLevelEnabled(LogType _type)
	{
		return enabledLogLevels.Contains(_type);
	}

	public string GetDescription()
	{
		return "Terminal Window";
	}
}
