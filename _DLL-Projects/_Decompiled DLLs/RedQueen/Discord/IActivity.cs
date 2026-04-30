namespace Discord;

internal interface IActivity
{
	string Name { get; }

	ActivityType Type { get; }

	ActivityProperties Flags { get; }

	string Details { get; }
}
