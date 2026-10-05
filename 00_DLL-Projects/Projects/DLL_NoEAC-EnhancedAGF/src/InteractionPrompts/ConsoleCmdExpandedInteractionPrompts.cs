using System.Collections.Generic;
using UnityEngine;

namespace ExpandedInteractionPrompts
{
    public class ConsoleCmdExpandedInteractionPrompts : ConsoleCmdAbstract
    {
        public override string[] getCommands()
        {
            return new[]
            {
                "agfep",
                "agf-ep",
                "agfeip",
                "agf-eip"
            };
        }

        public override string getDescription()
        {
            return "Set Expanded Interaction Prompts HUD mode: full, partial, or off.";
        }

        public override string getHelp()
        {
            return "Usage:\n  agfep full\n  agfep partial\n  agfep off\n  agfep status\n";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            _ = _senderInfo;
            string mode = (_params != null && _params.Count > 0 ? _params[0] : "status")?.Trim().ToLowerInvariant();
            switch (mode)
            {
                case "full":
                    PromptHudMode.Set(PromptHudMode.Full);
                    SdtdConsole.Instance.Output("[AGF-EP] HUD mode: full");
                    return;
                case "partial":
                    PromptHudMode.Set(PromptHudMode.Partial);
                    SdtdConsole.Instance.Output("[AGF-EP] HUD mode: partial (header only)");
                    return;
                case "off":
                    PromptHudMode.Set(PromptHudMode.Off);
                    SdtdConsole.Instance.Output("[AGF-EP] HUD mode: off");
                    return;
                default:
                    SdtdConsole.Instance.Output("[AGF-EP] HUD mode: " + PromptHudMode.Name(PromptHudMode.Current));
                    SdtdConsole.Instance.Output(getHelp());
                    return;
            }
        }
    }

    public static class PromptHudMode
    {
        public const int Full = 0;
        public const int Partial = 1;
        public const int Off = 2;
        private const string PrefsKey = "agfEipMode";

        public static int Current => Mathf.Clamp(PlayerPrefs.GetInt(PrefsKey, Full), Full, Off);

        public static void Set(int mode)
        {
            PlayerPrefs.SetInt(PrefsKey, Mathf.Clamp(mode, Full, Off));
            PlayerPrefs.Save();
        }

        public static string Name(int mode)
        {
            switch (mode)
            {
                case Partial:
                    return "partial";
                case Off:
                    return "off";
                default:
                    return "full";
            }
        }
    }
}
