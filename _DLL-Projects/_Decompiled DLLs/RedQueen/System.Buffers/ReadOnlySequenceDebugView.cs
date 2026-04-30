using System.Diagnostics;

namespace System.Buffers;

internal sealed class ReadOnlySequenceDebugView<T>
{
	[DebuggerDisplay("Count: {Segments.Length}", Name = "Segments")]
	public struct ReadOnlySequenceDebugViewSegments
	{
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public ReadOnlyMemory<T>[] Segments { get; set; }
	}

	private readonly T[] _array;

	private readonly ReadOnlySequenceDebugViewSegments _segments;

	public ReadOnlySequenceDebugViewSegments BufferSegments => _segments;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public T[] Items => _array;

	public ReadOnlySequenceDebugView(ReadOnlySequence<T> sequence)
	{
		_array = BuffersExtensions.ToArray(ref sequence);
		int num = 0;
		ReadOnlySequence<T>.Enumerator enumerator = sequence.GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlyMemory<T> current = enumerator.Current;
			num++;
		}
		ReadOnlyMemory<T>[] array = new ReadOnlyMemory<T>[num];
		int num2 = 0;
		ReadOnlySequence<T>.Enumerator enumerator2 = sequence.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			ReadOnlyMemory<T> current2 = enumerator2.Current;
			array[num2] = current2;
			num2++;
		}
		_segments = new ReadOnlySequenceDebugViewSegments
		{
			Segments = array
		};
	}
}
