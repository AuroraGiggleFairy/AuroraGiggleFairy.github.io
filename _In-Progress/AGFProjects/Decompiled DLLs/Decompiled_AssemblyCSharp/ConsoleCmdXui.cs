using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdXui : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "xui" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Execute XUi operations";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n   xui open <window group name> [instance] [closeOthers]\n   xui close <window group name> [instance]\n   xui reload [window group name] [instance]\n   xui list <\"instances\" / \"windows\"> [instance]\n";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count >= 1)
		{
			if (_params[0].EqualsCaseInsensitive("open"))
			{
				ExecuteOpen(_params);
			}
			else if (_params[0].EqualsCaseInsensitive("close"))
			{
				ExecuteClose(_params);
			}
			else if (_params[0].EqualsCaseInsensitive("reload"))
			{
				ExecuteReload(_params);
			}
			else if (_params[0].EqualsCaseInsensitive("list"))
			{
				ExecuteList(_params);
			}
			else if (_params[0].EqualsCaseInsensitive("limits"))
			{
				ExecuteLimits();
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid sub command \"" + _params[0] + "\".");
			}
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No sub command given.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteOpen(List<string> _params)
	{
		if (_params.Count < 2 || _params.Count > 4)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 2 to 4, found " + _params.Count + ".");
			return;
		}
		string name = _params[1];
		bool bModal = true;
		int _xuiInstanceId;
		XUi xuiInstance = getXuiInstance(_params, 2, out _xuiInstanceId);
		if (!(xuiInstance == null))
		{
			if (_params.Count > 3)
			{
				bModal = ConsoleHelper.ParseParamBool(_params[3]);
			}
			name = getXuiWindow(xuiInstance, name);
			if (name != null)
			{
				xuiInstance.playerUI.windowManager.Open(name, bModal);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("XUi window group \"" + name + "\" opened.");
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("XUi window group \"" + _params[1] + "\" does not exist.");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteClose(List<string> _params)
	{
		if (_params.Count < 2 || _params.Count > 3)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 2 to 3, found " + _params.Count + ".");
			return;
		}
		string name = _params[1];
		int _xuiInstanceId;
		XUi xuiInstance = getXuiInstance(_params, 2, out _xuiInstanceId);
		if (!(xuiInstance == null))
		{
			name = getXuiWindow(xuiInstance, name);
			if (name != null)
			{
				xuiInstance.playerUI.windowManager.Close(name);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("XUi window group \"" + name + "\" closed.");
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("XUi window group \"" + _params[1] + "\" does not exist.");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteReload(List<string> _params)
	{
		if (_params.Count > 3)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 1 to 3, found " + _params.Count + ".");
			return;
		}
		string text = "*";
		if (_params.Count > 1)
		{
			text = _params[1];
		}
		int _xuiInstanceId;
		XUi xuiInstance = getXuiInstance(_params, 2, out _xuiInstanceId);
		if (xuiInstance == null)
		{
			return;
		}
		if (text == "*")
		{
			XUi.Reload(xuiInstance.playerUI);
			xuiInstance.SetDataConnections();
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("XUi windows reloaded.");
			return;
		}
		text = getXuiWindow(xuiInstance, text);
		if (text != null)
		{
			XUi.ReloadWindow(xuiInstance.playerUI, text);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("XUi window group \"" + text + "\" reloaded.");
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("XUi window group \"" + _params[1] + "\" does not exist.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteList(List<string> _params)
	{
		if (_params.Count < 2)
		{
			_params.Add("windows");
		}
		if (_params[1].EqualsCaseInsensitive("instances"))
		{
			XUi[] array = xuiInstances();
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Loaded XUi instances:");
			for (int i = 0; i < array.Length; i++)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  " + i + ". " + array[i].playerUI.windowManager.gameObject.name);
			}
		}
		else if (_params[1].EqualsCaseInsensitive("windows"))
		{
			int _xuiInstanceId;
			XUi xuiInstance = getXuiInstance(_params, 2, out _xuiInstanceId);
			if (!(xuiInstance == null))
			{
				List<string> list = new List<string>();
				for (int j = 0; j < xuiInstance.WindowGroups.Count; j++)
				{
					list.Add(xuiInstance.WindowGroups[j].ID);
				}
				list.Sort();
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Loaded XUi window groups in instance " + _xuiInstanceId + " (\"" + xuiInstance.playerUI.gameObject.name + "\"):");
				for (int k = 0; k < list.Count; k++)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  " + list[k]);
				}
			}
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Argument 2 has to be either \"instances\" or \"windows\".");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteLimits()
	{
		XUi xUi = xuiInstances()[0];
		string xuiWindow = getXuiWindow(xUi, "uiLimitsTest");
		if (xuiWindow == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("XUi window group \"uiLimitsTest\" does not exist.");
		}
		else if (xUi.playerUI.windowManager.IsWindowOpen(xuiWindow))
		{
			xUi.playerUI.windowManager.Close(xuiWindow);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("XUi window group \"" + xuiWindow + "\" closed.");
		}
		else
		{
			xUi.playerUI.windowManager.Open(xuiWindow, _bModal: false);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("XUi window group \"" + xuiWindow + "\" opened.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUi getXuiInstance(List<string> _params, int _index, out int _xuiInstanceId)
	{
		XUi[] array = xuiInstances();
		_xuiInstanceId = array.Length - 1;
		if (_params.Count > _index)
		{
			if (!StringParsers.TryParseSInt32(_params[_index], out _xuiInstanceId))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[_index] + "\" is not a valid integer.");
				return null;
			}
			if (_xuiInstanceId >= array.Length)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("XUi instance " + _xuiInstanceId + " does not exist.");
				return null;
			}
		}
		return array[_xuiInstanceId];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string getXuiWindow(XUi _xuiInstance, string _name)
	{
		for (int i = 0; i < _xuiInstance.WindowGroups.Count; i++)
		{
			if (_xuiInstance.WindowGroups[i].ID.EqualsCaseInsensitive(_name))
			{
				return _xuiInstance.WindowGroups[i].ID;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUi[] xuiInstances()
	{
		XUi[] array = UnityEngine.Object.FindObjectsOfType<XUi>();
		Array.Sort(array, [PublicizedFrom(EAccessModifier.Internal)] (XUi _x, XUi _y) => string.Compare(_x.playerUI.windowManager.gameObject.name, _y.playerUI.windowManager.gameObject.name, StringComparison.Ordinal));
		return array;
	}
}
