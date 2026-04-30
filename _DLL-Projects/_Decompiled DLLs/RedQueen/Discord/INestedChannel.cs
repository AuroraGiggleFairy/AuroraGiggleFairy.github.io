using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord;

internal interface INestedChannel : IGuildChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	ulong? CategoryId { get; }

	Task<ICategoryChannel> GetCategoryAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

	Task SyncPermissionsAsync(RequestOptions options = null);

	Task<IInviteMetadata> CreateInviteAsync(int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null);

	Task<IInviteMetadata> CreateInviteToApplicationAsync(ulong applicationId, int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null);

	Task<IInviteMetadata> CreateInviteToApplicationAsync(DefaultApplications application, int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null);

	Task<IInviteMetadata> CreateInviteToStreamAsync(IUser user, int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null);

	Task<IReadOnlyCollection<IInviteMetadata>> GetInvitesAsync(RequestOptions options = null);
}
