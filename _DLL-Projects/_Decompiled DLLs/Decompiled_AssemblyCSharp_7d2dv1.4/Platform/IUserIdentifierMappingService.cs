using System.Collections.Generic;

namespace Platform;

public interface IUserIdentifierMappingService
{
	bool CanQuery(PlatformUserIdentifierAbs _id);

	void QueryMappedAccountDetails(PlatformUserIdentifierAbs _id, EPlatformIdentifier _platform, MappedAccountQueryCallback _callback);

	void QueryMappedAccountsDetails(IReadOnlyList<MappedAccountRequest> _requests, MappedAccountsQueryCallback _callback);
}
