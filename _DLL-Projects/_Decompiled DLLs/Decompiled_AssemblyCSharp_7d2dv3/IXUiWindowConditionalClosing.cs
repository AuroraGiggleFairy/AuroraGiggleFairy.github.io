using System;

public interface IXUiWindowConditionalClosing
{
	void TryClose(Action _onClosed, Action _onCancelled);
}
