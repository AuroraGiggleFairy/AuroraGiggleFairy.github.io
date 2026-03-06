using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserDirectConnect : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MultiplayerPrivilegeNotification wdwMultiplayerPrivileges;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtIp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtPort;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDirectConnectConnect;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnCancel;

	public XUiController cancelTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex ipPortMatcher = new Regex("^(.*):(\\d+)$");

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		return false;
	}

	public override void Init()
	{
		base.Init();
		txtIp = (XUiC_TextInput)GetChildById("txtIp");
		txtPort = (XUiC_TextInput)GetChildById("txtPort");
		txtIp.OnClipboardHandler += TxtIp_OnClipboardHandler;
		txtIp.OnSubmitHandler += Txt_OnSubmitHandler;
		txtPort.OnSubmitHandler += Txt_OnSubmitHandler;
		txtIp.OnChangeHandler += validateIpPort;
		txtPort.OnChangeHandler += validateIpPort;
		txtIp.SelectOnTab = txtPort;
		txtPort.SelectOnTab = txtIp;
		btnCancel = (XUiC_SimpleButton)GetChildById("btnCancel");
		btnCancel.OnPressed += BtnCancel_OnPressed;
		btnDirectConnectConnect = (XUiC_SimpleButton)GetChildById("btnDirectConnectConnect");
		btnDirectConnectConnect.OnPressed += BtnConnect_OnPressed;
		btnDirectConnectConnect.Enabled = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiC_MultiplayerPrivilegeNotification.Close();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtIp_OnClipboardHandler(UIInput.ClipboardAction _actiontype, string _oldtext, int _selstart, int _selend, string _actionresulttext)
	{
		if (_actiontype != UIInput.ClipboardAction.Paste || _selend - _selstart != _oldtext.Length)
		{
			return;
		}
		Match match = ipPortMatcher.Match(_actionresulttext);
		if (match.Success)
		{
			string value = match.Groups[1].Value;
			if (StringParsers.TryParseSInt32(match.Groups[2].Value, out var _result))
			{
				txtIp.Text = value;
				txtPort.Text = _result.ToString();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Txt_OnSubmitHandler(XUiController _sender, string _text)
	{
		BtnConnect_OnPressed(_sender, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConnect_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.ViewComponent.IsVisible = false;
		saveIpPort();
		if (wdwMultiplayerPrivileges == null)
		{
			wdwMultiplayerPrivileges = XUiC_MultiplayerPrivilegeNotification.GetWindow();
		}
		wdwMultiplayerPrivileges?.ResolvePrivilegesWithDialog(EUserPerms.Multiplayer, [PublicizedFrom(EAccessModifier.Private)] (bool result) =>
		{
			if (result)
			{
				string text = txtIp.Text.Trim();
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
				gameServerInfo.SetValue(GameInfoInt.Port, int.Parse(txtPort.Text));
				GameManager.Instance.showOpenerMovieOnLoad = false;
				SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo = gameServerInfo;
				base.xui.playerUI.windowManager.Close(windowGroup.ID);
				SingletonMonoBehaviour<ConnectionManager>.Instance.Connect(SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
			}
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		Hide();
	}

	public void Show(XUiController _cancelTarget)
	{
		viewComponent.IsVisible = true;
		if (txtIp != null)
		{
			txtIp.Text = GamePrefs.GetString(EnumGamePrefs.ConnectToServerIP);
			txtPort.Text = GamePrefs.GetInt(EnumGamePrefs.ConnectToServerPort).ToString();
			txtIp.SetSelected();
			validateIpPort(null, null, _changeFromCode: true);
		}
		cancelTarget = _cancelTarget;
		base.xui.playerUI.CursorController.SetNavigationLockView(viewComponent);
		if (btnCancel != null)
		{
			btnCancel.SelectCursorElement(_withDelay: true);
		}
	}

	public void Hide()
	{
		viewComponent.IsVisible = false;
		saveIpPort();
		base.xui.playerUI.CursorController.SetNavigationLockView(null);
		if (cancelTarget != null)
		{
			cancelTarget.SelectCursorElement(_withDelay: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void validateIpPort(XUiController _sender, string _newText, bool _changeFromCode)
	{
		int result;
		bool enabled = int.TryParse(txtPort.Text, out result) && txtIp.Text.Length > 0 && result > 0 && result < 65533;
		btnDirectConnectConnect.Enabled = enabled;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void saveIpPort()
	{
		if (txtIp != null && btnDirectConnectConnect.Enabled)
		{
			GamePrefs.Set(EnumGamePrefs.ConnectToServerIP, txtIp.Text);
			GamePrefs.Set(EnumGamePrefs.ConnectToServerPort, StringParsers.ParseSInt32(txtPort.Text));
		}
	}
}
