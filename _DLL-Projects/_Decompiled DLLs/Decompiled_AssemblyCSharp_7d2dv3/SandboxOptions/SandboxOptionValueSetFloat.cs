namespace SandboxOptions;

public class SandboxOptionValueSetFloat : SandboxOptionValueSet
{
	public float[] FloatValues;

	public override void Init()
	{
		string[] displayValues = DisplayValues;
		DisplayValues = new string[FloatValues.Length];
		string format = ((DisplayFormat != null) ? Localization.Get(DisplayFormat) : "{0}");
		for (int i = 0; i < FloatValues.Length; i++)
		{
			string text = "";
			if (AlternateDisplayValues != null && AlternateDisplayValues.Length > i)
			{
				text = AlternateDisplayValues[i];
			}
			if (displayValues != null && displayValues.Length > i)
			{
				DisplayValues[i] = string.Format(Localization.Get(displayValues[i]), (text != "") ? text : ((object)(FloatValues[i] * 100f)));
			}
			else
			{
				DisplayValues[i] = string.Format(format, (text != "") ? text : ((object)(FloatValues[i] * 100f)));
			}
		}
	}

	public override bool GetFloatValue(int index, out float val)
	{
		if (index >= 0 && index < FloatValues.Length)
		{
			val = FloatValues[index];
			return true;
		}
		val = 0f;
		return false;
	}

	public override int GetFloatIndex(float val)
	{
		for (int i = 0; i < FloatValues.Length; i++)
		{
			if (val == FloatValues[i])
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
			return index < FloatValues.Length;
		}
		return false;
	}

	public override string GetValueListString()
	{
		return string.Join(", ", FloatValues);
	}
}
