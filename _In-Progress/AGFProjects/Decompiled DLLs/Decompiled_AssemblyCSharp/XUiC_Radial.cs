using System;
using System.Collections.Generic;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Radial : XUiController
{
	public delegate void CommandHandlerDelegate(XUiC_Radial _xuiRadial, int _commandIndex, RadialContextAbs _context);

	public delegate bool RadialStillValidDelegate(XUiC_Radial _xuiRadial, RadialContextAbs _context);

	public class RadialContextAbs
	{
	}

	public class RadialContextHoldingSlotIndex : RadialContextAbs
	{
		public readonly int ItemSlotIndex;

		public RadialContextHoldingSlotIndex(int _itemSlotIndex)
		{
			ItemSlotIndex = _itemSlotIndex;
		}
	}

	public class RadialContextBlock : RadialContextAbs
	{
		public readonly Vector3i BlockPos;

		public readonly int ClusterIdx;

		public readonly BlockValue BlockValue;

		public readonly EntityPlayerLocal EntityFocusing;

		public readonly BlockActivationCommand[] Commands;

		public readonly BlockActivationCommand[] CustomCommands;

		public RadialContextBlock(Vector3i _blockPos, int _clusterIdx, BlockValue _blockValue, EntityPlayerLocal _entityFocusing, BlockActivationCommand[] _commands, BlockActivationCommand[] _customCommands)
		{
			BlockPos = _blockPos;
			ClusterIdx = _clusterIdx;
			BlockValue = _blockValue;
			EntityFocusing = _entityFocusing;
			Commands = _commands;
			CustomCommands = _customCommands;
		}
	}

	public class RadialContextEntity : RadialContextAbs
	{
		public readonly Vector3i BlockPos;

		public readonly EntityAlive EntityFocusing;

		public readonly Entity EntityFocused;

		public readonly EntityActivationCommand[] Commands;

		public readonly EntityActivationCommand[] CustomCommands;

		public RadialContextEntity(Vector3i _blockPos, EntityAlive _entityFocusing, Entity _entityFocused, EntityActivationCommand[] _commands, EntityActivationCommand[] _customCommands)
		{
			BlockPos = _blockPos;
			EntityFocusing = _entityFocusing;
			EntityFocused = _entityFocused;
			Commands = _commands;
			CustomCommands = _customCommands;
		}
	}

	public enum eDefaultQuickAction
	{
		CameraChange,
		ToggleAttachment
	}

	public static string ID;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RadialEntry[] menuItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[] menuItemState;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasSpecialActionPriorToRadialVisibility;

	[PublicizedFrom(EAccessModifier.Private)]
	public RadialContextAbs context;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIUtils.ButtonIcon selectionControllerButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public CommandHandlerDelegate commandHandler;

	[PublicizedFrom(EAccessModifier.Private)]
	public RadialStillValidDelegate validCheckDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpenRequested;

	[PublicizedFrom(EAccessModifier.Private)]
	public float displayDelay = 0.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float openTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRadialSelectedScale = 1.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showingCallouts;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label selectionText;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ItemValue> activatableItemPool = new List<ItemValue>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int toolbeltSwapDirection;

	public XUiC_RadialEntry mCurrentlySelectedEntry
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (selectedIndex >= 0 && selectedIndex < menuItem.Length)
			{
				return menuItem[selectedIndex];
			}
			return null;
		}
	}

	public override void Init()
	{
		base.Init();
		menuItem = GetChildrenByType<XUiC_RadialEntry>();
		menuItemState = new bool[menuItem.Length];
		for (int i = 0; i < menuItem.Length; i++)
		{
			menuItem[i].OnHover += XUiC_Radial_OnHover;
			menuItem[i].ViewComponent.IsVisible = false;
			menuItem[i].MenuItemIndex = i;
			menuItemState[i] = false;
		}
		selectionText = GetChildById("selection").ViewComponent as XUiV_Label;
		ID = windowGroup.ID;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiC_Radial_OnPress(XUiC_RadialEntry _sender)
	{
		selectedIndex = _sender.MenuItemIndex;
		CallContextAction();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiC_Radial_OnHover(XUiController _sender, bool _isOver)
	{
		XUiC_RadialEntry xUiC_RadialEntry = (XUiC_RadialEntry)_sender;
		if (_isOver)
		{
			SelectionEffect(xUiC_RadialEntry, _selected: true);
			selectedIndex = xUiC_RadialEntry.MenuItemIndex;
		}
		else
		{
			SelectionEffect(xUiC_RadialEntry, _selected: false);
			selectedIndex = int.MinValue;
		}
	}

	public void Open()
	{
		openTime = Time.time;
		isOpenRequested = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		for (int i = 0; i < menuItem.Length; i++)
		{
			menuItem[i].ViewComponent.IsVisible = menuItemState[i];
		}
		if (!showingCallouts)
		{
			showingCallouts = true;
			UIUtils.ButtonIcon button = (radialButtonIsLeftSide(base.xui.playerUI.playerInput) ? UIUtils.ButtonIcon.RightStick : UIUtils.ButtonIcon.LeftStick);
			base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.calloutWindow.AddCallout(button, "igcoRadialHighlight", XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.calloutWindow.AddCallout(button, "igcoRadialSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		}
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			base.xui.playerUI.entityPlayer.SetControllable(_b: false);
			base.xui.playerUI.CursorController.SetCursorHidden(_hidden: true);
		}
		base.xui.playerUI.entityPlayer.ClearMovementInputs();
	}

	public override void OnClose()
	{
		base.OnClose();
		for (int i = 0; i < menuItem.Length; i++)
		{
			SelectionEffect(menuItem[i], _selected: false);
		}
		context = null;
		commandHandler = null;
		validCheckDelegate = null;
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		showingCallouts = false;
		isOpenRequested = false;
		base.xui.playerUI.entityPlayer.SetControllable(_b: true);
		base.xui.playerUI.CursorController.SetCursorHidden(_hidden: false);
	}

	public override bool AlwaysUpdate()
	{
		return true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!isOpenRequested)
		{
			return;
		}
		bool flag = Time.time - openTime >= displayDelay;
		if (flag && !base.IsOpen)
		{
			if (validCheckDelegate != null && !validCheckDelegate(this, context))
			{
				Close();
				return;
			}
			base.xui.playerUI.windowManager.Open("radial", _bModal: true, _bIsNotEscClosable: true);
		}
		PlayerActionsLocal playerInput = base.xui.playerUI.playerInput;
		if (!radialButtonPressed(playerInput))
		{
			if (mCurrentlySelectedEntry != null)
			{
				XUiC_Radial_OnPress(mCurrentlySelectedEntry);
			}
			else
			{
				if (!flag)
				{
					if (hasSpecialActionPriorToRadialVisibility)
					{
						if (InputUtils.ShiftKeyPressed)
						{
							selectedIndex = -2;
						}
						else
						{
							selectedIndex = -1;
						}
					}
					else if (selectedIndex == int.MinValue)
					{
						for (int i = 0; i < menuItemState.Length; i++)
						{
							if (menuItemState[i])
							{
								selectedIndex = i;
								break;
							}
						}
					}
				}
				if (selectedIndex != int.MinValue && !GameManager.Instance.IsPaused())
				{
					CallContextAction();
				}
				else
				{
					Close();
				}
			}
		}
		if (base.IsOpen && radialButtonPressed(playerInput))
		{
			CalculateSelectionFromController(playerInput);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool radialButtonPressed(PlayerActionsLocal _actionSet)
	{
		if (!_actionSet.Activate.IsPressed && !_actionSet.PermanentActions.Activate.IsPressed && !_actionSet.Reload.IsPressed && !_actionSet.PermanentActions.Reload.IsPressed && !_actionSet.ToggleFlashlight.IsPressed && !_actionSet.PermanentActions.ToggleFlashlight.IsPressed && !_actionSet.Inventory.IsPressed && !_actionSet.VehicleActions.Inventory.IsPressed && !_actionSet.PermanentActions.Inventory.IsPressed && !_actionSet.Swap.IsPressed && !_actionSet.PermanentActions.Swap.IsPressed && !_actionSet.InventorySlotLeft.IsPressed && !_actionSet.InventorySlotRight.IsPressed)
		{
			if (!_actionSet.QuickMenu.IsPressed)
			{
				return _actionSet.PermanentActions.QuickMenu.IsPressed;
			}
			return true;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool radialButtonIsLeftSide(PlayerActionsLocal _actionSet)
	{
		if (!_actionSet.ToggleFlashlight.IsPressed)
		{
			return _actionSet.PermanentActions.ToggleFlashlight.IsPressed;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CallContextAction()
	{
		if (selectedIndex == int.MinValue)
		{
			Close();
			return;
		}
		if (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			Close();
			return;
		}
		if (commandHandler == null)
		{
			Close();
			return;
		}
		if (validCheckDelegate != null && !validCheckDelegate(this, context))
		{
			Close();
			return;
		}
		int commandIndex;
		if (selectedIndex < 0)
		{
			commandIndex = selectedIndex;
		}
		else
		{
			if (!menuItemState[selectedIndex])
			{
				Close();
				return;
			}
			commandIndex = menuItem[selectedIndex].CommandIndex;
		}
		RadialContextAbs radialContextAbs = context;
		CommandHandlerDelegate commandHandlerDelegate = commandHandler;
		Close();
		commandHandlerDelegate(this, commandIndex, radialContextAbs);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Close()
	{
		if (GameManager.Instance == null)
		{
			Log.Out("GetGameManager is null");
		}
		if (base.xui.playerUI.windowManager == null)
		{
			Log.Out("GetWindowManager is null");
		}
		isOpenRequested = false;
		base.xui.playerUI.windowManager.Close("radial");
	}

	public void ResetRadialEntries()
	{
		for (int i = 0; i < menuItemState.Length; i++)
		{
			menuItemState[i] = false;
			menuItem[i].SetHighlighted(_highlighted: false);
		}
	}

	public void SetCommonData(UIUtils.ButtonIcon _controllerButtonForSelect, CommandHandlerDelegate _commandHandlerFunc, RadialContextAbs _context = null, int _preSelectedCommandIndex = -1, bool _hasSpecialActionPriorToRadialVisibility = false, RadialStillValidDelegate _radialValidityCallback = null)
	{
		updateRadialButtonPositions();
		int num = currentEnabledEntriesCount();
		context = _context;
		selectionControllerButton = _controllerButtonForSelect;
		commandHandler = _commandHandlerFunc;
		validCheckDelegate = _radialValidityCallback;
		hasSpecialActionPriorToRadialVisibility = _hasSpecialActionPriorToRadialVisibility;
		DefaultSelect();
		selectedIndex = int.MinValue;
		if (_preSelectedCommandIndex >= 0)
		{
			for (int i = 0; i < menuItem.Length; i++)
			{
				if (menuItemState[i] && menuItem[i].CommandIndex == _preSelectedCommandIndex)
				{
					selectedIndex = i;
				}
			}
		}
		if (num == 0 || num == 1)
		{
			selectedIndex = 0;
			if (hasSpecialActionPriorToRadialVisibility)
			{
				if (InputUtils.ShiftKeyPressed)
				{
					selectedIndex = -2;
				}
				else
				{
					selectedIndex = -1;
				}
			}
			if (num == 1 || selectedIndex < 0)
			{
				CallContextAction();
				return;
			}
			Close();
		}
		for (int j = 0; j < menuItem.Length; j++)
		{
			menuItem[j].ViewComponent.IsVisible = j < num;
			menuItem[j].ViewComponent.Enabled = j < num;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DefaultSelect()
	{
		SetHovered(null);
		ResetIconsScale();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalculateSelectionFromController(PlayerActionsLocal _actionSet)
	{
		bool flag = base.xui.playerUI.entityPlayer != null && base.xui.playerUI.entityPlayer.AttachedToEntity is EntityVehicle;
		Vector2 vector = (flag ? _actionSet.VehicleActions.Look.Value : _actionSet.Look.Value);
		if (vector == Vector2.zero)
		{
			vector = (flag ? _actionSet.VehicleActions.LeftStick.Value : _actionSet.Move.Value);
		}
		if (vector.magnitude < 0.75f)
		{
			return;
		}
		float num = 361f;
		XUiC_RadialEntry xUiC_RadialEntry = null;
		for (int i = 0; i < menuItem.Length; i++)
		{
			if (menuItemState[i])
			{
				XUiC_RadialEntry xUiC_RadialEntry2 = menuItem[i];
				Vector3 localPosition = xUiC_RadialEntry2.ViewComponent.UiTransform.localPosition;
				float num2 = Vector2.Angle(to: new Vector2(localPosition.x, localPosition.y), from: vector);
				if (num2 < num)
				{
					num = num2;
					xUiC_RadialEntry = xUiC_RadialEntry2;
				}
			}
		}
		if (xUiC_RadialEntry != null)
		{
			SetHovered(xUiC_RadialEntry);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetIconsScale()
	{
		for (int i = 0; i < menuItem.Length; i++)
		{
			menuItem[i].ResetScale();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SelectionEffect(XUiC_RadialEntry _entry, bool _selected)
	{
		if (!_selected)
		{
			_entry?.SetScale(1f);
			selectionText.Text = "";
		}
		else if (_entry != null)
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
			{
				_entry.ViewComponent.PlayHoverSound();
			}
			_entry.SetScale(1.5f);
			selectionText.Text = _entry.SelectionText;
		}
		else
		{
			selectionText.Text = "";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetHovered(XUiC_RadialEntry _newSelected)
	{
		if (mCurrentlySelectedEntry != _newSelected)
		{
			SelectionEffect(mCurrentlySelectedEntry, _selected: false);
			selectedIndex = Array.IndexOf(menuItem, _newSelected);
			SelectionEffect(mCurrentlySelectedEntry, _selected: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentEnabledEntriesCount()
	{
		int num = 0;
		for (int i = 0; i < menuItem.Length; i++)
		{
			if (menuItemState[i])
			{
				num++;
			}
		}
		return num;
	}

	public void CreateRadialEntry(int _commandIdx, string _icon, string _atlas = "UIAtlas", string _text = "", string _selectionText = "", bool _highlighted = false)
	{
		CreateRadialEntry(_commandIdx, _icon, Color.white, _atlas, _text, _selectionText, _highlighted);
	}

	public void CreateRadialEntry(int _commandIdx, string _icon, Color _iconColor, string _atlas = "UIAtlas", string _text = "", string _selectionText = "", bool _highlighted = false)
	{
		int num = currentEnabledEntriesCount();
		if (num < menuItem.Length)
		{
			menuItemState[num] = true;
			XUiC_RadialEntry obj = menuItem[num];
			obj.CommandIndex = _commandIdx;
			obj.SetAtlas(_atlas);
			obj.SetSprite(_icon, _iconColor);
			obj.SetText(_text);
			obj.SetHighlighted(_highlighted);
			obj.SelectionText = _selectionText;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void updateRadialButtonPositions()
	{
		int num = currentEnabledEntriesCount();
		int num2 = ((num > 1) ? 50 : 0);
		float num3 = Utils.FastMax(1, num);
		int num4 = 0;
		for (int i = 0; i < menuItem.Length; i++)
		{
			float f = -MathF.PI / 2f - 2f / num3 * (float)num4 * MathF.PI;
			float x = ((float)(-num) * 12.5f - (float)num2) * Mathf.Cos(f);
			float y = ((float)(-num) * 12.5f - (float)num2) * Mathf.Sin(f);
			menuItem[i].ViewComponent.UiTransform.localPosition = new Vector3(x, y, 0f);
			num4++;
		}
	}

	public static bool RadialValidSameHoldingSlotIndex(XUiC_Radial _sender, RadialContextAbs _context)
	{
		if (!(_context is RadialContextHoldingSlotIndex radialContextHoldingSlotIndex))
		{
			return false;
		}
		return _sender.xui.playerUI.entityPlayer.inventory.holdingItemIdx == radialContextHoldingSlotIndex.ItemSlotIndex;
	}

	public void SetCurrentBlockData(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, EntityPlayerLocal _entityFocusing)
	{
		ResetRadialEntries();
		BlockActivationCommand[] blockActivationCommands = _blockValue.Block.GetBlockActivationCommands(_world, _blockValue, _cIdx, _blockPos, _entityFocusing);
		BlockActivationCommand[] customCmds = _blockValue.Block.CustomCmds;
		int num = blockActivationCommands.Length + customCmds.Length;
		for (int i = 0; i < num; i++)
		{
			if (i < blockActivationCommands.Length)
			{
				if (blockActivationCommands[i].enabled)
				{
					CreateRadialEntry(i, $"ui_game_symbol_{blockActivationCommands[i].icon}", "UIAtlas", "", Localization.Get("blockcommand_" + blockActivationCommands[i].text), blockActivationCommands[i].highlighted);
				}
				continue;
			}
			int num2 = i - blockActivationCommands.Length;
			if (customCmds[num2].enabled)
			{
				CreateRadialEntry(i, customCmds[num2].icon, customCmds[num2].iconColor, "UIAtlas", "", Localization.Get(customCmds[num2].text), customCmds[num2].highlighted);
			}
		}
		SetCommonData(UIUtils.GetButtonIconForAction(base.xui.playerUI.playerInput.Activate), handleBlockCommand, new RadialContextBlock(_blockPos, _cIdx, _blockValue, _entityFocusing, blockActivationCommands, customCmds));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleBlockCommand(XUiC_Radial _sender, int _commandIndex, RadialContextAbs _context)
	{
		if (!(_context is RadialContextBlock radialContextBlock))
		{
			return;
		}
		if (_commandIndex < radialContextBlock.Commands.Length)
		{
			radialContextBlock.BlockValue.Block.OnBlockActivated(radialContextBlock.Commands[_commandIndex].text, GameManager.Instance.World, radialContextBlock.ClusterIdx, radialContextBlock.BlockPos, radialContextBlock.BlockValue, radialContextBlock.EntityFocusing);
			return;
		}
		BlockActivationCommand blockActivationCommand = radialContextBlock.CustomCommands[_commandIndex - radialContextBlock.Commands.Length];
		if (blockActivationCommand.activateTime > 0f)
		{
			LocalPlayerUI playerUI = radialContextBlock.EntityFocusing.PlayerUI;
			playerUI.windowManager.Open("timer", _bModal: true);
			XUiC_Timer childByType = playerUI.xui.GetChildByType<XUiC_Timer>();
			TimerEventData timerEventData = new TimerEventData();
			timerEventData.Data = new object[3] { blockActivationCommand.eventName, radialContextBlock.BlockPos, radialContextBlock.EntityFocusing };
			timerEventData.CloseOnHit = true;
			timerEventData.Event += EventData_Event;
			childByType.SetTimer(blockActivationCommand.activateTime, timerEventData);
		}
		else
		{
			GameEventManager.Current.HandleAction(blockActivationCommand.eventName, null, radialContextBlock.EntityFocusing, twitchActivated: false, radialContextBlock.BlockPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_Event(TimerEventData timerData)
	{
		object[] obj = (object[])timerData.Data;
		string name = (string)obj[0];
		Vector3i vector3i = (Vector3i)obj[1];
		EntityPlayerLocal entity = obj[2] as EntityPlayerLocal;
		GameEventManager.Current.HandleAction(name, null, entity, twitchActivated: false, vector3i);
	}

	public void SetCurrentEntityData(WorldBase _world, Entity _entity, ITileEntity _te, EntityAlive _entityFocusing)
	{
		ResetRadialEntries();
		Vector3i vector3i = _te.ToWorldPos();
		EntityActivationCommand[] activationCommands = _entity.GetActivationCommands(vector3i, _entityFocusing);
		EntityActivationCommand[] customCmds = _entity.CustomCmds;
		int num = activationCommands.Length + customCmds.Length;
		for (int i = 0; i < num; i++)
		{
			if (i < activationCommands.Length)
			{
				if (activationCommands[i].enabled)
				{
					CreateRadialEntry(i, $"ui_game_symbol_{activationCommands[i].icon}", "UIAtlas", "", Localization.Get("entitycommand_" + activationCommands[i].text));
				}
				continue;
			}
			int num2 = i - activationCommands.Length;
			if (customCmds[num2].enabled)
			{
				CreateRadialEntry(i, customCmds[num2].icon, customCmds[num2].iconColor, "UIAtlas", "", Localization.Get(customCmds[num2].text));
			}
		}
		SetCommonData(UIUtils.GetButtonIconForAction(base.xui.playerUI.playerInput.Activate), handleEntityCommand, new RadialContextEntity(vector3i, _entityFocusing, _entity, activationCommands, customCmds));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleEntityCommand(XUiC_Radial _sender, int _commandIndex, RadialContextAbs _context)
	{
		if (!(_context is RadialContextEntity radialContextEntity))
		{
			return;
		}
		if (_commandIndex < radialContextEntity.Commands.Length)
		{
			radialContextEntity.EntityFocused.OnEntityActivated(_commandIndex, radialContextEntity.BlockPos, radialContextEntity.EntityFocusing);
			return;
		}
		EntityActivationCommand entityActivationCommand = radialContextEntity.CustomCommands[_commandIndex - radialContextEntity.Commands.Length];
		if (entityActivationCommand.activateTime > 0f)
		{
			LocalPlayerUI playerUI = (radialContextEntity.EntityFocusing as EntityPlayerLocal).PlayerUI;
			playerUI.windowManager.Open("timer", _bModal: true);
			XUiC_Timer childByType = playerUI.xui.GetChildByType<XUiC_Timer>();
			TimerEventData timerEventData = new TimerEventData();
			timerEventData.Data = new object[3] { entityActivationCommand.eventName, radialContextEntity.BlockPos, radialContextEntity.EntityFocusing };
			timerEventData.CloseOnHit = true;
			timerEventData.Event += EventData_Event;
			childByType.SetTimer(entityActivationCommand.activateTime, timerEventData);
		}
		else
		{
			GameEventManager.Current.HandleAction(entityActivationCommand.eventName, null, radialContextEntity.EntityFocusing, twitchActivated: false, radialContextEntity.BlockPos);
		}
	}

	public void SetActivatableItemData(EntityPlayerLocal _epl)
	{
		ResetRadialEntries();
		activatableItemPool.Clear();
		_epl.CollectActivatableItems(activatableItemPool);
		for (int i = 0; i < activatableItemPool.Count; i++)
		{
			CreateRadialEntry(i, activatableItemPool[i].ItemClass.GetIconName(), (activatableItemPool[i].Activated > 0) ? "ItemIconAtlas" : "ItemIconAtlasGreyscale", "", activatableItemPool[i].ItemClass.GetLocalizedItemName());
		}
		SetCommonData(UIUtils.GetButtonIconForAction(_epl.playerInput.Activate), handleActivatableItemCommand, new RadialContextHoldingSlotIndex(_epl.inventory.holdingItemIdx), -1, _hasSpecialActionPriorToRadialVisibility: false, RadialValidSameHoldingSlotIndex);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleActivatableItemCommand(XUiC_Radial _sender, int _commandIndex, RadialContextAbs _context)
	{
		EntityPlayerLocal entityPlayer = _sender.xui.playerUI.entityPlayer;
		MinEventParams.CopyTo(entityPlayer.MinEventContext, MinEventParams.CachedEventParam);
		activatableItemPool.Clear();
		entityPlayer.CollectActivatableItems(activatableItemPool);
		if (activatableItemPool[_commandIndex].Activated == 0)
		{
			MinEventParams.CachedEventParam.ItemValue = activatableItemPool[_commandIndex];
			activatableItemPool[_commandIndex].FireEvent(MinEventTypes.onSelfItemActivate, MinEventParams.CachedEventParam);
			activatableItemPool[_commandIndex].Activated = 1;
			entityPlayer.bPlayerStatsChanged = true;
		}
		else
		{
			MinEventParams.CachedEventParam.ItemValue = activatableItemPool[_commandIndex];
			activatableItemPool[_commandIndex].FireEvent(MinEventTypes.onSelfItemDeactivate, MinEventParams.CachedEventParam);
			activatableItemPool[_commandIndex].Activated = 0;
			entityPlayer.bPlayerStatsChanged = true;
		}
		entityPlayer.inventory.CallOnToolbeltChangedInternal();
	}

	public void SetupMenuData()
	{
		ResetRadialEntries();
		bool num = GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled);
		CreateRadialEntry(0, "ui_game_symbol_hammer", "UIAtlas", "", Localization.Get("xuiWPcrafting"));
		CreateRadialEntry(1, "ui_game_symbol_character", "UIAtlas", "", Localization.Get("xuiWPcharacter"));
		CreateRadialEntry(2, "ui_game_symbol_map", "UIAtlas", "", Localization.Get("xuiWPmap"));
		CreateRadialEntry(3, "ui_game_symbol_skills", "UIAtlas", "", Localization.Get("xuiWPskills"));
		CreateRadialEntry(4, "ui_game_symbol_quest", "UIAtlas", "", Localization.Get("xuiWPquests"));
		CreateRadialEntry(5, "ui_game_symbol_challenge", "UIAtlas", "", Localization.Get("xuiChallenges"));
		CreateRadialEntry(6, "ui_game_symbol_players", "UIAtlas", "", Localization.Get("xuiWPplayers"));
		if (num)
		{
			CreateRadialEntry(7, "ui_game_symbol_lightbulb", "UIAtlas", "", Localization.Get("xuiWPcreative"));
		}
		if (EntityDrone.DebugModeEnabled)
		{
			CreateRadialEntry(8, "ui_game_symbol_drone", Localization.Get("entityJunkDrone"));
		}
		CreateRadialEntry(9, "ui_game_symbol_chat", "UIAtlas", "", Localization.Get("inpActChatName"));
		SetCommonData(UIUtils.GetButtonIconForAction(base.xui.playerUI.playerInput.Inventory), handleMenuCommand);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleMenuCommand(XUiC_Radial _sender, int _commandIndex, RadialContextAbs _context)
	{
		EntityPlayerLocal entityPlayer = _sender.xui.playerUI.entityPlayer;
		switch (_commandIndex)
		{
		case 0:
			XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayer, "crafting");
			break;
		case 1:
			XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayer, "character");
			break;
		case 2:
			XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayer, "map");
			break;
		case 3:
			XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayer, "skills");
			break;
		case 4:
			XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayer, "quests");
			break;
		case 5:
			XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayer, "challenges");
			break;
		case 6:
			XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayer, "players");
			break;
		case 7:
			XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayer, "creative");
			break;
		case 8:
			if (entityPlayer.OwnedEntityCount > 0)
			{
				OwnedEntityData ownedEntityData = entityPlayer.GetOwnedEntities()[0];
				EntityDrone entityDrone = GameManager.Instance.World.GetEntity(ownedEntityData.Id) as EntityDrone;
				if ((bool)entityDrone)
				{
					entityDrone.startDialog(entityPlayer);
				}
			}
			break;
		case 9:
			base.xui.playerUI.windowManager.Open(XUiC_Chat.ID, _bModal: true);
			break;
		default:
			XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayer, "crafting");
			break;
		}
	}

	public void SetupToolbeltMenu(int _direction)
	{
		toolbeltSwapDirection = _direction;
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		ResetRadialEntries();
		ItemStack[] slots = entityPlayer.inventory.GetSlots();
		for (int i = 0; i < Mathf.Min(slots.Length, menuItem.Length); i++)
		{
			if (i != entityPlayer.inventory.DUMMY_SLOT_IDX)
			{
				if (slots[i].IsEmpty())
				{
					CreateRadialEntry(i, "");
				}
				else
				{
					CreateRadialEntry(i, slots[i].itemValue.ItemClass.GetIconName(), slots[i].itemValue.ItemClass.GetIconTint(slots[i].itemValue), "ItemIconAtlas", slots[i].itemValue.ItemClass.CanStack() ? slots[i].count.ToString() : "", slots[i].itemValue.ItemClass.GetLocalizedItemName());
				}
			}
		}
		SetCommonData(UIUtils.GetButtonIconForAction(base.xui.playerUI.playerInput.Swap), HandleToolbeltCommand);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleToolbeltCommand(XUiC_Radial _sender, int _commandIndex, RadialContextAbs _context)
	{
		if (_sender.mCurrentlySelectedEntry == null || Time.time - openTime < 0.4f)
		{
			EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
			if (toolbeltSwapDirection == 0)
			{
				int bestQuickSwapSlot = entityPlayer.inventory.GetBestQuickSwapSlot();
				entityPlayer.MoveController.SetInventoryIdxFromScroll(bestQuickSwapSlot);
				return;
			}
			int focusedItemIdx = entityPlayer.inventory.GetFocusedItemIdx();
			if (toolbeltSwapDirection < 0)
			{
				focusedItemIdx--;
				if (focusedItemIdx < 0)
				{
					focusedItemIdx = entityPlayer.inventory.PUBLIC_SLOTS - 1;
				}
			}
			else
			{
				focusedItemIdx++;
				if (focusedItemIdx >= entityPlayer.inventory.PUBLIC_SLOTS)
				{
					focusedItemIdx = 0;
				}
			}
			entityPlayer.MoveController.SetInventoryIdxFromScroll(focusedItemIdx);
		}
		else if (_commandIndex >= 0 && base.xui.playerUI.entityPlayer.inventory.holdingItemIdx != _commandIndex)
		{
			base.xui.playerUI.entityPlayer.inventory.SetHoldingItemIdx(_commandIndex);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool getBasicBlockInfo(out EntityPlayerLocal _epl, out ItemClassBlock.ItemBlockInventoryData _ibid, out Block _blockHolding, out Block _blockSelectedShape, out bool _hasAutoRotation, out bool _onlySimpleRotations, out bool _hasCopyRotation, out bool _allowShapes, out bool _hasCopyAutoShape, out bool _hasCopyShapeLegacy, out bool _allowPainting)
	{
		_hasAutoRotation = false;
		_onlySimpleRotations = false;
		_hasCopyRotation = false;
		_hasCopyAutoShape = false;
		_hasCopyShapeLegacy = false;
		_allowPainting = false;
		_epl = base.xui.playerUI.entityPlayer;
		Inventory inventory = _epl.inventory;
		_blockHolding = inventory.GetHoldingBlock().GetBlock();
		_ibid = inventory.holdingItemData as ItemClassBlock.ItemBlockInventoryData;
		_allowShapes = _blockHolding.SelectAlternates;
		_blockSelectedShape = null;
		if (_blockHolding == null || _ibid == null)
		{
			return false;
		}
		_blockSelectedShape = (_allowShapes ? _blockHolding.GetAltBlock(_ibid.itemValue.Meta) : _blockHolding);
		_hasAutoRotation = _blockSelectedShape.BlockPlacementHelper != BlockPlacement.None;
		_onlySimpleRotations = (_blockSelectedShape.AllowedRotations & EBlockRotationClasses.Advanced) == 0;
		_hasCopyRotation = _epl.HitInfo.bHitValid && !_epl.HitInfo.hit.blockValue.isair && _blockSelectedShape.SupportsRotation(_epl.HitInfo.hit.blockValue.rotation);
		if (_allowShapes && _epl.HitInfo.bHitValid)
		{
			Block block = _epl.HitInfo.hit.blockValue.Block;
			if (block.GetAutoShapeType() != EAutoShapeType.None && _blockHolding.AutoShapeSupportsShapeName(block.GetAutoShapeShapeName()))
			{
				_hasCopyAutoShape = true;
			}
			else if (_blockHolding.ContainsAlternateBlock(block.GetBlockName()))
			{
				_hasCopyShapeLegacy = true;
			}
		}
		_allowPainting = _blockSelectedShape.shape is BlockShapeNew && _blockSelectedShape.MeshIndex == 0;
		return true;
	}

	public void SetupBlockShapeData()
	{
		ResetRadialEntries();
		if (!getBasicBlockInfo(out var _epl, out var _, out var _, out var _, out var _hasAutoRotation, out var _onlySimpleRotations, out var _hasCopyRotation, out var _allowShapes, out var _hasCopyAutoShape, out var _hasCopyShapeLegacy, out var _allowPainting))
		{
			SetCommonData(UIUtils.GetButtonIconForAction(base.xui.playerUI.playerInput.Activate), handleBlockShapeCommand);
			return;
		}
		if (_allowShapes)
		{
			CreateRadialEntry(0, "ui_game_symbol_all_blocks", "UIAtlas", "", Localization.Get("xuiShape"));
			if (_hasCopyAutoShape || _hasCopyShapeLegacy)
			{
				CreateRadialEntry(1, "ui_game_symbol_copy_shape", "UIAtlas", "", Localization.Get("xuiCopyShape"));
				if (!_onlySimpleRotations && _hasCopyRotation)
				{
					CreateRadialEntry(8, "ui_game_symbol_copy_shape_and_rotation", "UIAtlas", "", Localization.Get("xuiCopyShapeAndRotation"));
				}
			}
		}
		CreateRadialEntry(2, "ui_game_symbol_rotate_simple", "UIAtlas", "", Localization.Get("xuiSimpleRotation"));
		if (!_onlySimpleRotations)
		{
			CreateRadialEntry(3, "ui_game_symbol_rotate_advanced", "UIAtlas", "", Localization.Get("xuiAdvancedRotation"));
			CreateRadialEntry(4, "ui_game_symbol_rotate_on_face", "UIAtlas", "", Localization.Get("xuiOnFaceRotation"));
			if (_hasAutoRotation)
			{
				CreateRadialEntry(5, "ui_game_symbol_rotate_auto", "UIAtlas", "", Localization.Get("xuiAutoRotation"));
			}
			if (_hasCopyRotation)
			{
				CreateRadialEntry(6, "ui_game_symbol_paint_copy_block", "UIAtlas", "", Localization.Get("xuiCopyRotation"));
			}
		}
		if (_allowPainting && (GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled)))
		{
			CreateRadialEntry(7, "ui_game_symbol_paint_bucket", "UIAtlas", "", Localization.Get("xuiMaterials"));
		}
		SetCommonData(UIUtils.GetButtonIconForAction(base.xui.playerUI.playerInput.Activate), handleBlockShapeCommand, new RadialContextHoldingSlotIndex(_epl.inventory.holdingItemIdx), -1, _hasSpecialActionPriorToRadialVisibility: true, RadialValidSameHoldingSlotIndex);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleBlockShapeCommand(XUiC_Radial _sender, int _commandIndex, RadialContextAbs _context)
	{
		if (!getBasicBlockInfo(out var _epl, out var _ibid, out var _blockHolding, out var _blockSelectedShape, out var _hasAutoRotation, out var _onlySimpleRotations, out var _hasCopyRotation, out var _allowShapes, out var _hasCopyAutoShape, out var _hasCopyShapeLegacy, out var _))
		{
			return;
		}
		switch (_commandIndex)
		{
		case -2:
		case 0:
			if (_allowShapes)
			{
				base.xui.GetChildByType<XUiC_ShapesWindow>().ItemValue = _epl.inventory.holdingItemItemValue.Clone();
				base.xui.playerUI.windowManager.Open("shapes", _bModal: true);
			}
			break;
		case -1:
			_blockSelectedShape.RotateHoldingBlock(_ibid, _increaseRotation: false);
			break;
		case 1:
			if (_hasCopyAutoShape || _hasCopyShapeLegacy)
			{
				Block block = _epl.HitInfo.hit.blockValue.Block;
				copyShape(_epl, _hasCopyAutoShape, _blockHolding, block, _ibid);
			}
			break;
		case 2:
			_ibid.mode = BlockPlacement.EnumRotationMode.Simple;
			break;
		case 3:
			if (!_onlySimpleRotations)
			{
				_ibid.mode = BlockPlacement.EnumRotationMode.Advanced;
			}
			break;
		case 4:
			if (!_onlySimpleRotations)
			{
				_ibid.mode = BlockPlacement.EnumRotationMode.ToFace;
			}
			break;
		case 5:
			if (!_onlySimpleRotations && _hasAutoRotation)
			{
				_ibid.mode = BlockPlacement.EnumRotationMode.Auto;
			}
			break;
		case 6:
			if (!_onlySimpleRotations && _hasCopyRotation)
			{
				BlockValue blockValue2 = _epl.HitInfo.hit.blockValue;
				if (blockValue2.ischild)
				{
					blockValue2 = _epl.world.GetBlock(blockValue2.Block.multiBlockPos.GetParentPos(_epl.HitInfo.hit.blockPos, blockValue2));
				}
				copyRotation(_ibid, blockValue2);
			}
			break;
		case 7:
			base.xui.playerUI.windowManager.Open("materials", _bModal: true);
			break;
		case 8:
			if (!_onlySimpleRotations && _hasCopyRotation && (_hasCopyAutoShape || _hasCopyShapeLegacy))
			{
				BlockValue blockValue = _epl.HitInfo.hit.blockValue;
				if (blockValue.ischild)
				{
					blockValue = _epl.world.GetBlock(blockValue.Block.multiBlockPos.GetParentPos(_epl.HitInfo.hit.blockPos, blockValue));
				}
				copyShape(_epl, _hasCopyAutoShape, _blockHolding, blockValue.Block, _ibid);
				copyRotation(_ibid, blockValue);
			}
			break;
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void copyRotation(ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData, BlockValue _bvLookingAt)
		{
			itemBlockInventoryData.rotation = _bvLookingAt.rotation;
			itemBlockInventoryData.mode = BlockPlacement.EnumRotationMode.Advanced;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void copyShape(EntityPlayerLocal entityPlayerLocal, bool flag, Block _block, Block _targetedBlock, ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData)
		{
			int num = (flag ? _block.AutoShapeAlternateShapeNameIndex(_targetedBlock.GetAutoShapeShapeName()) : _block.GetAlternateBlockIndex(_targetedBlock.GetBlockName()));
			if (num >= 0)
			{
				itemBlockInventoryData.itemValue.Meta = num;
				XUiC_Toolbelt childByType = ((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow("toolbelt")).Controller.GetChildByType<XUiC_Toolbelt>();
				if (childByType != null)
				{
					int holdingItemIdx = entityPlayerLocal.inventory.holdingItemIdx;
					XUiC_ItemStack slotControl = childByType.GetSlotControl(holdingItemIdx);
					slotControl.ItemStack = new ItemStack(itemBlockInventoryData.itemValue, itemBlockInventoryData.itemStack.count);
					slotControl.ForceRefreshItemStack();
				}
			}
		}
	}

	public void SetupQuickActionsMenu(EntityPlayerLocal _epl)
	{
		ResetRadialEntries();
		activatableItemPool.Clear();
		_epl.CollectActivatableItems(activatableItemPool);
		int num = GamePrefs.GetInt(EnumGamePrefs.OptionsControlsDefaultQuickAction);
		if (num == 0)
		{
			CreateRadialEntry(0, "ui_game_symbol_camera", "UIAtlas", "", Localization.Get("inpActCameraChangeName"));
			CreateRadialEntry(1, "ui_game_symbol_drop_item", "UIAtlas", "", Localization.Get("inpActPlayerDropName"));
		}
		else
		{
			if (activatableItemPool.Count > 0)
			{
				CreateRadialEntry(0, activatableItemPool[0].ItemClass.GetIconName(), (activatableItemPool[0].Activated > 0) ? "ItemIconAtlas" : "ItemIconAtlasGreyscale", "", activatableItemPool[0].ItemClass.GetLocalizedItemName());
			}
			else
			{
				CreateRadialEntry(0, "ui_game_symbol_x", "UIAtlas", "", Localization.Get("radialNoActivatableItems"));
			}
			CreateRadialEntry(1, "ui_game_symbol_camera", "UIAtlas", "", Localization.Get("inpActCameraChangeName"));
			CreateRadialEntry(2, "ui_game_symbol_drop_item", "UIAtlas", "", Localization.Get("inpActPlayerDropName"));
		}
		for (int i = ((num == 1) ? 1 : 0); i < activatableItemPool.Count; i++)
		{
			int commandIdx = i + 2;
			CreateRadialEntry(commandIdx, activatableItemPool[i].ItemClass.GetIconName(), (activatableItemPool[i].Activated > 0) ? "ItemIconAtlas" : "ItemIconAtlasGreyscale", "", activatableItemPool[i].ItemClass.GetLocalizedItemName());
		}
		SetCommonData(UIUtils.GetButtonIconForAction(base.xui.playerUI.playerInput.PermanentActions.QuickMenu), HandleQuickActionsCommand);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleQuickActionsCommand(XUiC_Radial _sender, int _commandIndex, RadialContextAbs _context)
	{
		EntityPlayerLocal entityPlayer = _sender.xui.playerUI.entityPlayer;
		eDefaultQuickAction eDefaultQuickAction2 = (eDefaultQuickAction)GamePrefs.GetInt(EnumGamePrefs.OptionsControlsDefaultQuickAction);
		switch (_commandIndex)
		{
		case 0:
			if (eDefaultQuickAction2 == eDefaultQuickAction.CameraChange)
			{
				entityPlayer.MoveController.cameraChangeRequested = true;
			}
			else if (activatableItemPool.Count > 0)
			{
				handleActivatableItemCommand(this, 0, _context);
			}
			break;
		case 1:
			if (eDefaultQuickAction2 == eDefaultQuickAction.CameraChange)
			{
				entityPlayer.MoveController.DropHeldItem();
			}
			else
			{
				entityPlayer.MoveController.cameraChangeRequested = true;
			}
			break;
		case 2:
			if (eDefaultQuickAction2 == eDefaultQuickAction.CameraChange)
			{
				handleActivatableItemCommand(this, _commandIndex - 2, _context);
			}
			else
			{
				entityPlayer.MoveController.DropHeldItem();
			}
			break;
		default:
			if (_commandIndex > 1)
			{
				handleActivatableItemCommand(this, _commandIndex - 2, _context);
			}
			break;
		}
	}
}
