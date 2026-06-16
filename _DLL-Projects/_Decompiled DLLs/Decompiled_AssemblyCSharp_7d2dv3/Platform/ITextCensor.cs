using System;

namespace Platform;

public interface ITextCensor
{
	void Init(IPlatform _owner);

	void Update();

	void CensorProfanity(string _input, PlatformUserIdentifierAbs _author, Action<CensoredTextResult> _censoredCallback);
}
