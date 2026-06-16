using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserDirectConnect : XUiController
{
	[XuiBindComponent("txtIp", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtIp;

	[XuiBindComponent("txtPort", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtPort;

	[XuiBindComponent("btnConnect", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnConnect;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex ipPortMatcher = new Regex("^(.*):(\\d+)$");

	[PublicizedFrom(EAccessModifier.Private)]
	public Action confirmed;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action cancelled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validData;

	public string Ip
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return txtIp?.Text.Trim();
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			txtIp.Text = value;
			IsDirty = true;
		}
	}

	public int Port
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (int.TryParse(txtPort.Text, out var result))
			{
				return result;
			}
			return -1;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			txtPort.Text = value.ToString();
			IsDirty = true;
		}
	}

	[XuiXmlBinding("validdata")]
	public bool ValidData
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return validData;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			validData = value;
			IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		txtIp.SelectOnTab = txtPort;
		txtPort.SelectOnTab = txtIp;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
		if (XUiUtils.HotkeysAllowedFor(viewComponent))
		{
			if (xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
			{
				BtnCancel_OnPressed(this, -1);
			}
			if (xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
			{
				BtnConnect_OnPressed(this, -1);
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (txtIp != null)
		{
			Ip = GamePrefs.GetString(EnumGamePrefs.ConnectToServerIP);
			Port = GamePrefs.GetInt(EnumGamePrefs.ConnectToServerPort);
			txtIp.SetSelected();
			validateIpPort(null, null, _changeFromCode: true);
		}
		btnCancel?.SelectCursorElement(_withDelay: true);
	}

	public override void OnClose()
	{
		base.OnClose();
		saveIpPort();
		XUiC_MultiplayerPrivilegeNotification.Close();
		cancelled = null;
		confirmed = null;
	}

	[XuiBindEvent("OnClipboardHandler", "txtIp")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtIp_OnClipboardHandler(UIInput.ClipboardAction _actionType, string _oldText, int _selStart, int _selEnd, string _actionResultText)
	{
		if (_actionType != UIInput.ClipboardAction.Paste || _selEnd - _selStart != _oldText.Length)
		{
			return;
		}
		Match match = ipPortMatcher.Match(_actionResultText);
		if (match.Success)
		{
			string value = match.Groups[1].Value;
			if (StringParsers.TryParseSInt32(match.Groups[2].Value, out var _result))
			{
				Ip = value;
				Port = _result;
			}
		}
	}

	[XuiBindEvent("OnSubmitHandler", "txtIp")]
	[XuiBindEvent("OnSubmitHandler", "txtPort")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Txt_OnSubmitHandler(XUiController _sender, string _text)
	{
		BtnConnect_OnPressed(_sender, -1);
	}

	[XuiBindEvent("OnPress", "btnConnect")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConnect_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!btnConnect.ViewComponent.Enabled)
		{
			return;
		}
		Action action = confirmed;
		xui.playerUI.windowManager.Close(windowGroup);
		action?.Invoke();
		XUiC_MultiplayerPrivilegeNotification.ResolvePrivilegesWithDialog(EUserPerms.Multiplayer, [PublicizedFrom(EAccessModifier.Private)] (bool _result) =>
		{
			if (_result)
			{
				string text = Ip;
				if (!long.TryParse(text.Replace(".", ""), out var _))
				{
					try
					{
						IPHostEntry hostEntry = Dns.GetHostEntry(text);
						if (hostEntry.AddressList.Length == 0)
						{
							Log.Out("No valid IP for server found");
							return;
						}
						text = hostEntry.AddressList[0].ToString();
						if (hostEntry.AddressList[0].AddressFamily != AddressFamily.InterNetwork)
						{
							for (int i = 1; i < hostEntry.AddressList.Length; i++)
							{
								if (hostEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
								{
									text = hostEntry.AddressList[i].ToString();
									break;
								}
							}
						}
					}
					catch (SocketException ex)
					{
						Log.Out("No such hostname: \"" + text + "\": " + ex);
						return;
					}
				}
				Log.Out("Connect by IP");
				GameServerInfo gameServerInfo = new GameServerInfo();
				gameServerInfo.SetValue(GameInfoString.IP, text);
				gameServerInfo.SetValue(GameInfoInt.Port, Port);
				GameManager.Instance.showOpenerMovieOnLoad = false;
				SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo = gameServerInfo;
				SingletonMonoBehaviour<ConnectionManager>.Instance.Connect(SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
			}
		});
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		Action action = cancelled;
		xui.playerUI.windowManager.Close(windowGroup);
		action?.Invoke();
	}

	[XuiBindEvent("OnChangeHandler", "txtIp")]
	[XuiBindEvent("OnChangeHandler", "txtPort")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void validateIpPort(XUiController _sender, string _newText, bool _changeFromCode)
	{
		int port = Port;
		ValidData = port > 0 && port < 65533 && Ip.Length > 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void saveIpPort()
	{
		if (ValidData)
		{
			GamePrefs.Set(EnumGamePrefs.ConnectToServerIP, Ip);
			GamePrefs.Set(EnumGamePrefs.ConnectToServerPort, Port);
		}
	}

	public static void Open(XUi _xui, Action _onConfirmed, Action _onCancelled)
	{
		XUiC_ServerBrowserDirectConnect childByType = _xui.GetChildByType<XUiC_ServerBrowserDirectConnect>();
		childByType.confirmed = _onConfirmed;
		childByType.cancelled = _onCancelled;
		_xui.playerUI.windowManager.Open(childByType.windowGroup, _bModal: false);
	}
}
