using System.Collections.Generic;

namespace Platform;

public interface IUserDetailsService
{
	void Init(IPlatform owner);

	void RequestUserDetailsUpdate(IReadOnlyList<UserDetailsRequest> requestedUsers, UserDetailsRequestCompleteHandler onComplete);
}
