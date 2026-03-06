using System;

public class PlayerActionData
{
	public enum EAppliesToInputType
	{
		None,
		KbdMouseOnly,
		ControllerOnly,
		Both
	}

	public class ActionSetUserData
	{
		public readonly PlayerActionsBase[] bindingsConflictWithSet;

		public ActionSetUserData(params PlayerActionsBase[] _bindingsConflictWithSet)
		{
			bindingsConflictWithSet = _bindingsConflictWithSet;
		}
	}

	public class ActionTab : IComparable<ActionTab>
	{
		public readonly string tabNameKey;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int tabPriority;

		public string LocalizedName => Localization.Get(tabNameKey);

		public ActionTab(string _tabNameKey, int _tabPriority)
		{
			tabNameKey = _tabNameKey;
			tabPriority = _tabPriority;
		}

		public int CompareTo(ActionTab _other)
		{
			int num = tabPriority;
			return num.CompareTo(_other.tabPriority);
		}
	}

	public class ActionGroup : IComparable<ActionGroup>
	{
		public readonly string groupNameKey;

		public readonly string groupDescKey;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int groupPriority;

		public readonly ActionTab actionTab;

		public string LocalizedName => Localization.Get(groupNameKey);

		public string LocalizedDescription
		{
			get
			{
				string text = Localization.Get(groupDescKey);
				if (!(text != groupDescKey))
				{
					return null;
				}
				return text;
			}
		}

		public ActionGroup(string _groupNameKey, string _groupDescKey, int _groupPriority, ActionTab _actionTab)
		{
			groupNameKey = _groupNameKey;
			groupDescKey = _groupDescKey ?? (groupNameKey.Replace("Name", "") + "Desc");
			groupPriority = _groupPriority;
			actionTab = _actionTab;
		}

		public int CompareTo(ActionGroup _other)
		{
			int num = groupPriority;
			return num.CompareTo(_other.groupPriority);
		}
	}

	public class ActionUserData
	{
		public readonly string actionNameKey;

		public readonly string actionDescKey;

		public readonly ActionGroup actionGroup;

		public readonly EAppliesToInputType appliesToInputType;

		public readonly bool allowRebind;

		public readonly bool allowMultipleBindings;

		public readonly bool doNotDisplay;

		public readonly bool defaultOnStartup;

		public string LocalizedName => Localization.Get(actionNameKey);

		public string LocalizedDescription
		{
			get
			{
				string text = Localization.Get(actionDescKey);
				if (!(text != actionDescKey))
				{
					return null;
				}
				return text;
			}
		}

		public ActionUserData(string _actionNameKey, string _actionDescKey, ActionGroup _actionGroup, EAppliesToInputType _appliesToInputType = EAppliesToInputType.Both, bool _allowRebind = true, bool _allowMultipleRebindings = false, bool _doNotDisplay = false, bool _defaultOnStartup = true)
		{
			actionNameKey = _actionNameKey;
			actionDescKey = _actionDescKey ?? (actionNameKey.Replace("Name", "") + "Desc");
			actionGroup = _actionGroup;
			appliesToInputType = _appliesToInputType;
			allowRebind = _allowRebind;
			allowMultipleBindings = _allowMultipleRebindings;
			doNotDisplay = _doNotDisplay;
			defaultOnStartup = _defaultOnStartup;
			if (actionGroup == null)
			{
				throw new ArgumentNullException("_actionGroup");
			}
		}
	}

	public static readonly ActionTab TabMovement = new ActionTab("inpTabPlayerControl", 0);

	public static readonly ActionTab TabToolbelt = new ActionTab("inpTabToolbelt", 10);

	public static readonly ActionTab TabVehicle = new ActionTab("inpTabVehicle", 15);

	public static readonly ActionTab TabMenus = new ActionTab("inpTabMenus", 20);

	public static readonly ActionTab TabUi = new ActionTab("inpTabUi", 30);

	public static readonly ActionTab TabOther = new ActionTab("inpTabOther", 40);

	public static readonly ActionTab TabEdit = new ActionTab("inpTabEdit", 50);

	public static readonly ActionTab TabGlobal = new ActionTab("inpTabGlobal", 60);

	public static readonly ActionGroup GroupPlayerControl = new ActionGroup("inpGrpPlayerControlName", null, 0, TabMovement);

	public static readonly ActionGroup GroupToolbelt = new ActionGroup("inpGrpToolbeltName", null, 10, TabToolbelt);

	public static readonly ActionGroup GroupVehicle = new ActionGroup("inpGrpVehicleName", null, 15, TabVehicle);

	public static readonly ActionGroup GroupMenu = new ActionGroup("inpGrpMenuName", null, 20, TabMenus);

	public static readonly ActionGroup GroupDialogs = new ActionGroup("inpGrpDialogsName", null, 30, TabMenus);

	public static readonly ActionGroup GroupUI = new ActionGroup("inpGrpUiName", null, 40, TabUi);

	public static readonly ActionGroup GroupMp = new ActionGroup("inpGrpMpName", null, 50, TabOther);

	public static readonly ActionGroup GroupAdmin = new ActionGroup("inpGrpAdminName", null, 60, TabOther);

	public static readonly ActionGroup GroupGlobalFunctions = new ActionGroup("inpGrpGlobalFunctionsName", null, 80, TabGlobal);

	public static readonly ActionGroup GroupDebugFunctions = new ActionGroup("inpGrpDebugFunctionsName", null, 100, TabGlobal);

	public static readonly ActionGroup GroupEditCamera = new ActionGroup("inpGrpCameraName", null, 20, TabEdit);

	public static readonly ActionGroup GroupEditSelection = new ActionGroup("inpGrpSelectionName", null, 40, TabEdit);

	public static readonly ActionGroup GroupEditOther = new ActionGroup("inpGrpOtherName", null, 60, TabEdit);
}
