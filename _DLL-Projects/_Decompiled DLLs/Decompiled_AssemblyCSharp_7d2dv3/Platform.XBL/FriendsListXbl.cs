using System.Collections.Generic;
using Unity.XGamingRuntime;

namespace Platform.XBL;

public class FriendsListXbl
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SocialManagerXbl socialManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public XblSocialManagerUserGroupHandle friendsUserGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<ulong> friendXuids;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<ulong> friendsXuidsTemp;

	public FriendsListXbl(SocialManagerXbl socialManager)
	{
		this.socialManager = socialManager;
		if (!socialManager.TryCreateUserGroup(XblPresenceFilter.All, XblRelationshipFilter.Friends, OnFriendsListChanged, out friendsUserGroup))
		{
			Log.Error("[FriendsListXbl] failed to create friends social manager group");
			friendsUserGroup = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnFriendsListChanged(ulong[] friends)
	{
		if (friendsXuidsTemp == null)
		{
			friendsXuidsTemp = new HashSet<ulong>();
		}
		foreach (ulong item in friends)
		{
			friendsXuidsTemp.Add(item);
		}
		HashSet<ulong> hashSet = friendsXuidsTemp;
		HashSet<ulong> hashSet2 = friendXuids;
		friendXuids = hashSet;
		friendsXuidsTemp = hashSet2;
		friendsXuidsTemp?.Clear();
		XblXuidMapper.ResolveUserIdentifiers(friendXuids);
	}

	public bool IsFriend(ulong xuid)
	{
		if (friendsUserGroup == null)
		{
			Log.Error("[FriendsListXbl] could not check IsFriend, friends user group has not been initialized yet.");
			return false;
		}
		if (friendXuids == null)
		{
			Log.Error("[FriendsListXbl] could not check IsFriend, friends list has not been retrieved yet");
			return false;
		}
		return friendXuids.Contains(xuid);
	}
}
