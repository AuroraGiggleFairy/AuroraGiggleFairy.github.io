using System;
using System.Collections;
using System.Xml.Linq;
using Twitch;

public class TwitchActionsFromXml
{
	public static IEnumerator CreateTwitchActions(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		TwitchActionManager.Current.Cleanup();
		if (!root.HasElements)
		{
			throw new Exception("No element <twitch> found!");
		}
		HandleTwitchSettings(root);
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		foreach (XElement item in root.Elements())
		{
			if (item.Name == "action")
			{
				ParseTwitchActions(item);
			}
			else if (item.Name == "actions_preset")
			{
				ParseActionsPreset(item);
			}
			else if (item.Name == "votes_preset")
			{
				ParseVotesPreset(item);
			}
			else if (item.Name == "vote_entry")
			{
				ParseVoteEntry(item);
			}
			else if (item.Name == "vote_type")
			{
				ParseVoteTypeEntry(item);
			}
			else if (item.Name == "command_permission")
			{
				ParseCommandPermission(item);
			}
			else if (item.Name == "cooldown_preset")
			{
				ParseCooldownPreset(item);
			}
			else if (item.Name == "category")
			{
				XElement element = item;
				if (element.HasAttribute("name"))
				{
					string attribute = element.GetAttribute("name");
					string displayName = (element.HasAttribute("display_name") ? Localization.Get(element.GetAttribute("display_name")) : attribute);
					bool showInCommandList = !element.HasAttribute("show_command_list") || StringParsers.ParseBool(element.GetAttribute("show_command_list"));
					bool alwaysShowInMenu = element.HasAttribute("always_show_in_menu") && StringParsers.ParseBool(element.GetAttribute("always_show_in_menu"));
					if (element.HasAttribute("icon"))
					{
						TwitchActionManager.Current.CategoryList.Add(new TwitchActionManager.ActionCategory
						{
							Name = attribute,
							DisplayName = displayName,
							Icon = element.GetAttribute("icon"),
							ShowInCommandList = showInCommandList,
							AlwaysShowInMenu = alwaysShowInMenu
						});
					}
					else
					{
						TwitchActionManager.Current.CategoryList.Add(new TwitchActionManager.ActionCategory
						{
							Name = attribute,
							DisplayName = displayName,
							Icon = "",
							ShowInCommandList = showInCommandList,
							AlwaysShowInMenu = alwaysShowInMenu
						});
					}
				}
			}
			else if (item.Name == "random_group")
			{
				ParseRandomGroup(item);
			}
			else
			{
				if (!(item.Name == "tip"))
				{
					throw new Exception("Unrecognized xml element " + item.Name);
				}
				XElement element2 = item;
				if (element2.HasAttribute("name"))
				{
					TwitchManager.Current.AddTip(element2.GetAttribute("name"));
				}
			}
			if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
	}

	public static IEnumerator CreateTwitchEvents(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		TwitchManager.Current.CleanupEventData();
		if (root == null)
		{
			throw new Exception("No element <twitch_events> found!");
		}
		HandleTwitchSettings(root);
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		foreach (XElement item in root.Elements())
		{
			if (item.Name == "bit_event")
			{
				ParseEvent(item, item.Name.LocalName);
			}
			else if (item.Name == "events_preset")
			{
				ParseEventsPreset(item, item.Name.LocalName);
			}
			else if (item.Name == "sub_event")
			{
				ParseSubEvent(item, item.Name.LocalName);
			}
			else if (item.Name == "gift_sub_event")
			{
				ParseSubEvent(item, item.Name.LocalName);
			}
			else if (item.Name == "raid_event")
			{
				ParseEvent(item, item.Name.LocalName);
			}
			else if (item.Name == "charity_event")
			{
				ParseEvent(item, item.Name.LocalName);
			}
			else if (item.Name == "hype_train_event")
			{
				ParseHypeTrainEvent(item, item.Name.LocalName);
			}
			else if (item.Name == "channel_point_event")
			{
				ParseChannelPointEvent(item);
			}
			else
			{
				if (!(item.Name == "creator_goal_event"))
				{
					throw new Exception("Unrecognized xml element " + item.Name);
				}
				ParseCreatorGoalEvent(item, item.Name.LocalName);
			}
			if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void HandleTwitchSettings(XElement e)
	{
		TwitchManager current = TwitchManager.Current;
		if (e.HasAttribute("starting_points"))
		{
			current.ViewerData.StartingPoints = StringParsers.ParseSInt32(e.GetAttribute("starting_points"));
		}
		if (e.HasAttribute("party_kill_reward_max"))
		{
			int num = StringParsers.ParseSInt32(e.GetAttribute("party_kill_reward_max"));
			if (num < 0)
			{
				num = 0;
			}
			current.PartyKillRewardMax = num;
		}
		if (e.HasAttribute("chat_activity_time"))
		{
			float num2 = StringParsers.ParseFloat(e.GetAttribute("chat_activity_time"));
			if (num2 < 0f)
			{
				num2 = 0f;
			}
			TwitchViewerData.ChattingAddedTimeAmount = num2;
		}
		if (e.HasAttribute("nonsub_pp_cap"))
		{
			current.ViewerData.NonSubPointCap = StringParsers.ParseFloat(e.GetAttribute("nonsub_pp_cap"));
		}
		if (e.HasAttribute("sub_pp_cap"))
		{
			current.ViewerData.SubPointCap = StringParsers.ParseFloat(e.GetAttribute("sub_pp_cap"));
		}
		if (e.HasAttribute("pimp_pot_type"))
		{
			switch (e.GetAttribute("pimp_pot_type").ToLower())
			{
			case "pp":
				current.PimpPotType = TwitchManager.PimpPotSettings.EnabledPP;
				break;
			case "sp":
				current.PimpPotType = TwitchManager.PimpPotSettings.EnabledSP;
				break;
			case "disabled":
				current.PimpPotType = TwitchManager.PimpPotSettings.Disabled;
				break;
			}
		}
		if (e.HasAttribute("pimp_pot_default"))
		{
			TwitchManager.PimpPotDefault = StringParsers.ParseSInt32(e.GetAttribute("pimp_pot_default"));
		}
		if (e.HasAttribute("denied_crate_event"))
		{
			current.DeniedCrateEvent = e.GetAttribute("denied_crate_event");
		}
		if (e.HasAttribute("stealing_crate_event"))
		{
			current.StealingCrateEvent = e.GetAttribute("stealing_crate_event");
		}
		if (e.HasAttribute("party_respawn_event"))
		{
			current.PartyRespawnEvent = e.GetAttribute("party_respawn_event");
		}
		if (e.HasAttribute("on_death_event"))
		{
			current.OnPlayerDeathEvent = e.GetAttribute("on_death_event");
		}
		if (e.HasAttribute("on_player_respawn_event"))
		{
			current.OnPlayerRespawnEvent = e.GetAttribute("on_player_respawn_event");
		}
		if (e.HasAttribute("sub_tier1_points"))
		{
			current.ViewerData.SubPointAddTier1 = StringParsers.ParseSInt32(e.GetAttribute("sub_tier1_points"));
		}
		if (e.HasAttribute("sub_tier2_points"))
		{
			current.ViewerData.SubPointAddTier2 = StringParsers.ParseSInt32(e.GetAttribute("sub_tier2_points"));
		}
		if (e.HasAttribute("sub_tier3_points"))
		{
			current.ViewerData.SubPointAddTier3 = StringParsers.ParseSInt32(e.GetAttribute("sub_tier3_points"));
		}
		if (e.HasAttribute("gift_sub_tier1_points"))
		{
			current.ViewerData.GiftSubPointAddTier1 = StringParsers.ParseSInt32(e.GetAttribute("gift_sub_tier1_points"));
		}
		if (e.HasAttribute("gift_sub_tier2_points"))
		{
			current.ViewerData.GiftSubPointAddTier2 = StringParsers.ParseSInt32(e.GetAttribute("gift_sub_tier2_points"));
		}
		if (e.HasAttribute("gift_sub_tier3_points"))
		{
			current.ViewerData.GiftSubPointAddTier3 = StringParsers.ParseSInt32(e.GetAttribute("gift_sub_tier3_points"));
		}
	}

	public static void Reload(XmlFile xmlFile)
	{
		ThreadManager.RunCoroutineSync(CreateTwitchActions(xmlFile));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DynamicProperties HandleExtends(TwitchAction extendedClass)
	{
		if (extendedClass.Properties != null)
		{
			DynamicProperties dynamicProperties = new DynamicProperties();
			dynamicProperties.CopyFrom(extendedClass.Properties, TwitchAction.ExtendsExcludes);
			return dynamicProperties;
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseActionsPreset(XElement e)
	{
		TwitchActionPreset twitchActionPreset = new TwitchActionPreset();
		if (e.HasAttribute("name"))
		{
			twitchActionPreset.Name = e.GetAttribute("name");
		}
		if (e.HasAttribute("title"))
		{
			twitchActionPreset.Title = e.GetAttribute("title");
		}
		if (e.HasAttribute("title_key"))
		{
			twitchActionPreset.Title = Localization.Get(e.GetAttribute("title_key"));
		}
		if (e.HasAttribute("description"))
		{
			twitchActionPreset.Description = e.GetAttribute("description");
		}
		if (e.HasAttribute("description_key"))
		{
			twitchActionPreset.Description = Localization.Get(e.GetAttribute("description_key"));
		}
		if (e.HasAttribute("default"))
		{
			twitchActionPreset.IsDefault = StringParsers.ParseBool(e.GetAttribute("default"));
		}
		if (e.HasAttribute("enabled"))
		{
			twitchActionPreset.IsEnabled = StringParsers.ParseBool(e.GetAttribute("enabled"));
		}
		if (e.HasAttribute("is_empty"))
		{
			twitchActionPreset.IsEmpty = StringParsers.ParseBool(e.GetAttribute("is_empty"));
		}
		if (e.HasAttribute("allow_point_generation"))
		{
			twitchActionPreset.AllowPointGeneration = StringParsers.ParseBool(e.GetAttribute("allow_point_generation"));
		}
		if (e.HasAttribute("use_helper_reward"))
		{
			twitchActionPreset.UseHelperReward = StringParsers.ParseBool(e.GetAttribute("use_helper_reward"));
		}
		if (e.HasAttribute("show_new_commands"))
		{
			twitchActionPreset.ShowNewCommands = StringParsers.ParseBool(e.GetAttribute("show_new_commands"));
		}
		foreach (XElement item in e.Elements())
		{
			if (item.Name == "cooldown_modifier")
			{
				ParseCooldownModifier(item, twitchActionPreset);
			}
		}
		TwitchManager.Current.AddTwitchActionPreset(twitchActionPreset);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseCooldownModifier(XElement e, TwitchActionPreset actionPreset)
	{
		TwitchActionCooldownModifier twitchActionCooldownModifier = new TwitchActionCooldownModifier();
		if (e.HasAttribute("category"))
		{
			twitchActionCooldownModifier.CategoryName = e.GetAttribute("category");
		}
		if (e.HasAttribute("action"))
		{
			twitchActionCooldownModifier.ActionName = e.GetAttribute("action");
		}
		if (e.HasAttribute("value"))
		{
			twitchActionCooldownModifier.Value = StringParsers.ParseFloat(e.GetAttribute("value"));
		}
		if (e.HasAttribute("modifier"))
		{
			twitchActionCooldownModifier.Modifier = EnumUtils.Parse<PassiveEffect.ValueModifierTypes>(e.GetAttribute("modifier"));
		}
		actionPreset.AddCooldownModifier(twitchActionCooldownModifier);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseVotesPreset(XElement e)
	{
		TwitchVotePreset twitchVotePreset = new TwitchVotePreset();
		if (e.HasAttribute("name"))
		{
			twitchVotePreset.Name = e.GetAttribute("name");
		}
		if (e.HasAttribute("title"))
		{
			twitchVotePreset.Title = e.GetAttribute("title");
		}
		if (e.HasAttribute("title_key"))
		{
			twitchVotePreset.Title = Localization.Get(e.GetAttribute("title_key"));
		}
		if (e.HasAttribute("description"))
		{
			twitchVotePreset.Description = e.GetAttribute("description");
		}
		if (e.HasAttribute("description_key"))
		{
			twitchVotePreset.Description = Localization.Get(e.GetAttribute("description_key"));
		}
		if (e.HasAttribute("default"))
		{
			twitchVotePreset.IsDefault = StringParsers.ParseBool(e.GetAttribute("default"));
		}
		if (e.HasAttribute("is_empty"))
		{
			twitchVotePreset.IsEmpty = StringParsers.ParseBool(e.GetAttribute("is_empty"));
		}
		if (e.HasAttribute("boss_vote_setting"))
		{
			twitchVotePreset.BossVoteSetting = (TwitchVotingManager.BossVoteSettings)Enum.Parse(typeof(TwitchVotingManager.BossVoteSettings), e.GetAttribute("boss_vote_setting"), ignoreCase: true);
		}
		TwitchManager.Current.AddTwitchVotePreset(twitchVotePreset);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseTwitchActions(XElement e)
	{
		DynamicProperties dynamicProperties = null;
		if (e.HasAttribute("extends"))
		{
			string attribute = e.GetAttribute("extends");
			if (TwitchActionManager.TwitchActions.ContainsKey(attribute))
			{
				dynamicProperties = HandleExtends(TwitchActionManager.TwitchActions[attribute] ?? throw new Exception($"Extends twitch action {attribute} is not specified.'"));
			}
		}
		TwitchAction twitchAction = new TwitchAction();
		foreach (XElement item in e.Elements())
		{
			if (item.Name == "property")
			{
				if (dynamicProperties == null)
				{
					dynamicProperties = new DynamicProperties();
				}
				dynamicProperties.Add(item);
			}
			else if (item.Name == "cooldown_addition")
			{
				ParseCooldownAddition(item, twitchAction, null);
			}
		}
		if (e.HasAttribute("name"))
		{
			twitchAction.Name = e.GetAttribute("name");
		}
		bool flag = true;
		if (dynamicProperties != null)
		{
			flag = twitchAction.ParseProperties(dynamicProperties);
		}
		twitchAction.Init();
		if (flag)
		{
			TwitchActionManager.Current.AddAction(twitchAction);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DynamicProperties HandleExtends(TwitchVote extendedClass)
	{
		if (extendedClass.Properties != null)
		{
			DynamicProperties dynamicProperties = new DynamicProperties();
			dynamicProperties.CopyFrom(extendedClass.Properties, TwitchVote.ExtendsExcludes);
			return dynamicProperties;
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseVoteEntry(XElement e)
	{
		DynamicProperties dynamicProperties = null;
		if (e.HasAttribute("extends"))
		{
			string attribute = e.GetAttribute("extends");
			if (TwitchActionManager.TwitchVotes.ContainsKey(attribute))
			{
				dynamicProperties = HandleExtends(TwitchActionManager.TwitchVotes[attribute] ?? throw new Exception($"Extends twitch vote {attribute} is not specified.'"));
			}
		}
		TwitchVote twitchVote = new TwitchVote();
		foreach (XElement item in e.Elements())
		{
			if (item.Name == "property")
			{
				if (dynamicProperties == null)
				{
					dynamicProperties = new DynamicProperties();
				}
				dynamicProperties.Add(item);
			}
			else if (item.Name == "cooldown_addition")
			{
				ParseCooldownAddition(item, null, twitchVote);
			}
			else if (item.Name == "requirement")
			{
				ParseVoteRequirement(item, twitchVote);
			}
		}
		twitchVote.VoteName = e.GetAttribute("name");
		if (dynamicProperties != null)
		{
			twitchVote.ParseProperties(dynamicProperties);
		}
		TwitchActionManager.Current.AddVoteClass(twitchVote);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseCommandPermission(XElement e)
	{
		if (e.HasAttribute("command") && e.HasAttribute("permission"))
		{
			string attribute = e.GetAttribute("command");
			BaseTwitchCommand.PermissionLevels permissionLevel = Enum.Parse<BaseTwitchCommand.PermissionLevels>(e.GetAttribute("permission"));
			BaseTwitchCommand.AddCommandPermissionOverride(attribute, permissionLevel);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseVoteTypeEntry(XElement e)
	{
		DynamicProperties dynamicProperties = null;
		foreach (XElement item in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(item);
		}
		TwitchVoteType twitchVoteType = new TwitchVoteType();
		twitchVoteType.Name = e.GetAttribute("name");
		if (dynamicProperties != null)
		{
			twitchVoteType.ParseProperties(dynamicProperties);
		}
		TwitchManager.Current.VotingManager.AddVoteType(twitchVoteType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseCooldownPreset(XElement e)
	{
		CooldownPreset cooldownPreset = new CooldownPreset();
		cooldownPreset.Name = e.GetAttribute("name");
		DynamicProperties dynamicProperties = null;
		foreach (XElement item in e.Elements())
		{
			if (item.Name == "property")
			{
				if (dynamicProperties == null)
				{
					dynamicProperties = new DynamicProperties();
				}
				dynamicProperties.Add(item);
			}
			else if (item.Name == "cooldown_entry")
			{
				ParseCooldownEntry(item, cooldownPreset);
			}
		}
		if (dynamicProperties != null)
		{
			cooldownPreset.ParseProperties(dynamicProperties);
		}
		TwitchManager.Current.AddCooldownPreset(cooldownPreset);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseCooldownEntry(XElement e, CooldownPreset preset)
	{
		int _result = -1;
		int _result2 = -1;
		int _result3 = -1;
		int _result4 = 180;
		if (e.HasAttribute("start"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("start"), out _result);
		}
		if (e.HasAttribute("end"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("end"), out _result2);
		}
		if (e.HasAttribute("cooldown_max"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("cooldown_max"), out _result3);
		}
		if (e.HasAttribute("cooldown_max"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("cooldown_max"), out _result3);
		}
		if (e.HasAttribute("cooldown_time"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("cooldown_time"), out _result4);
		}
		if (_result3 != -1)
		{
			preset.AddCooldownMaxEntry(_result, _result2, _result3, _result4);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseRandomGroup(XElement e)
	{
		string text = "";
		int _result = -1;
		if (e.HasAttribute("random_count"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("random_count"), out _result);
		}
		if (e.HasAttribute("name"))
		{
			text = e.GetAttribute("name");
		}
		if (text != "")
		{
			TwitchManager.Current.AddRandomGroup(text, _result);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseCooldownAddition(XElement e, TwitchAction action, TwitchVote vote)
	{
		string text = "";
		int _result = 0;
		bool isAction = true;
		if (e.HasAttribute("name"))
		{
			text = e.GetAttribute("name");
		}
		if (e.HasAttribute("time"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("time"), out _result);
		}
		if (e.HasAttribute("is_action"))
		{
			isAction = StringParsers.ParseBool(e.GetAttribute("is_action"));
		}
		if (text != "" && _result != 0)
		{
			if (action != null)
			{
				action.AddCooldownAddition(new TwitchActionCooldownAddition
				{
					ActionName = text,
					CooldownTime = _result,
					IsAction = isAction
				});
			}
			else
			{
				vote?.AddCooldownAddition(new TwitchActionCooldownAddition
				{
					ActionName = text,
					CooldownTime = _result,
					IsAction = isAction
				});
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseVoteRequirement(XElement e, TwitchVote vote)
	{
		DynamicProperties dynamicProperties = null;
		foreach (XElement item in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(item);
		}
		string text = "";
		if (e.HasAttribute("class"))
		{
			text = e.GetAttribute("class");
		}
		else
		{
			if (!dynamicProperties.Contains("class"))
			{
				throw new Exception("Game Event Action Requirement must have a class!");
			}
			text = dynamicProperties.Values["class"];
		}
		BaseTwitchVoteRequirement baseTwitchVoteRequirement = null;
		text = $"Twitch.TwitchVoteRequirement{text}";
		try
		{
			baseTwitchVoteRequirement = (BaseTwitchVoteRequirement)Activator.CreateInstance(Type.GetType(text));
		}
		catch (Exception)
		{
			throw new Exception("No twitch vote requirement class '" + text + " found!");
		}
		if (dynamicProperties != null)
		{
			baseTwitchVoteRequirement.ParseProperties(dynamicProperties);
		}
		baseTwitchVoteRequirement.Init();
		vote.AddVoteRequirement(baseTwitchVoteRequirement);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseEventsPreset(XElement e, string nodeName)
	{
		TwitchEventPreset twitchEventPreset = new TwitchEventPreset();
		if (e.HasAttribute("name"))
		{
			twitchEventPreset.Name = e.GetAttribute("name");
		}
		if (e.HasAttribute("title"))
		{
			twitchEventPreset.Title = e.GetAttribute("title");
		}
		if (e.HasAttribute("title_key"))
		{
			twitchEventPreset.Title = Localization.Get(e.GetAttribute("title_key"));
		}
		if (e.HasAttribute("description"))
		{
			twitchEventPreset.Description = e.GetAttribute("description");
		}
		if (e.HasAttribute("description_key"))
		{
			twitchEventPreset.Description = Localization.Get(e.GetAttribute("description_key"));
		}
		if (e.HasAttribute("default"))
		{
			twitchEventPreset.IsDefault = StringParsers.ParseBool(e.GetAttribute("default"));
		}
		if (e.HasAttribute("is_empty"))
		{
			twitchEventPreset.IsEmpty = StringParsers.ParseBool(e.GetAttribute("is_empty"));
		}
		TwitchManager.Current.AddTwitchEventPreset(twitchEventPreset);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseEvent(XElement e, string nodeName)
	{
		TwitchEventEntry twitchEventEntry = new TwitchEventEntry();
		if (e.HasAttribute("start_amount"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("start_amount"), out twitchEventEntry.StartAmount);
		}
		if (e.HasAttribute("end_amount"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("end_amount"), out twitchEventEntry.EndAmount);
		}
		if (e.HasAttribute("event_name"))
		{
			twitchEventEntry.EventName = e.GetAttribute("event_name");
		}
		if (e.HasAttribute("event_title"))
		{
			twitchEventEntry.EventTitle = e.GetAttribute("event_title");
		}
		if (e.HasAttribute("event_title_key"))
		{
			twitchEventEntry.EventTitle = Localization.Get(e.GetAttribute("event_title_key"));
		}
		if (e.HasAttribute("safe_allowed"))
		{
			twitchEventEntry.SafeAllowed = StringParsers.ParseBool(e.GetAttribute("safe_allowed"));
		}
		if (e.HasAttribute("cooldown_allowed"))
		{
			twitchEventEntry.CooldownAllowed = StringParsers.ParseBool(e.GetAttribute("cooldown_allowed"));
		}
		if (e.HasAttribute("vote_allowed"))
		{
			twitchEventEntry.VoteEventAllowed = StringParsers.ParseBool(e.GetAttribute("vote_allowed"));
		}
		if (e.HasAttribute("pp_add_amount"))
		{
			twitchEventEntry.PPAmount = StringParsers.ParseSInt32(e.GetAttribute("pp_add_amount"));
		}
		if (e.HasAttribute("sp_add_amount"))
		{
			twitchEventEntry.SPAmount = StringParsers.ParseSInt32(e.GetAttribute("sp_add_amount"));
		}
		if (e.HasAttribute("pimp_pot_add"))
		{
			twitchEventEntry.PimpPotAdd = StringParsers.ParseSInt32(e.GetAttribute("pimp_pot_add"));
		}
		if (e.HasAttribute("bit_pot_add"))
		{
			twitchEventEntry.BitPotAdd = StringParsers.ParseSInt32(e.GetAttribute("bit_pot_add"));
		}
		if (e.HasAttribute("cooldown_add"))
		{
			twitchEventEntry.CooldownAdd = StringParsers.ParseSInt32(e.GetAttribute("cooldown_add"));
		}
		string text = "";
		if (e.HasAttribute("presets"))
		{
			text = e.GetAttribute("presets");
		}
		if (text == "")
		{
			return;
		}
		string[] array = text.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			TwitchEventPreset eventPreset = TwitchManager.Current.GetEventPreset(array[i]);
			if (eventPreset != null && (twitchEventEntry.EventName != "" || twitchEventEntry.SPAmount > 0 || twitchEventEntry.PPAmount > 0 || twitchEventEntry.PimpPotAdd > 0 || twitchEventEntry.BitPotAdd > 0 || twitchEventEntry.CooldownAdd > 0))
			{
				switch (nodeName)
				{
				case "bit_event":
					twitchEventEntry.EventType = BaseTwitchEventEntry.EventTypes.Bits;
					eventPreset.AddBitEvent(twitchEventEntry);
					break;
				case "raid_event":
					twitchEventEntry.EventType = BaseTwitchEventEntry.EventTypes.Raid;
					eventPreset.AddRaidEvent(twitchEventEntry);
					break;
				case "charity_event":
					twitchEventEntry.EventType = BaseTwitchEventEntry.EventTypes.Charity;
					eventPreset.AddCharityEvent(twitchEventEntry);
					break;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseHypeTrainEvent(XElement e, string nodeName)
	{
		TwitchHypeTrainEventEntry twitchHypeTrainEventEntry = new TwitchHypeTrainEventEntry();
		if (e.HasAttribute("start_amount"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("start_amount"), out twitchHypeTrainEventEntry.StartAmount);
		}
		if (e.HasAttribute("end_amount"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("end_amount"), out twitchHypeTrainEventEntry.EndAmount);
		}
		if (e.HasAttribute("event_name"))
		{
			twitchHypeTrainEventEntry.EventName = e.GetAttribute("event_name");
		}
		if (e.HasAttribute("event_title"))
		{
			twitchHypeTrainEventEntry.EventTitle = e.GetAttribute("event_title");
		}
		if (e.HasAttribute("event_title_key"))
		{
			twitchHypeTrainEventEntry.EventTitle = Localization.Get(e.GetAttribute("event_title_key"));
		}
		if (e.HasAttribute("safe_allowed"))
		{
			twitchHypeTrainEventEntry.SafeAllowed = StringParsers.ParseBool(e.GetAttribute("safe_allowed"));
		}
		if (e.HasAttribute("starting_cooldown_allowed"))
		{
			twitchHypeTrainEventEntry.StartingCooldownAllowed = StringParsers.ParseBool(e.GetAttribute("starting_cooldown_allowed"));
		}
		if (e.HasAttribute("cooldown_allowed"))
		{
			twitchHypeTrainEventEntry.CooldownAllowed = StringParsers.ParseBool(e.GetAttribute("cooldown_allowed"));
		}
		if (e.HasAttribute("vote_allowed"))
		{
			twitchHypeTrainEventEntry.VoteEventAllowed = StringParsers.ParseBool(e.GetAttribute("vote_allowed"));
		}
		if (e.HasAttribute("pp_add_amount"))
		{
			twitchHypeTrainEventEntry.PPAmount = StringParsers.ParseSInt32(e.GetAttribute("pp_add_amount"));
		}
		if (e.HasAttribute("sp_add_amount"))
		{
			twitchHypeTrainEventEntry.SPAmount = StringParsers.ParseSInt32(e.GetAttribute("sp_add_amount"));
		}
		if (e.HasAttribute("pimp_pot_add"))
		{
			twitchHypeTrainEventEntry.PimpPotAdd = StringParsers.ParseSInt32(e.GetAttribute("pimp_pot_add"));
		}
		if (e.HasAttribute("bit_pot_add"))
		{
			twitchHypeTrainEventEntry.BitPotAdd = StringParsers.ParseSInt32(e.GetAttribute("bit_pot_add"));
		}
		if (e.HasAttribute("cooldown_add"))
		{
			twitchHypeTrainEventEntry.CooldownAdd = StringParsers.ParseSInt32(e.GetAttribute("cooldown_add"));
		}
		if (e.HasAttribute("reward_amount"))
		{
			twitchHypeTrainEventEntry.RewardAmount = StringParsers.ParseSInt32(e.GetAttribute("reward_amount"));
		}
		if (e.HasAttribute("reward_type"))
		{
			twitchHypeTrainEventEntry.RewardType = (TwitchAction.PointTypes)Enum.Parse(typeof(TwitchAction.PointTypes), e.GetAttribute("reward_type"));
		}
		string text = "";
		if (e.HasAttribute("presets"))
		{
			text = e.GetAttribute("presets");
		}
		if (text == "")
		{
			return;
		}
		string[] array = text.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			TwitchEventPreset eventPreset = TwitchManager.Current.GetEventPreset(array[i]);
			if (eventPreset != null && (twitchHypeTrainEventEntry.EventName != "" || twitchHypeTrainEventEntry.SPAmount > 0 || twitchHypeTrainEventEntry.PPAmount > 0 || twitchHypeTrainEventEntry.PimpPotAdd > 0 || twitchHypeTrainEventEntry.BitPotAdd > 0 || twitchHypeTrainEventEntry.CooldownAdd > 0))
			{
				twitchHypeTrainEventEntry.EventType = BaseTwitchEventEntry.EventTypes.HypeTrain;
				eventPreset.AddHypeTrainEvent(twitchHypeTrainEventEntry);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseCreatorGoalEvent(XElement e, string nodeName)
	{
		TwitchCreatorGoalEventEntry twitchCreatorGoalEventEntry = new TwitchCreatorGoalEventEntry();
		if (e.HasAttribute("goal_type"))
		{
			twitchCreatorGoalEventEntry.GoalType = e.GetAttribute("goal_type").ToLower();
		}
		else
		{
			twitchCreatorGoalEventEntry.GoalType = "Subs";
		}
		if (e.HasAttribute("event_name"))
		{
			twitchCreatorGoalEventEntry.EventName = e.GetAttribute("event_name");
		}
		if (e.HasAttribute("event_title"))
		{
			twitchCreatorGoalEventEntry.EventTitle = e.GetAttribute("event_title");
		}
		if (e.HasAttribute("event_title_key"))
		{
			twitchCreatorGoalEventEntry.EventTitle = Localization.Get(e.GetAttribute("event_title_key"));
		}
		if (e.HasAttribute("safe_allowed"))
		{
			twitchCreatorGoalEventEntry.SafeAllowed = StringParsers.ParseBool(e.GetAttribute("safe_allowed"));
		}
		if (e.HasAttribute("starting_cooldown_allowed"))
		{
			twitchCreatorGoalEventEntry.StartingCooldownAllowed = StringParsers.ParseBool(e.GetAttribute("starting_cooldown_allowed"));
		}
		if (e.HasAttribute("cooldown_allowed"))
		{
			twitchCreatorGoalEventEntry.CooldownAllowed = StringParsers.ParseBool(e.GetAttribute("cooldown_allowed"));
		}
		if (e.HasAttribute("vote_allowed"))
		{
			twitchCreatorGoalEventEntry.VoteEventAllowed = StringParsers.ParseBool(e.GetAttribute("vote_allowed"));
		}
		if (e.HasAttribute("pp_add_amount"))
		{
			twitchCreatorGoalEventEntry.PPAmount = StringParsers.ParseSInt32(e.GetAttribute("pp_add_amount"));
		}
		if (e.HasAttribute("sp_add_amount"))
		{
			twitchCreatorGoalEventEntry.SPAmount = StringParsers.ParseSInt32(e.GetAttribute("sp_add_amount"));
		}
		if (e.HasAttribute("pimp_pot_add"))
		{
			twitchCreatorGoalEventEntry.PimpPotAdd = StringParsers.ParseSInt32(e.GetAttribute("pimp_pot_add"));
		}
		if (e.HasAttribute("bit_pot_add"))
		{
			twitchCreatorGoalEventEntry.BitPotAdd = StringParsers.ParseSInt32(e.GetAttribute("bit_pot_add"));
		}
		if (e.HasAttribute("cooldown_add"))
		{
			twitchCreatorGoalEventEntry.CooldownAdd = StringParsers.ParseSInt32(e.GetAttribute("cooldown_add"));
		}
		if (e.HasAttribute("reward_amount"))
		{
			twitchCreatorGoalEventEntry.RewardAmount = StringParsers.ParseSInt32(e.GetAttribute("reward_amount"));
		}
		if (e.HasAttribute("reward_type"))
		{
			twitchCreatorGoalEventEntry.RewardType = (TwitchAction.PointTypes)Enum.Parse(typeof(TwitchAction.PointTypes), e.GetAttribute("reward_type"));
		}
		string text = "";
		if (e.HasAttribute("presets"))
		{
			text = e.GetAttribute("presets");
		}
		if (text == "")
		{
			return;
		}
		string[] array = text.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			TwitchEventPreset eventPreset = TwitchManager.Current.GetEventPreset(array[i]);
			if (eventPreset != null && (twitchCreatorGoalEventEntry.EventName != "" || twitchCreatorGoalEventEntry.SPAmount > 0 || twitchCreatorGoalEventEntry.PPAmount > 0 || twitchCreatorGoalEventEntry.PimpPotAdd > 0 || twitchCreatorGoalEventEntry.BitPotAdd > 0 || twitchCreatorGoalEventEntry.CooldownAdd > 0))
			{
				twitchCreatorGoalEventEntry.EventType = BaseTwitchEventEntry.EventTypes.CreatorGoal;
				eventPreset.AddCreatorGoalEvent(twitchCreatorGoalEventEntry);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseSubEvent(XElement e, string nodeName)
	{
		TwitchSubEventEntry twitchSubEventEntry = new TwitchSubEventEntry();
		if (e.HasAttribute("start_amount"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("start_amount"), out twitchSubEventEntry.StartAmount);
		}
		if (e.HasAttribute("end_amount"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("end_amount"), out twitchSubEventEntry.EndAmount);
		}
		if (e.HasAttribute("event_name"))
		{
			twitchSubEventEntry.EventName = e.GetAttribute("event_name");
		}
		if (e.HasAttribute("event_title"))
		{
			twitchSubEventEntry.EventTitle = e.GetAttribute("event_title");
		}
		if (e.HasAttribute("event_title_key"))
		{
			twitchSubEventEntry.EventTitle = Localization.Get(e.GetAttribute("event_title_key"));
		}
		if (e.HasAttribute("safe_allowed"))
		{
			twitchSubEventEntry.SafeAllowed = StringParsers.ParseBool(e.GetAttribute("safe_allowed"));
		}
		if (e.HasAttribute("cooldown_allowed"))
		{
			twitchSubEventEntry.CooldownAllowed = StringParsers.ParseBool(e.GetAttribute("cooldown_allowed"));
		}
		if (e.HasAttribute("vote_allowed"))
		{
			twitchSubEventEntry.VoteEventAllowed = StringParsers.ParseBool(e.GetAttribute("vote_allowed"));
		}
		if (e.HasAttribute("rewards_bit_pot"))
		{
			twitchSubEventEntry.RewardsBitPot = StringParsers.ParseBool(e.GetAttribute("rewards_bit_pot"));
		}
		if (e.HasAttribute("pp_add_amount"))
		{
			twitchSubEventEntry.PPAmount = StringParsers.ParseSInt32(e.GetAttribute("pp_add_amount"));
		}
		if (e.HasAttribute("sp_add_amount"))
		{
			twitchSubEventEntry.SPAmount = StringParsers.ParseSInt32(e.GetAttribute("sp_add_amount"));
		}
		if (e.HasAttribute("pimp_pot_add"))
		{
			twitchSubEventEntry.PimpPotAdd = StringParsers.ParseSInt32(e.GetAttribute("pimp_pot_add"));
		}
		if (e.HasAttribute("bit_pot_add"))
		{
			twitchSubEventEntry.BitPotAdd = StringParsers.ParseSInt32(e.GetAttribute("bit_pot_add"));
		}
		if (e.HasAttribute("cooldown_add"))
		{
			twitchSubEventEntry.CooldownAdd = StringParsers.ParseSInt32(e.GetAttribute("cooldown_add"));
		}
		if (e.HasAttribute("sub_tier"))
		{
			twitchSubEventEntry.SubTier = (TwitchSubEventEntry.SubTierTypes)Enum.Parse(typeof(TwitchSubEventEntry.SubTierTypes), e.GetAttribute("sub_tier"));
		}
		string text = "";
		if (e.HasAttribute("presets"))
		{
			text = e.GetAttribute("presets");
		}
		if (text == "")
		{
			return;
		}
		string[] array = text.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			TwitchEventPreset eventPreset = TwitchManager.Current.GetEventPreset(array[i]);
			if (eventPreset != null && (twitchSubEventEntry.EventName != "" || twitchSubEventEntry.SPAmount > 0 || twitchSubEventEntry.PPAmount > 0 || twitchSubEventEntry.PimpPotAdd > 0 || twitchSubEventEntry.BitPotAdd > 0 || twitchSubEventEntry.CooldownAdd > 0))
			{
				if (nodeName == "sub_event")
				{
					twitchSubEventEntry.EventType = BaseTwitchEventEntry.EventTypes.Subs;
					eventPreset.AddSubEvent(twitchSubEventEntry);
				}
				else if (nodeName == "gift_sub_event")
				{
					twitchSubEventEntry.EventType = BaseTwitchEventEntry.EventTypes.GiftSubs;
					eventPreset.AddGiftSubEvent(twitchSubEventEntry);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseChannelPointEvent(XElement e)
	{
		TwitchChannelPointEventEntry twitchChannelPointEventEntry = new TwitchChannelPointEventEntry();
		if (e.HasAttribute("channel_point_title"))
		{
			twitchChannelPointEventEntry.ChannelPointTitle = e.GetAttribute("channel_point_title");
		}
		if (e.HasAttribute("channel_point_title_key"))
		{
			twitchChannelPointEventEntry.ChannelPointTitle = Localization.Get(e.GetAttribute("channel_point_title_key"));
		}
		if (e.HasAttribute("event_name"))
		{
			twitchChannelPointEventEntry.EventName = e.GetAttribute("event_name");
		}
		if (e.HasAttribute("event_title"))
		{
			twitchChannelPointEventEntry.EventTitle = e.GetAttribute("event_title");
		}
		if (e.HasAttribute("event_title_key"))
		{
			twitchChannelPointEventEntry.EventTitle = Localization.Get(e.GetAttribute("event_title_key"));
		}
		if (e.HasAttribute("pp_add_amount"))
		{
			twitchChannelPointEventEntry.PPAmount = StringParsers.ParseSInt32(e.GetAttribute("pp_add_amount"));
		}
		if (e.HasAttribute("sp_add_amount"))
		{
			twitchChannelPointEventEntry.SPAmount = StringParsers.ParseSInt32(e.GetAttribute("sp_add_amount"));
		}
		if (e.HasAttribute("safe_allowed"))
		{
			twitchChannelPointEventEntry.SafeAllowed = StringParsers.ParseBool(e.GetAttribute("safe_allowed"));
		}
		if (e.HasAttribute("cooldown_allowed"))
		{
			twitchChannelPointEventEntry.CooldownAllowed = StringParsers.ParseBool(e.GetAttribute("cooldown_allowed"));
		}
		if (e.HasAttribute("vote_allowed"))
		{
			twitchChannelPointEventEntry.VoteEventAllowed = StringParsers.ParseBool(e.GetAttribute("vote_allowed"));
		}
		if (e.HasAttribute("pimp_pot_add"))
		{
			twitchChannelPointEventEntry.PimpPotAdd = StringParsers.ParseSInt32(e.GetAttribute("pimp_pot_add"));
		}
		if (e.HasAttribute("bit_pot_add"))
		{
			twitchChannelPointEventEntry.BitPotAdd = StringParsers.ParseSInt32(e.GetAttribute("bit_pot_add"));
		}
		if (e.HasAttribute("cooldown_add"))
		{
			twitchChannelPointEventEntry.CooldownAdd = StringParsers.ParseSInt32(e.GetAttribute("cooldown_add"));
		}
		if (e.HasAttribute("cost"))
		{
			twitchChannelPointEventEntry.Cost = StringParsers.ParseSInt32(e.GetAttribute("cost"));
		}
		if (e.HasAttribute("max_per_stream"))
		{
			twitchChannelPointEventEntry.MaxPerStream = StringParsers.ParseSInt32(e.GetAttribute("max_per_stream"));
		}
		if (e.HasAttribute("max_per_user_per_stream"))
		{
			twitchChannelPointEventEntry.MaxPerUserPerStream = StringParsers.ParseSInt32(e.GetAttribute("max_per_user_per_stream"));
		}
		if (e.HasAttribute("global_cooldown"))
		{
			twitchChannelPointEventEntry.GlobalCooldown = StringParsers.ParseSInt32(e.GetAttribute("global_cooldown"));
		}
		if (e.HasAttribute("auto_create"))
		{
			twitchChannelPointEventEntry.AutoCreate = StringParsers.ParseBool(e.GetAttribute("auto_create"));
		}
		string text = "";
		if (e.HasAttribute("presets"))
		{
			text = e.GetAttribute("presets");
		}
		if (text == "")
		{
			return;
		}
		string[] array = text.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			TwitchEventPreset eventPreset = TwitchManager.Current.GetEventPreset(array[i]);
			if (eventPreset != null && (twitchChannelPointEventEntry.EventName != "" || twitchChannelPointEventEntry.SPAmount > 0 || twitchChannelPointEventEntry.PPAmount > 0 || twitchChannelPointEventEntry.PimpPotAdd > 0 || twitchChannelPointEventEntry.BitPotAdd > 0 || twitchChannelPointEventEntry.CooldownAdd > 0))
			{
				twitchChannelPointEventEntry.EventType = BaseTwitchEventEntry.EventTypes.ChannelPoints;
				eventPreset.AddChannelPointEvent(twitchChannelPointEventEntry);
			}
		}
	}
}
