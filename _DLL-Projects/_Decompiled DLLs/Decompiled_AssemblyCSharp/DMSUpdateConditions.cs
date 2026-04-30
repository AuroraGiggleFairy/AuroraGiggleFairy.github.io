public struct DMSUpdateConditions
{
	[PublicizedFrom(EAccessModifier.Private)]
	public byte BoolHolder;

	public bool DoesPlayerExist
	{
		set
		{
			SetBoolHolder(value, 128);
		}
	}

	public bool IsGameUnPaused
	{
		set
		{
			SetBoolHolder(value, 64);
		}
	}

	public bool IsDMSInitialized
	{
		set
		{
			SetBoolHolder(value, 32);
		}
	}

	public bool IsDMSEnabled
	{
		set
		{
			SetBoolHolder(value, 16);
		}
	}

	public bool CanUpdate => BoolHolder == 240;

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetBoolHolder(bool _value, byte _place)
	{
		if (_value)
		{
			BoolHolder |= _place;
		}
		else
		{
			BoolHolder &= (byte)(~_place);
		}
	}
}
