using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SpawnNearFriendsList : XUiC_List<XUiC_SpawnNearFriendsList.ListEntry>
{
	[Preserve]
	public class ListEntry : XUiListEntry<ListEntry>
	{
		public readonly PersistentPlayerData PersistentPlayerData;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string name;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string displayName;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool showIconCrossplay;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string iconCrossplaySprite;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3i position;

		[PublicizedFrom(EAccessModifier.Private)]
		public BiomeDefinition biome;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool locationAllowed;

		[PublicizedFrom(EAccessModifier.Private)]
		public string invalidLocationReason;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string friendPlatform;

		public bool IsFriend => !string.IsNullOrEmpty(friendPlatform);

		public bool ValidSpawn
		{
			get
			{
				if (IsFriend)
				{
					return locationAllowed;
				}
				return false;
			}
		}

		public string InvalidSpawnReason
		{
			get
			{
				if (IsFriend)
				{
					if (locationAllowed)
					{
						return null;
					}
					return invalidLocationReason;
				}
				return Localization.Get("xuiSpawnNearFriendNotAFriend");
			}
		}

		public ListEntry(int _randomSeed)
		{
			GameRandom tempGameRandom = GameRandomManager.Instance.GetTempGameRandom(_randomSeed);
			int randomInt = tempGameRandom.RandomInt;
			name = randomInt.ToString();
			displayName = name;
			position = new Vector3i(tempGameRandom.RandomRange(-100, 101), 0, tempGameRandom.RandomRange(-100, 101));
			locationAllowed = randomInt / 2 % 2 == 0;
			friendPlatform = ((randomInt % 2 == 0) ? "X" : null);
			if (!locationAllowed)
			{
				invalidLocationReason = Localization.Get("xuiSpawnNearFriendNotInForest");
			}
			showIconCrossplay = randomInt % 2 == 0;
			iconCrossplaySprite = "ui_platform_pc";
		}

		public ListEntry(PersistentPlayerData _persistentPlayerData, bool _showPlatformIcons)
		{
			name = _persistentPlayerData.PlayerName.AuthoredName.Text;
			displayName = _persistentPlayerData.PlayerName.SafeDisplayName;
			PersistentPlayerData = _persistentPlayerData;
			PlayerData playerData = _persistentPlayerData.PlayerData;
			iconCrossplaySprite = PlatformManager.NativePlatform.Utils.GetCrossplayPlayerIcon(playerData.PlayGroup, _fetchGenericIcons: true, playerData.NativeId.PlatformIdentifier);
			showIconCrossplay = _showPlatformIcons;
			if (DiscordManager.Instance.TryGetUserFromEntityId(_persistentPlayerData.EntityId, out var _user) && _user.IsFriend)
			{
				friendPlatform = "Discord";
			}
			if (!PlatformUserIdentifierAbs.Equals(playerData.NativeId, playerData.PrimaryId) && playerData.PrimaryId != null && PlatformManager.MultiPlatform.User.IsFriend(playerData.PrimaryId) && friendPlatform == null)
			{
				friendPlatform = PlatformManager.GetPlatformDisplayName(playerData.PrimaryId.PlatformIdentifier);
			}
			if (PlatformManager.MultiPlatform.User.IsFriend(playerData.NativeId) && friendPlatform == null)
			{
				friendPlatform = PlatformManager.GetPlatformDisplayName(playerData.NativeId.PlatformIdentifier);
			}
			UpdatePosition();
		}

		public void UpdatePosition()
		{
			if (PersistentPlayerData == null)
			{
				return;
			}
			position = PersistentPlayerData.Position;
			biome = GameManager.Instance.World?.GetBiomeInWorld(position.x, position.z);
			bool flag;
			switch (SpawnNearFriendMode)
			{
			case AllowSpawnNearFriend.Always:
				locationAllowed = true;
				invalidLocationReason = string.Empty;
				break;
			case AllowSpawnNearFriend.InForest:
			{
				BiomeDefinition.BiomeType? biomeType = biome?.m_BiomeType;
				if (biomeType.HasValue)
				{
					BiomeDefinition.BiomeType valueOrDefault = biomeType.GetValueOrDefault();
					if ((uint)(valueOrDefault - 2) <= 1u)
					{
						flag = true;
						goto IL_00ad;
					}
				}
				flag = false;
				goto IL_00ad;
			}
			default:
				{
					locationAllowed = false;
					invalidLocationReason = Localization.Get("xuiSpawnNearFriendDisabled");
					break;
				}
				IL_00ad:
				locationAllowed = flag;
				if (!locationAllowed)
				{
					invalidLocationReason = Localization.Get("xuiSpawnNearFriendNotInForest");
				}
				break;
			}
		}

		public override int CompareTo(ListEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			int num = IsFriend.CompareTo(_otherEntry.IsFriend);
			if (num != 0)
			{
				return -num;
			}
			num = ValidSpawn.CompareTo(_otherEntry.ValidSpawn);
			if (num != 0)
			{
				return -num;
			}
			return string.Compare(name, _otherEntry.name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "name":
				_value = displayName;
				return true;
			case "position":
				_value = ValueDisplayFormatters.WorldPos(position);
				return true;
			case "biomeName":
				_value = biome?.LocalizedName ?? "-";
				return true;
			case "locationAllowed":
				_value = locationAllowed.ToString();
				return true;
			case "isFriend":
				_value = IsFriend.ToString();
				return true;
			case "friendPlatform":
				_value = friendPlatform ?? "-";
				return true;
			case "validSpawn":
				_value = ValidSpawn.ToString();
				return true;
			case "invalidSpawnReason":
				_value = InvalidSpawnReason ?? "";
				return true;
			case "showIconCrossplay":
			{
				bool flag = showIconCrossplay;
				_value = flag.ToString();
				return true;
			}
			case "iconCrossplaySprite":
				_value = iconCrossplaySprite;
				return true;
			case "canShowProfile":
				_value = (PersistentPlayerData != null && PlatformManager.NativePlatform.User.CanShowProfile(PersistentPlayerData.PlayerData.NativeId)).ToString();
				return true;
			default:
				return false;
			}
		}

		public override bool MatchesSearch(string _searchString)
		{
			return name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "name":
				_value = string.Empty;
				return true;
			case "position":
				_value = string.Empty;
				return true;
			case "biomeName":
				_value = string.Empty;
				return true;
			case "locationAllowed":
				_value = false.ToString();
				return true;
			case "isFriend":
				_value = false.ToString();
				return true;
			case "friendPlatform":
				_value = string.Empty;
				return true;
			case "validSpawn":
				_value = false.ToString();
				return true;
			case "invalidSpawnReason":
				_value = string.Empty;
				return true;
			case "showIconCrossplay":
				_value = false.ToString();
				return true;
			case "iconCrossplaySprite":
				_value = string.Empty;
				return true;
			case "canShowProfile":
				_value = false.ToString();
				return true;
			default:
				return false;
			}
		}
	}

	[Preserve]
	public class SpawnNearFriendsListEntryController : XUiC_ListEntry<ListEntry>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public XUiController btnProfile;

		public override void Init()
		{
			base.Init();
			btnProfile = GetChildById("btnViewProfile");
			btnProfile.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				ListEntry entry = GetEntry();
				if (entry != null && entry.PersistentPlayerData != null && entry.PersistentPlayerData.NativeId != null && PlatformManager.MultiPlatform.User.CanShowProfile(entry.PersistentPlayerData.NativeId))
				{
					PlatformManager.MultiPlatform.User.ShowProfile(entry.PersistentPlayerData.NativeId);
				}
			};
		}
	}

	public static XUiC_SpawnNearFriendsList Instance;

	public static AllowSpawnNearFriend SpawnNearFriendMode
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentMode == ProtocolManager.NetworkType.None)
			{
				return (AllowSpawnNearFriend)GamePrefs.GetInt(EnumGamePrefs.AllowSpawnNearFriend);
			}
			return (AllowSpawnNearFriend)(SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentGameServerInfoServerOrClient?.GetValue(GameInfoInt.AllowSpawnNearFriend) ?? 1);
		}
	}

	public event Action<PersistentPlayerData> SpawnClicked;

	public override void Init()
	{
		base.Init();
		Instance = this;
		if (!(GetChildById("btnSpawnNearFriend") is XUiC_SimpleButton xUiC_SimpleButton))
		{
			return;
		}
		xUiC_SimpleButton.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
		{
			ListEntry listEntry = base.SelectedEntry?.GetEntry();
			if (listEntry != null)
			{
				this.SpawnClicked?.Invoke(listEntry.PersistentPlayerData);
			}
		};
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RebuildList();
		ObservableDictionary<PlatformUserIdentifierAbs, PersistentPlayerData> observableDictionary = GameManager.Instance.persistentPlayers?.Players;
		if (observableDictionary != null)
		{
			observableDictionary.EntryAdded += playersDictChanged;
			observableDictionary.EntryModified += playersDictChanged;
			observableDictionary.EntryRemoved += playersDictChanged;
			observableDictionary.EntryUpdatedValue += playersDictChanged;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		ObservableDictionary<PlatformUserIdentifierAbs, PersistentPlayerData> observableDictionary = GameManager.Instance.persistentPlayers?.Players;
		if (observableDictionary != null)
		{
			observableDictionary.EntryAdded -= playersDictChanged;
			observableDictionary.EntryModified -= playersDictChanged;
			observableDictionary.EntryRemoved -= playersDictChanged;
			observableDictionary.EntryUpdatedValue -= playersDictChanged;
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		Instance = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playersDictChanged(object _sender, DictionaryChangedEventArgs<PlatformUserIdentifierAbs, PersistentPlayerData> _e)
	{
		UpdatePlayers();
	}

	public void UpdatePlayers()
	{
		int previouslySelectedEntityId = CurrentSelectedEntry?.PersistentPlayerData.EntityId ?? (-1);
		RebuildList();
		if (previouslySelectedEntityId != -1)
		{
			base.SelectedEntryIndex = filteredEntries.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (ListEntry _entry) => _entry.PersistentPlayerData.EntityId == previouslySelectedEntityId);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (!(_bindingName == "selectionValid"))
		{
			if (_bindingName == "invalidSpawnReason")
			{
				_value = CurrentSelectedEntry?.InvalidSpawnReason ?? "";
				return true;
			}
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = (CurrentSelectedEntry?.ValidSpawn ?? false).ToString();
		return true;
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentMode == ProtocolManager.NetworkType.None)
		{
			for (int i = 0; i < 10; i++)
			{
				allEntries.Add(new ListEntry(i + (int)(Time.unscaledTime * 1000f)));
			}
			allEntries.Sort();
		}
		if (GameManager.Instance.World == null)
		{
			base.RebuildList(_resetFilter);
			return;
		}
		PersistentPlayerList persistentPlayers = GameManager.Instance.persistentPlayers;
		if (persistentPlayers == null)
		{
			return;
		}
		bool value = SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentGameServerInfoServerOrClient.GetValue(GameInfoBool.AllowCrossplay);
		foreach (var (_, persistentPlayerData2) in persistentPlayers.Players)
		{
			if (persistentPlayerData2.EntityId != -1 && (!PlatformUserManager.GetOrCreate(persistentPlayerData2.PrimaryId).Blocked.TryGetValue(EBlockType.Play, out var value2) || !value2.IsBlocked()))
			{
				allEntries.Add(new ListEntry(persistentPlayerData2, value));
			}
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}
}
