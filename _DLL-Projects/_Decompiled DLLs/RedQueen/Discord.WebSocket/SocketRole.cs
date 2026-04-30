using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class SocketRole : SocketEntity<ulong>, IRole, ISnowflakeEntity, IEntity<ulong>, IDeletable, IMentionable, IComparable<IRole>
{
	public SocketGuild Guild { get; }

	public Color Color { get; private set; }

	public bool IsHoisted { get; private set; }

	public bool IsManaged { get; private set; }

	public bool IsMentionable { get; private set; }

	public string Name { get; private set; }

	public Emoji Emoji { get; private set; }

	public string Icon { get; private set; }

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

	public IEnumerable<SocketGuildUser> Members => Guild.Users.Where((SocketGuildUser x) => x.Roles.Any((SocketRole r) => r.Id == base.Id));

	private string DebuggerDisplay => $"{Name} ({base.Id})";

	IGuild IRole.Guild => Guild;

	internal SocketRole(SocketGuild guild, ulong id)
		: base(guild?.Discord, id)
	{
		Guild = guild;
	}

	internal static SocketRole Create(SocketGuild guild, ClientState state, Role model)
	{
		SocketRole socketRole = new SocketRole(guild, model.Id);
		socketRole.Update(state, model);
		return socketRole;
	}

	internal void Update(ClientState state, Role model)
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

	public Task ModifyAsync(Action<RoleProperties> func, RequestOptions options = null)
	{
		return RoleHelper.ModifyAsync(this, base.Discord, func, options);
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return RoleHelper.DeleteAsync(this, base.Discord, options);
	}

	public string GetIconUrl()
	{
		return CDN.GetGuildRoleIconUrl(base.Id, Icon);
	}

	public override string ToString()
	{
		return Name;
	}

	internal SocketRole Clone()
	{
		return MemberwiseClone() as SocketRole;
	}

	public int CompareTo(IRole role)
	{
		return RoleUtils.Compare(this, role);
	}
}
