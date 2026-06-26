using System.Collections.Generic;

namespace Platform;

public interface IPlatformUserData : IPlatformUser
{
	IReadOnlyDictionary<EBlockType, IPlatformUserBlockedData> Blocked { get; }

	string Name { get; }

	void MarkBlockedStateChanged();

	void RequestUserDetailsUpdate();
}
