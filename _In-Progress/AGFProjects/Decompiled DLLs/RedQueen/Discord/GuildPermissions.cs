using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal struct GuildPermissions
{
	public static readonly GuildPermissions None;

	public static readonly GuildPermissions Webhook;

	public static readonly GuildPermissions All;

	public ulong RawValue
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public bool CreateInstantInvite => Permissions.GetValue(RawValue, GuildPermission.CreateInstantInvite);

	public bool BanMembers => Permissions.GetValue(RawValue, GuildPermission.BanMembers);

	public bool KickMembers => Permissions.GetValue(RawValue, GuildPermission.KickMembers);

	public bool Administrator => Permissions.GetValue(RawValue, GuildPermission.Administrator);

	public bool ManageChannels => Permissions.GetValue(RawValue, GuildPermission.ManageChannels);

	public bool ManageGuild => Permissions.GetValue(RawValue, GuildPermission.ManageGuild);

	public bool AddReactions => Permissions.GetValue(RawValue, GuildPermission.AddReactions);

	public bool ViewAuditLog => Permissions.GetValue(RawValue, GuildPermission.ViewAuditLog);

	public bool ViewGuildInsights => Permissions.GetValue(RawValue, GuildPermission.ViewGuildInsights);

	public bool ViewChannel => Permissions.GetValue(RawValue, GuildPermission.ViewChannel);

	public bool SendMessages => Permissions.GetValue(RawValue, GuildPermission.SendMessages);

	public bool SendTTSMessages => Permissions.GetValue(RawValue, GuildPermission.SendTTSMessages);

	public bool ManageMessages => Permissions.GetValue(RawValue, GuildPermission.ManageMessages);

	public bool EmbedLinks => Permissions.GetValue(RawValue, GuildPermission.EmbedLinks);

	public bool AttachFiles => Permissions.GetValue(RawValue, GuildPermission.AttachFiles);

	public bool ReadMessageHistory => Permissions.GetValue(RawValue, GuildPermission.ReadMessageHistory);

	public bool MentionEveryone => Permissions.GetValue(RawValue, GuildPermission.MentionEveryone);

	public bool UseExternalEmojis => Permissions.GetValue(RawValue, GuildPermission.UseExternalEmojis);

	public bool Connect => Permissions.GetValue(RawValue, GuildPermission.Connect);

	public bool Speak => Permissions.GetValue(RawValue, GuildPermission.Speak);

	public bool MuteMembers => Permissions.GetValue(RawValue, GuildPermission.MuteMembers);

	public bool DeafenMembers => Permissions.GetValue(RawValue, GuildPermission.DeafenMembers);

	public bool MoveMembers => Permissions.GetValue(RawValue, GuildPermission.MoveMembers);

	public bool UseVAD => Permissions.GetValue(RawValue, GuildPermission.UseVAD);

	public bool PrioritySpeaker => Permissions.GetValue(RawValue, GuildPermission.PrioritySpeaker);

	public bool Stream => Permissions.GetValue(RawValue, GuildPermission.Stream);

	public bool ChangeNickname => Permissions.GetValue(RawValue, GuildPermission.ChangeNickname);

	public bool ManageNicknames => Permissions.GetValue(RawValue, GuildPermission.ManageNicknames);

	public bool ManageRoles => Permissions.GetValue(RawValue, GuildPermission.ManageRoles);

	public bool ManageWebhooks => Permissions.GetValue(RawValue, GuildPermission.ManageWebhooks);

	public bool ManageEmojisAndStickers => Permissions.GetValue(RawValue, GuildPermission.ManageEmojisAndStickers);

	public bool UseApplicationCommands => Permissions.GetValue(RawValue, GuildPermission.UseApplicationCommands);

	public bool RequestToSpeak => Permissions.GetValue(RawValue, GuildPermission.RequestToSpeak);

	public bool ManageEvents => Permissions.GetValue(RawValue, GuildPermission.ManageEvents);

	public bool ManageThreads => Permissions.GetValue(RawValue, GuildPermission.ManageThreads);

	public bool CreatePublicThreads => Permissions.GetValue(RawValue, GuildPermission.CreatePublicThreads);

	public bool CreatePrivateThreads => Permissions.GetValue(RawValue, GuildPermission.CreatePrivateThreads);

	public bool UseExternalStickers => Permissions.GetValue(RawValue, GuildPermission.UseExternalStickers);

	public bool SendMessagesInThreads => Permissions.GetValue(RawValue, GuildPermission.SendMessagesInThreads);

	public bool StartEmbeddedActivities => Permissions.GetValue(RawValue, GuildPermission.StartEmbeddedActivities);

	public bool ModerateMembers => Permissions.GetValue(RawValue, GuildPermission.ModerateMembers);

	private string DebuggerDisplay => string.Join(", ", ToList()) ?? "";

	public GuildPermissions(ulong rawValue)
	{
		RawValue = rawValue;
	}

	public GuildPermissions(string rawValue)
	{
		RawValue = ulong.Parse(rawValue);
	}

	private GuildPermissions(ulong initialValue, bool? createInstantInvite = null, bool? kickMembers = null, bool? banMembers = null, bool? administrator = null, bool? manageChannels = null, bool? manageGuild = null, bool? addReactions = null, bool? viewAuditLog = null, bool? viewGuildInsights = null, bool? viewChannel = null, bool? sendMessages = null, bool? sendTTSMessages = null, bool? manageMessages = null, bool? embedLinks = null, bool? attachFiles = null, bool? readMessageHistory = null, bool? mentionEveryone = null, bool? useExternalEmojis = null, bool? connect = null, bool? speak = null, bool? muteMembers = null, bool? deafenMembers = null, bool? moveMembers = null, bool? useVoiceActivation = null, bool? prioritySpeaker = null, bool? stream = null, bool? changeNickname = null, bool? manageNicknames = null, bool? manageRoles = null, bool? manageWebhooks = null, bool? manageEmojisAndStickers = null, bool? useApplicationCommands = null, bool? requestToSpeak = null, bool? manageEvents = null, bool? manageThreads = null, bool? createPublicThreads = null, bool? createPrivateThreads = null, bool? useExternalStickers = null, bool? sendMessagesInThreads = null, bool? startEmbeddedActivities = null, bool? moderateMembers = null)
	{
		ulong rawValue = initialValue;
		Permissions.SetValue(ref rawValue, createInstantInvite, GuildPermission.CreateInstantInvite);
		Permissions.SetValue(ref rawValue, banMembers, GuildPermission.BanMembers);
		Permissions.SetValue(ref rawValue, kickMembers, GuildPermission.KickMembers);
		Permissions.SetValue(ref rawValue, administrator, GuildPermission.Administrator);
		Permissions.SetValue(ref rawValue, manageChannels, GuildPermission.ManageChannels);
		Permissions.SetValue(ref rawValue, manageGuild, GuildPermission.ManageGuild);
		Permissions.SetValue(ref rawValue, addReactions, GuildPermission.AddReactions);
		Permissions.SetValue(ref rawValue, viewAuditLog, GuildPermission.ViewAuditLog);
		Permissions.SetValue(ref rawValue, viewGuildInsights, GuildPermission.ViewGuildInsights);
		Permissions.SetValue(ref rawValue, viewChannel, GuildPermission.ViewChannel);
		Permissions.SetValue(ref rawValue, sendMessages, GuildPermission.SendMessages);
		Permissions.SetValue(ref rawValue, sendTTSMessages, GuildPermission.SendTTSMessages);
		Permissions.SetValue(ref rawValue, manageMessages, GuildPermission.ManageMessages);
		Permissions.SetValue(ref rawValue, embedLinks, GuildPermission.EmbedLinks);
		Permissions.SetValue(ref rawValue, attachFiles, GuildPermission.AttachFiles);
		Permissions.SetValue(ref rawValue, readMessageHistory, GuildPermission.ReadMessageHistory);
		Permissions.SetValue(ref rawValue, mentionEveryone, GuildPermission.MentionEveryone);
		Permissions.SetValue(ref rawValue, useExternalEmojis, GuildPermission.UseExternalEmojis);
		Permissions.SetValue(ref rawValue, connect, GuildPermission.Connect);
		Permissions.SetValue(ref rawValue, speak, GuildPermission.Speak);
		Permissions.SetValue(ref rawValue, muteMembers, GuildPermission.MuteMembers);
		Permissions.SetValue(ref rawValue, deafenMembers, GuildPermission.DeafenMembers);
		Permissions.SetValue(ref rawValue, moveMembers, GuildPermission.MoveMembers);
		Permissions.SetValue(ref rawValue, useVoiceActivation, GuildPermission.UseVAD);
		Permissions.SetValue(ref rawValue, prioritySpeaker, GuildPermission.PrioritySpeaker);
		Permissions.SetValue(ref rawValue, stream, GuildPermission.Stream);
		Permissions.SetValue(ref rawValue, changeNickname, GuildPermission.ChangeNickname);
		Permissions.SetValue(ref rawValue, manageNicknames, GuildPermission.ManageNicknames);
		Permissions.SetValue(ref rawValue, manageRoles, GuildPermission.ManageRoles);
		Permissions.SetValue(ref rawValue, manageWebhooks, GuildPermission.ManageWebhooks);
		Permissions.SetValue(ref rawValue, manageEmojisAndStickers, GuildPermission.ManageEmojisAndStickers);
		Permissions.SetValue(ref rawValue, useApplicationCommands, GuildPermission.UseApplicationCommands);
		Permissions.SetValue(ref rawValue, requestToSpeak, GuildPermission.RequestToSpeak);
		Permissions.SetValue(ref rawValue, manageEvents, GuildPermission.ManageEvents);
		Permissions.SetValue(ref rawValue, manageThreads, GuildPermission.ManageThreads);
		Permissions.SetValue(ref rawValue, createPublicThreads, GuildPermission.CreatePublicThreads);
		Permissions.SetValue(ref rawValue, createPrivateThreads, GuildPermission.CreatePrivateThreads);
		Permissions.SetValue(ref rawValue, useExternalStickers, GuildPermission.UseExternalStickers);
		Permissions.SetValue(ref rawValue, sendMessagesInThreads, GuildPermission.SendMessagesInThreads);
		Permissions.SetValue(ref rawValue, startEmbeddedActivities, GuildPermission.StartEmbeddedActivities);
		Permissions.SetValue(ref rawValue, moderateMembers, GuildPermission.ModerateMembers);
		RawValue = rawValue;
	}

	public GuildPermissions(bool createInstantInvite = false, bool kickMembers = false, bool banMembers = false, bool administrator = false, bool manageChannels = false, bool manageGuild = false, bool addReactions = false, bool viewAuditLog = false, bool viewGuildInsights = false, bool viewChannel = false, bool sendMessages = false, bool sendTTSMessages = false, bool manageMessages = false, bool embedLinks = false, bool attachFiles = false, bool readMessageHistory = false, bool mentionEveryone = false, bool useExternalEmojis = false, bool connect = false, bool speak = false, bool muteMembers = false, bool deafenMembers = false, bool moveMembers = false, bool useVoiceActivation = false, bool prioritySpeaker = false, bool stream = false, bool changeNickname = false, bool manageNicknames = false, bool manageRoles = false, bool manageWebhooks = false, bool manageEmojisAndStickers = false, bool useApplicationCommands = false, bool requestToSpeak = false, bool manageEvents = false, bool manageThreads = false, bool createPublicThreads = false, bool createPrivateThreads = false, bool useExternalStickers = false, bool sendMessagesInThreads = false, bool startEmbeddedActivities = false, bool moderateMembers = false)
	{
		this = new GuildPermissions(0uL, createInstantInvite, manageRoles: manageRoles, kickMembers: kickMembers, banMembers: banMembers, administrator: administrator, manageChannels: manageChannels, manageGuild: manageGuild, addReactions: addReactions, viewAuditLog: viewAuditLog, viewGuildInsights: viewGuildInsights, viewChannel: viewChannel, sendMessages: sendMessages, sendTTSMessages: sendTTSMessages, manageMessages: manageMessages, embedLinks: embedLinks, attachFiles: attachFiles, readMessageHistory: readMessageHistory, mentionEveryone: mentionEveryone, useExternalEmojis: useExternalEmojis, connect: connect, speak: speak, muteMembers: muteMembers, deafenMembers: deafenMembers, moveMembers: moveMembers, useVoiceActivation: useVoiceActivation, prioritySpeaker: prioritySpeaker, stream: stream, changeNickname: changeNickname, manageNicknames: manageNicknames, manageWebhooks: manageWebhooks, manageEmojisAndStickers: manageEmojisAndStickers, useApplicationCommands: useApplicationCommands, requestToSpeak: requestToSpeak, manageEvents: manageEvents, manageThreads: manageThreads, createPublicThreads: createPublicThreads, createPrivateThreads: createPrivateThreads, useExternalStickers: useExternalStickers, sendMessagesInThreads: sendMessagesInThreads, startEmbeddedActivities: startEmbeddedActivities, moderateMembers: moderateMembers);
	}

	public GuildPermissions Modify(bool? createInstantInvite = null, bool? kickMembers = null, bool? banMembers = null, bool? administrator = null, bool? manageChannels = null, bool? manageGuild = null, bool? addReactions = null, bool? viewAuditLog = null, bool? viewGuildInsights = null, bool? viewChannel = null, bool? sendMessages = null, bool? sendTTSMessages = null, bool? manageMessages = null, bool? embedLinks = null, bool? attachFiles = null, bool? readMessageHistory = null, bool? mentionEveryone = null, bool? useExternalEmojis = null, bool? connect = null, bool? speak = null, bool? muteMembers = null, bool? deafenMembers = null, bool? moveMembers = null, bool? useVoiceActivation = null, bool? prioritySpeaker = null, bool? stream = null, bool? changeNickname = null, bool? manageNicknames = null, bool? manageRoles = null, bool? manageWebhooks = null, bool? manageEmojisAndStickers = null, bool? useApplicationCommands = null, bool? requestToSpeak = null, bool? manageEvents = null, bool? manageThreads = null, bool? createPublicThreads = null, bool? createPrivateThreads = null, bool? useExternalStickers = null, bool? sendMessagesInThreads = null, bool? startEmbeddedActivities = null, bool? moderateMembers = null)
	{
		return new GuildPermissions(RawValue, createInstantInvite, kickMembers, banMembers, administrator, manageChannels, manageGuild, addReactions, viewAuditLog, viewGuildInsights, viewChannel, sendMessages, sendTTSMessages, manageMessages, embedLinks, attachFiles, readMessageHistory, mentionEveryone, useExternalEmojis, connect, speak, muteMembers, deafenMembers, moveMembers, useVoiceActivation, prioritySpeaker, stream, changeNickname, manageNicknames, manageRoles, manageWebhooks, manageEmojisAndStickers, useApplicationCommands, requestToSpeak, manageEvents, manageThreads, createPublicThreads, createPrivateThreads, useExternalStickers, sendMessagesInThreads, startEmbeddedActivities, moderateMembers);
	}

	public bool Has(GuildPermission permission)
	{
		return Permissions.GetValue(RawValue, permission);
	}

	public List<GuildPermission> ToList()
	{
		List<GuildPermission> list = new List<GuildPermission>();
		for (byte b = 0; b < 53; b++)
		{
			ulong num = (ulong)(1L << (int)b);
			if ((RawValue & num) != 0L)
			{
				list.Add((GuildPermission)num);
			}
		}
		return list;
	}

	internal void Ensure(GuildPermission permissions)
	{
		if (!Has(permissions))
		{
			IEnumerable<GuildPermission> source = Enum.GetValues(typeof(GuildPermission)).Cast<GuildPermission>();
			ulong currentValues = RawValue;
			IEnumerable<GuildPermission> source2 = source.Where((GuildPermission x) => permissions.HasFlag(x) && !Permissions.GetValue(currentValues, x));
			throw new InvalidOperationException("Missing required guild permission" + ((source2.Count() > 1) ? "s" : "") + " " + string.Join(", ", source2.Select((GuildPermission x) => x.ToString())) + " in order to execute this operation.");
		}
	}

	public override string ToString()
	{
		return RawValue.ToString();
	}

	static GuildPermissions()
	{
		None = default(GuildPermissions);
		Webhook = new GuildPermissions(55296uL);
		All = new GuildPermissions(ulong.MaxValue);
	}
}
