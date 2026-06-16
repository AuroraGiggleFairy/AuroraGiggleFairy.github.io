using System;
using System.Collections.Generic;
using System.Text;

public class ObjectDuplicateChecker<T> where T : class, IEquatable<T>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class ObjectInfo
	{
		public T Value;

		public int DupeCount;

		public int SharedCount;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ObjectInfo> Objects = new List<ObjectInfo>();

	public void AddValue(T value)
	{
		for (int i = 0; i < Objects.Count; i++)
		{
			if (Objects[i].Value == value)
			{
				if (Objects[i].Value == value)
				{
					Objects[i].SharedCount++;
				}
				else
				{
					Objects[i].DupeCount++;
				}
				return;
			}
		}
		Objects.Add(new ObjectInfo
		{
			Value = value,
			DupeCount = 0,
			SharedCount = 1
		});
	}

	public string GenerateReport()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ObjectInfo @object in Objects)
		{
			int sizeAuto = MemoryTracker.GetSizeAuto(@object.Value);
			int num4 = sizeAuto + sizeAuto * @object.DupeCount;
			num += num4;
			num2 += sizeAuto;
			num3 += sizeAuto * @object.SharedCount;
		}
		stringBuilder.AppendLine($"    Size(in memory): {(double)num / 1024.0:F2}kb, Size(est unduped): {(double)num2 / 1024.0:F2}kb, Size(saved by sharing): {(double)num3 / 1024.0:F2}kb");
		return stringBuilder.ToString();
	}
}
