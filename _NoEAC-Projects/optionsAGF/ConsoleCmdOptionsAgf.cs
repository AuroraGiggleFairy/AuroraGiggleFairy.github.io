using System;
using System.Collections.Generic;

public class ConsoleCmdOptionsAgf : ConsoleCmdAbstract
{
    public override string[] getCommands()
    {
        return new[] { "agfoptions", "optionsagf" };
    }

    public override string getDescription()
    {
        return "Manage optionsAGF settings.";
    }

    public override string getHelp()
    {
        return "Usage:\n" +
               "  agfoptions\n" +
               "  agfoptions <option_key> <off|on|numbers|cycle>\n" +
               "Example:\n" +
               "  agfoptions screamer_alert_mode numbers";
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        OptionsRegistry.EnsureInitialized();
        var rows = OptionsRegistry.GetRows();

        if (_params == null || _params.Count == 0)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                bool available = row.IsAvailable();
                OptionMode mode = OptionsRegistry.GetMode(row.Key, row.DefaultMode);
                SdtdConsole.Instance.Output((i + 1) + ". " + row.Key + " = " + (available ? OptionsRegistry.GetModeLabel(mode) : "Not Installed"));
            }
            return;
        }

        if (_params.Count < 2)
        {
            SdtdConsole.Instance.Output("Missing mode argument. Use: off | on | numbers | cycle");
            return;
        }

        string key = _params[0];
        string modeText = _params[1];
        OptionsRegistry.OptionRow target = null;
        foreach (var row in rows)
        {
            if (string.Equals(row.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                target = row;
                break;
            }
        }

        if (target == null)
        {
            SdtdConsole.Instance.Output("Unknown option key: " + key);
            return;
        }

        if (!target.IsAvailable())
        {
            SdtdConsole.Instance.Output("Option is unavailable because required mod/content is not loaded: " + target.Key);
            return;
        }

        OptionMode current = OptionsRegistry.GetMode(target.Key, target.DefaultMode);
        OptionMode next;
        if (!TryParseMode(modeText, current, out next))
        {
            SdtdConsole.Instance.Output("Invalid mode: " + modeText + ". Use off | on | numbers | cycle");
            return;
        }

        OptionsRegistry.SetMode(target.Key, next);
        XUiC_OptionsAgfUi.MarkAllDirty();
        SdtdConsole.Instance.Output(target.Key + " set to " + OptionsRegistry.GetModeLabel(next));
    }

    private static bool TryParseMode(string text, OptionMode current, out OptionMode mode)
    {
        mode = current;
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        switch (text.Trim().ToLowerInvariant())
        {
            case "0":
            case "off":
                mode = OptionMode.Off;
                return true;
            case "1":
            case "on":
                mode = OptionMode.On;
                return true;
            case "2":
            case "numbers":
            case "onwithnumbers":
            case "on+#":
                mode = OptionMode.OnWithNumbers;
                return true;
            case "cycle":
                mode = current == OptionMode.Off
                    ? OptionMode.On
                    : (current == OptionMode.On ? OptionMode.OnWithNumbers : OptionMode.Off);
                return true;
            default:
                return false;
        }
    }
}
