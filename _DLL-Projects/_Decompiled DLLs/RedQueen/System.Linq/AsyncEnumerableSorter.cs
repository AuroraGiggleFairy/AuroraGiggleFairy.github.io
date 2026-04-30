using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.Linq;

[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(0)]
[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
internal abstract class AsyncEnumerableSorter<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TElement>
{
	internal abstract ValueTask ComputeKeys(TElement[] elements, int count);

	internal abstract int CompareAnyKeys(int index1, int index2);

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1 })]
	public async ValueTask<int[]> Sort(TElement[] elements, int count)
	{
		int[] array = await ComputeMap(elements, count).ConfigureAwait(continueOnCapturedContext: false);
		QuickSort(array, 0, count - 1);
		return array;
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1 })]
	public async ValueTask<int[]> Sort(TElement[] elements, int count, int minIndexInclusive, int maxIndexInclusive)
	{
		int[] array = await ComputeMap(elements, count).ConfigureAwait(continueOnCapturedContext: false);
		PartialQuickSort(array, 0, count - 1, minIndexInclusive, maxIndexInclusive);
		return array;
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1 })]
	public async ValueTask<TElement> ElementAt(TElement[] elements, int count, int index)
	{
		int[] map = await ComputeMap(elements, count).ConfigureAwait(continueOnCapturedContext: false);
		return (index == 0) ? elements[Min(map, count)] : elements[QuickSelect(map, count - 1, index)];
	}

	[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1 })]
	private async ValueTask<int[]> ComputeMap(TElement[] elements, int count)
	{
		await ComputeKeys(elements, count).ConfigureAwait(continueOnCapturedContext: false);
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = i;
		}
		return array;
	}

	protected abstract void QuickSort(int[] map, int left, int right);

	protected abstract void PartialQuickSort(int[] map, int left, int right, int minIndexInclusive, int maxIndexInclusive);

	protected abstract int QuickSelect(int[] map, int right, int idx);

	protected abstract int Min(int[] map, int count);
}
