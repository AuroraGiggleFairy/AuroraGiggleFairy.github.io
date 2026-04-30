using System.Text;

public class ConstantValueMetric : IMetric
{
	public int value;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Header { get; set; }

	public void AppendLastValue(StringBuilder builder)
	{
		builder.Append(value);
	}

	public void Cleanup()
	{
	}
}
