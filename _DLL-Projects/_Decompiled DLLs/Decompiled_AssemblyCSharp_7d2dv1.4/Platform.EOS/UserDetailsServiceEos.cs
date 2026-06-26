using System.Collections.Generic;

namespace Platform.EOS;

public class UserDetailsServiceEos : IUserDetailsService
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EosUserIdMapper idMapper;

	[PublicizedFrom(EAccessModifier.Private)]
	public User user;

	public void Init(IPlatform owner)
	{
		idMapper = (EosUserIdMapper)owner.IdMappingService;
		user = (User)owner.User;
	}

	public void RequestUserDetailsUpdate(IReadOnlyList<UserDetailsRequest> requestedUsers, UserDetailsRequestCompleteHandler onComplete)
	{
		List<MappedAccountRequest> list = null;
		List<int> requestIndices = null;
		for (int i = 0; i < requestedUsers.Count; i++)
		{
			UserDetailsRequest userDetailsRequest = requestedUsers[i];
			if (userDetailsRequest.Id.Equals(user.PlatformUserId))
			{
				string text = GamePrefs.GetString(EnumGamePrefs.PlayerName);
				if (!string.IsNullOrEmpty(text))
				{
					userDetailsRequest.details.name = text;
					userDetailsRequest.IsSuccess = true;
					continue;
				}
				Log.Error("[EOS] RequestUserDetailsUpdate: PlayerName not set yet, requesting details for local player");
			}
			if (idMapper.CanQuery(userDetailsRequest.Id))
			{
				if (list == null)
				{
					list = new List<MappedAccountRequest>();
				}
				if (requestIndices == null)
				{
					requestIndices = new List<int>();
				}
				list.Add(new MappedAccountRequest(userDetailsRequest.Id, userDetailsRequest.NativePlatform));
				requestIndices.Add(i);
			}
		}
		if (list == null)
		{
			onComplete(requestedUsers);
		}
		else
		{
			idMapper.QueryMappedAccountsDetails(list, OnAccountsMapped);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		void OnAccountsMapped(IReadOnlyList<MappedAccountRequest> completedMappingRequests)
		{
			for (int j = 0; j < completedMappingRequests.Count; j++)
			{
				MappedAccountRequest mappedAccountRequest = completedMappingRequests[j];
				int index = requestIndices[j];
				UserDetailsRequest userDetailsRequest2 = requestedUsers[index];
				userDetailsRequest2.IsSuccess = mappedAccountRequest.Result == MappedAccountQueryResult.Success;
				if (userDetailsRequest2.IsSuccess)
				{
					userDetailsRequest2.details.name = mappedAccountRequest.DisplayName;
				}
			}
			onComplete(requestedUsers);
		}
	}
}
