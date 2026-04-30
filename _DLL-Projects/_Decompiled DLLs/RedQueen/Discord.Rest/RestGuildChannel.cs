using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal class RestGuildChannel : RestChannel, IGuildChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	private System.Collections.Immutable.ImmutableArray<Overwrite> _overwrites;

	public virtual IReadOnlyCollection<Overwrite> PermissionOverwrites => _overwrites;

	internal IGuild Guild { get; }

	public string Name { get; private set; }

	public int Position { get; private set; }

	public ulong GuildId => Guild.Id;

	IGuild IGuildChannel.Guild
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

	internal RestGuildChannel(BaseDiscordClient discord, IGuild guild, ulong id)
		: base(discord, id)
	{
		Guild = guild;
	}

	internal static RestGuildChannel Create(BaseDiscordClient discord, IGuild guild, Channel model)
	{
		switch (model.Type)
		{
		case ChannelType.News:
			return RestNewsChannel.Create(discord, guild, model);
		case ChannelType.Text:
			return RestTextChannel.Create(discord, guild, model);
		case ChannelType.Voice:
			return RestVoiceChannel.Create(discord, guild, model);
		case ChannelType.Stage:
			return RestStageChannel.Create(discord, guild, model);
		case ChannelType.Forum:
			return RestForumChannel.Create(discord, guild, model);
		case ChannelType.Category:
			return RestCategoryChannel.Create(discord, guild, model);
		case ChannelType.NewsThread:
		case ChannelType.PublicThread:
		case ChannelType.PrivateThread:
			return RestThreadChannel.Create(discord, guild, model);
		default:
			return new RestGuildChannel(discord, guild, model.Id);
		}
	}

	internal override void Update(Channel model)
	{
		Name = model.Name.Value;
		if (model.Position.IsSpecified)
		{
			Position = model.Position.Value;
		}
		if (model.PermissionOverwrites.IsSpecified)
		{
			global::Discord.API.Overwrite[] value = model.PermissionOverwrites.Value;
			System.Collections.Immutable.ImmutableArray<Overwrite>.Builder builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<Overwrite>(value.Length);
			for (int i = 0; i < value.Length; i++)
			{
				builder.Add(value[i].ToEntity());
			}
			_overwrites = builder.ToImmutable();
		}
	}

	public override async Task UpdateAsync(RequestOptions options = null)
	{
		Update(await base.Discord.ApiClient.GetChannelAsync(GuildId, base.Id, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public async Task ModifyAsync(Action<GuildChannelProperties> func, RequestOptions options = null)
	{
		Update(await ChannelHelper.ModifyAsync(this, base.Discord, func, options).ConfigureAwait(continueOnCapturedContext: false));
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
		_overwrites = _overwrites.Add(new Overwrite(user.Id, PermissionTarget.User, new OverwritePermissions(permissions.AllowValue, permissions.DenyValue)));
	}

	public virtual async Task AddPermissionOverwriteAsync(IRole role, OverwritePermissions permissions, RequestOptions options = null)
	{
		await ChannelHelper.AddPermissionOverwriteAsync(this, base.Discord, role, permissions, options).ConfigureAwait(continueOnCapturedContext: false);
		_overwrites = _overwrites.Add(new Overwrite(role.Id, PermissionTarget.Role, new OverwritePermissions(permissions.AllowValue, permissions.DenyValue)));
	}

	public virtual async Task RemovePermissionOverwriteAsync(IUser user, RequestOptions options = null)
	{
		await ChannelHelper.RemovePermissionOverwriteAsync(this, base.Discord, user, options).ConfigureAwait(continueOnCapturedContext: false);
		for (int i = 0; i < _overwrites.Length; i++)
		{
			if (_overwrites[i].TargetId == user.Id)
			{
				_overwrites = _overwrites.RemoveAt(i);
				break;
			}
		}
	}

	public virtual async Task RemovePermissionOverwriteAsync(IRole role, RequestOptions options = null)
	{
		await ChannelHelper.RemovePermissionOverwriteAsync(this, base.Discord, role, options).ConfigureAwait(continueOnCapturedContext: false);
		for (int i = 0; i < _overwrites.Length; i++)
		{
			if (_overwrites[i].TargetId == role.Id)
			{
				_overwrites = _overwrites.RemoveAt(i);
				break;
			}
		}
	}

	public override string ToString()
	{
		return Name;
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
		return AsyncEnumerable.Empty<IReadOnlyCollection<IGuildUser>>();
	}

	Task<IGuildUser> IGuildChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult<IGuildUser>(null);
	}

	IAsyncEnumerable<IReadOnlyCollection<IUser>> IChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
	{
		return AsyncEnumerable.Empty<IReadOnlyCollection<IUser>>();
	}

	Task<IUser> IChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult<IUser>(null);
	}
}
