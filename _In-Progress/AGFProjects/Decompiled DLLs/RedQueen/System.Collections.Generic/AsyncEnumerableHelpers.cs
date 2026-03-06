using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Generic;

[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(0)]
internal static class AsyncEnumerableHelpers
{
	[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(0)]
	internal struct ArrayWithLength<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] T>
	{
		[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(1)]
		public T[] Array;

		public int Length;
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1, 1 })]
	internal static async ValueTask<T[]> ToArray<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] T>(IAsyncEnumerable<T> source, CancellationToken cancellationToken)
	{
		ArrayWithLength<T> arrayWithLength = await ToArrayWithLength(source, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		Array.Resize(ref arrayWithLength.Array, arrayWithLength.Length);
		return arrayWithLength.Array;
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 0, 1 })]
	internal static async ValueTask<ArrayWithLength<T>> ToArrayWithLength<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] T>(IAsyncEnumerable<T> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		ArrayWithLength<T> result = default(ArrayWithLength<T>);
		if (source is ICollection<T> { Count: var count } collection)
		{
			if (count != 0)
			{
				result.Array = new T[count];
				collection.CopyTo(result.Array, 0);
				result.Length = count;
				return result;
			}
		}
		else
		{
			{
				ConfiguredCancelableAsyncEnumerable<T>.Enumerator en = AsyncEnumerableExt.GetConfiguredAsyncEnumerator(source, cancellationToken, continueOnCapturedContext: false);
				try
				{
					if (await en.MoveNextAsync())
					{
						T[] arr = new T[4]
						{
							en.Current,
							default(T),
							default(T),
							default(T)
						};
						int count2 = 1;
						while (await en.MoveNextAsync())
						{
							if (count2 == arr.Length)
							{
								int num = count2 << 1;
								if ((uint)num > 2146435071u)
								{
									num = ((2146435071 <= count2) ? (count2 + 1) : 2146435071);
								}
								Array.Resize(ref arr, num);
							}
							arr[count2++] = en.Current;
						}
						result.Length = count2;
						result.Array = arr;
						return result;
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable = en as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
			}
		}
		result.Length = 0;
		result.Array = Array.Empty<T>();
		return result;
	}

	internal static async Task<System.Linq.Set<T>> ToSet<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] T>(IAsyncEnumerable<T> source, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<T> comparer, CancellationToken cancellationToken)
	{
		System.Linq.Set<T> set = new System.Linq.Set<T>(comparer);
		await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			set.Add(item);
		}
		return set;
	}
}
