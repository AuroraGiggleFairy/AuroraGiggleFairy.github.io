using System.Collections.Generic;
using UnityEngine;

namespace RepairToolsPlus
{
    public class RepairHighlightManager : MonoBehaviour
    {
        private static RepairHighlightManager _instance;
        public static RepairHighlightManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Init();
                }
                return _instance;
            }
        }

        private const float ScanInterval = 0.2f;
        private const int HorizontalRadius = 10;
        private const int VerticalRadius = 5;
        private const float BeamThickness = 0.02f;
        private const float FocusedBeamThickness = 0.045f;

        private float _timeSinceLastScan;
        private bool _isHoldingRepairTool;
        private bool _isActivelyWorking;
        private Vector3i _activeWorkingBlock = Vector3i.zero;

        private TierWireframeRenderer _rendererFocus;
        private TierWireframeRenderer _rendererRed;
        private TierWireframeRenderer _rendererOrange;
        private TierWireframeRenderer _rendererGold;

        private struct ScannedBlock
        {
            public Vector3i Pos;
            public Vector3i Size;
            public int Tier; // 1=Red, 2=Orange, 3=Gold
        }

        private readonly List<ScannedBlock> _scannedBlocks = new List<ScannedBlock>(128);
        private readonly HashSet<Vector3i> _seenPositions = new HashSet<Vector3i>();

        private static readonly Vector3i[] FaceNormals = new Vector3i[]
        {
            new Vector3i(0, 1, 0),
            new Vector3i(0, -1, 0),
            new Vector3i(0, 0, 1),
            new Vector3i(0, 0, -1),
            new Vector3i(1, 0, 0),
            new Vector3i(-1, 0, 0)
        };

        // Canonical edge map: tracks how many blocks share each edge, and the highest priority tier
        private readonly Dictionary<GridEdge, int> _edgeCount = new Dictionary<GridEdge, int>(512);
        private readonly Dictionary<GridEdge, int> _edgeTier = new Dictionary<GridEdge, int>(512);

        public static void Init()
        {
            if (_instance != null) return;

            GameObject go = new GameObject("RepairHighlightManager");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<RepairHighlightManager>();
            Log.Out("[RepairToolsPlus] RepairHighlightManager GameObject instantiated.");
        }

        private void Awake()
        {
            _instance = this;
            InitRenderers();
        }

        private void InitRenderers()
        {
            _rendererFocus = new TierWireframeRenderer();
            _rendererFocus.Init(transform, "Tier_Focus_Green", new Color(0.15f, 1f, 0.35f, 1f));

            _rendererRed = new TierWireframeRenderer();
            _rendererRed.Init(transform, "Tier_Crit_Red", new Color(1f, 0.15f, 0.12f, 1f));

            _rendererOrange = new TierWireframeRenderer();
            _rendererOrange.Init(transform, "Tier_Mid_Orange", new Color(1f, 0.5f, 0.05f, 1f));

            _rendererGold = new TierWireframeRenderer();
            _rendererGold.Init(transform, "Tier_High_Gold", new Color(1f, 0.85f, 0.1f, 1f));
        }

        public void OnHoldingUpdate(EntityPlayerLocal player, ItemActionData actionData)
        {
            SetHoldingState(true);

            // Visually select the working block ONLY while holding down the tool action to repair/upgrade
            bool isSecondaryHeld = player.playerInput != null && player.playerInput.Secondary.IsPressed;
            bool isUseStarted = (actionData as ItemActionRepair.InventoryDataRepair)?.bUseStarted == true;

            if (isSecondaryHeld || isUseStarted)
            {
                Vector3i target = RepairTargetFinder.GetBestTarget(player);
                SetActivelyWorking(true, target);
            }
            else
            {
                SetActivelyWorking(false, Vector3i.zero);
            }
        }

        public void SetActivelyWorking(bool working, Vector3i targetBlock)
        {
            _isActivelyWorking = working;
            _activeWorkingBlock = working ? targetBlock : Vector3i.zero;
        }

        public void OnStopHolding(EntityPlayerLocal player)
        {
            SetHoldingState(false);
            SetActivelyWorking(false, Vector3i.zero);
        }

        public void CheckPeriodicTimeout()
        {
            EntityPlayerLocal player = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (player == null || !RepairToolsRadialManager.IsHoldingRepairTool(player))
            {
                SetHoldingState(false);
                SetActivelyWorking(false, Vector3i.zero);
            }
        }

        public void SetHoldingState(bool holding)
        {
            if (_isHoldingRepairTool == holding) return;
            _isHoldingRepairTool = holding;

            if (!holding)
            {
                ClearVisuals();
            }
            else
            {
                _timeSinceLastScan = ScanInterval;
            }
        }

        private void Update()
        {
            if (!_isHoldingRepairTool) return;

            EntityPlayerLocal player = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (player == null || player.IsDead())
            {
                ClearVisuals();
                return;
            }

            if (!RepairToolsSettings.IsDamageVisionActive)
            {
                ClearVisuals();
                return;
            }

            _timeSinceLastScan += Time.deltaTime;
            if (_timeSinceLastScan >= ScanInterval)
            {
                _timeSinceLastScan = 0f;
                ScanBlocks(player);
            }

            RebuildMergedGrid(player);
        }

        private void ScanBlocks(EntityPlayerLocal player)
        {
            _scannedBlocks.Clear();
            _seenPositions.Clear();

            World world = GameManager.Instance.World;
            if (world == null) return;

            Vector3i playerPos = World.worldToBlockPos(player.position);
            int minX = playerPos.x - HorizontalRadius;
            int maxX = playerPos.x + HorizontalRadius;
            int minZ = playerPos.z - HorizontalRadius;
            int maxZ = playerPos.z + HorizontalRadius;
            int minY = playerPos.y - VerticalRadius;
            int maxY = playerPos.y + VerticalRadius;

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        Vector3i pos = new Vector3i(x, y, z);
                        BlockValue bv = world.GetBlock(pos);
                        if (bv.isair) continue;

                        Block block = bv.Block;
                        if (block == null) continue;

                        // Filter check: All vs Blocks Only vs Terrain Only
                        bool isTerrain = block.shape.IsTerrain();
                        if (RepairToolsSettings.CurrentFilter == DamageFilterMode.BlocksOnly && isTerrain) continue;
                        if (RepairToolsSettings.CurrentFilter == DamageFilterMode.TerrainOnly && !isTerrain) continue;

                        Vector3i rootPos = pos;
                        Vector3i size = Vector3i.one;
                        if (bv.ischild && block.multiBlockPos != null)
                        {
                            rootPos = block.multiBlockPos.GetParentPos(pos, bv);
                            bv = world.GetBlock(rootPos);
                            block = bv.Block;
                        }
                        else if (block.isMultiBlock && block.multiBlockPos != null)
                        {
                            size = block.multiBlockPos.dim;
                        }

                        if (!_seenPositions.Add(rootPos)) continue;

                        // Ambient wireframe grid: damage > 0
                        if (bv.damage > 0 && block != null && block.MaxDamage > 0)
                        {
                            int pct = Mathf.Clamp(Mathf.RoundToInt((1f - (float)bv.damage / block.MaxDamage) * 100f), 0, 99);
                            int tier = (pct < 40) ? 1 : (pct < 75) ? 2 : 3;
                            _scannedBlocks.Add(new ScannedBlock { Pos = rootPos, Size = size, Tier = tier });
                        }
                    }
                }
            }
        }

        private void RebuildMergedGrid(EntityPlayerLocal player)
        {
            _edgeCount.Clear();
            _edgeTier.Clear();

            // 1. Populate all 1-unit edge segments of damaged blocks
            for (int i = 0; i < _scannedBlocks.Count; i++)
            {
                ScannedBlock b = _scannedBlocks[i];
                AddBlockUnitEdgesToMap(b.Pos, b.Size, b.Tier);
            }

            Vector3 originPos = Origin.position;

            _rendererRed.BeginFrame();
            _rendererOrange.BeginFrame();
            _rendererGold.BeginFrame();
            _rendererFocus.BeginFrame();

            // 2. Draw damaged blocks' exterior boundary edges:
            // "if anything shares an edge, that edge should not be shown."
            foreach (KeyValuePair<GridEdge, int> kvp in _edgeCount)
            {
                GridEdge edge = kvp.Key;
                int count = kvp.Value;

                // Shared edge check: if count > 1, 2 or more blocks share this edge -> DO NOT SHOW IT!
                if (count > 1) continue;

                int tier = _edgeTier[edge];
                switch (tier)
                {
                    case 1:
                        _rendererRed.AddBeam(edge, BeamThickness, originPos);
                        break;
                    case 2:
                        _rendererOrange.AddBeam(edge, BeamThickness, originPos);
                        break;
                    case 3:
                        _rendererGold.AddBeam(edge, BeamThickness, originPos);
                        break;
                }
            }

            // 3. Actively worked-on block: ONLY shown while holding down the tool button to repair/upgrade
            // AND ONLY when the block is NOT viewable by the player (e.g. deep inside the wall or occluded)
            if (_isActivelyWorking && _activeWorkingBlock != Vector3i.zero && !IsBlockViewable(player, _activeWorkingBlock))
            {
                World world = GameManager.Instance?.World;
                if (world != null)
                {
                    BlockValue focusedBv = world.GetBlock(_activeWorkingBlock);
                    if (!focusedBv.isair && focusedBv.Block != null)
                    {
                        Vector3i size = Vector3i.one;
                        Vector3i root = _activeWorkingBlock;
                        if (focusedBv.ischild && focusedBv.Block.multiBlockPos != null)
                        {
                            root = focusedBv.Block.multiBlockPos.GetParentPos(_activeWorkingBlock, focusedBv);
                            focusedBv = world.GetBlock(root);
                        }
                        if (focusedBv.Block != null && focusedBv.Block.isMultiBlock && focusedBv.Block.multiBlockPos != null)
                        {
                            size = focusedBv.Block.multiBlockPos.dim;
                        }

                        AddActiveTargetEdges(root, size, originPos);
                    }
                }
            }

            _rendererRed.EndFrame();
            _rendererOrange.EndFrame();
            _rendererGold.EndFrame();
            _rendererFocus.EndFrame();
        }

        public static bool IsBlockViewable(EntityPlayerLocal player, Vector3i targetBlock)
        {
            if (player == null || player.world == null) return false;
            World world = player.world;

            // 1. Surface level check: block directly under the player's crosshair
            if (player.HitInfo != null && player.HitInfo.bHitValid && GameUtils.IsBlockOrTerrain(player.HitInfo.tag))
            {
                Vector3i surfacePos = player.HitInfo.hit.blockPos;
                if (IsSameBlockOrParent(world, surfacePos, targetBlock))
                {
                    return true;
                }
            }

            // 2. Line of sight check from camera to target block
            Transform cam = player.cameraTransform;
            Vector3 camPosUnity = (cam != null) ? cam.position : (player.position + Vector3.up * player.GetEyeHeight() - Origin.position);
            Vector3 camPosWorld = camPosUnity + Origin.position;

            Vector3 blockCenterWorld = targetBlock.ToVector3Center();
            Vector3 toCam = camPosWorld - blockCenterWorld;

            for (int i = 0; i < FaceNormals.Length; i++)
            {
                Vector3i n = FaceNormals[i];

                // Only consider faces directed towards the camera
                float dot = n.x * toCam.x + n.y * toCam.y + n.z * toCam.z;
                if (dot <= 0.05f) continue;

                // Check if the neighbor in this direction is open (air, water, non-solid, transparent)
                Vector3i neighborPos = targetBlock + n;
                BlockValue neighborBv = world.GetBlock(neighborPos);
                if (!neighborBv.isair && neighborBv.Block != null)
                {
                    // If neighbor is an opaque solid cube, this face is completely covered
                    if (neighborBv.Block.shape.IsSolidCube && neighborBv.Block.MeshIndex != 3)
                    {
                        continue;
                    }
                }

                // Sample point slightly in front of the face
                Vector3 faceCenterWorld = blockCenterWorld + new Vector3(n.x, n.y, n.z) * 0.48f;
                Vector3 faceCenterUnity = faceCenterWorld - Origin.position;

                Vector3 dir = faceCenterUnity - camPosUnity;
                float dist = dir.magnitude;
                if (dist <= 0.1f)
                {
                    return true;
                }

                dir /= dist;

                // Raycast from camera towards the face center
                if (Physics.Raycast(new Ray(camPosUnity, dir), out RaycastHit hit, dist + 0.15f, -538488845, QueryTriggerInteraction.Ignore))
                {
                    Vector3 hitWorld = hit.point + Origin.position + dir * 0.02f;
                    Vector3i hitBlock = World.worldToBlockPos(hitWorld);

                    if (IsSameBlockOrParent(world, hitBlock, targetBlock))
                    {
                        return true;
                    }
                }
            }

            // 3. Fallback: check center directly in case shape is non-cube (e.g. plate, pole, frame)
            Vector3 centerDir = (blockCenterWorld - Origin.position) - camPosUnity;
            float centerDist = centerDir.magnitude;
            if (centerDist > 0.1f)
            {
                centerDir /= centerDist;
                if (Physics.Raycast(new Ray(camPosUnity, centerDir), out RaycastHit hit, centerDist + 0.15f, -538488845, QueryTriggerInteraction.Ignore))
                {
                    Vector3 hitWorld = hit.point + Origin.position + centerDir * 0.02f;
                    Vector3i hitBlock = World.worldToBlockPos(hitWorld);

                    if (IsSameBlockOrParent(world, hitBlock, targetBlock))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsSameBlockOrParent(World world, Vector3i a, Vector3i b)
        {
            if (a == b) return true;
            if (world == null) return false;

            BlockValue bva = world.GetBlock(a);
            if (bva.ischild && bva.Block?.multiBlockPos != null)
            {
                a = bva.Block.multiBlockPos.GetParentPos(a, bva);
            }

            BlockValue bvb = world.GetBlock(b);
            if (bvb.ischild && bvb.Block?.multiBlockPos != null)
            {
                b = bvb.Block.multiBlockPos.GetParentPos(b, bvb);
            }

            return a == b;
        }

        private void AddBlockUnitEdgesToMap(Vector3i pos, Vector3i size, int tier)
        {
            int minX = pos.x;
            int maxX = pos.x + size.x;
            int minY = pos.y;
            int maxY = pos.y + size.y;
            int minZ = pos.z;
            int maxZ = pos.z + size.z;

            // Subdivide 4 edges along X into 1-unit segments
            for (int x = minX; x < maxX; x++)
            {
                AddUnitEdge(new GridEdge(new Vector3i(x, minY, minZ), new Vector3i(x + 1, minY, minZ)), tier);
                AddUnitEdge(new GridEdge(new Vector3i(x, maxY, minZ), new Vector3i(x + 1, maxY, minZ)), tier);
                AddUnitEdge(new GridEdge(new Vector3i(x, minY, maxZ), new Vector3i(x + 1, minY, maxZ)), tier);
                AddUnitEdge(new GridEdge(new Vector3i(x, maxY, maxZ), new Vector3i(x + 1, maxY, maxZ)), tier);
            }

            // Subdivide 4 edges along Y into 1-unit segments
            for (int y = minY; y < maxY; y++)
            {
                AddUnitEdge(new GridEdge(new Vector3i(minX, y, minZ), new Vector3i(minX, y + 1, minZ)), tier);
                AddUnitEdge(new GridEdge(new Vector3i(maxX, y, minZ), new Vector3i(maxX, y + 1, minZ)), tier);
                AddUnitEdge(new GridEdge(new Vector3i(minX, y, maxZ), new Vector3i(minX, y + 1, maxZ)), tier);
                AddUnitEdge(new GridEdge(new Vector3i(maxX, y, maxZ), new Vector3i(maxX, y + 1, maxZ)), tier);
            }

            // Subdivide 4 edges along Z into 1-unit segments
            for (int z = minZ; z < maxZ; z++)
            {
                AddUnitEdge(new GridEdge(new Vector3i(minX, minY, z), new Vector3i(minX, minY, z + 1)), tier);
                AddUnitEdge(new GridEdge(new Vector3i(maxX, minY, z), new Vector3i(maxX, minY, z + 1)), tier);
                AddUnitEdge(new GridEdge(new Vector3i(minX, maxY, z), new Vector3i(minX, maxY, z + 1)), tier);
                AddUnitEdge(new GridEdge(new Vector3i(maxX, maxY, z), new Vector3i(maxX, maxY, z + 1)), tier);
            }
        }

        private void AddUnitEdge(GridEdge edge, int tier)
        {
            if (_edgeCount.TryGetValue(edge, out int count))
            {
                _edgeCount[edge] = count + 1;
                if (tier < _edgeTier[edge])
                {
                    _edgeTier[edge] = tier;
                }
            }
            else
            {
                _edgeCount[edge] = 1;
                _edgeTier[edge] = tier;
            }
        }

        private void AddActiveTargetEdges(Vector3i pos, Vector3i size, Vector3 originPos)
        {
            int minX = pos.x;
            int maxX = pos.x + size.x;
            int minY = pos.y;
            int maxY = pos.y + size.y;
            int minZ = pos.z;
            int maxZ = pos.z + size.z;

            // 4 edges parallel to X
            _rendererFocus.AddBeam(new GridEdge(new Vector3i(minX, minY, minZ), new Vector3i(maxX, minY, minZ)), FocusedBeamThickness, originPos);
            _rendererFocus.AddBeam(new GridEdge(new Vector3i(minX, maxY, minZ), new Vector3i(maxX, maxY, minZ)), FocusedBeamThickness, originPos);
            _rendererFocus.AddBeam(new GridEdge(new Vector3i(minX, minY, maxZ), new Vector3i(maxX, minY, maxZ)), FocusedBeamThickness, originPos);
            _rendererFocus.AddBeam(new GridEdge(new Vector3i(minX, maxY, maxZ), new Vector3i(maxX, maxY, maxZ)), FocusedBeamThickness, originPos);

            // 4 edges parallel to Y
            _rendererFocus.AddBeam(new GridEdge(new Vector3i(minX, minY, minZ), new Vector3i(minX, maxY, minZ)), FocusedBeamThickness, originPos);
            _rendererFocus.AddBeam(new GridEdge(new Vector3i(maxX, minY, minZ), new Vector3i(maxX, maxY, minZ)), FocusedBeamThickness, originPos);
            _rendererFocus.AddBeam(new GridEdge(new Vector3i(minX, minY, maxZ), new Vector3i(minX, maxY, maxZ)), FocusedBeamThickness, originPos);
            _rendererFocus.AddBeam(new GridEdge(new Vector3i(maxX, minY, maxZ), new Vector3i(maxX, maxY, maxZ)), FocusedBeamThickness, originPos);

            // 4 edges parallel to Z
            _rendererFocus.AddBeam(new GridEdge(new Vector3i(minX, minY, minZ), new Vector3i(minX, minY, maxZ)), FocusedBeamThickness, originPos);
            _rendererFocus.AddBeam(new GridEdge(new Vector3i(maxX, minY, minZ), new Vector3i(maxX, minY, maxZ)), FocusedBeamThickness, originPos);
            _rendererFocus.AddBeam(new GridEdge(new Vector3i(minX, maxY, minZ), new Vector3i(minX, maxY, maxZ)), FocusedBeamThickness, originPos);
            _rendererFocus.AddBeam(new GridEdge(new Vector3i(maxX, maxY, minZ), new Vector3i(maxX, maxY, maxZ)), FocusedBeamThickness, originPos);
        }

        public void ClearVisuals()
        {
            _scannedBlocks.Clear();
            _seenPositions.Clear();
            _edgeCount.Clear();
            _edgeTier.Clear();
            _activeWorkingBlock = Vector3i.zero;

            _rendererFocus?.Hide();
            _rendererRed?.Hide();
            _rendererOrange?.Hide();
            _rendererGold?.Hide();
        }

        private void OnDestroy()
        {
            ClearVisuals();
        }
    }
}
