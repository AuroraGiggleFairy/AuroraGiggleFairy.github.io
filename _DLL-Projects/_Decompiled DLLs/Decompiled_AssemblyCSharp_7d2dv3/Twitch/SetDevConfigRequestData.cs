using System;
using System.Collections.Generic;

namespace Twitch;

[Serializable]
public class SetDevConfigRequestData
{
	public List<string> actionTypes;

	public List<string> players;
}
