using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BuffPopoutList : XUiController, IEntityUINotificationChanged
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class Data
	{
		public GameObject Item;

		public float TimeAdded;

		public EntityUINotification Notification;

		public UISprite Sprite;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool initialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public float height;

	[PublicizedFrom(EAccessModifier.Private)]
	public int yOffset;

	public Transform PrefabItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 spriteSize;

	public EntityUINotification SelectedNotification;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Data> items = new List<Data>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityUINotification> disabledItems = new List<EntityUINotification>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer LocalPlayer
	{
		get; [PublicizedFrom(EAccessModifier.Internal)]
		set;
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("item");
		PrefabItems = childById.ViewComponent.UiTransform;
		height = childById.ViewComponent.Size.y + 2;
		childById.xui.BuffPopoutList = this;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (LocalPlayer == null && XUi.IsGameRunning())
		{
			LocalPlayer = base.xui.playerUI.entityPlayer;
		}
		GUIWindowManager windowManager = base.xui.playerUI.windowManager;
		if (windowManager.IsHUDEnabled() || (base.xui.dragAndDrop.InMenu && windowManager.IsHUDPartialHidden()))
		{
			if (base.ViewComponent.IsVisible && LocalPlayer.IsDead())
			{
				base.ViewComponent.IsVisible = false;
			}
			else if (!base.ViewComponent.IsVisible && !LocalPlayer.IsDead())
			{
				base.ViewComponent.IsVisible = true;
			}
		}
		else
		{
			base.ViewComponent.IsVisible = false;
		}
		for (int i = 0; i < items.Count; i++)
		{
			Data data = items[i];
			if (data.Notification.Buff != null && data.Notification.Buff.Paused)
			{
				removeEntry(data.Notification, i);
				disabledItems.Add(data.Notification);
				continue;
			}
			if (data.Notification.DisplayMode == EnumEntityUINotificationDisplayMode.IconPlusCurrentValue)
			{
				UILabel component = data.Item.transform.Find("TextContent").GetComponent<UILabel>();
				switch (data.Notification.Units)
				{
				case "%":
					if (component != null)
					{
						component.text = (int)(data.Notification.CurrentValue * 100f) + "%";
					}
					break;
				case "°":
					if (component != null)
					{
						component.text = ValueDisplayFormatters.Temperature(data.Notification.CurrentValue);
					}
					break;
				case "cvar":
				{
					BuffClass buffClass = data.Notification.Buff.BuffClass;
					if (buffClass.DisplayValueKey != null)
					{
						string format = Localization.Get(buffClass.DisplayValueKey);
						switch (buffClass.DisplayValueFormat)
						{
						case BuffClass.CVarDisplayFormat.Degrees:
							component.text = string.Format(format, ValueDisplayFormatters.Temperature(data.Notification.CurrentValue));
							break;
						case BuffClass.CVarDisplayFormat.Time:
							component.text = string.Format(format, GetCVarValueAsTimeString(data.Notification.CurrentValue));
							break;
						default:
							component.text = string.Format(format, data.Notification.CurrentValue);
							break;
						}
					}
					else if (buffClass.DisplayValueFormat == BuffClass.CVarDisplayFormat.Time)
					{
						component.text = GetCVarValueAsTimeString(data.Notification.CurrentValue);
					}
					else
					{
						component.text = ((int)data.Notification.CurrentValue).ToString();
					}
					break;
				}
				case "duration":
					component.text = GetCVarValueAsTimeString(data.Notification.Buff.BuffClass.DurationMax - data.Notification.Buff.DurationInSeconds);
					break;
				default:
					if (data.Notification.Buff.BuffClass.DisplayValueKey != null)
					{
						if (data.Notification.Buff.BuffClass.DisplayValueFormat == BuffClass.CVarDisplayFormat.Time)
						{
							component.text = string.Format(Localization.Get(data.Notification.Buff.BuffClass.DisplayValueKey), GetCVarValueAsTimeString(data.Notification.CurrentValue));
						}
						else
						{
							component.text = string.Format(Localization.Get(data.Notification.Buff.BuffClass.DisplayValueKey), data.Notification.CurrentValue);
						}
					}
					else
					{
						component.text = ((int)data.Notification.CurrentValue).ToString();
					}
					break;
				}
			}
			else
			{
				_ = data.Notification.DisplayMode;
				_ = 2;
			}
			bool flag = false;
			if (data.Notification.Buff != null)
			{
				flag = EffectManager.GetValue(PassiveEffects.BuffBlink, null, 0f, LocalPlayer, null, data.Notification.Buff.BuffClass.NameTag, calcEquipment: false, calcHoldingItem: false, calcProgression: false) >= 1f;
			}
			if (data.Notification.Buff != null && (data.Notification.Buff.BuffClass.IconBlink || flag))
			{
				Color color = data.Notification.GetColor();
				float num = Mathf.PingPong(Time.time, 0.5f);
				data.Sprite.color = Color.Lerp(Color.grey, color, num * 4f);
				float num2 = 1f;
				if (num > 0.25f)
				{
					num2 = 1f + num - 0.25f;
				}
				data.Sprite.SetDimensions((int)(spriteSize.x * num2), (int)(spriteSize.y * num2));
			}
			else
			{
				data.Sprite.color = data.Notification.GetColor();
				data.Sprite.SetDimensions((int)spriteSize.x, (int)spriteSize.y);
			}
		}
		if (disabledItems.Count <= 0)
		{
			return;
		}
		for (int num3 = disabledItems.Count - 1; num3 >= 0; num3--)
		{
			EntityUINotification entityUINotification = disabledItems[num3];
			if (!entityUINotification.Buff.Paused)
			{
				AddNotification(entityUINotification);
				disabledItems.RemoveAt(num3);
			}
		}
		updateEntries();
	}

	public static string GetCVarValueAsTimeString(float cvarValue)
	{
		return XUiM_PlayerBuffs.GetCVarValueAsTimeString(cvarValue);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!initialized)
		{
			PrefabItems.gameObject.SetActive(value: false);
			EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
			List<EntityUINotification> notifications = entityPlayer.PlayerStats.Notifications;
			for (int i = 0; i < notifications.Count; i++)
			{
				AddNotification(notifications[i]);
			}
			entityPlayer.PlayerStats.AddUINotificationChangedDelegate(this);
			initialized = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		if (entityPlayer != null)
		{
			entityPlayer.PlayerStats.RemoveUINotificationChangedDelegate(this);
		}
		initialized = false;
		for (int i = 0; i < items.Count; i++)
		{
			UnityEngine.Object.Destroy(items[i].Item.gameObject);
		}
		items.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeEntry(EntityUINotification notification, int currentIndex = -1)
	{
		int num = ((currentIndex == -1) ? GetNotificationIndex(notification) : currentIndex);
		if (num != -1)
		{
			TemporaryObject temporaryObject = items[num].Item.transform.GetComponent<TemporaryObject>();
			if (temporaryObject == null)
			{
				temporaryObject = items[num].Item.transform.gameObject.AddComponent<TemporaryObject>();
			}
			temporaryObject.enabled = true;
			TweenColor tweenColor = items[num].Item.transform.GetComponent<TweenColor>();
			if (tweenColor == null)
			{
				tweenColor = items[num].Item.transform.gameObject.AddComponent<TweenColor>();
			}
			tweenColor.from = Color.white;
			tweenColor.to = new Color(1f, 1f, 1f, 0f);
			tweenColor.enabled = true;
			tweenColor.duration = 0.4f;
			TweenScale tweenScale = items[num].Item.gameObject.AddComponent<TweenScale>();
			tweenScale.from = Vector3.one;
			tweenScale.to = Vector3.zero;
			tweenScale.enabled = true;
			tweenScale.duration = 0.5f;
			items.RemoveAt(num);
			updateEntries();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetNotificationIndex(EntityUINotification notification)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].Notification.Subject == notification.Subject)
			{
				if (notification.Subject != EnumEntityUINotificationSubject.Buff)
				{
					return i;
				}
				if (items[i].Notification.Buff.BuffClass.ShowOnHUD && items[i].Notification.Buff.BuffClass.Name == notification.Buff.BuffClass.Name)
				{
					return i;
				}
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateEntries()
	{
		int num = 0;
		for (int i = 0; i < items.Count; i++)
		{
			if (!items[i].Notification.Buff.Paused)
			{
				TweenPosition component = items[i].Item.GetComponent<TweenPosition>();
				if ((bool)component)
				{
					UnityEngine.Object.Destroy(component);
				}
				component = items[i].Item.AddComponent<TweenPosition>();
				component.from = items[i].Item.transform.localPosition;
				component.to = new Vector3(items[i].Item.transform.localPosition.x, (float)num * height + (float)yOffset, items[i].Item.transform.localPosition.z);
				component.enabled = true;
				num++;
			}
		}
	}

	public void EntityUINotificationAdded(EntityUINotification _notification)
	{
		AddNotification(_notification);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddNotification(EntityUINotification _notification)
	{
		int notificationIndex = GetNotificationIndex(_notification);
		if (notificationIndex == -1)
		{
			if (!(_notification.Icon != ""))
			{
				return;
			}
			if (_notification.Buff != null)
			{
				if (!_notification.Buff.BuffClass.ShowOnHUD)
				{
					return;
				}
				if (_notification.Buff.Paused)
				{
					disabledItems.Add(_notification);
				}
			}
			GameObject gameObject = base.ViewComponent.UiTransform.gameObject.AddChild(PrefabItems.gameObject);
			gameObject.SetActive(value: true);
			gameObject.GetComponent<BoxCollider>().center = Vector3.zero;
			gameObject.GetComponent<UIPanel>();
			UIEventListener uIEventListener = UIEventListener.Get(gameObject.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnNotificationClicked));
			gameObject.transform.Find("Background").GetComponent<UISprite>().color = Color.white;
			UISprite component = gameObject.transform.Find("Icon").GetComponent<UISprite>();
			component.atlas = base.xui.GetAtlasByName(((UnityEngine.Object)component.atlas).name, _notification.Icon);
			component.spriteName = _notification.Icon;
			component.color = _notification.GetColor();
			if (spriteSize == Vector2.zero)
			{
				spriteSize = new Vector2(component.width, component.height);
			}
			UILabel component2 = gameObject.transform.Find("TextContent").GetComponent<UILabel>();
			if (_notification.DisplayMode == EnumEntityUINotificationDisplayMode.IconPlusCurrentValue)
			{
				string units = _notification.Units;
				if (!(units == "%"))
				{
					if (units == "°")
					{
						if (component2 != null)
						{
							component2.text = _notification.CurrentValue.ToCultureInvariantString("0") + "°";
						}
					}
					else if (component2 != null)
					{
						component2.text = _notification.CurrentValue.ToCultureInvariantString("0");
					}
				}
				else if (component2 != null)
				{
					component2.text = (_notification.CurrentValue * 100f).ToCultureInvariantString("0") + "%";
				}
			}
			gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, (float)items.Count * height + (float)yOffset, gameObject.transform.localPosition.z);
			Data data = new Data();
			data.Item = gameObject;
			data.TimeAdded = Time.time;
			data.Notification = _notification;
			data.Sprite = component;
			if (_notification.Buff != null && _notification.Buff.BuffClass.TooltipKey != null)
			{
				GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, _notification.Buff.BuffClass.TooltipKey);
			}
			items.Add(data);
		}
		else
		{
			items[notificationIndex].Notification = _notification;
			items[notificationIndex].TimeAdded = Time.time;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnNotificationClicked(GameObject go)
	{
		HandleClickForItem(go);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleClickForItem(GameObject go)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].Item == go)
			{
				Manager.PlayInsidePlayerHead("craft_click_craft");
				if (!base.xui.playerUI.windowManager.IsWindowOpen("character"))
				{
					XUiC_WindowSelector.OpenSelectorAndWindow(base.xui.playerUI.entityPlayer, "character");
				}
				SelectedNotification = items[i].Notification;
				break;
			}
		}
	}

	public void EntityUINotificationRemoved(EntityUINotification _notification)
	{
		removeEntry(_notification);
	}

	public void SetYOffset(int _yOffset)
	{
		if (_yOffset != yOffset)
		{
			yOffset = _yOffset;
			updateEntries();
		}
	}
}
