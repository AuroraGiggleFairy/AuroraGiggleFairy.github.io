using System.Collections.Generic;

namespace LootTimerHolds
{
    public class ConsoleCmdLootTimerHolds : ConsoleCmdAbstract
    {
        public override string[] getCommands()
        {
            return new[]
            {
                "agf-lt",
                "agflt",
                "agf-loottimerholds"
            };
        }

        public override string getDescription()
        {
            return "Show whether world loot restock timers hold after reopen/proximity.";
        }

        public override string getHelp()
        {
            return "Usage:\n  agflt status\n";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            _ = _senderInfo;
            int days = GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays);
            SdtdConsole.Instance.Output("[AGF-LT] Loot Timer Holds is active.");
            SdtdConsole.Instance.Output("[AGF-LT] Loot Respawn Days: " + days);
            SdtdConsole.Instance.Output("[AGF-LT] World loot keeps its first-open stamp; empty+due still restocks.");
        }
    }
}
