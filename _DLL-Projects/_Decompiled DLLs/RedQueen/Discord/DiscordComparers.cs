using System;
using System.Collections.Generic;

namespace Discord;

internal static class DiscordComparers
{
	private sealed class EntityEqualityComparer<TEntity, TId> : EqualityComparer<TEntity> where TEntity : IEntity<TId> where TId : IEquatable<TId>
	{
		public override bool Equals(TEntity x, TEntity y)
		{
			if (x == null)
			{
				if (y == null)
				{
					return true;
				}
				return false;
			}
			if (y == null)
			{
				return false;
			}
			return x.Id.Equals(y.Id);
		}

		public override int GetHashCode(TEntity obj)
		{
			if (obj == null)
			{
				return 0;
			}
			return obj.Id.GetHashCode();
		}
	}

	public static IEqualityComparer<IUser> UserComparer { get; } = new EntityEqualityComparer<IUser, ulong>();

	public static IEqualityComparer<IGuild> GuildComparer { get; } = new EntityEqualityComparer<IGuild, ulong>();

	public static IEqualityComparer<IChannel> ChannelComparer { get; } = new EntityEqualityComparer<IChannel, ulong>();

	public static IEqualityComparer<IRole> RoleComparer { get; } = new EntityEqualityComparer<IRole, ulong>();

	public static IEqualityComparer<IMessage> MessageComparer { get; } = new EntityEqualityComparer<IMessage, ulong>();
}
