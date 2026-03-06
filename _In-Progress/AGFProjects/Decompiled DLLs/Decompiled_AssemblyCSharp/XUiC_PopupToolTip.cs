using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PopupToolTip : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class Tooltip : IEquatable<Tooltip>
	{
		public readonly string Key;

		public readonly string Text;

		public readonly string AlertSoundName;

		public readonly ToolTipEvent Event;

		public readonly float Timeout = -1f;

		public readonly bool Pinned;

		public bool RemoveOnDequeue;

		public readonly float CreationTime;

		public Tooltip(string _key, string _text, string _alertSoundName, ToolTipEvent _event, float _timeout = 0f, bool _pinned = false)
		{
			Key = _key;
			Text = _text;
			AlertSoundName = _alertSoundName;
			Event = _event;
			Timeout = _timeout;
			Pinned = _pinned;
			CreationTime = Time.time;
		}

		public bool Equals(Tooltip _other)
		{
			if (_other == null)
			{
				return false;
			}
			if (this == _other)
			{
				return true;
			}
			return Text == _other.Text;
		}

		public override bool Equals(object _obj)
		{
			if (_obj == null)
			{
				return false;
			}
			if (this == _obj)
			{
				return true;
			}
			if (_obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((Tooltip)_obj);
		}

		public override int GetHashCode()
		{
			if (Text == null)
			{
				return 0;
			}
			return Text.GetHashCode();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int yOffsetSecondRow = 75;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Toolbelt toolbelt;

	[PublicizedFrom(EAccessModifier.Private)]
	public string tooltipText = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public float textAlphaTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public float textAlphaCurrent;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CountdownTimer countdownTooltip = new CountdownTimer(5f);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pauseToolTips;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Queue<Tooltip> tooltipQueue = new Queue<Tooltip>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Tooltip immediateTip;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt textalphaFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt yoffsetFormatter = new CachedStringFormatterInt();

	public float TextAlphaCurrent
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return textAlphaCurrent;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			textAlphaCurrent = value;
			IsDirty = true;
		}
	}

	public string TooltipText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			tooltipText = value;
			IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		toolbelt = base.xui.GetChildByType<XUiC_Toolbelt>();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (GameStats.GetInt(EnumGameStats.GameState) == 1)
		{
			if ((base.xui.playerUI != null && base.xui.playerUI.entityPlayer != null && base.xui.playerUI.entityPlayer.IsDead()) || !base.xui.playerUI.windowManager.IsHUDEnabled())
			{
				ClearTooltipsInternal();
			}
			if (!pauseToolTips)
			{
				TextAlphaCurrent = Mathf.Lerp(textAlphaCurrent, textAlphaTarget, _dt * 3f);
				if (countdownTooltip.HasPassed() && base.xui.isReady && !XUiC_SubtitlesDisplay.IsDisplaying)
				{
					DisplayTooltipText();
				}
			}
		}
		if (IsDirty)
		{
			RefreshBindings(_forceAll: true);
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "text":
			_value = tooltipText;
			return true;
		case "textalpha":
			_value = textalphaFormatter.Format((int)(255f * textAlphaCurrent));
			return true;
		case "yoffset_secondrow":
			_value = yoffsetFormatter.Format((toolbelt != null && toolbelt.HasSecondRow) ? yOffsetSecondRow : 0);
			return true;
		default:
			return false;
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "yoffset_second_row")
		{
			yOffsetSecondRow = StringParsers.ParseSInt32(_value);
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearTooltipsInternal()
	{
		tooltipQueue.Clear();
		TextAlphaCurrent = 0f;
		textAlphaTarget = 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QueueTooltipInternal(string _text, string[] _args, string _alertSound, ToolTipEvent _eventHandler, bool _showImmediately, bool _pinTooltip, float _timeout)
	{
		if (string.IsNullOrEmpty(_text) && string.IsNullOrEmpty(_alertSound) && _eventHandler == null)
		{
			return;
		}
		string text = Localization.Get(_text);
		if (_args != null && _args.Length != 0)
		{
			text = string.Format(text, _args);
		}
		Tooltip item = new Tooltip(_text, text, _alertSound, _eventHandler, _timeout, _pinTooltip);
		if (_pinTooltip)
		{
			immediateTip = item;
			if (!tooltipQueue.Contains(item))
			{
				tooltipQueue.Enqueue(item);
			}
			DisplayTooltipText();
		}
		else if (_showImmediately)
		{
			immediateTip = item;
			DisplayTooltipText();
		}
		else if (!tooltipQueue.Contains(item))
		{
			tooltipQueue.Enqueue(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemovePinnedTooltipInternal(string _key)
	{
		foreach (Tooltip item in tooltipQueue)
		{
			if (item.Pinned && item.Key == _key)
			{
				item.RemoveOnDequeue = true;
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DisplayTooltipText()
	{
		if (tooltipQueue.Count == 0 && immediateTip == null)
		{
			TextAlphaCurrent = 0f;
			TooltipText = string.Empty;
			return;
		}
		Tooltip tooltip = null;
		bool flag = false;
		if (immediateTip != null)
		{
			tooltip = immediateTip;
			immediateTip = null;
		}
		else
		{
			while (tooltipQueue.Count > 0)
			{
				tooltip = tooltipQueue.Dequeue();
				if (tooltip.RemoveOnDequeue)
				{
					tooltip = null;
					continue;
				}
				if (tooltip.Pinned)
				{
					if (tooltipQueue.Count == 0)
					{
						flag = true;
					}
					tooltipQueue.Enqueue(tooltip);
					break;
				}
				if (!(tooltip.Timeout > 0f) || !(Time.time - tooltip.CreationTime >= tooltip.Timeout))
				{
					break;
				}
				tooltip = null;
			}
		}
		if (tooltip == null)
		{
			TextAlphaCurrent = 0f;
			TooltipText = string.Empty;
			return;
		}
		if (!string.IsNullOrEmpty(tooltip.AlertSoundName))
		{
			Manager.PlayInsidePlayerHead(tooltip.AlertSoundName);
		}
		tooltip.Event?.HandleEvent();
		TextAlphaCurrent = (flag ? TextAlphaCurrent : 0f);
		if (!string.IsNullOrEmpty(tooltip.Text))
		{
			textAlphaTarget = 1f;
			TooltipText = tooltip.Text;
		}
		else
		{
			textAlphaTarget = 0f;
			TooltipText = "";
		}
		countdownTooltip.ResetAndRestart();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetToolTipPauseInternal(bool _isPaused)
	{
		pauseToolTips = _isPaused;
		if (_isPaused)
		{
			TextAlphaCurrent = 0f;
		}
		else
		{
			TextAlphaCurrent = textAlphaTarget;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearCurrentTooltipInternal()
	{
		TooltipText = "";
		TextAlphaCurrent = 0f;
		textAlphaTarget = 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_PopupToolTip GetInstance(XUi _xui)
	{
		return ((XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(ID))?.Controller?.GetChildByType<XUiC_PopupToolTip>();
	}

	public static void ClearTooltips(XUi _xui)
	{
		GetInstance(_xui)?.ClearTooltipsInternal();
	}

	public static void QueueTooltip(XUi _xui, string _text, string[] _args, string _alertSound, ToolTipEvent _eventHandler, bool _showImmediately, bool _pinTooltip, float _timeout = 0f)
	{
		GetInstance(_xui)?.QueueTooltipInternal(_text, _args, _alertSound, _eventHandler, _showImmediately, _pinTooltip, _timeout);
	}

	public static void SetToolTipPause(XUi _xui, bool _isPaused)
	{
		GetInstance(_xui)?.SetToolTipPauseInternal(_isPaused);
	}

	public static void ClearCurrentTooltip(XUi _xui)
	{
		GetInstance(_xui)?.ClearCurrentTooltipInternal();
	}

	public static void RemovePinnedTooltip(XUi _xui, string _key)
	{
		GetInstance(_xui)?.RemovePinnedTooltipInternal(_key);
	}
}
