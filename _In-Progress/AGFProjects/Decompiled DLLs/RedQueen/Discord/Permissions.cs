using System.Runtime.CompilerServices;

namespace Discord;

internal static class Permissions
{
	public const int MaxBits = 53;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PermValue GetValue(ulong allow, ulong deny, ChannelPermission flag)
	{
		return GetValue(allow, deny, (ulong)flag);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PermValue GetValue(ulong allow, ulong deny, GuildPermission flag)
	{
		return GetValue(allow, deny, (ulong)flag);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PermValue GetValue(ulong allow, ulong deny, ulong flag)
	{
		if (HasFlag(allow, flag))
		{
			return PermValue.Allow;
		}
		if (HasFlag(deny, flag))
		{
			return PermValue.Deny;
		}
		return PermValue.Inherit;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool GetValue(ulong value, ChannelPermission flag)
	{
		return GetValue(value, (ulong)flag);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool GetValue(ulong value, GuildPermission flag)
	{
		return GetValue(value, (ulong)flag);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool GetValue(ulong value, ulong flag)
	{
		return HasFlag(value, flag);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetValue(ref ulong rawValue, bool? value, ChannelPermission flag)
	{
		SetValue(ref rawValue, value, (ulong)flag);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetValue(ref ulong rawValue, bool? value, GuildPermission flag)
	{
		SetValue(ref rawValue, value, (ulong)flag);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetValue(ref ulong rawValue, bool? value, ulong flag)
	{
		if (value.HasValue)
		{
			if (value == true)
			{
				SetFlag(ref rawValue, flag);
			}
			else
			{
				UnsetFlag(ref rawValue, flag);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetValue(ref ulong allow, ref ulong deny, PermValue? value, ChannelPermission flag)
	{
		SetValue(ref allow, ref deny, value, (ulong)flag);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetValue(ref ulong allow, ref ulong deny, PermValue? value, GuildPermission flag)
	{
		SetValue(ref allow, ref deny, value, (ulong)flag);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetValue(ref ulong allow, ref ulong deny, PermValue? value, ulong flag)
	{
		if (value.HasValue)
		{
			switch (value)
			{
			case PermValue.Allow:
				SetFlag(ref allow, flag);
				UnsetFlag(ref deny, flag);
				break;
			case PermValue.Deny:
				UnsetFlag(ref allow, flag);
				SetFlag(ref deny, flag);
				break;
			default:
				UnsetFlag(ref allow, flag);
				UnsetFlag(ref deny, flag);
				break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool HasFlag(ulong value, ulong flag)
	{
		return (value & flag) == flag;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetFlag(ref ulong value, ulong flag)
	{
		value |= flag;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void UnsetFlag(ref ulong value, ulong flag)
	{
		value &= ~flag;
	}

	public static ChannelPermissions ToChannelPerms(IGuildChannel channel, ulong guildPermissions)
	{
		return new ChannelPermissions(guildPermissions & ChannelPermissions.All(channel).RawValue);
	}

	public static ulong ResolveGuild(IGuild guild, IGuildUser user)
	{
		ulong num = 0uL;
		if (user.Id == guild.OwnerId)
		{
			num = GuildPermissions.All.RawValue;
		}
		else if (user.IsWebhook)
		{
			num = GuildPermissions.Webhook.RawValue;
		}
		else
		{
			foreach (ulong roleId in user.RoleIds)
			{
				num |= guild.GetRole(roleId)?.Permissions.RawValue ?? 0;
			}
			if (GetValue(num, GuildPermission.Administrator))
			{
				num = GuildPermissions.All.RawValue;
			}
		}
		return num;
	}

	public static ulong ResolveChannel(IGuild guild, IGuildUser user, IGuildChannel channel, ulong guildPermissions)
	{
		ulong num = 0uL;
		ulong rawValue = ChannelPermissions.All(channel).RawValue;
		if (GetValue(guildPermissions, GuildPermission.Administrator))
		{
			return rawValue;
		}
		num = guildPermissions;
		OverwritePermissions? permissionOverwrite = channel.GetPermissionOverwrite(guild.EveryoneRole);
		if (permissionOverwrite.HasValue)
		{
			num = (num & ~permissionOverwrite.Value.DenyValue) | permissionOverwrite.Value.AllowValue;
		}
		ulong num2 = 0uL;
		ulong num3 = 0uL;
		foreach (ulong roleId in user.RoleIds)
		{
			IRole role;
			if (roleId != guild.EveryoneRole.Id && (role = guild.GetRole(roleId)) != null)
			{
				permissionOverwrite = channel.GetPermissionOverwrite(role);
				if (permissionOverwrite.HasValue)
				{
					num3 |= permissionOverwrite.Value.AllowValue;
					num2 |= permissionOverwrite.Value.DenyValue;
				}
			}
		}
		num = (num & ~num2) | num3;
		permissionOverwrite = channel.GetPermissionOverwrite(user);
		if (permissionOverwrite.HasValue)
		{
			num = (num & ~permissionOverwrite.Value.DenyValue) | permissionOverwrite.Value.AllowValue;
		}
		if (channel is ITextChannel)
		{
			if (!GetValue(num, ChannelPermission.ViewChannel))
			{
				num = 0uL;
			}
			else if (!GetValue(num, ChannelPermission.SendMessages))
			{
				num &= 0xFFFFFFFFFFFFEFFFuL;
				num &= 0xFFFFFFFFFFFDFFFFuL;
				num &= 0xFFFFFFFFFFFFBFFFuL;
				num &= 0xFFFFFFFFFFFF7FFFuL;
			}
		}
		return num & rawValue;
	}
}
