namespace DynamicMusic;

public interface INotifiable
{
	void Notify();
}
public interface INotifiable<T>
{
	void Notify(T _state);
}
