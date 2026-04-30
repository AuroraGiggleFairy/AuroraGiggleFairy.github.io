using System;
using System.Collections.Generic;

namespace Twitch;

[Serializable]
public class ExtensionActionResponse
{
	public List<ExtensionAction> standardActions;

	public List<ExtensionBitAction> bitActions;
}
