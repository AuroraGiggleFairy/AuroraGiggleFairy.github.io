using System;
using System.Threading.Tasks;

namespace Discord;

internal interface IWebhook : IDeletable, ISnowflakeEntity, IEntity<ulong>
{
	string Token { get; }

	string Name { get; }

	string AvatarId { get; }

	ITextChannel Channel { get; }

	ulong ChannelId { get; }

	IGuild Guild { get; }

	ulong? GuildId { get; }

	IUser Creator { get; }

	ulong? ApplicationId { get; }

	string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128);

	Task ModifyAsync(Action<WebhookProperties> func, RequestOptions options = null);
}
