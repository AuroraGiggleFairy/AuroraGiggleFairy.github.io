namespace SandboxOptions;

public class SandboxOptionValueSetInt : SandboxOptionValueSet
{
	public int[] IntValues;

	public override void Init()
	{
		string[] displayValues = DisplayValues;
		DisplayValues = new string[IntValues.Length];
		string format = ((DisplayFormat != null) ? Localization.Get(DisplayFormat) : "{0}");
		for (int i = 0; i < IntValues.Length; i++)
		{
			string text = "";
			if (AlternateDisplayValues != null && AlternateDisplayValues.Length > i)
			{
				text = AlternateDisplayValues[i];
			}
			if (displayValues != null && displayValues.Length > i)
			{
				DisplayValues[i] = string.Format(Localization.Get(displayValues[i]), (text != "") ? text : ((object)IntValues[i]));
			}
			else
			{
				DisplayValues[i] = string.Format(format, (text != "") ? text : ((object)IntValues[i]));
			}
		}
	}

	public override bool GetIntValue(int index, out int val)
	{
		if (index >= 0 && index < IntValues.Length)
		{
			val = IntValues[index];
			return true;
		}
		val = 0;
		return false;
	}

	public override int GetIntIndex(int val)
	{
		for (int i = 0; i < IntValues.Length; i++)
		{
			if (val == IntValues[i])
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
			return index < IntValues.Length;
		}
		return false;
	}

	public override string GetValueListString()
	{
		return string.Join(", ", IntValues);
	}
}
