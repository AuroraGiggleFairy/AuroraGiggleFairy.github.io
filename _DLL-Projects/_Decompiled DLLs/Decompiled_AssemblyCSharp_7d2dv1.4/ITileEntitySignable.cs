public interface ITileEntitySignable : ITileEntity
{
	void SetText(AuthoredText _authoredText, bool _syncData = true);

	void SetText(string _text, bool _syncData = true, PlatformUserIdentifierAbs _signingPlayer = null);

	AuthoredText GetAuthoredText();

	bool CanRenderString(string _text);
}
