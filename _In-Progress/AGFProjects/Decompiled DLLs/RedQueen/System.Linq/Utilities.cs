using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

internal static class Utilities
{
	[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
	public static async ValueTask AddRangeAsync<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] T>(this List<T> list, IAsyncEnumerable<T> collection, CancellationToken cancellationToken)
	{
		if (collection is IEnumerable<T> collection2)
		{
			list.AddRange(collection2);
			return;
		}
		if (collection is IAsyncIListProvider<T> asyncIListProvider)
		{
			int num = await asyncIListProvider.GetCountAsync(onlyIfCheap: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (num == 0)
			{
				return;
			}
			if (num > 0)
			{
				int num2 = list.Count + num;
				if (list.Capacity < num2)
				{
					list.Capacity = num2;
				}
			}
		}
		await foreach (T item in collection.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			list.Add(item);
		}
	}
}
