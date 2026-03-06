using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class SocketThreadChannel : SocketTextChannel, IThreadChannel, ITextChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IMentionable, INestedChannel, IGuildChannel, IDeletable
{
	private readonly ConcurrentDictionary<ulong, SocketThreadUser> _members;

	private bool _usersDownloaded;

	private readonly object _downloadLock = new object();

	private readonly object _ownerLock = new object();

	private ulong _ownerId;

	public ThreadType Type { get; private set; }

	public SocketThreadUser Owner
	{
		get
		{
			lock (_ownerLock)
			{
				SocketThreadUser user = GetUser(_ownerId);
				if (user == null)
				{
					SocketGuildUser user2 = base.Guild.GetUser(_ownerId);
					if (user2 == null)
					{
						return null;
					}
					user = SocketThreadUser.Create(base.Guild, this, user2);
					_members[user.Id] = user;
					return user;
				}
				return user;
			}
		}
	}

	public SocketThreadUser CurrentUser => Users.FirstOrDefault((SocketThreadUser x) => x.Id == base.Discord.CurrentUser.Id);

	public bool HasJoined { get; private set; }

	public bool IsPrivateThread => Type == ThreadType.PrivateThread;

	public SocketGuildChannel ParentChannel { get; private set; }

	public int MessageCount { get; private set; }

	public int MemberCount { get; private set; }

	public bool IsArchived { get; private set; }

	public DateTimeOffset ArchiveTimestamp { get; private set; }

	public ThreadArchiveDuration AutoArchiveDuration { get; private set; }

	public bool IsLocked { get; private set; }

	public bool? IsInvitable { get; private set; }

	public override DateTimeOffset CreatedAt { get; }

	public new IReadOnlyCollection<SocketThreadUser> Users => _members.Values.ToImmutableArray();

	private string DebuggerDisplay => $"{base.Name} ({base.Id}, Thread)";

	public override IReadOnlyCollection<Overwrite> PermissionOverwrites
	{
		get
		{
			throw new NotSupportedException("This method is not supported in threads.");
		}
	}

	string IChannel.Name => base.Name;

	internal SocketThreadChannel(DiscordSocketClient discord, SocketGuild guild, ulong id, SocketGuildChannel parent, DateTimeOffset? createdAt)
		: base(discord, id, guild)
	{
		ParentChannel = parent;
		_members = new ConcurrentDictionary<ulong, SocketThreadUser>();
		CreatedAt = createdAt ?? new DateTimeOffset(2022, 1, 9, 0, 0, 0, TimeSpan.Zero);
	}

	internal new static SocketThreadChannel Create(SocketGuild guild, ClientState state, Channel model)
	{
		SocketGuildChannel channel = guild.GetChannel(model.CategoryId.Value);
		SocketThreadChannel socketThreadChannel = new SocketThreadChannel(guild.Discord, guild, model.Id, channel, model.ThreadMetadata.GetValueOrDefault()?.CreatedAt.GetValueOrDefault(null));
		socketThreadChannel.Update(state, model);
		return socketThreadChannel;
	}

	internal override void Update(ClientState state, Channel model)
	{
		base.Update(state, model);
		Type = (ThreadType)model.Type;
		MessageCount = model.MessageCount.GetValueOrDefault(-1);
		MemberCount = model.MemberCount.GetValueOrDefault(-1);
		if (model.ThreadMetadata.IsSpecified)
		{
			IsInvitable = model.ThreadMetadata.Value.Invitable.ToNullable();
			IsArchived = model.ThreadMetadata.Value.Archived;
			ArchiveTimestamp = model.ThreadMetadata.Value.ArchiveTimestamp;
			AutoArchiveDuration = model.ThreadMetadata.Value.AutoArchiveDuration;
			IsLocked = model.ThreadMetadata.Value.Locked.GetValueOrDefault(defaultValue: false);
		}
		if (model.OwnerId.IsSpecified)
		{
			_ownerId = model.OwnerId.Value;
		}
		HasJoined = model.ThreadMember.IsSpecified;
	}

	internal IReadOnlyCollection<SocketThreadUser> RemoveUsers(ulong[] users)
	{
		List<SocketThreadUser> list = new List<SocketThreadUser>();
		foreach (ulong key in users)
		{
			if (_members.TryRemove(key, out var value))
			{
				list.Add(value);
			}
		}
		return list.ToImmutableArray();
	}

	internal SocketThreadUser AddOrUpdateThreadMember(ThreadMember model, SocketGuildUser guildMember)
	{
		if (_members.TryGetValue(model.UserId.Value, out var value))
		{
			value.Update(model);
		}
		else
		{
			value = SocketThreadUser.Create(base.Guild, this, model, guildMember);
			value.GlobalUser.AddRef();
			_members[value.Id] = value;
		}
		return value;
	}

	public new SocketThreadUser GetUser(ulong id)
	{
		return Users.FirstOrDefault((SocketThreadUser x) => x.Id == id);
	}

	public async Task<IReadOnlyCollection<SocketThreadUser>> GetUsersAsync(RequestOptions options = null)
	{
		if (!_usersDownloaded)
		{
			await DownloadUsersAsync(options);
			_usersDownloaded = true;
		}
		return Users;
	}

	public async Task DownloadUsersAsync(RequestOptions options = null)
	{
		ThreadMember[] array = await base.Discord.ApiClient.ListThreadMembersAsync(base.Id, options);
		lock (_downloadLock)
		{
			ThreadMember[] array2 = array;
			foreach (ThreadMember threadMember in array2)
			{
				SocketGuildUser user = base.Guild.GetUser(threadMember.UserId.Value);
				AddOrUpdateThreadMember(threadMember, user);
			}
		}
	}

	internal new SocketThreadChannel Clone()
	{
		return MemberwiseClone() as SocketThreadChannel;
	}

	public Task JoinAsync(RequestOptions options = null)
	{
		return base.Discord.ApiClient.JoinThreadAsync(base.Id, options);
	}

	public Task LeaveAsync(RequestOptions options = null)
	{
		return base.Discord.ApiClient.LeaveThreadAsync(base.Id, options);
	}

	public Task AddUserAsync(IGuildUser user, RequestOptions options = null)
	{
		return base.Discord.ApiClient.AddThreadMemberAsync(base.Id, user.Id, options);
	}

	public Task RemoveUserAsync(IGuildUser user, RequestOptions options = null)
	{
		return base.Discord.ApiClient.RemoveThreadMemberAsync(base.Id, user.Id, options);
	}

	public override Task AddPermissionOverwriteAsync(IRole role, OverwritePermissions permissions, RequestOptions options = null)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}

	public override Task AddPermissionOverwriteAsync(IUser user, OverwritePermissions permissions, RequestOptions options = null)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}

	public override Task<IInviteMetadata> CreateInviteAsync(int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}

	public override Task<IInviteMetadata> CreateInviteToApplicationAsync(ulong applicationId, int? maxAge, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}

	public override Task<IInviteMetadata> CreateInviteToStreamAsync(IUser user, int? maxAge, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}

	public override Task<RestWebhook> CreateWebhookAsync(string name, Stream avatar = null, RequestOptions options = null)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}

	public override Task<IReadOnlyCollection<IInviteMetadata>> GetInvitesAsync(RequestOptions options = null)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}

	public override OverwritePermissions? GetPermissionOverwrite(IRole role)
	{
		return ParentChannel.GetPermissionOverwrite(role);
	}

	public override OverwritePermissions? GetPermissionOverwrite(IUser user)
	{
		return ParentChannel.GetPermissionOverwrite(user);
	}

	public override Task<RestWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}

	public override Task<IReadOnlyCollection<RestWebhook>> GetWebhooksAsync(RequestOptions options = null)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}

	public override Task ModifyAsync(Action<TextChannelProperties> func, RequestOptions options = null)
	{
		return ThreadHelper.ModifyAsync(this, base.Discord, func, options);
	}

	public override Task RemovePermissionOverwriteAsync(IRole role, RequestOptions options = null)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}

	public override Task RemovePermissionOverwriteAsync(IUser user, RequestOptions options = null)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}

	public override Task SyncPermissionsAsync(RequestOptions options = null)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}
}
