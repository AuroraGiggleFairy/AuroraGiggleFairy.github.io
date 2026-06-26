namespace Platform;

public interface IPlatformUserBlockedData
{
	EBlockType Type { get; }

	EUserBlockState State { get; }

	bool Locally { get; set; }
}
