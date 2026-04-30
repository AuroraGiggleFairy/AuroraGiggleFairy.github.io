using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Discord;

internal class GuildFeatures
{
	public GuildFeature Value { get; }

	public IReadOnlyCollection<string> Experimental { get; }

	public bool HasThreads => HasFeature(GuildFeature.ThreadsEnabled | GuildFeature.ThreadsEnabledTesting);

	public bool HasTextInVoice => HasFeature(GuildFeature.TextInVoiceEnabled);

	public bool IsStaffServer => HasFeature(GuildFeature.InternalEmployeeOnly);

	public bool IsHub => HasFeature(GuildFeature.Hub);

	public bool IsLinkedToHub => HasFeature(GuildFeature.LinkedToHub);

	public bool IsPartnered => HasFeature(GuildFeature.Partnered);

	public bool IsVerified => HasFeature(GuildFeature.Verified);

	public bool HasVanityUrl => HasFeature(GuildFeature.VanityUrl);

	public bool HasRoleSubscriptions => HasFeature(GuildFeature.RoleSubscriptionsAvailableForPurchase | GuildFeature.RoleSubscriptionsEnabled);

	public bool HasRoleIcons => HasFeature(GuildFeature.RoleIcons);

	public bool HasPrivateThreads => HasFeature(GuildFeature.PrivateThreads);

	internal GuildFeatures(GuildFeature value, string[] experimental)
	{
		Value = value;
		Experimental = experimental.ToImmutableArray();
	}

	public bool HasFeature(GuildFeature feature)
	{
		return Value.HasFlag(feature);
	}

	public bool HasFeature(string feature)
	{
		return Experimental.Contains(feature);
	}

	internal void EnsureFeature(GuildFeature feature)
	{
		if (!HasFeature(feature))
		{
			IEnumerable<GuildFeature> source = from GuildFeature x in Enum.GetValues(typeof(GuildFeature))
				where feature.HasFlag(x) && !Value.HasFlag(x)
				select x;
			throw new InvalidOperationException("Missing required guild feature" + ((source.Count() > 1) ? "s" : "") + " " + string.Join(", ", source.Select((GuildFeature x) => x.ToString())) + " in order to execute this operation.");
		}
	}
}
