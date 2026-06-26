using UnityEngine.Scripting;

namespace Platform.PSN;

[Preserve]
[UserIdentifierFactory(EPlatformIdentifier.PSN)]
public class UserIdentifierFactory : AbsUserIdentifierFactory
{
	public override PlatformUserIdentifierAbs FromId(string _idString)
	{
		Log.Out("[PSN] Creating PSN user identifier from: {0}", _idString);
		if (StringParsers.TryParseUInt64(_idString, out var _result))
		{
			return new UserIdentifierPSN(_result);
		}
		Log.Warning("[PSN] Could not parse PSN user from " + _idString);
		return null;
	}
}
