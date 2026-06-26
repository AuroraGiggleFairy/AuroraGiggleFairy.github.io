public class BlockRule<O, M>
{
	public O Output;

	public M[] Mask;

	public BlockRule()
	{
	}

	public BlockRule(O _output, M[] _mask)
	{
		Output = _output;
		Mask = _mask;
	}
}
