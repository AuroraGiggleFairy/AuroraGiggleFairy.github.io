using System.Globalization;

namespace Discord;

internal class GuildProperties
{
	public Optional<string> Name { get; set; }

	public Optional<IVoiceRegion> Region { get; set; }

	public Optional<string> RegionId { get; set; }

	public Optional<VerificationLevel> VerificationLevel { get; set; }

	public Optional<DefaultMessageNotifications> DefaultMessageNotifications { get; set; }

	public Optional<int> AfkTimeout { get; set; }

	public Optional<Image?> Icon { get; set; }

	public Optional<Image?> Banner { get; set; }

	public Optional<Image?> Splash { get; set; }

	public Optional<IVoiceChannel> AfkChannel { get; set; }

	public Optional<ulong?> AfkChannelId { get; set; }

	public Optional<ITextChannel> SystemChannel { get; set; }

	public Optional<ulong?> SystemChannelId { get; set; }

	public Optional<IUser> Owner { get; set; }

	public Optional<ulong> OwnerId { get; set; }

	public Optional<ExplicitContentFilterLevel> ExplicitContentFilter { get; set; }

	public Optional<SystemChannelMessageDeny> SystemChannelFlags { get; set; }

	public Optional<string> PreferredLocale { get; set; }

	public Optional<CultureInfo> PreferredCulture { get; set; }

	public Optional<bool> IsBoostProgressBarEnabled { get; set; }
}
