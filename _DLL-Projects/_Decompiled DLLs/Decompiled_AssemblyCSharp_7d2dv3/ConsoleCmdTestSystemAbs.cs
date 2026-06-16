using System;

public abstract class ConsoleCmdTestSystemAbs : ConsoleCmdAbstract
{
	public const string SuccessString = "<color=green>Success</color>";

	public const string FailureString = "<color=red>Failure</color>";

	public static void AssertAreApproximatelyEqual(long expected, long actual, long tolerance, string message)
	{
		Math.Abs(expected - actual);
	}

	public static void AssertArraysAreEqual<T>(T[] expected, T[] actual, string message)
	{
		for (int i = 0; i < expected.Length; i++)
		{
		}
	}

	public static bool AssertException(Action action)
	{
		try
		{
			action();
			return false;
		}
		catch (Exception arg)
		{
			Log.Out($"Expected exception:\n{arg}");
			return true;
		}
	}

	public static bool ExecutesWithoutExceptions(params Action[] testCases)
	{
		bool flag = true;
		foreach (Action testCase in testCases)
		{
			flag &= ExecutesWithoutException(testCase);
		}
		return flag;
	}

	public static bool ExecutesWithoutException(Action testCase)
	{
		bool flag = true;
		try
		{
			Log.Out(testCase.Method.Name ?? "");
			testCase();
		}
		catch (Exception e)
		{
			Log.Exception(e);
			flag = false;
		}
		finally
		{
			Log.Out(testCase.Method.Name + " result: " + (flag ? "<color=green>Success</color>" : "<color=red>Failure</color>") + ".");
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ConsoleCmdTestSystemAbs()
	{
	}
}
