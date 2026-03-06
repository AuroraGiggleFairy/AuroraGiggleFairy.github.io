namespace Discord;

internal class ApplicationCommandPermission
{
	public ulong TargetId { get; }

	public ApplicationCommandPermissionTarget TargetType { get; }

	public bool Permission { get; }

	internal ApplicationCommandPermission()
	{
	}

	public ApplicationCommandPermission(ulong targetId, ApplicationCommandPermissionTarget targetType, bool allow)
	{
		TargetId = targetId;
		TargetType = targetType;
		Permission = allow;
	}

	public ApplicationCommandPermission(IUser target, bool allow)
	{
		TargetId = target.Id;
		Permission = allow;
		TargetType = ApplicationCommandPermissionTarget.User;
	}

	public ApplicationCommandPermission(IRole target, bool allow)
	{
		TargetId = target.Id;
		Permission = allow;
		TargetType = ApplicationCommandPermissionTarget.Role;
	}
}
