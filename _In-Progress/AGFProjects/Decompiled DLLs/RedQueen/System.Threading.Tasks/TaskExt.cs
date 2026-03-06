using System.Runtime.CompilerServices;

namespace System.Threading.Tasks;

internal static class TaskExt
{
	[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(1)]
	public static readonly TaskCompletionSource<bool> True;

	static TaskExt()
	{
		True = new TaskCompletionSource<bool>();
		True.SetResult(result: true);
	}
}
