using System.Runtime.CompilerServices;

namespace Discord;

internal struct Overwrite
{
	public ulong TargetId
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public PermissionTarget TargetType
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public OverwritePermissions Permissions
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public Overwrite(ulong targetId, PermissionTarget targetType, OverwritePermissions permissions)
	{
		TargetId = targetId;
		TargetType = targetType;
		Permissions = permissions;
	}
}
