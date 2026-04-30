using System.Collections.Generic;

namespace Discord.Commands;

internal class CommandServiceConfig
{
	public RunMode DefaultRunMode { get; set; } = RunMode.Sync;

	public char SeparatorChar { get; set; } = ' ';

	public bool CaseSensitiveCommands { get; set; }

	public LogSeverity LogLevel { get; set; } = LogSeverity.Info;

	public bool ThrowOnError { get; set; } = true;

	public Dictionary<char, char> QuotationMarkAliasMap { get; set; } = QuotationAliasUtils.GetDefaultAliasMap;

	public bool IgnoreExtraArgs { get; set; }
}
