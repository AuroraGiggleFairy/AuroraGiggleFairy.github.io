using System;
using System.Threading.Tasks;

namespace Discord;

internal interface ISelfUser : IUser, ISnowflakeEntity, IEntity<ulong>, IMentionable, IPresence
{
	string Email { get; }

	bool IsVerified { get; }

	bool IsMfaEnabled { get; }

	UserProperties Flags { get; }

	PremiumType PremiumType { get; }

	string Locale { get; }

	Task ModifyAsync(Action<SelfUserProperties> func, RequestOptions options = null);
}
