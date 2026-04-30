using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal class RestThreadChannel : RestTextChannel, IThreadChannel, ITextChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IMentionable, INestedChannel, IGuildChannel, IDeletable
{
	public ThreadType Type { get; private set; }

	public bool HasJoined { get; private set; }

	public bool IsArchived { get; private set; }

	public ThreadArchiveDuration AutoArchiveDuration { get; private set; }

	public DateTimeOffset ArchiveTimestamp { get; private set; }

	public bool IsLocked { get; private set; }

	public int MemberCount { get; private set; }

	public int MessageCount { get; private set; }

	public bool? IsInvitable { get; private set; }

	public override DateTimeOffset CreatedAt { get; }

	public ulong ParentChannelId { get; private set; }

	public override IReadOnlyCollection<Overwrite> PermissionOverwrites
	{
		get
		{
			throw new NotSupportedException("This method is not supported in threads.");
		}
	}

	internal RestThreadChannel(BaseDiscordClient discord, IGuild guild, ulong id, DateTimeOffset? createdAt)
		: base(discord, guild, id)
	{
		CreatedAt = createdAt ?? new DateTimeOffset(2022, 1, 9, 0, 0, 0, TimeSpan.Zero);
	}

	internal new static RestThreadChannel Create(BaseDiscordClient discord, IGuild guild, Channel model)
	{
		RestThreadChannel restThreadChannel = new RestThreadChannel(discord, guild, model.Id, model.ThreadMetadata.GetValueOrDefault()?.CreatedAt.GetValueOrDefault());
		restThreadChannel.Update(model);
		return restThreadChannel;
	}

	internal override void Update(Channel model)
	{
		base.Update(model);
		HasJoined = model.ThreadMember.IsSpecified;
		if (model.ThreadMetadata.IsSpecified)
		{
			IsInvitable = model.ThreadMetadata.Value.Invitable.ToNullable();
			IsArchived = model.ThreadMetadata.Value.Archived;
			AutoArchiveDuration = model.ThreadMetadata.Value.AutoArchiveDuration;
			ArchiveTimestamp = model.ThreadMetadata.Value.ArchiveTimestamp;
			IsLocked = model.ThreadMetadata.Value.Locked.GetValueOrDefault(defaultValue: false);
		}
		MemberCount = model.MemberCount.GetValueOrDefault(0);
		MessageCount = model.MessageCount.GetValueOrDefault(0);
		Type = (ThreadType)model.Type;
		ParentChannelId = model.CategoryId.Value;
	}

	public new Task<RestThreadUser> GetUserAsync(ulong userId, RequestOptions options = null)
	{
		return ThreadHelper.GetUserAsync(userId, this, base.Discord, options);
	}

	public new async Task<IReadOnlyCollection<RestThreadUser>> GetUsersAsync(RequestOptions options = null)
	{
		return (await ThreadHelper.GetUsersAsync(this, base.Discord, options).ConfigureAwait(continueOnCapturedContext: false)).ToImmutableArray();
	}

	public override async Task ModifyAsync(Action<TextChannelProperties> func, RequestOptions options = null)
	{
		Update(await ThreadHelper.ModifyAsync(this, base.Discord, func, options));
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

	public override Task<ICategoryChannel> GetCategoryAsync(RequestOptions options = null)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}

	public override Task<IReadOnlyCollection<IInviteMetadata>> GetInvitesAsync(RequestOptions options = null)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}

	public override OverwritePermissions? GetPermissionOverwrite(IRole role)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}

	public override OverwritePermissions? GetPermissionOverwrite(IUser user)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}

	public override Task<RestWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}

	public override Task<IReadOnlyCollection<RestWebhook>> GetWebhooksAsync(RequestOptions options = null)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}

	public override Task RemovePermissionOverwriteAsync(IRole role, RequestOptions options = null)
	{
		throw new NotSupportedException("This method is not supported in threads.");
	}

	public override Task RemovePermissionOverwriteAsync(IUser user, RequestOptions options = null)
	{
		throw new NotSupportedException("This method is not supported in threads.");
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
}
