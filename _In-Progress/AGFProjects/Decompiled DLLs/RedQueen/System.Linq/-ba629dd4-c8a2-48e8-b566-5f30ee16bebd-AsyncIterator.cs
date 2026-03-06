using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.Linq;

[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 0, 1 })]
[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(1)]
internal abstract class _003Cba629dd4_002Dc8a2_002D48e8_002Db566_002D5f30ee16bebd_003EAsyncIterator<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TSource> : _003C6190e072_002D1e5f_002D4ff6_002D9577_002D34e73cf1fe40_003EAsyncIteratorBase<TSource>
{
	protected TSource _current;

	public override TSource Current => _current;

	public override ValueTask DisposeAsync()
	{
		_current = default(TSource);
		return base.DisposeAsync();
	}
}
