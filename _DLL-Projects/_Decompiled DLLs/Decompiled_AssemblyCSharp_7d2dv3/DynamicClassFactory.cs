using System;

public abstract class DynamicClassFactory
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract object[] getTable();

	public object Instantiate(string _className)
	{
		Type type = Type.GetType(_className);
		if (type != null)
		{
			return Activator.CreateInstance(type);
		}
		object[] table = getTable();
		for (int i = 0; i < table.Length / 2; i++)
		{
			if (_className.Equals(Utils.UnCryptFromBase64((char[])table[i * 2 + 1])))
			{
				try
				{
					return Activator.CreateInstance((Type)table[i * 2]);
				}
				catch (Exception ex)
				{
					throw new Exception("Class '" + Utils.UnCryptFromBase64((char[])table[i * 2 + 1]) + "' not found! Msg: " + ex.Message);
				}
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public DynamicClassFactory()
	{
	}
}
