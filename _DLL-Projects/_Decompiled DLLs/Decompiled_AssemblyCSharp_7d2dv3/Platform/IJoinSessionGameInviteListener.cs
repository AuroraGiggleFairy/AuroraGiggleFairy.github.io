using System;
using System.Collections;

namespace Platform;

public interface IJoinSessionGameInviteListener
{
	void Init(IPlatform _owner);

	(string invite, string password) TakePendingInvite();

	IEnumerator ConnectToInvite(string _invite, string _password = null, Action<bool> _onFinished = null);

	string GetListenerIdentifier();
}
