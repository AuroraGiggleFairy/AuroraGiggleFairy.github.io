using System.Net;

namespace Twitch;

public class TwitchExtensionCommand
{
	public int userId;

	public string command;

	public int id;

	public bool isRerun;

	public TwitchExtensionCommand(HttpListenerRequest _req)
	{
		userId = StringParsers.ParseSInt32(_req.QueryString.Get(0));
		command = "#" + _req.QueryString.Get(1);
		id = StringParsers.ParseSInt32(_req.QueryString.Get(2));
		isRerun = StringParsers.ParseBool(_req.QueryString.Get(3));
		Log.Out($"{userId}: {command} : {id}");
	}
}
