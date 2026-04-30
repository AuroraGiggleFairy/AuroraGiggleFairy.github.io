using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestRole : RestEntity<ulong>, IRole, ISnowflakeEntity, IEntity<ulong>, IDeletable, IMentionable, IComparable<IRole>
{
	internal IGuild Guild { get; }

	public Color Color { get; private set; }

	public bool IsHoisted { get; private set; }

	public bool IsManaged { get; private set; }

	public bool IsMentionable { get; private set; }

	public string Name { get; private set; }

	public string Icon { get; private set; }

	public Emoji Emoji { get; private set; }

	public GuildPermissions Permissions { get; private set; }

	public int Position { get; private set; }

	public RoleTags Tags { get; private set; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public bool IsEveryone => base.Id == Guild.Id;

	public string Mention
	{
		get
		{
			if (!IsEveryone)
			{
				return MentionUtils.MentionRole(base.Id);
			}
			return "@everyone";
		}
	}

	private string DebuggerDisplay => $"{Name} ({base.Id})";

	IGuild IRole.Guild
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

	internal RestRole(BaseDiscordClient discord, IGuild guild, ulong id)
		: base(discord, id)
	{
		Guild = guild;
	}

	internal static RestRole Create(BaseDiscordClient discord, IGuild guild, Role model)
	{
		RestRole restRole = new RestRole(discord, guild, model.Id);
		restRole.Update(model);
		return restRole;
	}

	internal void Update(Role model)
	{
		Name = model.Name;
		IsHoisted = model.Hoist;
		IsManaged = model.Managed;
		IsMentionable = model.Mentionable;
		Position = model.Position;
		Color = new Color(model.Color);
		Permissions = new GuildPermissions(model.Permissions);
		if (model.Tags.IsSpecified)
		{
			Tags = model.Tags.Value.ToEntity();
		}
		if (model.Icon.IsSpecified)
		{
			Icon = model.Icon.Value;
		}
		if (model.Emoji.IsSpecified)
		{
			Emoji = new Emoji(model.Emoji.Value);
		}
	}

	public async Task ModifyAsync(Action<RoleProperties> func, RequestOptions options = null)
	{
		Update(await RoleHelper.ModifyAsync(this, base.Discord, func, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return RoleHelper.DeleteAsync(this, base.Discord, options);
	}

	public string GetIconUrl()
	{
		return CDN.GetGuildRoleIconUrl(base.Id, Icon);
	}

	public int CompareTo(IRole role)
	{
		return RoleUtils.Compare(this, role);
	}

	public override string ToString()
	{
		return Name;
	}
}
