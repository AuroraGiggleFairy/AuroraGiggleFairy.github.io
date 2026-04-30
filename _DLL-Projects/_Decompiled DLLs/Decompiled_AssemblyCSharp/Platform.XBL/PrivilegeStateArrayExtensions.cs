using System.Collections;

namespace Platform.XBL;

public static class PrivilegeStateArrayExtensions
{
	public static bool Has(this PrivilegeState[] privilegeStates)
	{
		for (int i = 0; i < privilegeStates.Length; i++)
		{
			if (!privilegeStates[i].Has)
			{
				return false;
			}
		}
		return true;
	}

	public static void ResolveSilent(this PrivilegeState[] privilegeStates)
	{
		for (int i = 0; i < privilegeStates.Length; i++)
		{
			privilegeStates[i].ResolveSilent();
		}
	}

	public static IEnumerator ResolveWithPrompt(this PrivilegeState[] privilegeStates, CoroutineCancellationToken _cancellationToken)
	{
		foreach (PrivilegeState privilegeState in privilegeStates)
		{
			yield return privilegeState.ResolveWithPrompt(_cancellationToken);
			if (_cancellationToken?.IsCancelled() ?? false)
			{
				break;
			}
		}
	}
}
