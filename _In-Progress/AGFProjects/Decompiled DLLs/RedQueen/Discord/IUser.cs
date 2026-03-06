using System.Threading.Tasks;

namespace Discord;

internal interface IUser : ISnowflakeEntity, IEntity<ulong>, IMentionable, IPresence
{
	string AvatarId { get; }

	string Discriminator { get; }

	ushort DiscriminatorValue { get; }

	bool IsBot { get; }

	bool IsWebhook { get; }

	string Username { get; }

	UserProperties? PublicFlags { get; }

	string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128);

	string GetDefaultAvatarUrl();

	Task<IDMChannel> CreateDMChannelAsync(RequestOptions options = null);
}
