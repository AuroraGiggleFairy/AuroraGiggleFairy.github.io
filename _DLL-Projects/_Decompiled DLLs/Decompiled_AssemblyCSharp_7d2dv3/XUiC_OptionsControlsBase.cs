using System;
using System.Collections;
using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public abstract class XUiC_OptionsControlsBase : XUiC_OptionsDialogBase
{
	[UnityEngine.Scripting.Preserve]
	public class XUiC_BindingEntry : XUiController
	{
		[XuiBindParent(true)]
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_OptionsDialogBase parentControls;

		[PublicizedFrom(EAccessModifier.Private)]
		public PlayerAction action;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isControllerBinding;

		[PublicizedFrom(EAccessModifier.Private)]
		public int frameOpened;

		[PublicizedFrom(EAccessModifier.Private)]
		public PlayerActionData.ActionUserData actionData;

		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingString;

		[XuiBindComponent("unbind", true)]
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_Button unbind;

		[XuiBindComponent("background", true)]
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_Button button;

		public PlayerAction Action
		{
			get
			{
				return action;
			}
			set
			{
				if (action != value)
				{
					if (action != null)
					{
						action.OnBindingsChanged -= actionBindingsChanged;
					}
					action = value;
					actionData = (PlayerActionData.ActionUserData)(value?.UserData);
					if (action != null)
					{
						action.OnBindingsChanged += actionBindingsChanged;
					}
					IsDirty = true;
				}
			}
		}

		[XuiXmlAttribute("for_controller", false)]
		public bool IsControllerBinding
		{
			get
			{
				return isControllerBinding;
			}
			set
			{
				if (value != isControllerBinding)
				{
					isControllerBinding = value;
					IsDirty = true;
				}
			}
		}

		[XuiXmlBinding("has_action")]
		public bool HasAction => action != null;

		[XuiXmlBinding("allow_rebind")]
		public bool AllowRebind => actionData?.allowRebind ?? false;

		[XuiXmlBinding("action_label")]
		public string ActionLabel => actionData?.LocalizedName ?? "";

		[XuiXmlBinding("action_description")]
		public string ActionDescription => actionData?.LocalizedDescription ?? "";

		[XuiXmlBinding("action_binding")]
		public string ActionBinding => bindingString ?? "";

		public override void Init()
		{
			base.Init();
			registerForInputStyleChanges();
		}

		public override void OnOpen()
		{
			base.OnOpen();
			frameOpened = Time.frameCount;
		}

		public override void Update(float _dt)
		{
			base.Update(_dt);
			if (IsDirty)
			{
				IsDirty = false;
				bindingString = action.GetBindingString(isControllerBinding, PlatformManager.NativePlatform.Input.CurrentControllerInputStyle);
				RefreshBindings();
			}
		}

		public override void Cleanup()
		{
			base.Cleanup();
			if (action != null)
			{
				action.OnBindingsChanged -= actionBindingsChanged;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void inputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
		{
			base.inputStyleChanged(_oldStyle, _newStyle);
			IsDirty = true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void actionBindingsChanged()
		{
			if (Time.frameCount >= frameOpened + 2)
			{
				IsDirty = true;
				parentControls.SetChanged();
			}
		}

		public void ResetToDefault()
		{
			action?.ResetBindings();
		}

		[XuiBindEvent("OnPress", "button")]
		[PublicizedFrom(EAccessModifier.Private)]
		public void newBindingClick(XUiController _sender, int _mouseButton)
		{
			if (HasAction && actionData.allowRebind)
			{
				XUiC_OptionsControlsNewBinding.GetNewBinding(xui, action, isControllerBinding);
			}
		}

		[XuiBindEvent("OnPress", "unbind")]
		[PublicizedFrom(EAccessModifier.Private)]
		public void unbindButtonClick(XUiController _sender, int _mouseButton)
		{
			if (HasAction && actionData.allowRebind)
			{
				action.UnbindBindingsOfType(isControllerBinding);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool initialized;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly List<string> actionBindingsOnOpen = new List<string>();

	public static event Action OnSettingsChanged;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void doResetToDefaultsInternal()
	{
		XUiC_BindingEntry[] childControllers = TabSelector.SelectedTab.GetChildControllers<XUiC_BindingEntry>("");
		for (int i = 0; i < childControllers.Length; i++)
		{
			childControllers[i].ResetToDefault();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void createControlsEntries();

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void afterChangesSaved()
	{
		base.afterChangesSaved();
		PlayerMoveController.UpdateControlsOptions();
		storeCurrentBindings();
		GameOptionsControls.Save();
		XUiC_OptionsControlsBase.OnSettingsChanged?.Invoke();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void storeCurrentBindings()
	{
		actionBindingsOnOpen.Clear();
		foreach (PlayerActionsBase actionSet in PlatformManager.NativePlatform.Input.ActionSets)
		{
			actionBindingsOnOpen.Add(actionSet.Save());
		}
	}

	public override void OnOpen()
	{
		if (!initialized)
		{
			createControlsEntries();
			initialized = true;
		}
		ThreadManager.StartCoroutine(storeBindingsLater());
		base.OnOpen();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator storeBindingsLater()
	{
		yield return null;
		yield return null;
		storeCurrentBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		PlatformManager.NativePlatform.Input.LoadActionSetsFromStrings(actionBindingsOnOpen);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_OptionsControlsBase()
	{
	}
}
