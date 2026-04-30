using System;

namespace Platform;

public interface IVirtualKeyboard
{
	void Init(IPlatform _owner);

	string Open(string _title, string _defaultText, Action<bool, string> _onTextReceived, UIInput.InputType _mode = UIInput.InputType.Standard, bool _multiLine = false, uint singleLineLength = 200u);

	void Destroy();
}
