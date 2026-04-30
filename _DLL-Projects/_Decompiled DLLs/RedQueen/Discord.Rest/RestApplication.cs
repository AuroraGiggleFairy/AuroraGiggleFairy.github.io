using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestApplication : RestEntity<ulong>, IApplication, ISnowflakeEntity, IEntity<ulong>
{
	protected string _iconId;

	public string Name { get; private set; }

	public string Description { get; private set; }

	public IReadOnlyCollection<string> RPCOrigins { get; private set; }

	public ApplicationFlags Flags { get; private set; }

	public bool IsBotPublic { get; private set; }

	public bool BotRequiresCodeGrant { get; private set; }

	public ITeam Team { get; private set; }

	public IUser Owner { get; private set; }

	public string TermsOfService { get; private set; }

	public string PrivacyPolicy { get; private set; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public string IconUrl => CDN.GetApplicationIconUrl(base.Id, _iconId);

	public ApplicationInstallParams InstallParams { get; private set; }

	public IReadOnlyCollection<string> Tags { get; private set; }

	private string DebuggerDisplay => $"{Name} ({base.Id})";

	internal RestApplication(BaseDiscordClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal static RestApplication Create(BaseDiscordClient discord, Application model)
	{
		RestApplication restApplication = new RestApplication(discord, model.Id);
		restApplication.Update(model);
		return restApplication;
	}

	internal void Update(Application model)
	{
		Description = model.Description;
		RPCOrigins = (model.RPCOrigins.IsSpecified ? model.RPCOrigins.Value.ToImmutableArray() : System.Collections.Immutable.ImmutableArray<string>.Empty);
		Name = model.Name;
		_iconId = model.Icon;
		IsBotPublic = model.IsBotPublic;
		BotRequiresCodeGrant = model.BotRequiresCodeGrant;
		Tags = model.Tags.GetValueOrDefault(null)?.ToImmutableArray() ?? System.Collections.Immutable.ImmutableArray<string>.Empty;
		PrivacyPolicy = model.PrivacyPolicy;
		TermsOfService = model.TermsOfService;
		InstallParams valueOrDefault = model.InstallParams.GetValueOrDefault(null);
		InstallParams = new ApplicationInstallParams(valueOrDefault?.Scopes ?? new string[0], (GuildPermission?)(((long?)valueOrDefault?.Permission) ?? ((GuildPermission?)null)));
		if (model.Flags.IsSpecified)
		{
			Flags = model.Flags.Value;
		}
		if (model.Owner.IsSpecified)
		{
			Owner = RestUser.Create(base.Discord, model.Owner.Value);
		}
		if (model.Team != null)
		{
			Team = RestTeam.Create(base.Discord, model.Team);
		}
	}

	public async Task UpdateAsync()
	{
		Application application = await base.Discord.ApiClient.GetMyApplicationAsync().ConfigureAwait(continueOnCapturedContext: false);
		if (application.Id != base.Id)
		{
			throw new InvalidOperationException("Unable to update this object from a different application token.");
		}
		Update(application);
	}

	public override string ToString()
	{
		return Name;
	}
}
