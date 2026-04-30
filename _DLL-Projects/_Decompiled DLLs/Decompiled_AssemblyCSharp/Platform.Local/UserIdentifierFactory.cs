using UnityEngine.Scripting;

namespace Platform.Local;

[Preserve]
[UserIdentifierFactory(EPlatformIdentifier.Local)]
public class UserIdentifierFactory : AbsUserIdentifierFactory
{
	public override PlatformUserIdentifierAbs FromId(string _userId)
	{
		return new UserIdentifierLocal(_userId);
	}
}
