using System;
using System.Globalization;
using System.Text;
using UnityEngine;

public class StringParsersTests
{
	[PublicizedFrom(EAccessModifier.Private)]
	public delegate T ParseFunc<T>(string _in);

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate bool TryParseFunc<T>(string _in, out T _out);

	[PublicizedFrom(EAccessModifier.Private)]
	public abstract class TestClassBase<TOut> where TOut : struct
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly string testName;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string[] inputValues;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool[] monoOk;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool[] customOk;

		[PublicizedFrom(EAccessModifier.Protected)]
		public TOut[] monoValue;

		[PublicizedFrom(EAccessModifier.Protected)]
		public TOut[] customValue;

		public TestClassBase(string _testName)
		{
			testName = _testName;
		}

		public abstract void RunTests(string[] _testValues, int _runCount);

		public StringBuilder GetResults()
		{
			int num = 0;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Test#;Input Value;Same;SameEx;ValM;ValC;OkM;OkC;");
			for (int i = 0; i < inputValues.Length; i++)
			{
				stringBuilder.AppendLine($"{i};\"{inputValues[i]}\";{monoValue[i].Equals(customValue[i])};{monoOk[i] == customOk[i]};{monoValue[i]};{customValue[i]};{monoOk[i]};{customOk[i]};");
				if (!monoValue[i].Equals(customValue[i]) || monoOk[i] != customOk[i])
				{
					num++;
				}
			}
			if (num > 0)
			{
				Log.Error(testName + " - failed: " + num);
			}
			return stringBuilder;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class TestClassParse<TOut> : TestClassBase<TOut> where TOut : struct
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly ParseFunc<TOut> monoFunc;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly ParseFunc<TOut> customFunc;

		public TestClassParse(string _testName, ParseFunc<TOut> _monoFunc, ParseFunc<TOut> _customFunc)
			: base(_testName)
		{
			monoFunc = _monoFunc;
			customFunc = _customFunc;
		}

		public override void RunTests(string[] _testValues, int _runCount)
		{
			inputValues = _testValues;
			monoOk = new bool[_testValues.Length];
			customOk = new bool[_testValues.Length];
			monoValue = new TOut[_testValues.Length];
			customValue = new TOut[_testValues.Length];
			for (int i = 0; i < _testValues.Length; i++)
			{
				monoOk[i] = true;
				customOk[i] = true;
			}
			foreach (string text in _testValues)
			{
				monoFunc(text);
				customFunc(text);
			}
			for (int k = 0; k < _runCount; k++)
			{
				int num = k % _testValues.Length;
				try
				{
					monoValue[num] = monoFunc(_testValues[num]);
				}
				catch (Exception)
				{
					monoOk[num] = false;
				}
			}
			for (int l = 0; l < _runCount; l++)
			{
				int num2 = l % _testValues.Length;
				try
				{
					customValue[num2] = customFunc(_testValues[num2]);
				}
				catch (Exception)
				{
					customOk[num2] = false;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class TestClassTryParse<TOut> : TestClassBase<TOut> where TOut : struct
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly TryParseFunc<TOut> monoFunc;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly TryParseFunc<TOut> customFunc;

		public TestClassTryParse(string _testName, TryParseFunc<TOut> _monoFunc, TryParseFunc<TOut> _customFunc)
			: base(_testName)
		{
			monoFunc = _monoFunc;
			customFunc = _customFunc;
		}

		public override void RunTests(string[] _testValues, int _runCount)
		{
			inputValues = _testValues;
			monoOk = new bool[_testValues.Length];
			customOk = new bool[_testValues.Length];
			monoValue = new TOut[_testValues.Length];
			customValue = new TOut[_testValues.Length];
			foreach (string text in _testValues)
			{
				monoFunc(text, out monoValue[0]);
				customFunc(text, out customValue[0]);
			}
			for (int j = 0; j < _runCount; j++)
			{
				int num = j % _testValues.Length;
				monoOk[num] = monoFunc(_testValues[num], out monoValue[num]);
			}
			for (int k = 0; k < _runCount; k++)
			{
				int num2 = k % _testValues.Length;
				customOk[num2] = customFunc(_testValues[num2], out customValue[num2]);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool P8S(string _in, out sbyte _out)
	{
		return StringParsers.TryParseSInt8(_in, out _out);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool P8Us(string _in, out byte _out)
	{
		return StringParsers.TryParseUInt8(_in, out _out);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool P16S(string _in, out short _out)
	{
		return StringParsers.TryParseSInt16(_in, out _out);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool P16Us(string _in, out ushort _out)
	{
		return StringParsers.TryParseUInt16(_in, out _out);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool P32S(string _in, out int _out)
	{
		return StringParsers.TryParseSInt32(_in, out _out);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool P32Us(string _in, out uint _out)
	{
		return StringParsers.TryParseUInt32(_in, out _out);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool P64S(string _in, out long _out)
	{
		return StringParsers.TryParseSInt64(_in, out _out);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool P64Us(string _in, out ulong _out)
	{
		return StringParsers.TryParseUInt64(_in, out _out);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool PB(string _in, out bool _out)
	{
		return StringParsers.TryParseBool(_in, out _out);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i PV3i(string _in)
	{
		return StringParsers.ParseVector3i(_in);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 PV2old(string _s)
	{
		string[] array = _s.Split(',');
		if (array.Length != 2)
		{
			return Vector2.zero;
		}
		return new Vector2(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 PV3old(string _s)
	{
		string[] array = _s.Split(',');
		if (array.Length != 3)
		{
			return Vector3.zero;
		}
		return new Vector3(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2d PV2dOld(string _s)
	{
		string[] array = _s.Split(',');
		if (array.Length != 2)
		{
			return Vector2d.Zero;
		}
		return new Vector2d(double.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), double.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3d PV3dOld(string _s)
	{
		string[] array = _s.Split(',');
		if (array.Length != 3)
		{
			return Vector3d.Zero;
		}
		return new Vector3d(double.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), double.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), double.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2i PV2iOld(string _s)
	{
		string[] array = _s.Split(',');
		if (array.Length != 2)
		{
			return Vector2i.zero;
		}
		return new Vector2i(int.Parse(array[0]), int.Parse(array[1]));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i PV3iOld(string _s)
	{
		string[] array = _s.Split(',');
		if (array.Length != 3)
		{
			return Vector3i.zero;
		}
		return new Vector3i(int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2]));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TEnum PEnumOld<TEnum>(string _s) where TEnum : struct, IConvertible
	{
		return (TEnum)Enum.Parse(typeof(TEnum), _s);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TEnum PEnumNew<TEnum>(string _s) where TEnum : struct, IConvertible
	{
		return EnumUtils.Parse<TEnum>(_s);
	}

	public static void RunTests()
	{
		int runCount = 10000;
		System.Random random = new System.Random();
		string[] array = new string[100];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ((random.Next() % 2 == 1) ? "-" : "") + (random.Next() & 0xFF);
		}
		array[0] = "-256";
		array[1] = "-257";
		array[2] = "255";
		array[3] = "256";
		array[4] = "-128";
		array[5] = "-129";
		array[6] = "127";
		array[7] = "128";
		array[8] = "-0";
		TestClassTryParse<sbyte> testClassTryParse = new TestClassTryParse<sbyte>("SInt8", sbyte.TryParse, P8S);
		testClassTryParse.RunTests(array, runCount);
		TestClassTryParse<byte> testClassTryParse2 = new TestClassTryParse<byte>("UInt8", byte.TryParse, P8Us);
		testClassTryParse2.RunTests(array, runCount);
		array = new string[100];
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = ((random.Next() % 2 == 1) ? "-" : "") + (random.Next() & 0xFFFF);
		}
		array[0] = "-65536";
		array[1] = "-65537";
		array[2] = "65535";
		array[3] = "65536";
		array[4] = "-32768";
		array[5] = "-32769";
		array[6] = "32767";
		array[7] = "32768";
		TestClassTryParse<short> testClassTryParse3 = new TestClassTryParse<short>("SInt16", short.TryParse, P16S);
		testClassTryParse3.RunTests(array, runCount);
		TestClassTryParse<ushort> testClassTryParse4 = new TestClassTryParse<ushort>("UInt16", ushort.TryParse, P16Us);
		testClassTryParse4.RunTests(array, runCount);
		array = new string[100];
		for (int k = 0; k < array.Length; k++)
		{
			array[k] = ((random.Next() % 2 == 1) ? "-" : "") + random.Next();
		}
		TestClassTryParse<int> testClassTryParse5 = new TestClassTryParse<int>("SInt32", int.TryParse, P32S);
		testClassTryParse5.RunTests(array, runCount);
		TestClassTryParse<uint> testClassTryParse6 = new TestClassTryParse<uint>("UInt32", uint.TryParse, P32Us);
		testClassTryParse6.RunTests(array, runCount);
		array = new string[100];
		for (int l = 0; l < array.Length; l++)
		{
			array[l] = ((random.Next() % 2 == 1) ? "-" : "") + random.Next() * random.Next();
		}
		array[0] = "-9223372036854775808";
		array[1] = "9223372036854775807";
		TestClassTryParse<long> testClassTryParse7 = new TestClassTryParse<long>("SInt64", long.TryParse, P64S);
		testClassTryParse7.RunTests(array, runCount);
		TestClassTryParse<ulong> testClassTryParse8 = new TestClassTryParse<ulong>("UInt64", ulong.TryParse, P64Us);
		testClassTryParse8.RunTests(array, runCount);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("SInt8");
		stringBuilder.Append(testClassTryParse.GetResults());
		stringBuilder.AppendLine("UInt8");
		stringBuilder.Append(testClassTryParse2.GetResults());
		stringBuilder.AppendLine("SInt16");
		stringBuilder.Append(testClassTryParse3.GetResults());
		stringBuilder.AppendLine("UInt16");
		stringBuilder.Append(testClassTryParse4.GetResults());
		stringBuilder.AppendLine("SInt32");
		stringBuilder.Append(testClassTryParse5.GetResults());
		stringBuilder.AppendLine("UInt32");
		stringBuilder.Append(testClassTryParse6.GetResults());
		stringBuilder.AppendLine("SInt64");
		stringBuilder.Append(testClassTryParse7.GetResults());
		stringBuilder.AppendLine("UInt64");
		stringBuilder.Append(testClassTryParse8.GetResults());
		SdFile.WriteAllText("E:\\parsing_int.txt", stringBuilder.ToString());
		array = new string[2] { "true", "false" };
		TestClassTryParse<bool> testClassTryParse9 = new TestClassTryParse<bool>("Bool NoWS", bool.TryParse, PB);
		testClassTryParse9.RunTests(array, runCount);
		array = new string[2] { " true  ", "   false " };
		TestClassTryParse<bool> testClassTryParse10 = new TestClassTryParse<bool>("Bool WS", bool.TryParse, PB);
		testClassTryParse10.RunTests(array, runCount);
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.AppendLine("Bool NoWs");
		stringBuilder2.Append(testClassTryParse9.GetResults());
		stringBuilder2.AppendLine("Bool Ws");
		stringBuilder2.Append(testClassTryParse10.GetResults());
		SdFile.WriteAllText("E:\\parsing_bool.txt", stringBuilder2.ToString());
		array = new string[100];
		for (int m = 0; m < array.Length; m++)
		{
			string text = ((random.Next() % 2 == 1) ? "-" : "") + (random.NextDouble() * (double)random.Next() * (double)random.Next()).ToCultureInvariantString();
			string text2 = ((random.Next() % 2 == 1) ? "-" : "") + (random.NextDouble() * (double)random.Next() * (double)random.Next()).ToCultureInvariantString();
			array[m] = text + "," + text2;
		}
		TestClassParse<Vector2d> testClassParse = new TestClassParse<Vector2d>("Vector2d", PV2dOld, StringParsers.ParseVector2d);
		testClassParse.RunTests(array, runCount);
		array = new string[100];
		for (int n = 0; n < array.Length; n++)
		{
			string text3 = ((random.Next() % 2 == 1) ? "-" : "") + (random.NextDouble() * (double)random.Next() * (double)random.Next()).ToCultureInvariantString();
			string text4 = ((random.Next() % 2 == 1) ? "-" : "") + (random.NextDouble() * (double)random.Next() * (double)random.Next()).ToCultureInvariantString();
			string text5 = ((random.Next() % 2 == 1) ? "-" : "") + (random.NextDouble() * (double)random.Next() * (double)random.Next()).ToCultureInvariantString();
			array[n] = text3 + "," + text4 + "," + text5;
		}
		TestClassParse<Vector3d> testClassParse2 = new TestClassParse<Vector3d>("Vector3d", PV3dOld, StringParsers.ParseVector3d);
		testClassParse2.RunTests(array, runCount);
		StringBuilder stringBuilder3 = new StringBuilder();
		stringBuilder3.AppendLine("Vector2d");
		stringBuilder3.Append(testClassParse.GetResults());
		stringBuilder3.AppendLine("Vector3d");
		stringBuilder3.Append(testClassParse2.GetResults());
		SdFile.WriteAllText("E:\\parsing_vectord.txt", stringBuilder3.ToString());
		array = new string[100];
		int count = EnumUtils.Names<GameInfoInt>().Count;
		for (int num = 0; num < array.Length; num++)
		{
			int index = random.Next() % count;
			array[num] = EnumUtils.Names<GameInfoInt>()[index];
		}
		TestClassParse<GameInfoInt> testClassParse3 = new TestClassParse<GameInfoInt>("Enum NoWs", PEnumOld<GameInfoInt>, PEnumNew<GameInfoInt>);
		testClassParse3.RunTests(array, runCount);
		for (int num2 = 0; num2 < array.Length; num2++)
		{
			array[num2] = " " + array[num2] + "  ";
		}
		TestClassParse<GameInfoInt> testClassParse4 = new TestClassParse<GameInfoInt>("Enum Ws", PEnumOld<GameInfoInt>, PEnumNew<GameInfoInt>);
		testClassParse4.RunTests(array, runCount);
		StringBuilder stringBuilder4 = new StringBuilder();
		stringBuilder4.AppendLine("Enums NoWs");
		stringBuilder4.Append(testClassParse3.GetResults());
		stringBuilder4.AppendLine("Enums Ws");
		stringBuilder4.Append(testClassParse4.GetResults());
		SdFile.WriteAllText("E:\\parsing_enums.txt", stringBuilder4.ToString());
	}
}
