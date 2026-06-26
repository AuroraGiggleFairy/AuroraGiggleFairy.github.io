using UnityEngine.Scripting;

namespace Platform.EOS;

[Preserve]
[UserIdentifierFactory(EPlatformIdentifier.EOS)]
public class UserIdentifierFactory : AbsUserIdentifierFactory
{
	public override PlatformUserIdentifierAbs FromId(string _userId)
	{
		return new UserIdentifierEos(_userId);
	}
}
