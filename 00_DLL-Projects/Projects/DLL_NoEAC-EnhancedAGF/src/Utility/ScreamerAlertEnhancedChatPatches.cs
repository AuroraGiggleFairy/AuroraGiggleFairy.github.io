using System;
using System.Text.RegularExpressions;
using HarmonyLib;

[HarmonyPatch]
public static class ScreamerAlertEnhancedChatPatches
{
    // Strip BB color/style tags only. Stamped chat wraps alert text in content brackets
    // like [Screamer Alert]; a naive \[[^\]]*\] strip would erase the alert words themselves.
    private static readonly Regex BbColorOrCloseTagRegex = new Regex(
        @"\[(?:-|/[A-Za-z]+|[A-Fa-f0-9]{6}|[A-Fa-f0-9]{8}|white|black|red|green|blue|yellow|cyan|magenta)\]",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    [HarmonyPatch(typeof(GameManager), "ChatMessageServer")]
    [HarmonyPrefix]
    private static bool ChatMessageServerPrefix(ref string _msg)
    {
        if (!ScreamerAlertEnhancedGate.ShouldProcessClientHooks()
            || !ScreamerAlertEnhancedGate.IsScreamerInPlay())
        {
            return true;
        }

        if (!ScreamerAlertEnhancedCommands.TryHandleOutgoingChatMessage(_msg, out bool consumeVanilla))
        {
            return true;
        }

        return !consumeVanilla;
    }

    [HarmonyPatch(typeof(XUiC_ChatOutput), "AddMessage")]
    [HarmonyPrefix]
    private static bool ChatOutputAddMessagePrefix(EnumGameMessages _messageType, EChatType _chatType, ref string _message)
    {
        if (!ScreamerAlertEnhancedGate.ShouldProcessClientHooks())
        {
            return true;
        }

        if (_messageType != EnumGameMessages.Chat)
        {
            return true;
        }

        if (_chatType != EChatType.Whisper || string.IsNullOrEmpty(_message))
        {
            return true;
        }

        if (!ScreamerAlertEnhancedGate.IsScreamerInPlay())
        {
            if (LooksLikeScreamerServerLine(_message))
            {
                ScreamerAlertEnhancedGate.MarkServerScreamerDetected();
            }

            return true;
        }

        if (!TryClassifyAlert(_message, out bool isHorde))
        {
            if (IsScreamerStatusLine(_message))
            {
                ScreamerAlertEnhancedGate.MarkServerScreamerDetected();
                bool hideAckLine = IsCapabilityAckLine(_message);
                if (hideAckLine)
                {
                    ScreamerAlertEnhancedCapabilityHello.MarkAcknowledged();
                }
                TrySyncLocalModeFromServerMessage(_message);
                return !hideAckLine;
            }

            return true;
        }

        ScreamerAlertEnhancedGate.MarkServerScreamerDetected();
        _ = isHorde;

        // Enhanced clients render screamer alerts via UI, not chat.
        return false;
    }

    private static bool LooksLikeScreamerServerLine(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return false;
        }

        return message.IndexOf("[ScreamerAlert]", StringComparison.OrdinalIgnoreCase) >= 0
            || message.IndexOf("[Screamer Alert", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool IsScreamerStatusLine(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return false;
        }

        // Legacy ack marker and current localized status/set replies.
        if (message.IndexOf("[ScreamerAlert]", StringComparison.OrdinalIgnoreCase) >= 0
            || message.IndexOf("[Screamer Alert", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        string normalized = NormalizeAlertText(message);
        return IsModeOrStatusResponse(normalized);
    }

    private static bool TryClassifyAlert(string message, out bool isHorde)
    {
        isHorde = false;

        string normalizedMessage = NormalizeAlertText(message);
        if (string.IsNullOrEmpty(normalizedMessage))
        {
            return false;
        }

        // "/agfsa" status and set replies contain "Screamer Alert = ..."; do not hide them as spawn alerts.
        if (IsModeOrStatusResponse(normalizedMessage))
        {
            return false;
        }

        string normalizedScoutLocalized = NormalizeAlertText(Localization.Get("ScreamerAlert_Scout"));
        string normalizedHordeLocalized = NormalizeAlertText(Localization.Get("ScreamerAlert_Horde"));
        bool containsScoutLocalized = ContainsText(normalizedMessage, normalizedScoutLocalized);
        bool containsHordeLocalized = ContainsText(normalizedMessage, normalizedHordeLocalized);

        // Check horde first because stamped horde text also contains "Screamer Alert".
        if (TextEquals(normalizedMessage, normalizedHordeLocalized)
            || containsHordeLocalized
            || TextEquals(normalizedMessage, "ScreamerAlert_Horde")
            || TextEquals(normalizedMessage, "Horde Incoming")
            || ContainsText(normalizedMessage, "Horde Incoming"))
        {
            isHorde = true;
            return true;
        }

        if (TextEquals(normalizedMessage, normalizedScoutLocalized)
            || containsScoutLocalized
            || TextEquals(normalizedMessage, "ScreamerAlert_Scout")
            || TextEquals(normalizedMessage, "Screamer Alert")
            || ContainsText(normalizedMessage, "Screamer Alert"))
        {
            isHorde = false;
            return true;
        }

        return false;
    }

    private static string NormalizeAlertText(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return string.Empty;
        }

        string withoutCount = ScreamerAlertModeSettings.StripNumberSuffix(message);
        string withoutColor = BbColorOrCloseTagRegex.Replace(withoutCount, string.Empty);
        // Unwrap remaining [content] brackets used by stamped chat templates.
        string unwrapped = Regex.Replace(withoutColor, @"\[([^\]]+)\]", "$1");
        return unwrapped.Trim();
    }

    private static bool TextEquals(string left, string right)
    {
        return !string.IsNullOrEmpty(left)
            && !string.IsNullOrEmpty(right)
            && string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsText(string source, string text)
    {
        return !string.IsNullOrEmpty(source)
            && !string.IsNullOrEmpty(text)
            && source.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool IsModeOrStatusResponse(string normalizedMessage)
    {
        if (string.IsNullOrEmpty(normalizedMessage))
        {
            return false;
        }

        string lower = normalizedMessage.ToLowerInvariant();
        if (lower.Contains("screamer alert") && lower.Contains("="))
        {
            return true;
        }

        return lower.Contains("options:")
            || lower.Contains("count requires enhancedagf")
            || lower.Contains("count needs enhancedagf")
            || lower.Contains("is now set to")
            || lower.Contains("mode set to")
            || lower.Contains("mode=")
            || lower.Contains("is currently")
            || lower.Contains("capability=enhancedagf");
    }

    private static bool IsCapabilityAckLine(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return false;
        }

        string normalized = NormalizeAlertText(message);
        if (string.IsNullOrEmpty(normalized))
        {
            return false;
        }

        string lower = normalized.ToLowerInvariant();
        return lower.IndexOf("capability=enhancedagf", StringComparison.Ordinal) >= 0
            && lower.IndexOf("ack", StringComparison.Ordinal) >= 0;
    }

    private static void TrySyncLocalModeFromServerMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        string normalized = NormalizeAlertText(message);
        if (string.IsNullOrEmpty(normalized))
        {
            return;
        }

        string lower = normalized.ToLowerInvariant();
        ScreamerAlertMode? nextMode = null;

        if (lower.Contains("screamer alert") && lower.Contains("="))
        {
            if (lower.Contains("count"))
            {
                nextMode = ScreamerAlertMode.OnWithNumbers;
            }
            else if (lower.Contains("off"))
            {
                nextMode = ScreamerAlertMode.Off;
            }
            else if (lower.Contains("on"))
            {
                nextMode = ScreamerAlertMode.On;
            }
        }
        else if (lower.Contains("is now set to") || lower.Contains("mode set to"))
        {
            if (lower.Contains("on + #") || lower.Contains("count"))
            {
                nextMode = ScreamerAlertMode.OnWithNumbers;
            }
            else if (lower.Contains("off"))
            {
                nextMode = ScreamerAlertMode.Off;
            }
            else if (lower.Contains("on"))
            {
                nextMode = ScreamerAlertMode.On;
            }
        }
        else if (lower.Contains("mode="))
        {
            int modeIndex = lower.IndexOf("mode=", StringComparison.Ordinal);
            if (modeIndex >= 0)
            {
                string token = lower.Substring(modeIndex + 5).Trim();
                if (token.StartsWith("on + #", StringComparison.Ordinal) || token.StartsWith("count", StringComparison.Ordinal))
                {
                    nextMode = ScreamerAlertMode.OnWithNumbers;
                }
                else if (token.StartsWith("off", StringComparison.Ordinal))
                {
                    nextMode = ScreamerAlertMode.Off;
                }
                else if (token.StartsWith("on", StringComparison.Ordinal))
                {
                    nextMode = ScreamerAlertMode.On;
                }
            }
        }
        else if (lower.Contains("is currently"))
        {
            if (lower.Contains("count"))
            {
                nextMode = ScreamerAlertMode.OnWithNumbers;
            }
            else if (lower.Contains("off"))
            {
                nextMode = ScreamerAlertMode.Off;
            }
            else if (lower.Contains("on"))
            {
                nextMode = ScreamerAlertMode.On;
            }
        }

        // Do NOT treat help text like "[COUNT requires EnhancedAGF]" as a mode change.
        // /agfsa status sends that as line 3 and it must not overwrite OFF/COUNT to ON.

        if (nextMode.HasValue)
        {
            ScreamerAlertModeSettings.SetModeForLocalPlayer(nextMode.Value);
        }
    }
}
