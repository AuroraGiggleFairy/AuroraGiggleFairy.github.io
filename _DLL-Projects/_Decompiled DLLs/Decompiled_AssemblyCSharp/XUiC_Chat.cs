using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Chat : XUiController
{
	[UsedImplicitly]
	[PublicizedFrom(EAccessModifier.Private)]
	public class ChatTarget : IComparable<ChatTarget>
	{
		public readonly EChatType ChatType;

		public readonly string TargetId;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ChatMessagingHandler messageHandler;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string displayText;

		[PublicizedFrom(EAccessModifier.Private)]
		public DateTime lastUsed;

		public readonly bool KeepForever;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Func<ChatTarget, XUi, bool> ValidCondition;

		public TimeSpan Age => DateTime.Now - lastUsed;

		public ChatTarget(EChatType _chatType, string _targetId = null, Func<ChatTarget, XUi, bool> _validCondition = null, bool _keepForever = false)
		{
			ChatType = _chatType;
			TargetId = _targetId;
			ValidCondition = _validCondition;
			messageHandler = messagingHandlers[(int)_chatType];
			if (messageHandler == null)
			{
				throw new ArgumentException($"Missing handler for chat type {_chatType}");
			}
			displayText = messageHandler.GetTargetDisplayNameDelegate(_chatType, _targetId);
			KeepForever = _keepForever;
			lastUsed = DateTime.Now;
		}

		public override string ToString()
		{
			return displayText;
		}

		public void Send(string _message)
		{
			messageHandler.SendMessageDelegate(ChatType, TargetId, _message);
			lastUsed = DateTime.Now;
		}

		public bool IsValid(XUi _xuiInstance)
		{
			return ValidCondition?.Invoke(this, _xuiInstance) ?? messageHandler?.IsValidTargetDelegate(ChatType, TargetId) ?? true;
		}

		public int CompareTo(ChatTarget _other)
		{
			if (this == _other)
			{
				return 0;
			}
			if (_other == null)
			{
				return -1;
			}
			bool keepForever = KeepForever;
			int num = -keepForever.CompareTo(_other.KeepForever);
			if (num != 0)
			{
				return num;
			}
			if (KeepForever)
			{
				num = ChatType.CompareTo(_other.ChatType);
				if (num != 0)
				{
					return num;
				}
			}
			return Age.CompareTo(_other.Age);
		}
	}

	public delegate bool IsValidTarget(EChatType _chatType, string _targetId);

	public delegate string GetTargetDisplayName(EChatType _chatType, string _targetId);

	public delegate void SendMessage(EChatType _chatType, string _targetId, string _message);

	[PublicizedFrom(EAccessModifier.Private)]
	public class ChatMessagingHandler
	{
		public readonly IsValidTarget IsValidTargetDelegate;

		public readonly GetTargetDisplayName GetTargetDisplayNameDelegate;

		public readonly SendMessage SendMessageDelegate;

		public ChatMessagingHandler(IsValidTarget _isValidTargetDelegate, GetTargetDisplayName _getTargetDisplayNameDelegate, SendMessage _sendMessageDelegate)
		{
			IsValidTargetDelegate = _isValidTargetDelegate;
			GetTargetDisplayNameDelegate = _getTargetDisplayNameDelegate;
			SendMessageDelegate = _sendMessageDelegate;
		}
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<ChatTarget> cbxTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> entityIdsFriends = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> entityIdsParty = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ChatTarget> allTargets = new List<ChatTarget>();

	[PublicizedFrom(EAccessModifier.Private)]
	public uint keepWhisperTargetsForMinutes = 15u;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ChatMessagingHandler[] messagingHandlers = new ChatMessagingHandler[(int)(EnumUtils.MaxValue<EChatType>() + 1)];

	public override void Init()
	{
		ID = windowGroup.ID;
		base.Init();
		cbxTarget = GetChildByType<XUiC_ComboBoxList<ChatTarget>>();
		txtInput = GetChildByType<XUiC_TextInput>();
		txtInput.OnSubmitHandler += TextInput_OnSubmitHandler;
		txtInput.OnInputAbortedHandler += TextInput_OnInputAbortedHandler;
		txtInput.SupportBbCode = false;
		registerRegularChatMessageHandlers();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void registerRegularChatMessageHandlers()
	{
		RegisterCustomMessagingHandler(EChatType.Global, IsValidTargetRegular, GetTargetDisplayNameRegular, SendMessageRegular);
		RegisterCustomMessagingHandler(EChatType.Friends, IsValidTargetRegular, GetTargetDisplayNameRegular, SendMessageRegular);
		RegisterCustomMessagingHandler(EChatType.Party, IsValidTargetRegular, GetTargetDisplayNameRegular, SendMessageRegular);
		RegisterCustomMessagingHandler(EChatType.Whisper, IsValidTargetRegular, GetTargetDisplayNameRegular, SendMessageRegular);
		allTargets.Add(new ChatTarget(EChatType.Global, null, null, _keepForever: true));
		allTargets.Add(new ChatTarget(EChatType.Friends, null, [PublicizedFrom(EAccessModifier.Private)] (ChatTarget _, XUi _) => entityIdsFriends.Count > 1, _keepForever: true));
		allTargets.Add(new ChatTarget(EChatType.Party, null, [PublicizedFrom(EAccessModifier.Private)] (ChatTarget _, XUi _) => entityIdsParty.Count > 1, _keepForever: true));
		[PublicizedFrom(EAccessModifier.Internal)]
		static string GetTargetDisplayNameRegular(EChatType _chatType, string _targetId)
		{
			if (_targetId == null)
			{
				return Localization.Get("xuiChatTarget" + _chatType.ToStringCached());
			}
			if (!int.TryParse(_targetId, out var result))
			{
				throw new ArgumentException("Could not parse chat entity id '" + _targetId + "'");
			}
			if (!GameManager.Instance.World.Players.dict.TryGetValue(result, out var value))
			{
				return Localization.Get("xuiChatTarget" + _chatType.ToStringCached());
			}
			return string.Format(Localization.Get("xuiChatTargetWhisper"), value.PlayerDisplayName);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static bool IsValidTargetRegular(EChatType _chatType, string _targetId)
		{
			if (_targetId == null)
			{
				return true;
			}
			if (!int.TryParse(_targetId, out var result))
			{
				throw new ArgumentException("Could not parse chat entity id '" + _targetId + "' for target validation");
			}
			if (!GameManager.Instance.World.Players.dict.TryGetValue(result, out var _))
			{
				return false;
			}
			return result != GameManager.Instance.World.GetPrimaryPlayer().entityId;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void SendMessageRegular(EChatType _chatType, string _targetId, string _message)
		{
			List<int> recipientEntityIds;
			if (_targetId == null)
			{
				recipientEntityIds = _chatType switch
				{
					EChatType.Global => null, 
					EChatType.Friends => entityIdsFriends, 
					EChatType.Party => entityIdsParty, 
					EChatType.Whisper => throw new ArgumentException("Whisper without ID not supported"), 
					_ => throw new ArgumentException("Invalid chat type without ID"), 
				};
			}
			else
			{
				if (!int.TryParse(_targetId, out var result))
				{
					throw new ArgumentException("Could not parse chat entity id '" + _targetId + "'");
				}
				_chatType = EChatType.Whisper;
				recipientEntityIds = new List<int> { result };
			}
			GameManager.Instance.ChatMessageServer(null, _chatType, base.xui.playerUI.entityPlayer.entityId, _message, recipientEntityIds, EMessageSender.SenderIdAsPlayer);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TextInput_OnInputAbortedHandler(XUiController _sender)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TextInput_OnSubmitHandler(XUiController _sender, string _text)
	{
		if (_text.Length > 0 && _text != " ")
		{
			_text = _text.Replace('\n', ' ');
			cbxTarget.Value.Send(_text);
			txtInput.Text = "";
		}
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	public override void OnOpen()
	{
		cbxTarget.Enabled = PermissionsManager.IsCommunicationAllowed();
		txtInput.Enabled = PermissionsManager.IsCommunicationAllowed();
		updateTargets();
		base.OnOpen();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateTargets()
	{
		entityIdsFriends.Clear();
		entityIdsFriends.Add(base.xui.playerUI.entityPlayer.entityId);
		foreach (EntityPlayer item in GameManager.Instance.World.Players.list)
		{
			if (item.IsFriendOfLocalPlayer)
			{
				entityIdsFriends.Add(item.entityId);
			}
		}
		entityIdsParty.Clear();
		entityIdsParty.Add(base.xui.playerUI.entityPlayer.entityId);
		if (base.xui.playerUI.entityPlayer.Party != null)
		{
			foreach (EntityPlayer member in base.xui.playerUI.entityPlayer.Party.MemberList)
			{
				if (member != base.xui.playerUI.entityPlayer)
				{
					entityIdsParty.Add(member.entityId);
				}
			}
		}
		allTargets.Sort();
		ChatTarget value = cbxTarget.Value;
		cbxTarget.Elements.Clear();
		for (int num = allTargets.Count - 1; num >= 0; num--)
		{
			ChatTarget chatTarget = allTargets[num];
			if (!chatTarget.KeepForever && chatTarget.Age.TotalMinutes > (double)keepWhisperTargetsForMinutes)
			{
				allTargets.RemoveAt(num);
			}
			else if (!chatTarget.IsValid(base.xui))
			{
				if (!chatTarget.KeepForever)
				{
					allTargets.RemoveAt(num);
				}
			}
			else
			{
				cbxTarget.Elements.Insert(0, chatTarget);
			}
		}
		if (value != null)
		{
			int num2 = cbxTarget.Elements.IndexOf(value);
			cbxTarget.SelectedIndex = ((num2 >= 0) ? num2 : 0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int enforceTarget(EChatType _chatType, string _targetId)
	{
		int num = cbxTarget.Elements.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (ChatTarget _target) => _target.ChatType == _chatType && _targetId == _target.TargetId);
		if (num >= 0)
		{
			return num;
		}
		if (string.IsNullOrEmpty(_targetId))
		{
			return -1;
		}
		allTargets.Add(new ChatTarget(_chatType, _targetId));
		updateTargets();
		return cbxTarget.Elements.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (ChatTarget _target) => _target.ChatType == _chatType && _targetId == _target.TargetId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void findOrAddTarget(EChatType _chatType, string _targetId)
	{
		int num = enforceTarget(_chatType, _targetId);
		if (num >= 0)
		{
			cbxTarget.SelectedIndex = num;
			txtInput.SetSelected();
		}
	}

	public override void UpdateInput()
	{
		base.UpdateInput();
		if (base.xui.playerUI.playerInput == null)
		{
			return;
		}
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			PlayerActionsGUI gUIActions = base.xui.playerUI.playerInput.GUIActions;
			if (gUIActions.Up.WasPressed)
			{
				cbxTarget.ChangeIndex(-1);
			}
			if (gUIActions.Down.WasPressed)
			{
				cbxTarget.ChangeIndex(1);
			}
		}
		if (base.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed)
		{
			base.xui.playerUI.windowManager.Close(windowGroup.ID);
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "keep_whisper_targets_for_minutes")
		{
			keepWhisperTargetsForMinutes = StringParsers.ParseUInt32(_value);
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}

	public static void RegisterCustomMessagingHandler(EChatType _chatType, IsValidTarget _isValidTargetDelegate, GetTargetDisplayName _getTargetDisplayNameDelegate, SendMessage _sendMessageDelegate)
	{
		if ((int)_chatType >= messagingHandlers.Length)
		{
			Log.Warning($"Can not register chat messaging handler for invalid chat type '{(int)_chatType}'");
		}
		else
		{
			messagingHandlers[(int)_chatType] = new ChatMessagingHandler(_isValidTargetDelegate, _getTargetDisplayNameDelegate, _sendMessageDelegate);
		}
	}

	public static void SetChatTarget(XUi _xuiInstance, EChatType _chatType, string _targetId)
	{
		XUiC_Chat windowByType = _xuiInstance.GetWindowByType<XUiC_Chat>();
		if (windowByType == null)
		{
			Log.Error("No chat window found!");
		}
		else if ((int)_chatType >= messagingHandlers.Length || messagingHandlers[(int)_chatType] == null)
		{
			Log.Warning($"Can not handle chat messaging, invalid chat type '{(int)_chatType}' or no handler defined");
		}
		else if (messagingHandlers[(int)_chatType].IsValidTargetDelegate(_chatType, _targetId))
		{
			windowByType.findOrAddTarget(_chatType, _targetId);
		}
	}

	public static void EnforceTargetExists(XUi _xuiInstance, EChatType _chatType, string _targetId)
	{
		XUiC_Chat windowByType = _xuiInstance.GetWindowByType<XUiC_Chat>();
		if (windowByType == null)
		{
			Log.Error("No chat window found!");
		}
		else if ((int)_chatType >= messagingHandlers.Length || messagingHandlers[(int)_chatType] == null)
		{
			Log.Warning($"Can not handle chat messaging, invalid chat type '{(int)_chatType}' or no handler defined");
		}
		else if (messagingHandlers[(int)_chatType].IsValidTargetDelegate(_chatType, _targetId))
		{
			windowByType.enforceTarget(_chatType, _targetId);
		}
	}
}
