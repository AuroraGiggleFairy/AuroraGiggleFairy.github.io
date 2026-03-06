namespace Platform;

public interface IPlatformUser
{
	PlatformUserIdentifierAbs PrimaryId { get; }

	PlatformUserIdentifierAbs NativeId { get; set; }
}
