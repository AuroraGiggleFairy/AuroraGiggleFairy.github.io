using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;

namespace WorldGenerationEngineFinal;

[BurstCompile(CompileSynchronously = true)]
public class RawStamp
{
	public struct Data
	{
		public float heightConst;

		public NativeArray<float> heightPixels;

		public float alphaConst;

		public NativeArray<float> alphaPixels;

		public NativeArray<float> waterPixels;

		public int width;

		public int height;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public delegate void SmoothAlpha_0000A2E3_0024PostfixBurstDelegate(ref Data d, int _boxSize);

	[PublicizedFrom(EAccessModifier.Internal)]
	public static class SmoothAlpha_0000A2E3_0024BurstDirectCall
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr Pointer;

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr DeferredCompilation;

		[BurstDiscard]
		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(SmoothAlpha_0000A2E3_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		static SmoothAlpha_0000A2E3_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(ref Data d, int _boxSize)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Data, int, void>)functionPointer)(ref d, _boxSize);
					return;
				}
			}
			SmoothAlpha_0024BurstManaged(ref d, _boxSize);
		}
	}

	public string name;

	public Data data;

	public int width => data.width;

	public int height => data.height;

	[BurstCompile(CompileSynchronously = true)]
	public static void SmoothAlpha(ref Data d, int _boxSize)
	{
		SmoothAlpha_0000A2E3_0024BurstDirectCall.Invoke(ref d, _boxSize);
	}

	public void BoxAlpha()
	{
		for (int i = 0; i < height; i += 4)
		{
			for (int j = 0; j < width; j += 4)
			{
				int num = j + i * width;
				double num2 = 0.0;
				for (int k = 0; k < 4; k++)
				{
					for (int l = 0; l < 4; l++)
					{
						num2 += (double)data.alphaPixels[num + l + k * width];
					}
				}
				num2 /= 16.0;
				for (int m = 0; m < 4; m++)
				{
					for (int n = 0; n < 4; n++)
					{
						data.alphaPixels[num + n + m * width] = (float)num2;
					}
				}
			}
		}
	}

	public void Clear()
	{
		data.heightPixels.Dispose();
		data.alphaPixels.Dispose();
		data.waterPixels.Dispose();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void SmoothAlpha_0024BurstManaged(ref Data d, int _boxSize)
	{
		NativeArray<float> alphaPixels = new NativeArray<float>(d.alphaPixels.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		for (int i = 0; i < d.height; i++)
		{
			for (int j = 0; j < d.width; j++)
			{
				float num = 0f;
				int num2 = 0;
				for (int k = -1; k < _boxSize; k++)
				{
					int num3 = i + k;
					if ((uint)num3 >= (uint)d.height)
					{
						continue;
					}
					for (int l = -1; l < _boxSize; l++)
					{
						int num4 = j + l;
						if ((uint)num4 < (uint)d.width)
						{
							num += d.alphaPixels[num4 + num3 * d.width];
							num2++;
						}
					}
				}
				num /= (float)num2;
				alphaPixels[j + i * d.width] = num;
			}
		}
		d.alphaPixels.Dispose();
		d.alphaPixels = alphaPixels;
	}
}
