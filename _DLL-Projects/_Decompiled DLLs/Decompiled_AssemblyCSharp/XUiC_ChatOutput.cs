using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChatOutput : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_TextList txtOutput;

	[PublicizedFrom(EAccessModifier.Private)]
	public BoxCollider collider;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fadeoutWaitTime = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fadeoutDuration = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentWaitTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useChatTypeColorForMessage;

	[PublicizedFrom(EAccessModifier.Private)]
	public string textColorMessage = Color.white.ToHexCode();

	[PublicizedFrom(EAccessModifier.Private)]
	public string textColorGlobal = Color.white.ToHexCode();

	[PublicizedFrom(EAccessModifier.Private)]
	public string textColorFriends = new Color(0f, 0.75f, 0f).ToHexCode();

	[PublicizedFrom(EAccessModifier.Private)]
	public string textColorParty = new Color(1f, 0.8f, 0f).ToHexCode();

	[PublicizedFrom(EAccessModifier.Private)]
	public string textColorWhisper = new Color(0.8f, 0f, 0f).ToHexCode();

	[PublicizedFrom(EAccessModifier.Private)]
	public const string TextColorDiscord = "5865f2";

	public override void Init()
	{
		ID = windowGroup.ID;
		base.Init();
		txtOutput = (XUiV_TextList)GetChildById("txtOutput").ViewComponent;
		collider = txtOutput.UiTransform.GetComponent<BoxCollider>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addMessage(EnumGameMessages _messageType, EChatType _chatType, EChatDirection _chatDirection, string _message, string _senderDisplayName, string _senderHandlerId)
	{
		if (txtOutput == null)
		{
			return;
		}
		if (!ThreadManager.IsMainThread())
		{
			ThreadManager.AddSingleTaskMainThread("AddTextListLine", [PublicizedFrom(EAccessModifier.Internal)] (object _) =>
			{
				addMessage(_messageType, _chatType, _chatDirection, _message, _senderDisplayName, _senderHandlerId);
			});
			return;
		}
		if (_messageType == EnumGameMessages.Chat)
		{
			if (_senderDisplayName == null)
			{
				_senderDisplayName = "";
			}
			if (_chatType == EChatType.Discord && !string.IsNullOrEmpty(_senderDisplayName))
			{
				_senderDisplayName += " [discord] ";
			}
			string text = (string.IsNullOrEmpty(_senderDisplayName) ? "" : (XUiUtils.BuildUrlFunctionString("Chat", (key: "ChatType", value: _chatType.ToStringCached()), (key: "Sender", value: _senderHandlerId)) + _senderDisplayName + "[/url]: "));
			string text2 = (useChatTypeColorForMessage ? "" : ("[-][" + textColorMessage + "]"));
			string text3 = Localization.Get((_chatDirection == EChatDirection.Outbound) ? "xuiChatDirectionTo" : "xuiChatDirectionFrom");
			_message = _chatType switch
			{
				EChatType.Global => "[" + textColorGlobal + "]" + text + text2 + XUiUtils.BuildUrlFunctionString("Chat", (key: "ChatType", value: _chatType.ToStringCached())) + _message + "[/url]", 
				EChatType.Friends => "[" + textColorFriends + "]" + XUiUtils.BuildUrlFunctionString("Chat", (key: "ChatType", value: _chatType.ToStringCached())) + "(Friends)[/url] " + text + text2 + _message, 
				EChatType.Party => "[" + textColorParty + "]" + XUiUtils.BuildUrlFunctionString("Chat", (key: "ChatType", value: _chatType.ToStringCached())) + "(Party)[/url] " + text + text2 + _message, 
				EChatType.Whisper => "[" + textColorWhisper + "]" + text3 + " " + text + text2 + _message, 
				EChatType.Discord => "[5865f2]" + text3 + " " + text + text2 + _message, 
				_ => throw new ArgumentOutOfRangeException("_chatType", _chatType, null), 
			};
		}
		_message = _message.Replace('\n', ' ');
		txtOutput.AddLine(_message);
		txtOutput.Label.alpha = 1f;
		currentWaitTime = fadeoutWaitTime + fadeoutDuration;
		txtOutput.TextList.scrollValue = 1f;
	}

	public static void AddMessage(XUi _xuiInstance, EnumGameMessages _messageType, string _message, EChatType _chatType = EChatType.Global, EChatDirection _chatDirection = EChatDirection.Inbound, int _senderId = -1, string _senderDisplayName = null, string _senderHandlerId = null, EMessageSender _messageSenderType = EMessageSender.None, GeneratedTextManager.TextFilteringMode _filteringMode = GeneratedTextManager.TextFilteringMode.None, GeneratedTextManager.BbCodeSupportMode _bbMode = GeneratedTextManager.BbCodeSupportMode.Supported)
	{
		if (_messageType == EnumGameMessages.Chat && !PermissionsManager.IsCommunicationAllowed())
		{
			return;
		}
		PlatformUserIdentifierAbs author = null;
		if (_senderId != -1 || _messageSenderType == EMessageSender.SenderIdAsPlayer)
		{
			PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_senderId);
			if (playerDataFromEntityID == null)
			{
				Log.Warning($"Could not find player name corresponding to entity id {_senderId}, discarding message");
				return;
			}
			if (playerDataFromEntityID.PlatformData.Blocked[EBlockType.TextChat].IsBlocked())
			{
				return;
			}
			author = playerDataFromEntityID.PrimaryId;
			_senderDisplayName = playerDataFromEntityID.PlayerName.DisplayName;
		}
		if (_messageType == EnumGameMessages.Chat && _messageSenderType == EMessageSender.SenderIdAsPlayer)
		{
			_bbMode = GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes;
		}
		GeneratedTextManager.GetDisplayText(_message, author, [PublicizedFrom(EAccessModifier.Internal)] (string _filteredMessage) =>
		{
			XUiController xUiController = _xuiInstance.FindWindowGroupByName(ID);
			if (xUiController != null)
			{
				_filteredMessage += "[ffffffff][/url][/b][/i][/u][/s][/sub][/sup]";
				XUiC_ChatOutput childByType = xUiController.GetChildByType<XUiC_ChatOutput>();
				string text = ((_messageSenderType != EMessageSender.Server) ? _senderDisplayName : Localization.Get("xuiChatServer"));
				_senderDisplayName = text;
				childByType.addMessage(_messageType, _chatType, _chatDirection, _filteredMessage, _senderDisplayName, _senderHandlerId);
			}
		}, _checkBlockState: false, _filteringMode, _bbMode);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		bool flag = base.xui.playerUI.windowManager.IsWindowOpen(XUiC_Chat.ID);
		collider.enabled = flag;
		if (flag)
		{
			currentWaitTime = fadeoutWaitTime + fadeoutDuration;
		}
		txtOutput.Label.alpha = Mathf.Lerp(0f, 1f, currentWaitTime / fadeoutDuration);
		currentWaitTime -= Time.deltaTime;
		if (GameManager.Instance == null || GameManager.Instance.World == null || base.xui.playerUI.entityPlayer == null || base.xui.playerUI.entityPlayer.IsDead())
		{
			txtOutput.Label.alpha = 0f;
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "fadeout_duration":
			fadeoutDuration = StringParsers.ParseFloat(_value);
			return true;
		case "fadeout_wait_time":
			fadeoutWaitTime = StringParsers.ParseFloat(_value);
			return true;
		case "use_chattype_color_for_message":
			useChatTypeColorForMessage = StringParsers.ParseBool(_value);
			return true;
		case "text_color_message":
			textColorMessage = _value;
			return true;
		case "text_color_global":
			textColorGlobal = _value;
			return true;
		case "text_color_friends":
			textColorFriends = _value;
			return true;
		case "text_color_party":
			textColorParty = _value;
			return true;
		case "text_color_whisper":
			textColorWhisper = _value;
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		}
	}
}
