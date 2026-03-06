using System.Runtime.CompilerServices;

[CompilerGenerated]
internal sealed class _003Ce143f2ba_002D1d62_002D4d52_002D98af_002Dccacd045c5ae_003E_003CPrivateImplementationDetails_003E
{
	internal static uint ComputeStringHash(string s)
	{
		uint num = default(uint);
		if (s != null)
		{
			num = 2166136261u;
			for (int i = 0; i < s.Length; i++)
			{
				num = (s[i] ^ num) * 16777619;
			}
		}
		return num;
	}
}
