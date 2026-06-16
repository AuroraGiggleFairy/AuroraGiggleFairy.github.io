using Audio;

public class PowerPressurePlate : PowerTrigger
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool pressed;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool lastPressed;

	public override PowerItemTypes PowerItemType => PowerItemTypes.PressurePlate;

	public bool Pressed
	{
		get
		{
			return pressed;
		}
		set
		{
			pressed = value;
			if (pressed && !lastPressed)
			{
				Manager.BroadcastPlay(Position.ToVector3(), "pressureplate_down");
			}
			lastPressed = pressed;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CheckForActiveChange()
	{
		base.CheckForActiveChange();
		if (!pressed && lastPressed)
		{
			Manager.BroadcastPlay(Position.ToVector3(), "pressureplate_up");
			if (powerTime == 0f)
			{
				isActive = false;
				HandleDisconnectChildren();
				SendHasLocalChangesToRoot();
				powerTime = -1f;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleSoundDisable()
	{
		base.HandleSoundDisable();
		lastPressed = pressed;
		pressed = false;
	}
}
