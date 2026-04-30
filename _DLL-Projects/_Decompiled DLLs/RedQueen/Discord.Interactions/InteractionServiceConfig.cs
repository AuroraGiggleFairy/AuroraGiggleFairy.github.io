using System.Threading.Tasks;

namespace Discord.Interactions;

internal class InteractionServiceConfig
{
	public LogSeverity LogLevel { get; set; } = LogSeverity.Info;

	public RunMode DefaultRunMode { get; set; } = RunMode.Async;

	public bool ThrowOnError { get; set; } = true;

	public char[] InteractionCustomIdDelimiters { get; set; }

	public string WildCardExpression { get; set; } = "*";

	public bool UseCompiledLambda { get; set; }

	public bool EnableAutocompleteHandlers { get; set; } = true;

	public bool AutoServiceScopes { get; set; } = true;

	public RestResponseCallback RestResponseCallback { get; set; } = (IInteractionContext ctx, string str) => Task.CompletedTask;

	public bool ExitOnMissingModalField { get; set; }

	public ILocalizationManager LocalizationManager { get; set; }
}
