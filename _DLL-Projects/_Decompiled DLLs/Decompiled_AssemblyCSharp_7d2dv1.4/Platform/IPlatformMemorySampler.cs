using System.Collections.Generic;
using System.Text;

namespace Platform;

public interface IPlatformMemorySampler
{
	IReadOnlyList<IPlatformMemoryStat> Statistics { get; }

	bool HasImportantChange();

	void Sample();

	void UpdateLast()
	{
		foreach (IPlatformMemoryStat statistic in Statistics)
		{
			statistic.UpdateLast();
		}
	}

	int RenderToTable(IList<StringBuilder> tableLines, bool includeDeltas)
	{
		if (tableLines == null)
		{
			tableLines = new List<StringBuilder>();
		}
		IReadOnlyList<IPlatformMemoryStat> statistics = Statistics;
		int num = (includeDeltas ? 1 : 0);
		int num2 = 2 + statistics.Count;
		int num3 = num2 + statistics.Count;
		for (int i = 0; i < num3; i++)
		{
			if (i < tableLines.Count)
			{
				tableLines[i].Clear();
			}
			else
			{
				tableLines.Add(new StringBuilder());
			}
		}
		int num4 = 0;
		StringBuilder stringBuilder = tableLines[0];
		StringBuilder stringBuilder2 = tableLines[1];
		for (int j = 0; j < statistics.Count; j++)
		{
			StringBuilder stringBuilder3 = tableLines[2 + j];
			IPlatformMemoryStat platformMemoryStat = statistics[j];
			stringBuilder3.Append(' ').Append(platformMemoryStat.Name);
			if (stringBuilder3.Length + 1 > num4)
			{
				num4 = stringBuilder3.Length + 1;
			}
		}
		while (stringBuilder2.Length < num4)
		{
			stringBuilder2.Append('-');
		}
		for (int k = 0; k <= num; k++)
		{
			bool flag = k > 0;
			foreach (MemoryStatColumn item in EnumUtils.Values<MemoryStatColumn>())
			{
				string text = item.ToString();
				int num5 = (flag ? 1 : 0) + text.Length;
				bool flag2 = false;
				for (int l = 0; l < statistics.Count; l++)
				{
					StringBuilder stringBuilder4 = tableLines[num2 + l];
					stringBuilder4.Clear();
					statistics[l].RenderColumn(stringBuilder4, item, flag);
					if (stringBuilder4.Length > 0)
					{
						flag2 = true;
					}
					if (stringBuilder4.Length > num5)
					{
						num5 = stringBuilder4.Length;
					}
				}
				if (!flag2)
				{
					continue;
				}
				for (int m = 0; m < num2; m++)
				{
					StringBuilder stringBuilder5 = tableLines[m];
					while (stringBuilder5.Length < num4)
					{
						stringBuilder5.Append(' ');
					}
				}
				for (int n = 0; n < num2; n++)
				{
					StringBuilder stringBuilder6 = tableLines[n];
					if (n == 1)
					{
						stringBuilder6.Append('+');
					}
					else
					{
						stringBuilder6.Append('|');
					}
				}
				for (int num6 = 0; num6 < statistics.Count; num6++)
				{
					StringBuilder stringBuilder7 = tableLines[num2 + num6];
					if (stringBuilder7.Length > 0)
					{
						StringBuilder stringBuilder8 = tableLines[2 + num6];
						stringBuilder8.Append(' ');
						int num7 = num5 - stringBuilder7.Length;
						for (int num8 = 0; num8 < num7; num8++)
						{
							stringBuilder8.Append(' ');
						}
						stringBuilder8.Append(stringBuilder7);
						if (stringBuilder8.Length + 1 > num4)
						{
							num4 = stringBuilder8.Length + 1;
						}
					}
				}
				stringBuilder.Append(' ');
				if (flag)
				{
					stringBuilder.Append("d");
				}
				stringBuilder.Append(text);
				if (stringBuilder.Length + 1 > num4)
				{
					num4 = stringBuilder.Length + 1;
				}
				while (stringBuilder2.Length < num4)
				{
					stringBuilder2.Append('-');
				}
			}
		}
		return num2;
	}
}
