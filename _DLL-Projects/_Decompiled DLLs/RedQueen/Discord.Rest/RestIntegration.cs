using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestIntegration : RestEntity<ulong>, IIntegration
{
	private long? _syncedAtTicks;

	public string Name { get; private set; }

	public string Type { get; private set; }

	public bool IsEnabled { get; private set; }

	public bool? IsSyncing { get; private set; }

	public ulong? RoleId { get; private set; }

	public bool? HasEnabledEmoticons { get; private set; }

	public IntegrationExpireBehavior? ExpireBehavior { get; private set; }

	public int? ExpireGracePeriod { get; private set; }

	IUser IIntegration.User => User;

	public IIntegrationAccount Account { get; private set; }

	public DateTimeOffset? SyncedAt => DateTimeUtils.FromTicks(_syncedAtTicks);

	public int? SubscriberCount { get; private set; }

	public bool? IsRevoked { get; private set; }

	public IIntegrationApplication Application { get; private set; }

	internal IGuild Guild { get; private set; }

	public RestUser User { get; private set; }

	private string DebuggerDisplay => string.Format("{0} ({1}{2})", Name, base.Id, IsEnabled ? ", Enabled" : "");

	public ulong GuildId { get; private set; }

	IGuild IIntegration.Guild
	{
		get
		{
			if (Guild != null)
			{
				return Guild;
			}
			throw new InvalidOperationException("Unable to return this entity's parent unless it was fetched through that object.");
		}
	}

	internal RestIntegration(BaseDiscordClient discord, IGuild guild, ulong id)
		: base(discord, id)
	{
		Guild = guild;
	}

	internal static RestIntegration Create(BaseDiscordClient discord, IGuild guild, Integration model)
	{
		RestIntegration restIntegration = new RestIntegration(discord, guild, model.Id);
		restIntegration.Update(model);
		return restIntegration;
	}

	internal void Update(Integration model)
	{
		Name = model.Name;
		Type = model.Type;
		IsEnabled = model.Enabled;
		IsSyncing = (model.Syncing.IsSpecified ? model.Syncing.Value : ((bool?)null));
		RoleId = (model.RoleId.IsSpecified ? model.RoleId.Value : ((ulong?)null));
		HasEnabledEmoticons = (model.EnableEmoticons.IsSpecified ? model.EnableEmoticons.Value : ((bool?)null));
		ExpireBehavior = (model.ExpireBehavior.IsSpecified ? new IntegrationExpireBehavior?(model.ExpireBehavior.Value) : ((IntegrationExpireBehavior?)null));
		ExpireGracePeriod = (model.ExpireGracePeriod.IsSpecified ? model.ExpireGracePeriod.Value : ((int?)null));
		User = (model.User.IsSpecified ? RestUser.Create(base.Discord, model.User.Value) : null);
		Account = (model.Account.IsSpecified ? RestIntegrationAccount.Create(model.Account.Value) : null);
		SubscriberCount = (model.SubscriberAccount.IsSpecified ? model.SubscriberAccount.Value : ((int?)null));
		IsRevoked = (model.Revoked.IsSpecified ? model.Revoked.Value : ((bool?)null));
		Application = (model.Application.IsSpecified ? RestIntegrationApplication.Create(base.Discord, model.Application.Value) : null);
		_syncedAtTicks = (model.SyncedAt.IsSpecified ? new long?(model.SyncedAt.Value.UtcTicks) : ((long?)null));
	}

	public async Task DeleteAsync()
	{
		await base.Discord.ApiClient.DeleteIntegrationAsync(GuildId, base.Id).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override string ToString()
	{
		return Name;
	}
}
