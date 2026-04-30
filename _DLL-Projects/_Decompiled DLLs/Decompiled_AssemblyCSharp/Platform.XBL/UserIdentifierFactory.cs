using UnityEngine.Scripting;

namespace Platform.XBL;

[Preserve]
[UserIdentifierFactory(EPlatformIdentifier.XBL)]
public class UserIdentifierFactory : AbsUserIdentifierFactory
{
	public override PlatformUserIdentifierAbs FromId(string _userId)
	{
		return new UserIdentifierXbl(_userId);
	}
}
