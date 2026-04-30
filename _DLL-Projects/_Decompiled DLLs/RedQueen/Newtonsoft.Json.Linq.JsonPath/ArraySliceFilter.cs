using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath;

internal class ArraySliceFilter : PathFilter
{
	public int? Start { get; set; }

	public int? End { get; set; }

	public int? Step { get; set; }

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonSelectSettings settings)
	{
		if (Step == 0)
		{
			throw new JsonException("Step cannot be zero.");
		}
		foreach (JToken item in current)
		{
			if (item is JArray a)
			{
				int stepCount = Step ?? 1;
				int num = Start ?? ((stepCount <= 0) ? (a.Count - 1) : 0);
				int stopIndex = End ?? ((stepCount > 0) ? a.Count : (-1));
				if (Start < 0)
				{
					num = a.Count + num;
				}
				if (End < 0)
				{
					stopIndex = a.Count + stopIndex;
				}
				num = Math.Max(num, (stepCount <= 0) ? int.MinValue : 0);
				num = Math.Min(num, (stepCount > 0) ? a.Count : (a.Count - 1));
				stopIndex = Math.Max(stopIndex, -1);
				stopIndex = Math.Min(stopIndex, a.Count);
				bool positiveStep = stepCount > 0;
				if (IsValid(num, stopIndex, positiveStep))
				{
					for (int i = num; IsValid(i, stopIndex, positiveStep); i += stepCount)
					{
						yield return a[i];
					}
				}
				else if (settings?.ErrorWhenNoMatch ?? false)
				{
					throw new JsonException("Array slice of {0} to {1} returned no results.".FormatWith(CultureInfo.InvariantCulture, Start.HasValue ? Start.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : "*", End.HasValue ? End.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : "*"));
				}
			}
			else if (settings?.ErrorWhenNoMatch ?? false)
			{
				throw new JsonException("Array slice is not valid on {0}.".FormatWith(CultureInfo.InvariantCulture, item.GetType().Name));
			}
		}
	}

	private bool IsValid(int index, int stopIndex, bool positiveStep)
	{
		if (positiveStep)
		{
			return index < stopIndex;
		}
		return index > stopIndex;
	}
}
