using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
internal static class ImmutableStack
{
	public static ImmutableStack<T> Create<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>()
	{
		return ImmutableStack<T>.Empty;
	}

	public static ImmutableStack<T> Create<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(T item)
	{
		return ImmutableStack<T>.Empty.Push(item);
	}

	public static ImmutableStack<T> CreateRange<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(IEnumerable<T> items)
	{
		Requires.NotNull(items, "items");
		ImmutableStack<T> immutableStack = ImmutableStack<T>.Empty;
		foreach (T item in items)
		{
			immutableStack = immutableStack.Push(item);
		}
		return immutableStack;
	}

	public static ImmutableStack<T> Create<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(params T[] items)
	{
		Requires.NotNull(items, "items");
		ImmutableStack<T> immutableStack = ImmutableStack<T>.Empty;
		foreach (T value in items)
		{
			immutableStack = immutableStack.Push(value);
		}
		return immutableStack;
	}

	public static IImmutableStack<T> Pop<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(this IImmutableStack<T> stack, out T value)
	{
		Requires.NotNull(stack, "stack");
		value = stack.Peek();
		return stack.Pop();
	}
}
[DebuggerTypeProxy(typeof(ImmutableEnumerableDebuggerProxy<>))]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
[DebuggerDisplay("IsEmpty = {IsEmpty}; Top = {_head}")]
internal sealed class ImmutableStack<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T> : IImmutableStack<T>, IEnumerable<T>, IEnumerable
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
	public struct Enumerator
	{
		private readonly ImmutableStack<T> _originalStack;

		private ImmutableStack<T> _remainingStack;

		public T Current
		{
			get
			{
				if (_remainingStack == null || _remainingStack.IsEmpty)
				{
					throw new InvalidOperationException();
				}
				return _remainingStack.Peek();
			}
		}

		internal Enumerator(ImmutableStack<T> stack)
		{
			Requires.NotNull(stack, "stack");
			_originalStack = stack;
			_remainingStack = null;
		}

		public bool MoveNext()
		{
			if (_remainingStack == null)
			{
				_remainingStack = _originalStack;
			}
			else if (!_remainingStack.IsEmpty)
			{
				_remainingStack = _remainingStack.Pop();
			}
			return !_remainingStack.IsEmpty;
		}
	}

	private class EnumeratorObject : IEnumerator<T>, IDisposable, IEnumerator
	{
		private readonly ImmutableStack<T> _originalStack;

		private ImmutableStack<T> _remainingStack;

		private bool _disposed;

		public T Current
		{
			get
			{
				ThrowIfDisposed();
				if (_remainingStack == null || _remainingStack.IsEmpty)
				{
					throw new InvalidOperationException();
				}
				return _remainingStack.Peek();
			}
		}

		object IEnumerator.Current => Current;

		internal EnumeratorObject(ImmutableStack<T> stack)
		{
			Requires.NotNull(stack, "stack");
			_originalStack = stack;
		}

		public bool MoveNext()
		{
			ThrowIfDisposed();
			if (_remainingStack == null)
			{
				_remainingStack = _originalStack;
			}
			else if (!_remainingStack.IsEmpty)
			{
				_remainingStack = _remainingStack.Pop();
			}
			return !_remainingStack.IsEmpty;
		}

		public void Reset()
		{
			ThrowIfDisposed();
			_remainingStack = null;
		}

		public void Dispose()
		{
			_disposed = true;
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
			{
				Requires.FailObjectDisposed(this);
			}
		}
	}

	private static readonly ImmutableStack<T> s_EmptyField = new ImmutableStack<T>();

	private readonly T _head;

	private readonly ImmutableStack<T> _tail;

	public static ImmutableStack<T> Empty => s_EmptyField;

	public bool IsEmpty => _tail == null;

	private ImmutableStack()
	{
	}

	private ImmutableStack(T head, ImmutableStack<T> tail)
	{
		_head = head;
		_tail = tail;
	}

	public ImmutableStack<T> Clear()
	{
		return Empty;
	}

	IImmutableStack<T> IImmutableStack<T>.Clear()
	{
		return Clear();
	}

	public T Peek()
	{
		if (IsEmpty)
		{
			throw new InvalidOperationException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.InvalidEmptyOperation);
		}
		return _head;
	}

	[return: _003C213e6825_002D2666_002D4c32_002Dad9b_002D2809d47857fd_003EIsReadOnly]
	public ref T PeekRef()
	{
		if (IsEmpty)
		{
			throw new InvalidOperationException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.InvalidEmptyOperation);
		}
		return ref _head;
	}

	public ImmutableStack<T> Push(T value)
	{
		return new ImmutableStack<T>(value, this);
	}

	IImmutableStack<T> IImmutableStack<T>.Push(T value)
	{
		return Push(value);
	}

	public ImmutableStack<T> Pop()
	{
		if (IsEmpty)
		{
			throw new InvalidOperationException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.InvalidEmptyOperation);
		}
		return _tail;
	}

	public ImmutableStack<T> Pop(out T value)
	{
		value = Peek();
		return Pop();
	}

	IImmutableStack<T> IImmutableStack<T>.Pop()
	{
		return Pop();
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		if (!IsEmpty)
		{
			return new EnumeratorObject(this);
		}
		return Enumerable.Empty<T>().GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new EnumeratorObject(this);
	}

	internal ImmutableStack<T> Reverse()
	{
		ImmutableStack<T> immutableStack = Clear();
		ImmutableStack<T> immutableStack2 = this;
		while (!immutableStack2.IsEmpty)
		{
			immutableStack = immutableStack.Push(immutableStack2.Peek());
			immutableStack2 = immutableStack2.Pop();
		}
		return immutableStack;
	}
}
