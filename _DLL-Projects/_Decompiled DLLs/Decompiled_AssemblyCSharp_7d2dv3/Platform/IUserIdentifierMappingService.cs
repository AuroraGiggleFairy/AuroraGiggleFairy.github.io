using System.Collections.Generic;

namespace Platform;

public interface IUserIdentifierMappingService
{
	bool CanQuery(PlatformUserIdentifierAbs _id);

	void QueryMappedAccountDetails(PlatformUserIdentifierAbs _id, EPlatformIdentifier _platform, MappedAccountQueryCallback _callback);

	void QueryMappedAccountsDetails(IReadOnlyList<MappedAccountRequest> _requests, MappedAccountsQueryCallback _callback);

	bool CanReverseQuery(EPlatformIdentifier _platform, string _platformId);

	void ReverseQueryMappedAccountDetails(EPlatformIdentifier _platform, string _platformId, MappedAccountReverseQueryCallback _callback);

	void ReverseQueryMappedAccountsDetails(IReadOnlyList<MappedAccountReverseRequest> _requests, MappedAccountsReverseQueryCallback _callback);
}
