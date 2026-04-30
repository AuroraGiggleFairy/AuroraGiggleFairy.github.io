using System.Text;

public class CallbackMetric : IMetric
{
	public delegate string GetLastValue();

	public GetLastValue callback;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Header { get; set; }

	public void AppendLastValue(StringBuilder builder)
	{
		builder.Append(callback());
	}

	public void Cleanup()
	{
	}
}
