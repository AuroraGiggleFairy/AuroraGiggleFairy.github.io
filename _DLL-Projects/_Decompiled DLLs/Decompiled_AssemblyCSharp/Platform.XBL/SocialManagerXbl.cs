using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XGamingRuntime;
using Unity.XGamingRuntime.Interop;
using UnityEngine;

namespace Platform.XBL;

public class SocialManagerXbl
{
	public delegate void UserGroupMembersChanged(ulong[] members);

	[PublicizedFrom(EAccessModifier.Private)]
	public class UserGroup
	{
		public readonly XblSocialManagerUserGroupHandle handle;

		public readonly UserGroupMembersChanged membersChangedCallback;

		public bool isLoaded;

		[PublicizedFrom(EAccessModifier.Private)]
		public ulong[] membersCache;

		public UserGroup(XblSocialManagerUserGroupHandle handle, UserGroupMembersChanged membersChangedCallback)
		{
			this.handle = handle;
			this.membersChangedCallback = membersChangedCallback;
		}

		public void NotifyChanged()
		{
			if (!isLoaded)
			{
				return;
			}
			ulong[] array = membersCache;
			int hr = SDK.XBL.XblSocialManagerUserGroupGetUsersTrackedByGroup(handle, out membersCache);
			if (!Unity.XGamingRuntime.Interop.HR.SUCCEEDED(hr))
			{
				XblHelpers.LogHR(hr, "XblSocialManagerUserGroupGetUsersTrackedByGroup");
				membersCache = null;
				return;
			}
			Array.Sort(membersCache);
			if (array != null && array.Length == membersCache.Length && Enumerable.SequenceEqual(membersCache, array))
			{
				Log.Out("[XBL] social manager skipping user group update as member list didn't change");
				return;
			}
			ulong[] array2 = new ulong[membersCache.Length];
			Array.Copy(membersCache, array2, membersCache.Length);
			membersChangedCallback(array2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUserHandle localUser;

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine updateCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<XblSocialManagerUserGroupHandle, UserGroup> userGroups = new Dictionary<XblSocialManagerUserGroupHandle, UserGroup>();

	public SocialManagerXbl(XUserHandle user)
	{
		int hr = SDK.XBL.XblSocialManagerAddLocalUser(user, XblSocialManagerExtraDetailLevel.NoExtraDetail);
		if (!Unity.XGamingRuntime.Interop.HR.SUCCEEDED(hr))
		{
			XblHelpers.LogHR(hr, "XblSocialManagerAddLocalUser");
		}
		else
		{
			localUser = user;
		}
	}

	public bool TryCreateUserGroup(XblPresenceFilter presenceFilter, XblRelationshipFilter relationshipFilter, UserGroupMembersChanged callback, out XblSocialManagerUserGroupHandle handle)
	{
		if (localUser == null)
		{
			Log.Error("[XBL] Social users lookup not available as the local user was not registered");
			handle = null;
			return false;
		}
		if (callback == null)
		{
			Log.Error("[XBL] TryCreateUserGroup null callback not permitted");
			handle = null;
			return false;
		}
		int hr = SDK.XBL.XblSocialManagerCreateSocialUserGroupFromFilters(localUser, presenceFilter, relationshipFilter, out handle);
		if (!Unity.XGamingRuntime.Interop.HR.SUCCEEDED(hr))
		{
			Log.Error($"[XBL] Failed to create user group for {presenceFilter} {relationshipFilter}");
			XblHelpers.LogHR(hr, "XblSocialManagerCreateSocialUserGroupFromFilters");
			handle = null;
			return false;
		}
		userGroups.Add(handle, new UserGroup(handle, callback));
		if (updateCoroutine == null)
		{
			updateCoroutine = ThreadManager.StartCoroutine(UpdateSocialManagerCoroutine());
		}
		return true;
	}

	public void DestroyUserGroup(XblSocialManagerUserGroupHandle handle)
	{
		userGroups.Remove(handle);
		int hr = SDK.XBL.XblSocialManagerDestroySocialUserGroup(handle);
		if (!Unity.XGamingRuntime.Interop.HR.SUCCEEDED(hr))
		{
			XblHelpers.LogHR(hr, "XblSocialManagerCreateSocialUserGroupFromFilters");
		}
		if (userGroups.Count == 0 && updateCoroutine != null)
		{
			ThreadManager.StopCoroutine(updateCoroutine);
			updateCoroutine = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator UpdateSocialManagerCoroutine()
	{
		int hr;
		while (true)
		{
			hr = SDK.XBL.XblSocialManagerDoWork(out var socialEvents);
			if (!Unity.XGamingRuntime.Interop.HR.SUCCEEDED(hr))
			{
				break;
			}
			if (socialEvents != null && socialEvents.Length != 0)
			{
				if (IsUpdateRequired(socialEvents))
				{
					foreach (UserGroup value2 in userGroups.Values)
					{
						value2.NotifyChanged();
					}
				}
				XblSocialManagerEvent[] array = socialEvents;
				foreach (XblSocialManagerEvent xblSocialManagerEvent in array)
				{
					if (xblSocialManagerEvent.EventType == XblSocialManagerEventType.SocialUserGroupLoaded)
					{
						if (!userGroups.TryGetValue(xblSocialManagerEvent.LoadedGroup, out var value))
						{
							Log.Error("[GameCore] LoadedGroup did not match saved handle");
							continue;
						}
						value.isLoaded = true;
						value.NotifyChanged();
					}
				}
			}
			yield return null;
		}
		XblHelpers.LogHR(hr, "XblAchievementsManagerDoWork");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool IsUpdateRequired(XblSocialManagerEvent[] socialEvents)
	{
		for (int i = 0; i < socialEvents.Length; i++)
		{
			switch (socialEvents[i].EventType)
			{
			case XblSocialManagerEventType.UsersAddedToSocialGraph:
			case XblSocialManagerEventType.UsersRemovedFromSocialGraph:
			case XblSocialManagerEventType.SocialRelationshipsChanged:
				return true;
			case XblSocialManagerEventType.PresenceChanged:
				return true;
			}
		}
		return false;
	}
}
