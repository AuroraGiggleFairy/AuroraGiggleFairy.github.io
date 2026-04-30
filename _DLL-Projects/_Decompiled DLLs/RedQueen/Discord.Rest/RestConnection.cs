using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestConnection : IConnection
{
	public string Id { get; private set; }

	public string Name { get; private set; }

	public string Type { get; private set; }

	public bool? IsRevoked { get; private set; }

	public IReadOnlyCollection<IIntegration> Integrations { get; private set; }

	public bool Verified { get; private set; }

	public bool FriendSync { get; private set; }

	public bool ShowActivity { get; private set; }

	public ConnectionVisibility Visibility { get; private set; }

	internal BaseDiscordClient Discord { get; }

	private string DebuggerDisplay => Name + " (" + Id + ", " + Type + ((IsRevoked == true) ? ", Revoked" : "") + ")";

	internal RestConnection(BaseDiscordClient discord)
	{
		Discord = discord;
	}

	internal static RestConnection Create(BaseDiscordClient discord, Connection model)
	{
		RestConnection restConnection = new RestConnection(discord);
		restConnection.Update(model);
		return restConnection;
	}

	internal void Update(Connection model)
	{
		Id = model.Id;
		Name = model.Name;
		Type = model.Type;
		IsRevoked = (model.Revoked.IsSpecified ? new bool?(model.Revoked.Value) : ((bool?)null));
		object integrations;
		if (!model.Integrations.IsSpecified)
		{
			integrations = null;
		}
		else
		{
			IReadOnlyCollection<IIntegration> readOnlyCollection = model.Integrations.Value.Select((Integration intergration) => RestIntegration.Create(Discord, null, intergration)).ToImmutableArray();
			integrations = readOnlyCollection;
		}
		Integrations = (IReadOnlyCollection<IIntegration>)integrations;
		Verified = model.Verified;
		FriendSync = model.FriendSync;
		ShowActivity = model.ShowActivity;
		Visibility = model.Visibility;
	}

	public override string ToString()
	{
		return Name;
	}
}
