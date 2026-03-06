public class QTDataElement
{
	public byte[] Data;

	public long Key;

	public int LLPosX;

	public int LLPosY;

	public QTDataElement()
	{
		Data = null;
		Key = 0L;
		LLPosX = 0;
		LLPosY = 0;
	}

	public QTDataElement(int _LLPosX, int _LLPosY, byte[] _Data)
	{
		LLPosX = _LLPosX;
		LLPosY = _LLPosY;
		Data = _Data;
	}
}
