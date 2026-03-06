namespace Platform;

public interface IPlatformUserBlockedResults
{
	IPlatformUser User { get; }

	void Block(EBlockType blockType);

	void BlockAll()
	{
		foreach (EBlockType item in EnumUtils.Values<EBlockType>())
		{
			Block(item);
		}
	}

	void Error();
}
