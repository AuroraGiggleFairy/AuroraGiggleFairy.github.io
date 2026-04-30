using System.Collections.Generic;

namespace Discord;

internal interface IApplication : ISnowflakeEntity, IEntity<ulong>
{
	string Name { get; }

	string Description { get; }

	IReadOnlyCollection<string> RPCOrigins { get; }

	ApplicationFlags Flags { get; }

	ApplicationInstallParams InstallParams { get; }

	IReadOnlyCollection<string> Tags { get; }

	string IconUrl { get; }

	bool IsBotPublic { get; }

	bool BotRequiresCodeGrant { get; }

	ITeam Team { get; }

	IUser Owner { get; }

	string TermsOfService { get; }

	string PrivacyPolicy { get; }
}
