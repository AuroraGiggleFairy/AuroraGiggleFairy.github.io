using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ExpandedInteractionPrompts
{
    public static class PromptCardLayout
    {
        private const int CardPadX = 14;
        private const int CardPadTop = 10;
        private const int CardPadBottom = 10;
        private const int HeaderPadX = 22;
        private const int HeaderPadY = 8;
        private const int MinHeaderWidth = 64;
        private const int MinHeaderHeight = 40;
        private const int MaxHeaderWidth = 720;
        private const int MaxHeaderHeight = 90;
        private const int ColWidth = 198;
        private const int StatRowH = 36;
        private const int PillH = 32;
        private const int IconSize = 26;
        private const int IconToText = 8;
        private const int PillPadX = 10;
        private const int BorderPx = 3;
        private const int TargetIconSize = 22;
        private const int TargetGap = 2;
        private const int CardWidth = CardPadX * 2 + ColWidth;

        // Repair/upgrade arrow sits on the crosshair (~64px). Keep the header plate above that.
        private const int RepairIconHalf = 32;
        private const int HeaderIconGap = 16;
        private const int PlateBottomY = RepairIconHalf + HeaderIconGap;

        // Fallback if focusedBlockHealth is closed. Real placement uses the HP label's printed bottom.
        private const int HealthBarCenterY = -40;
        private const int HealthBarHalf = 6;

        public static void Sync(XUiC_InteractionPrompt prompt)
        {
            if (prompt == null)
                return;

            var card = prompt.GetChildById("agfPromptCard")?.ViewComponent;
            var cardRim = prompt.GetChildById("agfPromptCardRim")?.ViewComponent as XUiV_Sprite;
            var cardBg = prompt.GetChildById("agfPromptCardBg")?.ViewComponent as XUiV_Sprite;
            var titleRim = prompt.GetChildById("agfPromptTitleRim")?.ViewComponent as XUiV_Sprite;
            var titleBg = prompt.GetChildById("agfPromptTitleBg")?.ViewComponent as XUiV_Sprite;
            var header = prompt.GetChildById("lblText")?.ViewComponent as XUiV_Label;
            if (header == null)
                return;

            if (string.IsNullOrEmpty(header.Text))
            {
                if (card != null)
                    card.IsVisible = false;
                if (titleRim != null)
                    titleRim.IsVisible = false;
                if (titleBg != null)
                    titleBg.IsVisible = false;
                return;
            }

            Vector2 printed = header.PrintedSize;
            int textW = Mathf.CeilToInt(printed.x);
            int textH = Mathf.CeilToInt(printed.y);
            if (textW < 8 || textH < 8)
            {
                textW = Math.Max(textW, 80);
                textH = Math.Max(textH, 34);
            }

            int headerW = Mathf.Clamp(Math.Max(textW + HeaderPadX * 2, MinHeaderWidth), MinHeaderWidth, MaxHeaderWidth);
            int headerH = Mathf.Clamp(Math.Max(textH + HeaderPadY * 2, MinHeaderHeight), MinHeaderHeight, MaxHeaderHeight);
            int headerCenterY = PlateBottomY + headerH / 2;

            if (titleRim != null)
                titleRim.IsVisible = true;
            if (titleBg != null)
                titleBg.IsVisible = true;
            SetSize(titleRim, headerW + BorderPx * 2, headerH + BorderPx * 2);
            SetPos(titleRim, 0, headerCenterY);
            SetSize(titleBg, headerW, headerH);
            SetPos(titleBg, 0, headerCenterY);
            SetPos(header, 0, headerCenterY);

            int cardW = CardWidth;
            int colX = -ColWidth / 2;

            int visualRow = 0;
            string raw = PromptTextField?.GetValue(prompt) as string ?? string.Empty;
            PromptStateHelpers.SplitPrompt(raw, out _, out string details);
            PromptStateHelpers.ParseRows(details, LayoutIcons, LayoutTexts, null, null, LayoutFillColors);

            visualRow += PlaceIfContent(prompt, PromptStateHelpers.SlotLock, colX, visualRow);
            for (int i = 0; i < PromptStateHelpers.SlotLock; i++)
                visualRow += PlaceIfContent(prompt, i, colX, visualRow);
            visualRow += PlaceIfContent(prompt, PromptStateHelpers.SlotOwner, colX, visualRow);

            bool hasDetails = visualRow > 0;
            if (card != null)
                card.IsVisible = hasDetails;
            if (!hasDetails || cardBg == null)
                return;

            int cardH = CardPadTop + visualRow * StatRowH + CardPadBottom;
            // Header plate bottom is PlateBottomY above the crosshair. Put the first pill the same
            // distance below the printed HP text (not the empty 30px label box).
            int healthBottom = GetHealthClusterBottom(prompt);
            int cardTop = healthBottom - PlateBottomY + CardPadTop;
            SetPos(card, 0, cardTop);
            if (cardRim != null)
                cardRim.IsVisible = false;
            SetSize(cardBg, cardW, cardH);
            SetPos(cardBg, 0, 0);
            PulseActivityIcons(prompt);
        }

        private static readonly FieldInfo PromptTextField =
            AccessTools.Field(typeof(XUiC_InteractionPrompt), "text");
        private static readonly string[] PulseIcons = new string[PromptStateHelpers.MaxRows];
        private static readonly string[] PulseTexts = new string[PromptStateHelpers.MaxRows];
        private static readonly string[] PulseColors = new string[PromptStateHelpers.MaxRows];
        private static readonly bool[] PulseBlinks = new bool[PromptStateHelpers.MaxRows];
        private static readonly string[] LayoutIcons = new string[PromptStateHelpers.MaxRows];
        private static readonly string[] LayoutTexts = new string[PromptStateHelpers.MaxRows];
        private static readonly string[] LayoutFillColors = new string[PromptStateHelpers.MaxRows];
        private static readonly string[] TargetIcons = new string[4];
        private static readonly Color PulseRed = new Color(1f, 0.15f, 0.12f, 1f);
        private static readonly Color PulseOrange = new Color(1f, 0.5f, 0f, 1f);
        private static readonly Color PulseGreen = new Color(0.35f, 0.95f, 0.4f, 1f);
        private static readonly Color TargetSelected = new Color(222f / 255f, 206f / 255f, 163f / 255f, 1f);

        private static void PulseActivityIcons(XUiC_InteractionPrompt prompt)
        {
            string raw = PromptTextField?.GetValue(prompt) as string ?? string.Empty;
            PromptStateHelpers.SplitPrompt(raw, out _, out string details);
            PromptStateHelpers.ParseRows(details, PulseIcons, PulseTexts, PulseColors, blinks: PulseBlinks);

            bool burning = FocusedWorkstationIsBurning(prompt);
            float wave = (Mathf.Sin(Time.unscaledTime * 8f) + 1f) * 0.5f;
            for (int i = 0; i < PromptStateHelpers.MaxStatCells; i++)
            {
                var icon = prompt.GetChildById("s" + i + "icon")?.ViewComponent as XUiV_Sprite;
                if (icon == null)
                    continue;

                Color target = Color.white;
                if (PromptStateHelpers.IsItemIcon(icon.SpriteName))
                    target = Color.white;
                else if (icon.SpriteName == PromptStateHelpers.IconFuel && burning)
                    target = Color.Lerp(Color.white, PulseRed, wave);
                else if (PulseBlinks[i] && icon.SpriteName == PromptStateHelpers.IconPower)
                    target = Color.Lerp(Color.white, PulseGreen, wave);
                else if (PulseBlinks[i])
                    target = Color.Lerp(Color.white, PulseOrange, wave);
                else if (!string.IsNullOrEmpty(PulseColors[i])
                    || icon.SpriteName == PromptStateHelpers.IconLock
                    || icon.SpriteName == PromptStateHelpers.IconUnlock)
                    continue;

                if (icon.Color == target)
                    continue;
                icon.Color = target;
                icon.SetDirty();
            }
        }

        private static bool FocusedWorkstationIsBurning(XUiC_InteractionPrompt prompt)
        {
            try
            {
                var player = prompt.xui?.playerUI?.entityPlayer as EntityPlayerLocal;
                var hit = player?.HitInfo;
                if (hit == null || !hit.bHitValid)
                    return false;
                var world = GameManager.Instance?.World;
                if (world == null)
                    return false;

                Vector3i pos = hit.hit.blockPos;
                BlockValue bv = hit.hit.blockValue;
                if (bv.ischild && bv.Block?.multiBlockPos != null)
                    pos = bv.Block.multiBlockPos.GetParentPos(pos, bv);

                var te = world.GetTileEntity(pos) as TileEntityWorkstation;
                return te != null && te.IsBurning;
            }
            catch
            {
                return false;
            }
        }

        private static int GetHealthClusterBottom(XUiC_InteractionPrompt prompt)
        {
            try
            {
                var health = prompt.xui?.FindWindowGroupByName("focusedBlockHealth")
                    ?.GetChildByType<XUiC_FocusedBlockHealth>();
                var win = health?.ViewComponent;
                if (win == null)
                    return HealthBarCenterY - HealthBarHalf;

                int winY = win.Position.y;
                var label = health.GetChildById("lblText")?.ViewComponent as XUiV_Label;
                if (label != null && !string.IsNullOrEmpty(label.Text))
                {
                    int top = winY + label.Position.y;
                    int h = Mathf.Max(Mathf.CeilToInt(label.PrintedSize.y), 22);
                    return top - h;
                }

                return winY - HealthBarHalf;
            }
            catch
            {
                return HealthBarCenterY - HealthBarHalf;
            }
        }

        private static int PlaceIfContent(XUiC_InteractionPrompt prompt, int idx, int x, int visualRow)
        {
            if (!StatHasContent(prompt, idx))
                return 0;
            int cy = -(CardPadTop + visualRow * StatRowH + StatRowH / 2);
            PlaceStat(prompt, idx, x, cy);
            return 1;
        }

        private static bool StatHasContent(XUiC_InteractionPrompt prompt, int idx)
        {
            var label = prompt.GetChildById("s" + idx + "label")?.ViewComponent as XUiV_Label;
            if (label != null && !string.IsNullOrEmpty(label.Text))
                return true;
            var rect = prompt.GetChildById("agfStat" + idx)?.ViewComponent;
            return rect != null && rect.IsVisible;
        }

        private static void PlaceStat(XUiC_InteractionPrompt prompt, int idx, int x, int cy)
        {
            var statRect = prompt.GetChildById("agfStat" + idx)?.ViewComponent;
            SetPos(statRect, x, cy);
            SetViewSize(statRect, ColWidth, StatRowH);

            var rim = prompt.GetChildById("s" + idx + "rim")?.ViewComponent as XUiV_Sprite;
            var bg = prompt.GetChildById("s" + idx + "bg")?.ViewComponent as XUiV_Sprite;
            var fill = prompt.GetChildById("s" + idx + "fill")?.ViewComponent as XUiV_Sprite;
            var icon = prompt.GetChildById("s" + idx + "icon")?.ViewComponent;
            var label = prompt.GetChildById("s" + idx + "label")?.ViewComponent as XUiV_Label;
            SetSize(rim, ColWidth, PillH);
            SetPos(rim, 0, 0);
            int innerW = ColWidth - BorderPx * 2;
            int innerH = PillH - BorderPx * 2;
            SetSize(bg, innerW, innerH);
            SetPos(bg, BorderPx, 0);
            ApplyPillTrackColor(bg, LayoutFillColors[idx]);
            SetPos(fill, BorderPx, 0);
            SetPillFill(fill, innerW, innerH);

            int iconX = PillPadX + IconSize / 2;
            int textX = PillPadX + IconSize + IconToText;
            int textW = Math.Max(ColWidth - textX - PillPadX, 24);
            SetPos(icon, iconX, 0);
            ApplySpriteAtlas(icon as XUiV_Sprite, LayoutIcons[idx]);
            PlaceItemIconBg(prompt, idx, iconX, LayoutIcons[idx]);
            SetPos(label, textX, 0);
            SetLabelSize(label, textW, 28);
            PlaceTargetIcons(prompt, idx, textX, LayoutTexts[idx], label);
        }

        private static readonly Color DefaultPillBg = new Color(108f / 255f, 108f / 255f, 108f / 255f, 170f / 255f);
        private const float FillTrackScale = 0.50f;

        private static void ApplyPillTrackColor(XUiV_Sprite bg, string fillRgba)
        {
            if (bg == null)
                return;

            Color track = DefaultPillBg;
            if (TryMakeFillTrack(fillRgba, out Color darkened))
                track = darkened;
            if (bg.Color == track)
                return;
            bg.Color = track;
            bg.SetDirty();
        }

        private static bool TryMakeFillTrack(string rgba, out Color track)
        {
            track = default;
            if (string.IsNullOrEmpty(rgba))
                return false;

            string[] parts = rgba.Split(',');
            if (parts.Length < 3)
                return false;
            if (!int.TryParse(parts[0], out int r)
                || !int.TryParse(parts[1], out int g)
                || !int.TryParse(parts[2], out int b))
                return false;

            float a = 170f / 255f;
            if (parts.Length > 3 && int.TryParse(parts[3], out int ai))
                a = Mathf.Clamp01(ai / 255f);

            track = new Color(
                Mathf.Clamp01(r / 255f * FillTrackScale),
                Mathf.Clamp01(g / 255f * FillTrackScale),
                Mathf.Clamp01(b / 255f * FillTrackScale),
                a);
            return true;
        }

        private static readonly Color ItemIconDisc = new Color(64f / 255f, 64f / 255f, 64f / 255f, 1f);
        private static readonly Color ItemIconRim = new Color(210f / 255f, 210f / 255f, 210f / 255f, 1f);

        private static void PlaceItemIconBg(XUiC_InteractionPrompt prompt, int idx, int iconX, string spriteName)
        {
            var rim = prompt.GetChildById("s" + idx + "iconrim")?.ViewComponent as XUiV_Sprite;
            var bg = prompt.GetChildById("s" + idx + "iconbg")?.ViewComponent as XUiV_Sprite;
            bool show = PromptStateHelpers.IsItemIcon(spriteName);
            PlaceItemIconLayer(rim, show, iconX, IconSize + 8, ItemIconRim);
            PlaceItemIconLayer(bg, show, iconX, IconSize + 4, ItemIconDisc);
        }

        private static void PlaceItemIconLayer(XUiV_Sprite sprite, bool show, int iconX, int size, Color color)
        {
            if (sprite == null)
                return;
            if (sprite.IsVisible != show)
            {
                sprite.IsVisible = show;
                sprite.SetDirty();
            }
            if (!show)
                return;
            if (sprite.Color != color)
            {
                sprite.Color = color;
                sprite.SetDirty();
            }
            SetPos(sprite, iconX, 0);
            SetSize(sprite, size, size);
        }

        private static void PlaceTargetIcons(XUiC_InteractionPrompt prompt, int idx, int textX, string text, XUiV_Label label)
        {
            bool isTarget = PromptStateHelpers.IsTargetRow(text);
            int n = isTarget ? PromptStateHelpers.ParseTargetIcons(text, TargetIcons) : 0;
            if (label != null && label.IsVisible == isTarget)
            {
                label.IsVisible = !isTarget;
                label.SetDirty();
            }

            for (int t = 0; t < 4; t++)
            {
                var spr = prompt.GetChildById("s" + idx + "t" + t)?.ViewComponent as XUiV_Sprite;
                if (spr == null)
                    continue;

                bool show = isTarget && t < n;
                if (spr.IsVisible != show)
                {
                    spr.IsVisible = show;
                    spr.SetDirty();
                }
                if (!show)
                    continue;

                if (spr.SpriteName != TargetIcons[t])
                {
                    spr.SpriteName = TargetIcons[t];
                    spr.SetDirty();
                }
                if (spr.Color != TargetSelected)
                {
                    spr.Color = TargetSelected;
                    spr.SetDirty();
                }
                ApplySpriteAtlas(spr, TargetIcons[t]);
                int tx = textX + TargetIconSize / 2 + t * (TargetIconSize + TargetGap);
                SetPos(spr, tx, 0);
                SetSize(spr, TargetIconSize, TargetIconSize);
            }
        }

        private static void ApplySpriteAtlas(XUiV_Sprite sprite, string spriteName)
        {
            if (sprite == null || string.IsNullOrEmpty(spriteName))
                return;

            string atlas = PromptStateHelpers.IsItemIcon(spriteName)
                ? PromptStateHelpers.ItemIconAtlas
                : PromptStateHelpers.UiAtlas;
            if (sprite.UIAtlas != atlas)
            {
                sprite.UIAtlas = atlas;
                if (sprite.SpriteName == spriteName)
                    sprite.applyAtlasAndSprite(true);
            }
            if (sprite.SpriteName != spriteName)
                sprite.SpriteName = spriteName;
        }

        private static void SetPillFill(XUiV_Sprite fill, int innerW, int innerH)
        {
            if (fill == null)
                return;

            float amount = Mathf.Clamp01(fill.Fill);
            int w = 0;
            if (amount > 0.001f)
            {
                w = Mathf.RoundToInt(innerW * amount);
                // Keep 9-slice caps from collapsing so the fill stays a pill, not a rectangle.
                int minW = Mathf.Min(innerH, innerW);
                if (amount >= 0.995f)
                    w = innerW;
                else
                    w = Mathf.Clamp(w, minW, innerW);
            }

            SetSize(fill, w, innerH);
            if (fill.IsVisible != (w > 0))
            {
                fill.IsVisible = w > 0;
                fill.SetDirty();
            }
        }

        private static void SetPos(XUiView view, int x, int y)
        {
            if (view == null)
                return;
            var pos = new Vector2i(x, y);
            if (view.Position == pos)
                return;
            view.Position = pos;
            view.SetDirty();
        }

        private static void SetSize(XUiV_Sprite sprite, int w, int h)
        {
            if (sprite == null)
                return;
            if (sprite.Width == w && sprite.Height == h)
                return;
            sprite.Width = w;
            sprite.Height = h;
            sprite.SetDirty();
        }

        private static void SetViewSize(XUiView view, int w, int h)
        {
            if (view == null)
                return;
            if (view.Size.x == w && view.Size.y == h)
                return;
            view.Size = new Vector2i(w, h);
            view.SetDirty();
        }

        private static void SetLabelSize(XUiV_Label label, int w, int h)
        {
            if (label == null)
                return;
            if (label.Size.x == w && label.Size.y == h)
                return;
            label.Size = new Vector2i(w, h);
            label.SetDirty();
        }
    }

    [HarmonyPatch(typeof(XUiC_InteractionPrompt), nameof(XUiC_InteractionPrompt.Update))]
    public static class Patch_XUiC_InteractionPrompt_Update
    {
        public static void Postfix(XUiC_InteractionPrompt __instance)
        {
            PromptCardLayout.Sync(__instance);
        }
    }

    [HarmonyPatch(typeof(XUiC_InteractionPrompt), "GetBindingValueInternal")]
    public static class Patch_XUiC_InteractionPrompt_GetBindingValueInternal
    {
        private static readonly FieldInfo TextField = AccessTools.Field(typeof(XUiC_InteractionPrompt), "text");
        private static readonly string[] RowIcons = new string[PromptStateHelpers.MaxRows];
        private static readonly string[] RowTexts = new string[PromptStateHelpers.MaxRows];
        private static readonly string[] RowColors = new string[PromptStateHelpers.MaxRows];
        private static readonly string[] RowFills = new string[PromptStateHelpers.MaxRows];
        private static readonly string[] RowFillColors = new string[PromptStateHelpers.MaxRows];

        public static void Postfix(XUiC_InteractionPrompt __instance, ref bool __result, ref string _value, string _bindingName)
        {
            if (string.IsNullOrEmpty(_bindingName))
                return;
            if (_bindingName != "header"
                && _bindingName != "hasdetails"
                && _bindingName != "cardvisible"
                && !_bindingName.StartsWith("s")
                && !_bindingName.StartsWith("row"))
                return;

            string raw = TextField?.GetValue(__instance) as string ?? string.Empty;
            PromptStateHelpers.SplitPrompt(raw, out string header, out string details);
            PromptStateHelpers.ParseRows(details, RowIcons, RowTexts, RowColors, RowFills, RowFillColors);

            if (_bindingName == "header" || _bindingName == "text")
            {
                _value = header;
                __result = true;
                return;
            }

            if (_bindingName == "cardvisible")
            {
                _value = string.IsNullOrEmpty(header) ? "false" : "true";
                __result = true;
                return;
            }

            if (_bindingName == "hasdetails")
            {
                _value = string.IsNullOrEmpty(details) ? "false" : "true";
                __result = true;
                return;
            }

            if (_bindingName.Length < 2 || _bindingName[0] != 's' || !char.IsDigit(_bindingName[1]))
                return;

            int idx = _bindingName[1] - '0';
            if (idx < 0 || idx >= PromptStateHelpers.MaxStatCells)
                return;

            bool has = !string.IsNullOrEmpty(RowTexts[idx]);
            if (_bindingName.EndsWith("fillcolor"))
            {
                _value = has && !string.IsNullOrEmpty(RowFillColors[idx])
                    ? RowFillColors[idx]
                    : "0,0,0,0";
                __result = true;
                return;
            }

            if (_bindingName.EndsWith("fill"))
            {
                _value = has && !string.IsNullOrEmpty(RowFills[idx]) ? RowFills[idx] : "0";
                __result = true;
                return;
            }

            if (_bindingName.EndsWith("visible"))
            {
                _value = has ? "true" : "false";
                __result = true;
                return;
            }

            if (_bindingName.EndsWith("icon"))
            {
                _value = has ? RowIcons[idx] : string.Empty;
                __result = true;
                return;
            }

            if (_bindingName.EndsWith("text"))
            {
                _value = has && !PromptStateHelpers.IsTargetRow(RowTexts[idx]) ? RowTexts[idx] : string.Empty;
                __result = true;
                return;
            }

            if (_bindingName.EndsWith("color"))
            {
                _value = PromptStateHelpers.HexToRgba(has ? RowColors[idx] : string.Empty);
                __result = true;
            }
        }
    }
}
