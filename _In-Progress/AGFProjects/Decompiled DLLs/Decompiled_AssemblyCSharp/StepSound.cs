public class StepSound
{
	public static StepSound stone = new StepSound("stone");

	public string name;

	public StepSound(string _name)
	{
		name = _name;
	}

	public static StepSound FromString(string _name)
	{
		return new StepSound(_name);
	}
}
