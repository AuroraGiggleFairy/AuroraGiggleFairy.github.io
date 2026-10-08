using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ExpandedInteractionPrompts
{
    public static class GrowthStagePrompt
    {
        private const string GrassSeedName = "plantedtreeGrassSeed1";
        private const string VehicleRespawnModName = "AGF-AutomobilesRespawn";
        private const string HostCVar = ".agfGrowHost";
        private const string ReplyNonceCVar = ".agfGrowPnonce";
        private const string ReplySecondsCVar = ".agfGrowPsec";
        private const string IconStage = "ui_game_symbol_crops";
        private const string IconClock = "ui_game_symbol_clock";
        private const string IconSun = "ui_game_symbol_lightbulb";
        private const string IconSoil = "farmPlotBlock";
        private const string ProblemColor = "ff8000";
        private const float RequestSeconds = 3f;
        private const float HostScanSeconds = 2f;

        private static readonly FieldInfo NextPlantField = AccessTools.Field(typeof(BlockPlantGrowing), "nextPlant");
        private static readonly FieldInfo FertileField = AccessTools.Field(typeof(BlockPlant), "fertileLevel");
        private static readonly FieldInfo LightGrowField = AccessTools.Field(typeof(BlockPlantGrowing), "lightLevelGrow");
        private static readonly FieldInfo TickerDictField = AccessTools.Field(typeof(WorldBlockTicker), "scheduledTicksDict");
        private static readonly FieldInfo PromptTextField = AccessTools.Field(typeof(XUiC_InteractionPrompt), "text");

        private static readonly Dictionary<int, int> NextByType = new Dictionary<int, int>();
        private static readonly Dictionary<int, Stage> Stages = new Dictionary<int, Stage>();
        private static readonly HashSet<string> VehicleSeedNames = new HashSet<string>(StringComparer.Ordinal)
        {
            "carAGFwideSedanPlanted",
            "carAGFsedansPlanted",
            "carAGFwide6LongPlanted",
            "carAGF6LongPlanted",
            "carAGFforkLiftPlanted",
            "carAGFtractorPlanted",
            "carAGFfireTruckPlanted",
            "carAGFarmyTruckPlanted",
            "carAGFfarmTruckPlanted",
            "carAGFboxTrucksPlanted",
            "carAGFbackHoePlanted",
            "carAGFexcavatorPlanted",
            "carAGFexcavatorClawPlanted",
            "carAGFsemiTruckPlanted",
            "carAGFsemiTruck01ModularPlanted",
            "carAGFpolicePlanted",
            "carAGFambulancePlanted",
            "carAGFtruckWorkingStiffPlanted",
            "carAGFbusCityPlanted",
            "carAGFbusSchoolPlanted",
            "carAGFbusSchoolShortPlanted",
            "carAGFbusShuttlePlanted"
        };
        private static bool vehicleRespawnLoaded;
        private static int builtListLength = -1;

        private static int pendingNonce;
        private static int pendingBlock = -1;
        private static Vector3i pendingPos;
        private static float nextRequestAt = -1f;
        private static int sampleNonce = -1;
        private static int sampleSeconds = -1;
        private static float sampleAt = -1f;
        private static float nextHostScanAt = -1f;
        private static int lastPaintSecond = -1;
        private static int lastPaintBlock = -1;
        private static Vector3i lastPaintPos;

        private struct Stage
        {
            public int Index;
            public int Count;
        }

        public static void OnPlayerSpawned(int entityId)
        {
            MarkHost(entityId);
        }

        public static void ServerTick()
        {
            ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
            if (manager == null || !manager.IsServer)
                return;
            if (Time.unscaledTime < nextHostScanAt)
                return;

            nextHostScanAt = Time.unscaledTime + HostScanSeconds;
            var players = GameManager.Instance?.World?.Players?.list;
            if (players == null)
                return;

            for (int i = 0; i < players.Count; i++)
                MarkHost(players[i]);
        }

        public static bool HandleServerQuery(int senderEntityId, string[] parts)
        {
            ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
            if (manager == null || !manager.IsServer)
                return false;
            if (parts == null || parts.Length < 7)
                return true;
            if (!int.TryParse(parts[2], out int x)
                || !int.TryParse(parts[3], out int y)
                || !int.TryParse(parts[4], out int z)
                || !int.TryParse(parts[5], out int blockId)
                || !int.TryParse(parts[6], out int nonce))
                return true;

            World world = GameManager.Instance?.World;
            EntityAlive player = world?.GetEntity(senderEntityId) as EntityAlive;
            if (player?.Buffs == null)
                return true;

            int seconds = QueryRemainingSeconds(world, new Vector3i(x, y, z), blockId);
            player.Buffs.SetCustomVar(ReplySecondsCVar, seconds, true);
            player.Buffs.SetCustomVar(ReplyNonceCVar, nonce, true);
            return true;
        }

        [HarmonyPatch(typeof(XUiC_InteractionPrompt), nameof(XUiC_InteractionPrompt.SetText))]
        public static class Patch_SetText_GrowthStage
        {
            public static void Prefix(ref string _text)
            {
                Decorate(ref _text);
            }
        }

        public static void TickPrompt(XUiC_InteractionPrompt prompt)
        {
            if (prompt?.xui?.playerUI == null || PromptTextField == null)
                return;

            string raw = PromptTextField.GetValue(prompt) as string;
            if (string.IsNullOrEmpty(raw))
                return;
            if (!TryLook(out _, out Vector3i pos, out BlockValue blockValue))
                return;
            if (!TryStage(blockValue.type, out _, out _))
                return;

            int second = Mathf.FloorToInt(Time.unscaledTime);
            if (second == lastPaintSecond && blockValue.type == lastPaintBlock && pos.Equals(lastPaintPos))
                return;

            lastPaintSecond = second;
            lastPaintBlock = blockValue.type;
            lastPaintPos = pos;
            XUiC_InteractionPrompt.SetText(prompt.xui.playerUI, raw);
        }

        private static void Decorate(ref string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            if (!TryLook(out World world, out Vector3i pos, out BlockValue blockValue))
                return;

            EnsureChains();
            if (!TryStage(blockValue.type, out int index, out int count))
                return;

            PromptStateHelpers.SplitPrompt(text, out string header, out _);
            var lines = new List<string>(2);
            lines.Add(PromptStateHelpers.FormatIconRow(
                IconStage,
                string.Format(Localization.Get("agfGrowStageAGF"), index, count),
                PromptStateHelpers.SlotL0));

            if (index < count)
            {
                if (NeedsRicherSoil(world, pos, blockValue))
                {
                    lines.Add(PromptStateHelpers.FormatIconRow(
                        IconSoil, Localization.Get("agfGrowSoilAGF"), PromptStateHelpers.SlotR0, ProblemColor));
                }
                else if (NeedsSunlight(world, pos, blockValue))
                {
                    lines.Add(PromptStateHelpers.FormatIconRow(
                        IconSun, Localization.Get("agfGrowSunAGF"), PromptStateHelpers.SlotR0, ProblemColor));
                }
                else if (TryRemainingSeconds(world, pos, blockValue.type, out int seconds))
                {
                    lines.Add(PromptStateHelpers.FormatIconRow(
                        IconClock, FormatClock(seconds), PromptStateHelpers.SlotR0));
                }
            }

            text = PromptStateHelpers.BuildPrompt(header, lines);
        }

        private static bool TryLook(out World world, out Vector3i pos, out BlockValue blockValue)
        {
            world = GameManager.Instance?.World;
            pos = Vector3i.zero;
            blockValue = BlockValue.Air;
            if (world == null)
                return false;

            EntityPlayerLocal player = world.GetPrimaryPlayer();
            WorldRayHitInfo hit = player?.HitInfo;
            if (player == null || !player.IsAlive() || player.AttachedToEntity != null || hit == null || !hit.bHitValid)
                return false;
            if (!string.IsNullOrEmpty(hit.tag) && (hit.tag.StartsWith("E_") || hit.tag == "Item"))
                return false;

            pos = LockableStationsClient.ResolveParentIfChild(world, hit.hit.blockPos);
            blockValue = world.GetBlock(pos);
            return blockValue.Block != null && !blockValue.isair;
        }

        private static bool TryRemainingSeconds(World world, Vector3i pos, int blockId, out int seconds)
        {
            seconds = 0;
            if (world == null)
                return false;
            if (!world.IsRemote())
                return TryLocalSeconds(world, pos, blockId, out seconds);

            RequestRemote(pos, blockId);
            return TryCachedSeconds(pos, blockId, out seconds);
        }

        private static bool TryLocalSeconds(World world, Vector3i pos, int blockId, out int seconds)
        {
            seconds = 0;
            int found = QueryRemainingSeconds(world, pos, blockId);
            if (found < 0)
                return false;
            seconds = found;
            return true;
        }

        private static int QueryRemainingSeconds(World world, Vector3i pos, int blockId)
        {
            if (world == null || blockId <= 0)
                return -1;

            EnsureChains();
            if (!Stages.ContainsKey(blockId))
                return -1;

            BlockValue blockValue = world.GetBlock(pos);
            if (blockValue.type != blockId)
                return -1;

            try
            {
                WorldBlockTicker ticker = world.GetWBT();
                var dict = TickerDictField?.GetValue(ticker) as Dictionary<int, WorldBlockTickerEntry>;
                if (dict == null)
                    return -1;
                if (!dict.TryGetValue(WorldBlockTickerEntry.ToHashCode(pos, blockId), out WorldBlockTickerEntry entry) || entry == null)
                    return -1;

                float ticksPerSecond = GameTimer.Instance != null ? GameTimer.Instance.ticksPerSecond : 20f;
                if (ticksPerSecond <= 0f)
                    ticksPerSecond = 20f;

                ulong now = GameTimer.Instance != null ? GameTimer.Instance.ticks : 0UL;
                if (entry.scheduledTime <= now)
                    return 0;

                return Mathf.CeilToInt((entry.scheduledTime - now) / ticksPerSecond);
            }
            catch
            {
                return -1;
            }
        }

        private static void RequestRemote(Vector3i pos, int blockId)
        {
            if (!HostReady())
                return;
            if (pendingBlock == blockId && pendingPos.Equals(pos) && Time.unscaledTime < nextRequestAt)
                return;

            EntityPlayerLocal player = GameManager.Instance?.World?.GetPrimaryPlayer();
            ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
            if (player == null || manager == null || manager.IsServer)
                return;

            pendingNonce++;
            if (pendingNonce > 1000000)
                pendingNonce = 1;
            pendingBlock = blockId;
            pendingPos = pos;
            nextRequestAt = Time.unscaledTime + RequestSeconds;

            NetPackageChat package = NetPackageManager.GetPackage<NetPackageChat>();
            if (package == null)
                return;

            string message = "/agfep grow " + pos.x + " " + pos.y + " " + pos.z + " " + blockId + " " + pendingNonce;
            manager.SendToServer(package.Setup(
                EChatType.Global,
                player.entityId,
                message,
                null,
                EMessageSender.SenderIdAsPlayer,
                GeneratedTextManager.BbCodeSupportMode.Supported));
        }

        private static bool TryCachedSeconds(Vector3i pos, int blockId, out int seconds)
        {
            seconds = 0;
            EntityPlayerLocal player = GameManager.Instance?.World?.GetPrimaryPlayer();
            EntityBuffs buffs = player?.Buffs;
            if (buffs == null || !buffs.HasCustomVar(ReplyNonceCVar))
                return false;

            int nonce = Mathf.RoundToInt(buffs.GetCustomVar(ReplyNonceCVar));
            if (nonce != pendingNonce || pendingBlock != blockId || !pendingPos.Equals(pos))
                return false;

            int reported = Mathf.RoundToInt(buffs.GetCustomVar(ReplySecondsCVar));
            if (nonce != sampleNonce)
            {
                sampleNonce = nonce;
                sampleSeconds = reported;
                sampleAt = Time.unscaledTime;
            }

            if (sampleSeconds < 0)
                return false;

            seconds = Mathf.Max(0, sampleSeconds - Mathf.FloorToInt(Time.unscaledTime - sampleAt));
            return true;
        }

        private static bool HostReady()
        {
            EntityPlayerLocal player = GameManager.Instance?.World?.GetPrimaryPlayer();
            EntityBuffs buffs = player?.Buffs;
            return buffs != null && buffs.HasCustomVar(HostCVar) && buffs.GetCustomVar(HostCVar) >= 1f;
        }

        private static bool NeedsRicherSoil(World world, Vector3i pos, BlockValue blockValue)
        {
            if (!(blockValue.Block is BlockPlantGrowing growing) || NextPlantField == null)
                return false;

            BlockValue next = (BlockValue)NextPlantField.GetValue(growing);
            if (next.isair || !(next.Block is BlockPlant nextPlant) || FertileField == null)
                return false;

            int need = (int)FertileField.GetValue(nextPlant);
            if (need <= 0)
                return false;

            Vector3i soilPos = pos + Vector3i.down;
            BlockValue soil = world.GetBlock(soilPos);
            int have = soil.Block != null ? soil.Block.blockMaterial.FertileLevel : 0;
            return have < need;
        }

        private static bool NeedsSunlight(World world, Vector3i pos, BlockValue blockValue)
        {
            if (!(blockValue.Block is BlockPlantGrowing growing))
                return false;
            if (world.IsOpenSkyAbove(pos.x, pos.y, pos.z))
                return false;

            int need = 8;
            if (LightGrowField != null)
                need = (int)LightGrowField.GetValue(growing);
            if (need <= 0)
                return false;

            ChunkCluster cache = world.ChunkCache;
            if (cache == null)
                return false;

            byte sun = cache.GetLight(pos + Vector3i.up, Chunk.LIGHT_TYPE.SUN);
            return sun < need;
        }

        private static string FormatClock(int seconds)
        {
            if (seconds < 0)
                seconds = 0;
            int mins = seconds / 60;
            int secs = seconds % 60;
            return mins.ToString("00") + ":" + secs.ToString("00");
        }

        private static bool TryStage(int blockId, out int index, out int count)
        {
            index = 0;
            count = 0;
            EnsureChains();
            if (!Stages.TryGetValue(blockId, out Stage stage))
                return false;
            index = stage.Index;
            count = stage.Count;
            return count > 0;
        }

        private static void EnsureChains()
        {
            Block[] list = Block.list;
            int length = list != null ? list.Length : 0;
            if (length == builtListLength && Stages.Count > 0)
                return;

            vehicleRespawnLoaded = IsVehicleRespawnLoaded();
            builtListLength = length;
            NextByType.Clear();
            Stages.Clear();
            if (list == null || NextPlantField == null)
                return;

            for (int i = 0; i < list.Length; i++)
            {
                if (!(list[i] is BlockPlantGrowing growing))
                    continue;
                BlockValue next = (BlockValue)NextPlantField.GetValue(growing);
                if (next.isair || next.type == growing.blockID)
                    continue;
                NextByType[growing.blockID] = next.type;
            }

            var targeted = new HashSet<int>(NextByType.Values);
            foreach (var pair in NextByType)
            {
                Block start = list[pair.Key];
                if (start == null || targeted.Contains(pair.Key))
                    continue;
                bool playerStart = start.CreativeMode == EnumCreativeMode.Player || start.CreativeMode == EnumCreativeMode.All;
                if (!playerStart && !IsVehicleRespawnSeed(start))
                    continue;

                var chain = new List<int>(8);
                int id = pair.Key;
                int guard = 0;
                bool grass = false;
                while (NextByType.ContainsKey(id) && guard++ < 32)
                {
                    if (chain.Contains(id))
                        break;
                    Block step = id >= 0 && id < list.Length ? list[id] : null;
                    if (step != null && step.GetBlockName() == GrassSeedName)
                        grass = true;
                    chain.Add(id);
                    id = NextByType[id];
                }

                if (id > 0 && !chain.Contains(id))
                {
                    Block end = id < list.Length ? list[id] : null;
                    if (end != null && end.GetBlockName() == GrassSeedName)
                        grass = true;
                    chain.Add(id);
                }

                if (grass || chain.Count == 0)
                    continue;

                for (int s = 0; s < chain.Count; s++)
                {
                    if (Stages.ContainsKey(chain[s]))
                        continue;
                    Stages[chain[s]] = new Stage { Index = s + 1, Count = chain.Count };
                }
            }
        }

        private static bool IsVehicleRespawnLoaded()
        {
            try
            {
                return ModManager.GetMod(VehicleRespawnModName) != null;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsVehicleRespawnSeed(Block block)
        {
            if (!vehicleRespawnLoaded || block == null)
                return false;
            string name = block.GetBlockName();
            return !string.IsNullOrEmpty(name) && VehicleSeedNames.Contains(name);
        }

        private static void MarkHost(int entityId)
        {
            World world = GameManager.Instance?.World;
            MarkHost(world?.GetEntity(entityId) as EntityAlive);
        }

        private static void MarkHost(EntityAlive player)
        {
            ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
            if (manager == null || !manager.IsServer || player?.Buffs == null)
                return;
            if (player.Buffs.HasCustomVar(HostCVar) && player.Buffs.GetCustomVar(HostCVar) >= 1f)
                return;
            player.Buffs.SetCustomVar(HostCVar, 1f, true);
        }
    }
}
