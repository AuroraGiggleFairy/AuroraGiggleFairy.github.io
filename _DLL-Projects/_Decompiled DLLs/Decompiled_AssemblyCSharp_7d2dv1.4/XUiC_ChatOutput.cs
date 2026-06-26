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

	public override void Init()
	{
		ID = windowGroup.ID;
		base.Init();
		txtOutput = (XUiV_TextList)GetChildById("txtOutput").ViewComponent;
		collider = txtOutput.UiTransform.GetComponent<BoxCollider>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddMessage(EnumGameMessages _messageType, EChatType _chatType, string _message)
	{
		if (txtOutput == null)
		{
			return;
		}
		if (!ThreadManager.IsMainThread())
		{
			ThreadManager.AddSingleTaskMainThread("AddTextListLine", [PublicizedFrom(EAccessModifier.Internal)] (object _) =>
			{
				AddMessage(_messageType, _chatType, _message);
			});
			return;
		}
		if (_messageType == EnumGameMessages.Chat)
		{
			switch (_chatType)
			{
			case EChatType.Global:
				_message = "[ffffff]" + _message;
				break;
			case EChatType.Friends:
				_message = "[00bb00]" + _message;
				break;
			case EChatType.Party:
				_message = "[ffcc00]" + _message;
				break;
			case EChatType.Whisper:
				_message = "[d00000]" + _message;
				break;
			default:
				throw new ArgumentOutOfRangeException("_chatType", _chatType, null);
			}
		}
		_message = _message.Replace('\n', ' ');
		txtOutput.AddLine(_message);
		txtOutput.Label.alpha = 1f;
		currentWaitTime = fadeoutWaitTime + fadeoutDuration;
		txtOutput.TextList.scrollValue = 1f;
	}

	public static void AddMessage(XUi _xuiInstance, EnumGameMessages _messageType, EChatType _chatType, string _message, int _senderId, EMessageSender _messageSender, GeneratedTextManager.TextFilteringMode _filteringMode = GeneratedTextManager.TextFilteringMode.None)
	{
		if (_messageType == EnumGameMessages.Chat && !PermissionsManager.IsCommunicationAllowed())
		{
			return;
		}
		if (_senderId != -1)
		{
			PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_senderId);
			if (playerDataFromEntityID == null)
			{
				Log.Warning($"Could not find player name corresponding to entity id {_senderId}, discarding message");
				return;
			}
			if (playerDataFromEntityID != null && playerDataFromEntityID.PlatformData.Blocked[EBlockType.TextChat].IsBlocked())
			{
				return;
			}
			if (_messageSender == EMessageSender.SenderIdAsPlayer)
			{
				_message = Utils.CreateGameMessage(playerDataFromEntityID?.PlayerName.DisplayName, _message);
			}
		}
		if (_messageSender == EMessageSender.Server)
		{
			_message = Utils.CreateGameMessage(Localization.Get("xuiChatServer"), _message);
		}
		GeneratedTextManager.BbCodeSupportMode bbSupportMode = GeneratedTextManager.BbCodeSupportMode.Supported;
		if (_messageType == EnumGameMessages.Chat && _messageSender == EMessageSender.SenderIdAsPlayer)
		{
			bbSupportMode = GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes;
		}
		GeneratedTextManager.GetDisplayText(_message, null, [PublicizedFrom(EAccessModifier.Internal)] (string _filteredMessage) =>
		{
			XUiController xUiController = _xuiInstance.FindWindowGroupByName(ID);
			if (xUiController != null)
			{
				_filteredMessage += "[ffffffff][/url][/b][/i][/u][/s][/sub][/sup]";
				xUiController.GetChildByType<XUiC_ChatOutput>().AddMessage(_messageType, _chatType, _filteredMessage);
			}
		}, _checkBlockState: false, _filteringMode, bbSupportMode);
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
		if (!(_name == "fadeout_duration"))
		{
			if (_name == "fadeout_wait_time")
			{
				fadeoutWaitTime = StringParsers.ParseFloat(_value);
				return true;
			}
			return base.ParseAttribute(_name, _value, _parent);
		}
		fadeoutDuration = StringParsers.ParseFloat(_value);
		return true;
	}
}
