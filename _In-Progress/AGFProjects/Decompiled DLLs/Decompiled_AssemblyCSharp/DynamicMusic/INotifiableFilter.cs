namespace DynamicMusic;

public interface INotifiableFilter<T> : INotifiable, IFilter<T>
{
}
public interface INotifiableFilter<T1, T2> : INotifiable<T1>, IFilter<T2>
{
}
