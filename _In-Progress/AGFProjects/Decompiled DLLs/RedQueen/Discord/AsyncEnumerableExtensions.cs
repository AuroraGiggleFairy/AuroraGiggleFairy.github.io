using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Discord;

internal static class AsyncEnumerableExtensions
{
	public static async Task<IEnumerable<T>> FlattenAsync<T>(this IAsyncEnumerable<IEnumerable<T>> source)
	{
		return await source.Flatten().ToArrayAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	public static IAsyncEnumerable<T> Flatten<T>(this IAsyncEnumerable<IEnumerable<T>> source)
	{
		return source.SelectMany([return: _003C9e0c2a9e_002Dcd98_002D48b2_002Dac89_002Dcfbfe504abd4_003ENullable(new byte[] { 1, 0 })] (IEnumerable<T> enumerable) => enumerable.ToAsyncEnumerable());
	}
}
