using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using InControl;
using UnityEngine;

public class ControllerDebugLabel : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string KEY_DEVICES = "Devices";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string KEY_KEY_CODES = "Key Codes";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SortedDictionary<string, Action<StringBuilder>> m_debugStringProviders;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> m_debugStringProviderNames;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringBuilder m_debugStringBuilderMain;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringBuilder m_debugStringBuilderForProvider;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringBuilder m_debugStringBuilderForControlsString;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public UILabel m_label;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly KeyCode[] m_allKeyCodes = (KeyCode[])Enum.GetValues(typeof(KeyCode));

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly InputControlType[] m_allControls = (InputControlType[])Enum.GetValues(typeof(InputControlType));

	public ControllerDebugLabel()
	{
		m_debugStringProviders = new SortedDictionary<string, Action<StringBuilder>>();
		m_debugStringProviderNames = new List<string>();
		m_debugStringBuilderMain = new StringBuilder(4096);
		m_debugStringBuilderForProvider = new StringBuilder(4096);
		m_debugStringBuilderForControlsString = new StringBuilder(512);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		m_label = GetComponent<UILabel>();
		AddDebugProvider("Key Codes", BuildKeyCodeString);
		AddDebugProvider("Devices", BuildDevicesString);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		RemoveDebugProvider("Key Codes");
		RemoveDebugProvider("Devices");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (m_label == null)
		{
			return;
		}
		m_debugStringBuilderMain.Clear();
		for (int i = 0; i < m_debugStringProviderNames.Count; i++)
		{
			string text = m_debugStringProviderNames[i];
			Action<StringBuilder> action = m_debugStringProviders[text];
			m_debugStringBuilderForProvider.Clear();
			try
			{
				action(m_debugStringBuilderForProvider);
			}
			catch (Exception ex)
			{
				m_debugStringBuilderForProvider.Clear();
				m_debugStringBuilderForProvider.Append(ex.Message);
			}
			if (m_debugStringBuilderForProvider.Length == 0)
			{
				continue;
			}
			if (m_debugStringBuilderMain.Length != 0)
			{
				m_debugStringBuilderMain.Append('\n');
			}
			m_debugStringBuilderMain.Append(text);
			m_debugStringBuilderMain.Append(": ");
			for (int j = 0; j < m_debugStringBuilderForProvider.Length; j++)
			{
				char c = m_debugStringBuilderForProvider[j];
				m_debugStringBuilderMain.Append(c);
				if (c == '\n')
				{
					for (int k = 0; k < text.Length + 2; k++)
					{
						m_debugStringBuilderMain.Append(' ');
					}
				}
			}
		}
		if (!m_debugStringBuilderMain.Equals(m_label.text))
		{
			m_label.text = m_debugStringBuilderMain.ToString();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateProviderNames()
	{
		m_debugStringProviderNames.Clear();
		m_debugStringProviderNames.AddRange(m_debugStringProviders.Keys);
	}

	public void AddDebugProvider(string providerName, Action<StringBuilder> provider)
	{
		bool num = !m_debugStringProviders.ContainsKey(providerName);
		m_debugStringProviders[providerName] = provider;
		if (num)
		{
			UpdateProviderNames();
		}
	}

	public void RemoveDebugProvider(string providerName)
	{
		if (m_debugStringProviders.Remove(providerName))
		{
			UpdateProviderNames();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildKeyCodeString(StringBuilder builder)
	{
		KeyCode[] allKeyCodes = m_allKeyCodes;
		foreach (KeyCode keyCode in allKeyCodes)
		{
			if (Input.GetKey(keyCode))
			{
				if (builder.Length != 0)
				{
					builder.Append(" + ");
				}
				builder.Append(keyCode.ToStringCached());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildDevicesString(StringBuilder builder)
	{
		ReadOnlyCollection<InputDevice> devices = InputManager.Devices;
		for (int i = 0; i < devices.Count; i++)
		{
			InputDevice inputDevice = devices[i];
			if (!inputDevice.IsActive)
			{
				continue;
			}
			m_debugStringBuilderForControlsString.Clear();
			BuildControlsString(m_debugStringBuilderForControlsString, inputDevice);
			if (m_debugStringBuilderForControlsString.Length != 0)
			{
				if (builder.Length != 0)
				{
					builder.Append('\n');
				}
				builder.Append(m_debugStringBuilderForControlsString);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildControlsString(StringBuilder builder, InputDevice device)
	{
		bool flag = false;
		builder.Append(device.Name);
		builder.Append(" (");
		builder.Append(device.Meta);
		builder.Append("): ");
		InputControlType[] allControls = m_allControls;
		foreach (InputControlType inputControlType in allControls)
		{
			if (inputControlType == InputControlType.None || inputControlType == InputControlType.Count)
			{
				continue;
			}
			InputControl control = device.GetControl(inputControlType);
			if (control.IsPressed || control.RawValue != 0f)
			{
				flag = true;
				if (builder.Length != 0)
				{
					builder.Append(" + ");
				}
				builder.Append(inputControlType.ToStringCached());
				if (control.IsAnalog)
				{
					builder.AppendFormat("={0:F4}", control.RawValue);
				}
			}
		}
		if (!flag)
		{
			builder.Clear();
		}
	}
}
