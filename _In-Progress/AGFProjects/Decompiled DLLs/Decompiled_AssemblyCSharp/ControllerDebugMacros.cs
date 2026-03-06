using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InControl;
using UnityEngine;

public class ControllerDebugMacros : MonoBehaviour
{
	public enum DebugDirection
	{
		Up,
		Down,
		Neutral
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SortedDictionary<string, Action<DebugDirection>> m_debugMacros;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string m_currentMacro;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int m_lastIndex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_keybindPressed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ControllerDebugLabel m_debugLabel;

	public ControllerDebugMacros()
	{
		m_debugMacros = new SortedDictionary<string, Action<DebugDirection>>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		m_debugLabel = GetComponent<ControllerDebugLabel>();
		m_debugLabel.AddDebugProvider("Debug Macros", BuildDebugMacroStatus);
		AddDebugMacro("Do Nothing", [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
		});
		AddDebugMacro("Open Console", MacroOpenConsole);
		AddDebugMacro("Toggle God Mode", MacroToggleGodMode);
		AddDebugMacro("Toggle Flying", MacroToggleFlying);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		m_debugLabel.RemoveDebugProvider("Debug Macros");
		RemoveDebugMacro("Open Console");
		RemoveDebugMacro("Toggle God Mode");
		RemoveDebugMacro("Toggle Flying");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (m_currentMacro == null || !m_debugMacros.ContainsKey(m_currentMacro))
		{
			if (m_lastIndex >= 0 && m_lastIndex < m_debugMacros.Keys.Count)
			{
				m_currentMacro = m_debugMacros.Keys.Skip(m_lastIndex).FirstOrDefault();
			}
			else
			{
				m_currentMacro = m_debugMacros.Keys.LastOrDefault();
				m_lastIndex = m_debugMacros.Keys.Count - 1;
			}
		}
		if (m_currentMacro == null)
		{
			return;
		}
		DebugDirection? executeMacroKeybind = GetExecuteMacroKeybind();
		if (executeMacroKeybind.HasValue)
		{
			DebugDirection valueOrDefault = executeMacroKeybind.GetValueOrDefault();
			if (!m_keybindPressed)
			{
				m_keybindPressed = true;
				if (m_debugMacros.ContainsKey(m_currentMacro))
				{
					m_debugMacros[m_currentMacro](valueOrDefault);
				}
			}
		}
		else
		{
			if (HasNextMacroKeybind())
			{
				if (m_keybindPressed)
				{
					return;
				}
				m_keybindPressed = true;
				bool flag = false;
				int num = 0;
				{
					foreach (string key in m_debugMacros.Keys)
					{
						if (flag)
						{
							m_currentMacro = key;
							m_lastIndex = num;
							break;
						}
						if (key == m_currentMacro)
						{
							flag = true;
						}
						num++;
					}
					return;
				}
			}
			if (HasPreviousMacroKeybind())
			{
				if (m_keybindPressed)
				{
					return;
				}
				m_keybindPressed = true;
				string text = null;
				int num2 = 0;
				{
					foreach (string key2 in m_debugMacros.Keys)
					{
						if (key2 == m_currentMacro)
						{
							if (text != null)
							{
								m_currentMacro = text;
								m_lastIndex = num2 - 1;
							}
							break;
						}
						text = key2;
						num2++;
					}
					return;
				}
			}
			m_keybindPressed = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildDebugMacroStatus(StringBuilder builder)
	{
		foreach (string key in m_debugMacros.Keys)
		{
			if (builder.Length > 0)
			{
				builder.Append(' ');
			}
			bool num = key == m_currentMacro;
			if (num)
			{
				builder.Append('[');
			}
			builder.Append(key);
			if (num)
			{
				builder.Append(']');
			}
		}
	}

	public void AddDebugMacro(string macroName, Action macro)
	{
		m_debugMacros[macroName] = [PublicizedFrom(EAccessModifier.Internal)] (DebugDirection _) =>
		{
			macro();
		};
	}

	public void AddDebugMacro(string macroName, Action<DebugDirection> macro)
	{
		m_debugMacros[macroName] = macro;
	}

	public void RemoveDebugMacro(string macroName)
	{
		m_debugMacros.Remove(macroName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HasPreviousMacroKeybind()
	{
		if (!HasKeyboardKeybind())
		{
			return HasControllerKeybind();
		}
		return true;
		[PublicizedFrom(EAccessModifier.Internal)]
		static bool HasControllerKeybind()
		{
			if (!InputManager.Enabled)
			{
				return false;
			}
			if (!InputManager.ActiveDevice.LeftBumper.IsPressed)
			{
				return false;
			}
			if (!InputManager.ActiveDevice.RightBumper.IsPressed)
			{
				return false;
			}
			if (!InputManager.ActiveDevice.DPadLeft.IsPressed)
			{
				return false;
			}
			return true;
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static bool HasKeyboardKeybind()
		{
			if (!InputUtils.ControlKeyPressed)
			{
				return false;
			}
			if (!InputUtils.ShiftKeyPressed)
			{
				return false;
			}
			if (!InputUtils.AltKeyPressed)
			{
				return false;
			}
			if (!Input.GetKey(KeyCode.LeftArrow))
			{
				return false;
			}
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HasNextMacroKeybind()
	{
		if (!HasKeyboardKeybind())
		{
			return HasControllerKeybind();
		}
		return true;
		[PublicizedFrom(EAccessModifier.Internal)]
		static bool HasControllerKeybind()
		{
			if (!InputManager.Enabled)
			{
				return false;
			}
			if (!InputManager.ActiveDevice.LeftBumper.IsPressed)
			{
				return false;
			}
			if (!InputManager.ActiveDevice.RightBumper.IsPressed)
			{
				return false;
			}
			if (!InputManager.ActiveDevice.DPadRight.IsPressed)
			{
				return false;
			}
			return true;
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static bool HasKeyboardKeybind()
		{
			if (!InputUtils.ControlKeyPressed)
			{
				return false;
			}
			if (!InputUtils.ShiftKeyPressed)
			{
				return false;
			}
			if (!InputUtils.AltKeyPressed)
			{
				return false;
			}
			if (!Input.GetKey(KeyCode.RightArrow))
			{
				return false;
			}
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DebugDirection? GetExecuteMacroKeybind()
	{
		return GetKeyboardKeybind() ?? GetControllerKeybind();
		[PublicizedFrom(EAccessModifier.Internal)]
		static DebugDirection? GetControllerKeybind()
		{
			if (!InputManager.Enabled)
			{
				return null;
			}
			if (!InputManager.ActiveDevice.LeftBumper.IsPressed)
			{
				return null;
			}
			if (!InputManager.ActiveDevice.RightBumper.IsPressed)
			{
				return null;
			}
			if (InputManager.ActiveDevice.Action1.IsPressed)
			{
				return DebugDirection.Neutral;
			}
			if (InputManager.ActiveDevice.DPadDown.IsPressed)
			{
				return DebugDirection.Down;
			}
			if (InputManager.ActiveDevice.DPadUp.IsPressed)
			{
				return DebugDirection.Up;
			}
			return null;
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static DebugDirection? GetKeyboardKeybind()
		{
			if (!InputUtils.ControlKeyPressed)
			{
				return null;
			}
			if (!InputUtils.ShiftKeyPressed)
			{
				return null;
			}
			if (!InputUtils.AltKeyPressed)
			{
				return null;
			}
			if (Input.GetKey(KeyCode.Menu))
			{
				return DebugDirection.Neutral;
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{
				return DebugDirection.Down;
			}
			if (Input.GetKey(KeyCode.UpArrow))
			{
				return DebugDirection.Up;
			}
			return null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MacroOpenConsole()
	{
		GUIWindowManager gUIWindowManager = UnityEngine.Object.FindObjectOfType<GUIWindowManager>();
		GameManager instance = GameManager.Instance;
		if ((bool)gUIWindowManager && (bool)instance)
		{
			GUIWindowConsole gUIConsole = instance.m_GUIConsole;
			if (gUIConsole != null)
			{
				gUIWindowManager.Open(gUIConsole, _bModal: false);
			}
			else
			{
				gUIWindowManager.Open(GUIWindowConsole.ID, _bModal: false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static EntityPlayerLocal GetPrimaryPlayer()
	{
		return GameManager.Instance.World?.GetPrimaryPlayer();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MacroToggleGodMode()
	{
		EntityPlayerLocal primaryPlayer = GetPrimaryPlayer();
		if ((bool)primaryPlayer)
		{
			DataItem<bool> isGodMode = primaryPlayer.IsGodMode;
			if (isGodMode.Value)
			{
				isGodMode.Value = false;
				primaryPlayer.Buffs.RemoveBuff("god");
			}
			else
			{
				isGodMode.Value = true;
				primaryPlayer.Buffs.AddBuff("god");
			}
			primaryPlayer.bEntityAliveFlagsChanged = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MacroToggleFlying()
	{
		EntityPlayerLocal primaryPlayer = GetPrimaryPlayer();
		if ((bool)primaryPlayer)
		{
			DataItem<bool> isFlyMode = primaryPlayer.IsFlyMode;
			isFlyMode.Value = !isFlyMode.Value;
			primaryPlayer.bEntityAliveFlagsChanged = true;
		}
	}
}
