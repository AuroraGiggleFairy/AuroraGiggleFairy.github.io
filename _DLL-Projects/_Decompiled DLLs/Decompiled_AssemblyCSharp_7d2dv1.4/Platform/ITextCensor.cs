using System;

namespace Platform;

public interface ITextCensor
{
	void Init(IPlatform _owner);

	void Update();

	void CensorProfanity(string _input, Action<CensoredTextResult> _censoredCallback);
}
