public interface ILaunchPref
{
	string Name { get; }

	bool TrySet(string stringRepresentation);
}
public interface ILaunchPref<out T> : ILaunchPref
{
	T Value { get; }
}
