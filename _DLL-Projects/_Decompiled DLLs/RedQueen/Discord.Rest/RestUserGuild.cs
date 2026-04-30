using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestUserGuild : RestEntity<ulong>, IUserGuild, IDeletable, ISnowflakeEntity, IEntity<ulong>
{
	private string _iconId;

	public string Name { get; private set; }

	public bool IsOwner { get; private set; }

	public GuildPermissions Permissions { get; private set; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public string IconUrl => CDN.GetGuildIconUrl(base.Id, _iconId);

	private string DebuggerDisplay => string.Format("{0} ({1}{2})", Name, base.Id, IsOwner ? ", Owned" : "");

	internal RestUserGuild(BaseDiscordClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal static RestUserGuild Create(BaseDiscordClient discord, UserGuild model)
	{
		RestUserGuild restUserGuild = new RestUserGuild(discord, model.Id);
		restUserGuild.Update(model);
		return restUserGuild;
	}

	internal void Update(UserGuild model)
	{
		_iconId = model.Icon;
		IsOwner = model.Owner;
		Name = model.Name;
		Permissions = new GuildPermissions(model.Permissions);
	}

	public async Task LeaveAsync(RequestOptions options = null)
	{
		await base.Discord.ApiClient.LeaveGuildAsync(base.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task DeleteAsync(RequestOptions options = null)
	{
		await base.Discord.ApiClient.DeleteGuildAsync(base.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override string ToString()
	{
		return Name;
	}
}
