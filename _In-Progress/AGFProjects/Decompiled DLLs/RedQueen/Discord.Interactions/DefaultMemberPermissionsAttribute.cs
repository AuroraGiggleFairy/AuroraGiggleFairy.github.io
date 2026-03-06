using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
internal class DefaultMemberPermissionsAttribute : Attribute
{
	public GuildPermission Permissions { get; }

	public DefaultMemberPermissionsAttribute(GuildPermission permissions)
	{
		Permissions = permissions;
	}
}
