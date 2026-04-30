using System;
using System.Collections.Generic;

namespace Discord;

internal class AllowedMentions
{
	private static readonly Lazy<AllowedMentions> none = new Lazy<AllowedMentions>(() => new AllowedMentions());

	private static readonly Lazy<AllowedMentions> all = new Lazy<AllowedMentions>(() => new AllowedMentions(AllowedMentionTypes.Roles | AllowedMentionTypes.Users | AllowedMentionTypes.Everyone));

	public static AllowedMentions None => none.Value;

	public static AllowedMentions All => all.Value;

	public AllowedMentionTypes? AllowedTypes { get; set; }

	public List<ulong> RoleIds { get; set; } = new List<ulong>();

	public List<ulong> UserIds { get; set; } = new List<ulong>();

	public bool? MentionRepliedUser { get; set; }

	public AllowedMentions(AllowedMentionTypes? allowedTypes = null)
	{
		AllowedTypes = allowedTypes;
	}
}
