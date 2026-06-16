using UnityEngine.Scripting;

namespace Platform.Steam;

[Preserve]
[UserIdentifierFactory(EPlatformIdentifier.Steam)]
public class UserIdentifierFactory : AbsUserIdentifierFactory
{
	public override PlatformUserIdentifierAbs FromId(string _userId)
	{
		return new UserIdentifierSteam(_userId);
	}
}
