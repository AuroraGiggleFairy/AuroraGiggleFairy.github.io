namespace SandboxOptions;

public class SandboxOptionValueSetBool : SandboxOptionValueSet
{
	public bool[] BoolValues;

	public override void Init()
	{
		for (int i = 0; i < DisplayValues.Length; i++)
		{
			DisplayValues[i] = Localization.Get(DisplayValues[i]);
		}
	}

	public override bool GetBoolValue(int index, out bool val)
	{
		if (index >= 0 && index < BoolValues.Length)
		{
			val = BoolValues[index];
			return true;
		}
		val = false;
		return false;
	}

	public override bool GetBoolValue(int index)
	{
		return BoolValues[index];
	}

	public override int GetBoolIndex(bool val)
	{
		for (int i = 0; i < BoolValues.Length; i++)
		{
			if (val == BoolValues[i])
			{
				return i;
			}
		}
		return -1;
	}

	public override bool IsValidIndex(int index)
	{
		if (index >= 0)
		{
			return index < BoolValues.Length;
		}
		return false;
	}

	public override string GetValueListString()
	{
		return string.Join(", ", BoolValues);
	}
}
