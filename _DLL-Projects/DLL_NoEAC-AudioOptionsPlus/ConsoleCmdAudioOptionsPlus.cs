using System;
using System.Collections.Generic;

namespace AudioOptionsPlus;

public class ConsoleCmdAudioOptionsPlus : ConsoleCmdAbstract
{
    public override string[] getCommands()
    {
        return new[] { "aop", "audiooptionsplus", "rmv" };
    }

    public override string getDescription()
    {
        return "Manage AudioOptionsPlus at runtime.";
    }

    public override string getHelp()
    {
        return "Usage:\n"
            + "  aop\n"
            + "  aop reload\n"
            + "Description:\n"
            + "  reload - Reload AudioOptionsPlus settings and reset runtime tracking.";
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        if (_params == null || _params.Count == 0)
        {
            SdtdConsole.Instance.Output("AudioOptionsPlus commands: reload");
            return;
        }

        string sub = _params[0]?.Trim().ToLowerInvariant() ?? string.Empty;
        if (sub == "reload")
        {
            try
            {
                AudioOptionsPlusConfig.Load();
                AudioOptionsPlusRuntime.ResetForGameStart();
                SdtdConsole.Instance.Output("[AudioOptionsPlus] Settings reloaded.");
            }
            catch (Exception ex)
            {
                SdtdConsole.Instance.Output("[AudioOptionsPlus] Reload failed: " + ex.Message);
            }

            return;
        }

        SdtdConsole.Instance.Output("Unknown subcommand: " + sub + ". Use: aop reload");
    }
}

