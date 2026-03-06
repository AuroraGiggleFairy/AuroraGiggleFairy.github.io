using System.IO;

public abstract class AIDirectorComponent
{
	public AIDirector Director;

	public GameRandom Random => Director.random;

	public virtual void Connect()
	{
	}

	public virtual void InitNewGame()
	{
	}

	public virtual void Tick(double _dt)
	{
	}

	public virtual void Read(BinaryReader _stream, int _version)
	{
	}

	public virtual void Write(BinaryWriter _stream)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AIDirectorComponent()
	{
	}
}
