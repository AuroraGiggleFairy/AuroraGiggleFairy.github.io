using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Linq;

[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(0)]
[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
internal sealed class SingleLinkedNode<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TSource>
{
	public TSource Item { get; }

	[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })]
	public System.Linq.SingleLinkedNode<TSource> Linked
	{
		[return: _003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })]
		get;
	}

	public SingleLinkedNode(TSource item)
	{
		Item = item;
	}

	private SingleLinkedNode(System.Linq.SingleLinkedNode<TSource> linked, TSource item)
	{
		Linked = linked;
		Item = item;
	}

	public System.Linq.SingleLinkedNode<TSource> Add(TSource item)
	{
		return new System.Linq.SingleLinkedNode<TSource>(this, item);
	}

	public int GetCount()
	{
		int num = 0;
		for (System.Linq.SingleLinkedNode<TSource> singleLinkedNode = this; singleLinkedNode != null; singleLinkedNode = singleLinkedNode.Linked)
		{
			num++;
		}
		return num;
	}

	public IEnumerator<TSource> GetEnumerator(int count)
	{
		return ((IEnumerable<TSource>)ToArray(count)).GetEnumerator();
	}

	public System.Linq.SingleLinkedNode<TSource> GetNode(int index)
	{
		System.Linq.SingleLinkedNode<TSource> singleLinkedNode = this;
		while (index > 0)
		{
			singleLinkedNode = singleLinkedNode.Linked;
			index--;
		}
		return singleLinkedNode;
	}

	private TSource[] ToArray(int count)
	{
		TSource[] array = new TSource[count];
		int num = count;
		for (System.Linq.SingleLinkedNode<TSource> singleLinkedNode = this; singleLinkedNode != null; singleLinkedNode = singleLinkedNode.Linked)
		{
			num--;
			array[num] = singleLinkedNode.Item;
		}
		return array;
	}
}
