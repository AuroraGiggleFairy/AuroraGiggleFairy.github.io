using System.Collections.Generic;

namespace Discord;

internal interface IConnection
{
	string Id { get; }

	string Name { get; }

	string Type { get; }

	bool? IsRevoked { get; }

	IReadOnlyCollection<IIntegration> Integrations { get; }

	bool Verified { get; }

	bool FriendSync { get; }

	bool ShowActivity { get; }

	ConnectionVisibility Visibility { get; }
}
