using System.Collections;
using System.Linq;
using Unity.XGamingRuntime;

namespace Platform.XBL;

public class UserPrivilegeHelper
{
	public readonly PrivilegeState Multiplayer;

	public readonly PrivilegeState Communications;

	public readonly PrivilegeState CrossPlay;

	public readonly PrivilegeState UserGeneratedContent;

	public readonly PrivilegeState[] AllAllowed;

	public readonly PrivilegeState[] MultiplayerAllowed;

	public readonly PrivilegeState[] CommunicationAllowed;

	public readonly PrivilegeState[] CrossPlayAllowed;

	public UserPrivilegeHelper(XUserHandle userHandle)
	{
		AllAllowed = new PrivilegeState[4]
		{
			(Multiplayer = new PrivilegeState(userHandle, XUserPrivilege.Multiplayer)),
			(Communications = new PrivilegeState(userHandle, XUserPrivilege.Communications)),
			(CrossPlay = new PrivilegeState(userHandle, XUserPrivilege.CrossPlay)),
			(UserGeneratedContent = new PrivilegeState(userHandle, XUserPrivilege.UserGeneratedContent))
		};
		MultiplayerAllowed = new PrivilegeState[2] { Multiplayer, UserGeneratedContent };
		CommunicationAllowed = new PrivilegeState[1] { Communications };
		CrossPlayAllowed = new PrivilegeState[1] { CrossPlay };
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator ResolveAllowed(bool canPrompt, CoroutineCancellationToken _cancellationToken = null, params PrivilegeState[] privilegeStates)
	{
		if (!canPrompt)
		{
			privilegeStates.ResolveSilent();
			return Enumerable.Empty<object>().GetEnumerator();
		}
		return privilegeStates.ResolveWithPrompt(_cancellationToken);
	}

	public IEnumerator ResolvePermissions(EUserPerms _perms, bool _canPrompt, CoroutineCancellationToken _cancellationToken = null)
	{
		if (_perms.HasMultiplayer() || _perms.HasHostMultiplayer())
		{
			yield return ResolveAllowed(_canPrompt, _cancellationToken, MultiplayerAllowed);
			if (_cancellationToken?.IsCancelled() ?? false)
			{
				yield break;
			}
		}
		if (_perms.HasCommunication())
		{
			yield return ResolveAllowed(_canPrompt, _cancellationToken, CommunicationAllowed);
			if (_cancellationToken?.IsCancelled() ?? false)
			{
				yield break;
			}
		}
		if (_perms.HasCrossplay())
		{
			yield return ResolveAllowed(_canPrompt, _cancellationToken, CrossPlayAllowed);
			_cancellationToken?.IsCancelled();
		}
	}
}
