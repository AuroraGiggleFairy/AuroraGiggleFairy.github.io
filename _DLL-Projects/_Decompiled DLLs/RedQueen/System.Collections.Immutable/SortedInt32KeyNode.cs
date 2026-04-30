using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

[DebuggerDisplay("{_key} = {_value}")]
[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
internal sealed class SortedInt32KeyNode<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue> : IBinaryTree
{
	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public struct Enumerator : IEnumerator<KeyValuePair<int, TValue>>, IDisposable, IEnumerator, ISecurePooledObjectUser
	{
		private static readonly SecureObjectPool<Stack<RefAsValueType<SortedInt32KeyNode<TValue>>>, Enumerator> s_enumeratingStacks = new SecureObjectPool<Stack<RefAsValueType<SortedInt32KeyNode<TValue>>>, Enumerator>();

		private readonly int _poolUserId;

		private SortedInt32KeyNode<TValue> _root;

		private SecurePooledObject<Stack<RefAsValueType<SortedInt32KeyNode<TValue>>>> _stack;

		private SortedInt32KeyNode<TValue> _current;

		[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
		public KeyValuePair<int, TValue> Current
		{
			[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
			get
			{
				ThrowIfDisposed();
				if (_current != null)
				{
					return _current.Value;
				}
				throw new InvalidOperationException();
			}
		}

		int ISecurePooledObjectUser.PoolUserId => _poolUserId;

		[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)]
		object IEnumerator.Current => Current;

		[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
		internal Enumerator(SortedInt32KeyNode<TValue> root)
		{
			Requires.NotNull(root, "root");
			_root = root;
			_current = null;
			_poolUserId = SecureObjectPool.NewId();
			_stack = null;
			if (!_root.IsEmpty)
			{
				if (!s_enumeratingStacks.TryTake(this, out _stack))
				{
					_stack = s_enumeratingStacks.PrepNew(this, new Stack<RefAsValueType<SortedInt32KeyNode<TValue>>>(root.Height));
				}
				PushLeft(_root);
			}
		}

		public void Dispose()
		{
			_root = null;
			_current = null;
			if (_stack != null && _stack.TryUse(ref this, out var value))
			{
				value.ClearFastWhenEmpty();
				s_enumeratingStacks.TryAdd(this, _stack);
			}
			_stack = null;
		}

		public bool MoveNext()
		{
			ThrowIfDisposed();
			if (_stack != null)
			{
				Stack<RefAsValueType<SortedInt32KeyNode<TValue>>> stack = _stack.Use(ref this);
				if (stack.Count > 0)
				{
					PushLeft((_current = stack.Pop().Value).Right);
					return true;
				}
			}
			_current = null;
			return false;
		}

		public void Reset()
		{
			ThrowIfDisposed();
			_current = null;
			if (_stack != null)
			{
				Stack<RefAsValueType<SortedInt32KeyNode<TValue>>> stack = _stack.Use(ref this);
				stack.ClearFastWhenEmpty();
				PushLeft(_root);
			}
		}

		internal void ThrowIfDisposed()
		{
			if (_root == null || (_stack != null && !_stack.IsOwned(ref this)))
			{
				Requires.FailObjectDisposed(this);
			}
		}

		private void PushLeft(SortedInt32KeyNode<TValue> node)
		{
			Requires.NotNull(node, "node");
			Stack<RefAsValueType<SortedInt32KeyNode<TValue>>> stack = _stack.Use(ref this);
			while (!node.IsEmpty)
			{
				stack.Push(new RefAsValueType<SortedInt32KeyNode<TValue>>(node));
				node = node.Left;
			}
		}
	}

	internal static readonly SortedInt32KeyNode<TValue> EmptyNode = new SortedInt32KeyNode<TValue>();

	private readonly int _key;

	private readonly TValue _value;

	private bool _frozen;

	private byte _height;

	private SortedInt32KeyNode<TValue> _left;

	private SortedInt32KeyNode<TValue> _right;

	public bool IsEmpty => _left == null;

	public int Height => _height;

	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })]
	public SortedInt32KeyNode<TValue> Left
	{
		[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })]
		get
		{
			return _left;
		}
	}

	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })]
	public SortedInt32KeyNode<TValue> Right
	{
		[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 1 })]
		get
		{
			return _right;
		}
	}

	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)]
	IBinaryTree IBinaryTree.Left => _left;

	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)]
	IBinaryTree IBinaryTree.Right => _right;

	int IBinaryTree.Count
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public KeyValuePair<int, TValue> Value
	{
		[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
		get
		{
			return new KeyValuePair<int, TValue>(_key, _value);
		}
	}

	internal IEnumerable<TValue> Values
	{
		get
		{
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				yield return enumerator.Current.Value;
			}
		}
	}

	private SortedInt32KeyNode()
	{
		_frozen = true;
	}

	private SortedInt32KeyNode(int key, TValue value, SortedInt32KeyNode<TValue> left, SortedInt32KeyNode<TValue> right, bool frozen = false)
	{
		Requires.NotNull(left, "left");
		Requires.NotNull(right, "right");
		_key = key;
		_value = value;
		_left = left;
		_right = right;
		_frozen = frozen;
		_height = checked((byte)(1 + Math.Max(left._height, right._height)));
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(0)]
	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	internal SortedInt32KeyNode<TValue> SetItem(int key, TValue value, IEqualityComparer<TValue> valueComparer, out bool replacedExistingValue, out bool mutated)
	{
		Requires.NotNull(valueComparer, "valueComparer");
		return SetOrAdd(key, value, valueComparer, overwriteExistingValue: true, out replacedExistingValue, out mutated);
	}

	internal SortedInt32KeyNode<TValue> Remove(int key, out bool mutated)
	{
		return RemoveRecursive(key, out mutated);
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	internal TValue GetValueOrDefault(int key)
	{
		SortedInt32KeyNode<TValue> sortedInt32KeyNode = this;
		while (true)
		{
			if (sortedInt32KeyNode.IsEmpty)
			{
				return default(TValue);
			}
			if (key == sortedInt32KeyNode._key)
			{
				break;
			}
			sortedInt32KeyNode = ((key <= sortedInt32KeyNode._key) ? sortedInt32KeyNode._left : sortedInt32KeyNode._right);
		}
		return sortedInt32KeyNode._value;
	}

	internal bool TryGetValue(int key, [_003C6723b510_002D2ae0_002D4796_002Dbe1b_002D098bdaf7a574_003EMaybeNullWhen(false)] out TValue value)
	{
		SortedInt32KeyNode<TValue> sortedInt32KeyNode = this;
		while (true)
		{
			if (sortedInt32KeyNode.IsEmpty)
			{
				value = default(TValue);
				return false;
			}
			if (key == sortedInt32KeyNode._key)
			{
				break;
			}
			sortedInt32KeyNode = ((key <= sortedInt32KeyNode._key) ? sortedInt32KeyNode._left : sortedInt32KeyNode._right);
		}
		value = sortedInt32KeyNode._value;
		return true;
	}

	internal void Freeze([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 2, 0, 1 })] Action<KeyValuePair<int, TValue>> freezeAction = null)
	{
		if (!_frozen)
		{
			freezeAction?.Invoke(new KeyValuePair<int, TValue>(_key, _value));
			_left.Freeze(freezeAction);
			_right.Freeze(freezeAction);
			_frozen = true;
		}
	}

	private static SortedInt32KeyNode<TValue> RotateLeft(SortedInt32KeyNode<TValue> tree)
	{
		Requires.NotNull(tree, "tree");
		if (tree._right.IsEmpty)
		{
			return tree;
		}
		SortedInt32KeyNode<TValue> right = tree._right;
		return right.Mutate(tree.Mutate(null, right._left));
	}

	private static SortedInt32KeyNode<TValue> RotateRight(SortedInt32KeyNode<TValue> tree)
	{
		Requires.NotNull(tree, "tree");
		if (tree._left.IsEmpty)
		{
			return tree;
		}
		SortedInt32KeyNode<TValue> left = tree._left;
		return left.Mutate(null, tree.Mutate(left._right));
	}

	private static SortedInt32KeyNode<TValue> DoubleLeft(SortedInt32KeyNode<TValue> tree)
	{
		Requires.NotNull(tree, "tree");
		if (tree._right.IsEmpty)
		{
			return tree;
		}
		SortedInt32KeyNode<TValue> tree2 = tree.Mutate(null, RotateRight(tree._right));
		return RotateLeft(tree2);
	}

	private static SortedInt32KeyNode<TValue> DoubleRight(SortedInt32KeyNode<TValue> tree)
	{
		Requires.NotNull(tree, "tree");
		if (tree._left.IsEmpty)
		{
			return tree;
		}
		SortedInt32KeyNode<TValue> tree2 = tree.Mutate(RotateLeft(tree._left));
		return RotateRight(tree2);
	}

	private static int Balance(SortedInt32KeyNode<TValue> tree)
	{
		Requires.NotNull(tree, "tree");
		return tree._right._height - tree._left._height;
	}

	private static bool IsRightHeavy(SortedInt32KeyNode<TValue> tree)
	{
		Requires.NotNull(tree, "tree");
		return Balance(tree) >= 2;
	}

	private static bool IsLeftHeavy(SortedInt32KeyNode<TValue> tree)
	{
		Requires.NotNull(tree, "tree");
		return Balance(tree) <= -2;
	}

	private static SortedInt32KeyNode<TValue> MakeBalanced(SortedInt32KeyNode<TValue> tree)
	{
		Requires.NotNull(tree, "tree");
		if (IsRightHeavy(tree))
		{
			if (Balance(tree._right) >= 0)
			{
				return RotateLeft(tree);
			}
			return DoubleLeft(tree);
		}
		if (IsLeftHeavy(tree))
		{
			if (Balance(tree._left) <= 0)
			{
				return RotateRight(tree);
			}
			return DoubleRight(tree);
		}
		return tree;
	}

	private SortedInt32KeyNode<TValue> SetOrAdd(int key, TValue value, IEqualityComparer<TValue> valueComparer, bool overwriteExistingValue, out bool replacedExistingValue, out bool mutated)
	{
		replacedExistingValue = false;
		if (IsEmpty)
		{
			mutated = true;
			return new SortedInt32KeyNode<TValue>(key, value, this, this);
		}
		SortedInt32KeyNode<TValue> sortedInt32KeyNode = this;
		if (key > _key)
		{
			SortedInt32KeyNode<TValue> right = _right.SetOrAdd(key, value, valueComparer, overwriteExistingValue, out replacedExistingValue, out mutated);
			if (mutated)
			{
				sortedInt32KeyNode = Mutate(null, right);
			}
		}
		else if (key < _key)
		{
			SortedInt32KeyNode<TValue> left = _left.SetOrAdd(key, value, valueComparer, overwriteExistingValue, out replacedExistingValue, out mutated);
			if (mutated)
			{
				sortedInt32KeyNode = Mutate(left);
			}
		}
		else
		{
			if (valueComparer.Equals(_value, value))
			{
				mutated = false;
				return this;
			}
			if (!overwriteExistingValue)
			{
				throw new ArgumentException(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.Format(_003Ced9a9250_002Dfc30_002D466d_002D93e4_002D5e61d5444a92_003ESR.DuplicateKey, key));
			}
			mutated = true;
			replacedExistingValue = true;
			sortedInt32KeyNode = new SortedInt32KeyNode<TValue>(key, value, _left, _right);
		}
		if (!mutated)
		{
			return sortedInt32KeyNode;
		}
		return MakeBalanced(sortedInt32KeyNode);
	}

	private SortedInt32KeyNode<TValue> RemoveRecursive(int key, out bool mutated)
	{
		if (IsEmpty)
		{
			mutated = false;
			return this;
		}
		SortedInt32KeyNode<TValue> sortedInt32KeyNode = this;
		if (key == _key)
		{
			mutated = true;
			if (_right.IsEmpty && _left.IsEmpty)
			{
				sortedInt32KeyNode = EmptyNode;
			}
			else if (_right.IsEmpty && !_left.IsEmpty)
			{
				sortedInt32KeyNode = _left;
			}
			else if (!_right.IsEmpty && _left.IsEmpty)
			{
				sortedInt32KeyNode = _right;
			}
			else
			{
				SortedInt32KeyNode<TValue> sortedInt32KeyNode2 = _right;
				while (!sortedInt32KeyNode2._left.IsEmpty)
				{
					sortedInt32KeyNode2 = sortedInt32KeyNode2._left;
				}
				bool mutated2;
				SortedInt32KeyNode<TValue> right = _right.Remove(sortedInt32KeyNode2._key, out mutated2);
				sortedInt32KeyNode = sortedInt32KeyNode2.Mutate(_left, right);
			}
		}
		else if (key < _key)
		{
			SortedInt32KeyNode<TValue> left = _left.Remove(key, out mutated);
			if (mutated)
			{
				sortedInt32KeyNode = Mutate(left);
			}
		}
		else
		{
			SortedInt32KeyNode<TValue> right2 = _right.Remove(key, out mutated);
			if (mutated)
			{
				sortedInt32KeyNode = Mutate(null, right2);
			}
		}
		if (!sortedInt32KeyNode.IsEmpty)
		{
			return MakeBalanced(sortedInt32KeyNode);
		}
		return sortedInt32KeyNode;
	}

	private SortedInt32KeyNode<TValue> Mutate(SortedInt32KeyNode<TValue> left = null, SortedInt32KeyNode<TValue> right = null)
	{
		if (_frozen)
		{
			return new SortedInt32KeyNode<TValue>(_key, _value, left ?? _left, right ?? _right);
		}
		if (left != null)
		{
			_left = left;
		}
		if (right != null)
		{
			_right = right;
		}
		_height = checked((byte)(1 + Math.Max(_left._height, _right._height)));
		return this;
	}
}
