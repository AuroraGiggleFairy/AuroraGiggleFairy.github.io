using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class SocketCategoryChannel : SocketGuildChannel, ICategoryChannel, IGuildChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	public override IReadOnlyCollection<SocketGuildUser> Users => base.Guild.Users.Where((SocketGuildUser x) => Permissions.GetValue(Permissions.ResolveChannel(base.Guild, x, this, Permissions.ResolveGuild(base.Guild, x)), ChannelPermission.ViewChannel)).ToImmutableArray();

	public IReadOnlyCollection<SocketGuildChannel> Channels => base.Guild.Channels.Where((SocketGuildChannel x) => x is INestedChannel { CategoryId: var categoryId } && categoryId == base.Id).ToImmutableArray();

	private string DebuggerDisplay => $"{base.Name} ({base.Id}, Category)";

	internal SocketCategoryChannel(DiscordSocketClient discord, ulong id, SocketGuild guild)
		: base(discord, id, guild)
	{
	}

	internal new static SocketCategoryChannel Create(SocketGuild guild, ClientState state, Channel model)
	{
		SocketCategoryChannel socketCategoryChannel = new SocketCategoryChannel(guild?.Discord, model.Id, guild);
		socketCategoryChannel.Update(state, model);
		return socketCategoryChannel;
	}

	public override SocketGuildUser GetUser(ulong id)
	{
		SocketGuildUser user = base.Guild.GetUser(id);
		if (user != null)
		{
			ulong guildPermissions = Permissions.ResolveGuild(base.Guild, user);
			if (Permissions.GetValue(Permissions.ResolveChannel(base.Guild, user, this, guildPermissions), ChannelPermission.ViewChannel))
			{
				return user;
			}
		}
		return null;
	}

	internal new SocketCategoryChannel Clone()
	{
		return MemberwiseClone() as SocketCategoryChannel;
	}

	IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
	{
		if (mode != CacheMode.AllowDownload)
		{
			return System.Collections.Immutable.ImmutableArray.Create((IReadOnlyCollection<IGuildUser>)Users).ToAsyncEnumerable();
		}
		return ChannelHelper.GetUsersAsync(this, base.Guild, base.Discord, null, null, options);
	}

	async Task<IGuildUser> IGuildChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		SocketGuildUser user = GetUser(id);
		if (user != null || mode == CacheMode.CacheOnly)
		{
			return user;
		}
		return await ChannelHelper.GetUserAsync(this, base.Guild, base.Discord, id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	IAsyncEnumerable<IReadOnlyCollection<IUser>> IChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
	{
		if (mode != CacheMode.AllowDownload)
		{
			return System.Collections.Immutable.ImmutableArray.Create((IReadOnlyCollection<IGuildUser>)Users).ToAsyncEnumerable();
		}
		return ChannelHelper.GetUsersAsync(this, base.Guild, base.Discord, null, null, options);
	}

	async Task<IUser> IChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		SocketGuildUser user = GetUser(id);
		if (user != null || mode == CacheMode.CacheOnly)
		{
			return user;
		}
		return await ChannelHelper.GetUserAsync(this, base.Guild, base.Discord, id, options).ConfigureAwait(continueOnCapturedContext: false);
	}
}
