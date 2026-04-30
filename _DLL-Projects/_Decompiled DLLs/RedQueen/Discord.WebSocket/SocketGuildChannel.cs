using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class SocketGuildChannel : SocketChannel, IGuildChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	private System.Collections.Immutable.ImmutableArray<Overwrite> _overwrites;

	public SocketGuild Guild { get; }

	public string Name { get; private set; }

	public int Position { get; private set; }

	public virtual IReadOnlyCollection<Overwrite> PermissionOverwrites => _overwrites;

	public new virtual IReadOnlyCollection<SocketGuildUser> Users => System.Collections.Immutable.ImmutableArray.Create<SocketGuildUser>();

	private string DebuggerDisplay => $"{Name} ({base.Id}, Guild)";

	IGuild IGuildChannel.Guild => Guild;

	ulong IGuildChannel.GuildId => Guild.Id;

	string IChannel.Name => Name;

	internal SocketGuildChannel(DiscordSocketClient discord, ulong id, SocketGuild guild)
		: base(discord, id)
	{
		Guild = guild;
	}

	internal static SocketGuildChannel Create(SocketGuild guild, ClientState state, Channel model)
	{
		switch (model.Type)
		{
		case ChannelType.News:
			return SocketNewsChannel.Create(guild, state, model);
		case ChannelType.Text:
			return SocketTextChannel.Create(guild, state, model);
		case ChannelType.Voice:
			return SocketVoiceChannel.Create(guild, state, model);
		case ChannelType.Category:
			return SocketCategoryChannel.Create(guild, state, model);
		case ChannelType.NewsThread:
		case ChannelType.PublicThread:
		case ChannelType.PrivateThread:
			return SocketThreadChannel.Create(guild, state, model);
		case ChannelType.Stage:
			return SocketStageChannel.Create(guild, state, model);
		case ChannelType.Forum:
			return SocketForumChannel.Create(guild, state, model);
		default:
			return new SocketGuildChannel(guild.Discord, model.Id, guild);
		}
	}

	internal override void Update(ClientState state, Channel model)
	{
		Name = model.Name.Value;
		Position = model.Position.GetValueOrDefault(0);
		global::Discord.API.Overwrite[] valueOrDefault = model.PermissionOverwrites.GetValueOrDefault(new global::Discord.API.Overwrite[0]);
		System.Collections.Immutable.ImmutableArray<Overwrite>.Builder builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<Overwrite>(valueOrDefault.Length);
		for (int i = 0; i < valueOrDefault.Length; i++)
		{
			builder.Add(valueOrDefault[i].ToEntity());
		}
		_overwrites = builder.ToImmutable();
	}

	public Task ModifyAsync(Action<GuildChannelProperties> func, RequestOptions options = null)
	{
		return ChannelHelper.ModifyAsync(this, base.Discord, func, options);
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return ChannelHelper.DeleteAsync(this, base.Discord, options);
	}

	public virtual OverwritePermissions? GetPermissionOverwrite(IUser user)
	{
		for (int i = 0; i < _overwrites.Length; i++)
		{
			if (_overwrites[i].TargetId == user.Id)
			{
				return _overwrites[i].Permissions;
			}
		}
		return null;
	}

	public virtual OverwritePermissions? GetPermissionOverwrite(IRole role)
	{
		for (int i = 0; i < _overwrites.Length; i++)
		{
			if (_overwrites[i].TargetId == role.Id)
			{
				return _overwrites[i].Permissions;
			}
		}
		return null;
	}

	public virtual async Task AddPermissionOverwriteAsync(IUser user, OverwritePermissions permissions, RequestOptions options = null)
	{
		await ChannelHelper.AddPermissionOverwriteAsync(this, base.Discord, user, permissions, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public virtual async Task AddPermissionOverwriteAsync(IRole role, OverwritePermissions permissions, RequestOptions options = null)
	{
		await ChannelHelper.AddPermissionOverwriteAsync(this, base.Discord, role, permissions, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public virtual async Task RemovePermissionOverwriteAsync(IUser user, RequestOptions options = null)
	{
		await ChannelHelper.RemovePermissionOverwriteAsync(this, base.Discord, user, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public virtual async Task RemovePermissionOverwriteAsync(IRole role, RequestOptions options = null)
	{
		await ChannelHelper.RemovePermissionOverwriteAsync(this, base.Discord, role, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public new virtual SocketGuildUser GetUser(ulong id)
	{
		return null;
	}

	public override string ToString()
	{
		return Name;
	}

	internal new SocketGuildChannel Clone()
	{
		return MemberwiseClone() as SocketGuildChannel;
	}

	internal override IReadOnlyCollection<SocketUser> GetUsersInternal()
	{
		return Users;
	}

	internal override SocketUser GetUserInternal(ulong id)
	{
		return GetUser(id);
	}

	OverwritePermissions? IGuildChannel.GetPermissionOverwrite(IRole role)
	{
		return GetPermissionOverwrite(role);
	}

	OverwritePermissions? IGuildChannel.GetPermissionOverwrite(IUser user)
	{
		return GetPermissionOverwrite(user);
	}

	async Task IGuildChannel.AddPermissionOverwriteAsync(IRole role, OverwritePermissions permissions, RequestOptions options)
	{
		await AddPermissionOverwriteAsync(role, permissions, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task IGuildChannel.AddPermissionOverwriteAsync(IUser user, OverwritePermissions permissions, RequestOptions options)
	{
		await AddPermissionOverwriteAsync(user, permissions, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task IGuildChannel.RemovePermissionOverwriteAsync(IRole role, RequestOptions options)
	{
		await RemovePermissionOverwriteAsync(role, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	async Task IGuildChannel.RemovePermissionOverwriteAsync(IUser user, RequestOptions options)
	{
		await RemovePermissionOverwriteAsync(user, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
	{
		return System.Collections.Immutable.ImmutableArray.Create((IReadOnlyCollection<IGuildUser>)Users).ToAsyncEnumerable();
	}

	Task<IGuildUser> IGuildChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IGuildUser)GetUser(id));
	}

	IAsyncEnumerable<IReadOnlyCollection<IUser>> IChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
	{
		return System.Collections.Immutable.ImmutableArray.Create((IReadOnlyCollection<IUser>)Users).ToAsyncEnumerable();
	}

	Task<IUser> IChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IUser)GetUser(id));
	}
}
