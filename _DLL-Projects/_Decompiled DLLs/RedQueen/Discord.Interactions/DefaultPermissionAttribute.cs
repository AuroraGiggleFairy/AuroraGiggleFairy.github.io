using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
[Obsolete("Soon to be deprecated, use Permissions-v2 attributes like EnabledInDmAttribute and DefaultMemberPermissionsAttribute")]
internal class DefaultPermissionAttribute : Attribute
{
	public bool IsDefaultPermission { get; }

	public DefaultPermissionAttribute(bool isDefaultPermission)
	{
		IsDefaultPermission = isDefaultPermission;
	}
}
