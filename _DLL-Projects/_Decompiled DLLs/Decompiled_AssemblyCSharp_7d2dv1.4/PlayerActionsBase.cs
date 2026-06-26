using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using InControl;

public abstract class PlayerActionsBase : PlayerActionSet
{
	public List<PlayerAction> ControllerRebindableActions = new List<PlayerAction>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Name
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public PlayerActionsBase()
	{
		typeof(PlayerActionSet).GetField("actionsByName", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(this, new CaseInsensitiveStringDictionary<PlayerAction>());
		base.ListenOptions = new BindingListenOptions
		{
			UnsetDuplicateBindingsOnSet = false,
			MaxAllowedBindings = 0u,
			MaxAllowedBindingsPerType = 1u,
			AllowDuplicateBindingsPerSet = true,
			IncludeKeys = false,
			IncludeMouseButtons = false,
			IncludeControllers = false,
			IncludeModifiersAsFirstClassKeys = true
		};
		base.ListenOptions.OnBindingFound = [PublicizedFrom(EAccessModifier.Internal)] (PlayerAction _action, BindingSource _binding) =>
		{
			if (!_action.HasBinding(_binding))
			{
				return true;
			}
			Log.Out("Binding already bound.");
			_action.StopListeningForBinding();
			return false;
		};
		BindingListenOptions bindingListenOptions = base.ListenOptions;
		bindingListenOptions.OnBindingAdded = (Action<PlayerAction, BindingSource>)Delegate.Combine(bindingListenOptions.OnBindingAdded, (Action<PlayerAction, BindingSource>)([PublicizedFrom(EAccessModifier.Internal)] (PlayerAction _action, BindingSource _binding) =>
		{
			Log.Out("Binding added for action {0} on device {1}: {2}", _action.Name, _binding.DeviceName, _binding.Name);
		}));
		BindingListenOptions bindingListenOptions2 = base.ListenOptions;
		bindingListenOptions2.OnBindingRejected = (Action<PlayerAction, BindingSource, BindingSourceRejectionType>)Delegate.Combine(bindingListenOptions2.OnBindingRejected, (Action<PlayerAction, BindingSource, BindingSourceRejectionType>)([PublicizedFrom(EAccessModifier.Internal)] (PlayerAction _action, BindingSource _binding, BindingSourceRejectionType _reason) =>
		{
			Log.Out("Binding rejected for action {0}: {1}", _action.Name, _reason.ToStringCached());
		}));
		InitActionSet();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitActionSet()
	{
		CreateActions();
		CreateDefaultKeyboardBindings();
		CreateDefaultJoystickBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void CreateActions();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void CreateDefaultKeyboardBindings();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void CreateDefaultJoystickBindings();

	public void ResetControllerBindings()
	{
		AsyncResetControllerBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public async void AsyncResetControllerBindings()
	{
		foreach (PlayerAction action in base.Actions)
		{
			if (!(action.UserData is PlayerActionData.ActionUserData { defaultOnStartup: not false }))
			{
				continue;
			}
			foreach (BindingSource binding in action.Bindings)
			{
				if (binding.BindingSourceType == BindingSourceType.DeviceBindingSource)
				{
					action.RemoveBinding(binding);
				}
			}
		}
		await Task.Yield();
		CreateDefaultJoystickBindings();
	}
}
