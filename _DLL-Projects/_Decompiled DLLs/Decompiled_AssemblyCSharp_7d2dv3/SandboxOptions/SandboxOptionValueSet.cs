namespace SandboxOptions;

public class SandboxOptionValueSet
{
	public class DisplayOverride
	{
		public int Index;

		public string DisplayText;
	}

	public string[] DisplayValues;

	public string[] AlternateDisplayValues;

	public string DisplayFormat;

	public virtual void Init()
	{
	}

	public virtual bool GetBoolValue(int index, out bool val)
	{
		val = false;
		return false;
	}

	public virtual bool GetIntValue(int index, out int val)
	{
		val = 0;
		return false;
	}

	public virtual bool GetFloatValue(int index, out float val)
	{
		val = 0f;
		return false;
	}

	public virtual bool GetBoolValue(int index)
	{
		return false;
	}

	public virtual int GetBoolIndex(bool val)
	{
		return 0;
	}

	public virtual int GetFloatIndex(float val)
	{
		return 0;
	}

	public virtual int GetIntIndex(int val)
	{
		return 0;
	}

	public virtual bool IsValidIndex(int index)
	{
		return false;
	}

	public string GetDisplayAtIndex(int index)
	{
		if (index >= 0 && index < DisplayValues.Length)
		{
			return DisplayValues[index];
		}
		return "";
	}

	public virtual string GetValueListString()
	{
		return "";
	}
}
