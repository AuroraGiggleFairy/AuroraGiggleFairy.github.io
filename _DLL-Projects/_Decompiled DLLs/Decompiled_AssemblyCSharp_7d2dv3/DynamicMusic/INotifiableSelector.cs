namespace DynamicMusic;

[PublicizedFrom(EAccessModifier.Internal)]
public interface INotifiableSelector<T1, T2> : INotifiable<T1>, ISelector<T2>
{
}
