using System.Text;

public interface IMetric
{
	string Header { get; }

	void AppendLastValue(StringBuilder builder);

	void Cleanup();
}
