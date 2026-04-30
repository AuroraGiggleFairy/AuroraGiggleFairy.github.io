using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.Linq;

[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1 })]
internal abstract class AsyncIterator<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TSource> : AsyncIteratorBase<TSource>
{
	protected TSource _current;

	public override TSource Current => _current;

	public override ValueTask DisposeAsync()
	{
		_current = default(TSource);
		return base.DisposeAsync();
	}
}
