using System;
using System.Reflection;
using System.Text;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(XUiV_Label), "updateData")]
public static class AgfFeatHangingIndentPatch
{
    private static readonly FieldInfo LabelField =
        AccessTools.Field(typeof(XUiV_LabelBase), "label")
        ?? AccessTools.Field(typeof(XUiV_Label), "label");

    private static readonly MethodInfo GetFormattedTextMethod =
        AccessTools.Method(typeof(XUiV_Label), "getFormattedText", new[] { typeof(string) })
        ?? AccessTools.Method(typeof(XUiV_LabelBase), "getFormattedText", new[] { typeof(string) });

    private static readonly MethodInfo UpdateNguiTextMethod = AccessTools.Method(typeof(UILabel), "UpdateNGUIText");

    private static readonly MethodInfo WrapTextMethod = FindWrapText();

    [HarmonyPostfix]
    private static void Postfix(XUiV_Label __instance)
    {
        if (__instance == null || LabelField == null)
        {
            return;
        }

        if (!IsFeatColumn(__instance))
        {
            return;
        }

        UILabel uiLabel = LabelField.GetValue(__instance) as UILabel;
        if (uiLabel == null)
        {
            return;
        }

        string source = __instance.Text;
        if (string.IsNullOrEmpty(source) || source.IndexOf(' ') < 0)
        {
            return;
        }

        string formatted = source;
        if (GetFormattedTextMethod != null)
        {
            formatted = GetFormattedTextMethod.Invoke(__instance, new object[] { source }) as string ?? source;
        }

        string hung = ApplyHangingIndent(uiLabel, formatted);
        if (!string.Equals(uiLabel.text, hung, StringComparison.Ordinal))
        {
            uiLabel.text = hung;
        }
    }

    private static bool IsFeatColumn(XUiV_Label view)
    {
        string id = view.ID;
        if (string.IsNullOrEmpty(id) || (!id.EndsWith("_col1", StringComparison.Ordinal) && !id.EndsWith("_col2", StringComparison.Ordinal)))
        {
            return false;
        }

        XUiController controller = view.Controller;
        while (controller != null)
        {
            XUiView viewComponent = controller.ViewComponent;
            if (viewComponent != null && string.Equals(viewComponent.ID, "windowAGFFeatures", StringComparison.Ordinal))
            {
                return true;
            }

            controller = controller.Parent;
        }

        return false;
    }

    private static string ApplyHangingIndent(UILabel uiLabel, string source)
    {
        if (uiLabel.width <= 8)
        {
            return source;
        }

        string[] lines = source.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        bool anyIndented = false;
        for (int i = 0; i < lines.Length; i++)
        {
            if (LeadingSpaceCount(lines[i]) > 0)
            {
                anyIndented = true;
                break;
            }
        }

        if (!anyIndented)
        {
            return source;
        }

        PrepareNguiText(uiLabel);

        StringBuilder builder = new StringBuilder(source.Length + 32);
        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0)
            {
                builder.Append('\n');
            }

            builder.Append(WrapIndentedLine(uiLabel, lines[i]));
        }

        return builder.ToString();
    }

    private static string WrapIndentedLine(UILabel uiLabel, string line)
    {
        int indent = LeadingSpaceCount(line);
        if (indent <= 0 || line.Length <= indent)
        {
            return line;
        }

        string pad = line.Substring(0, indent);
        string content = line.Substring(indent);
        int indentPx = MeasureWidth(pad);
        int wrapWidth = uiLabel.width - indentPx;
        if (wrapWidth < 16)
        {
            return line;
        }

        string wrapped = WrapToWidth(content, wrapWidth);
        if (string.IsNullOrEmpty(wrapped) || wrapped.IndexOf('\n') < 0)
        {
            return line;
        }

        string[] parts = wrapped.Replace("\r\n", "\n").Split('\n');
        StringBuilder builder = new StringBuilder(wrapped.Length + (pad.Length * parts.Length));
        for (int i = 0; i < parts.Length; i++)
        {
            if (i > 0)
            {
                builder.Append('\n');
            }

            builder.Append(pad);
            builder.Append(parts[i]);
        }

        return builder.ToString();
    }

    private static int LeadingSpaceCount(string line)
    {
        int count = 0;
        while (count < line.Length && line[count] == ' ')
        {
            count++;
        }

        return count;
    }

    private static void PrepareNguiText(UILabel uiLabel)
    {
        if (UpdateNguiTextMethod != null)
        {
            UpdateNguiTextMethod.Invoke(uiLabel, null);
            return;
        }

        _ = uiLabel.processedText;
    }

    private static int MeasureWidth(string text)
    {
        Vector2 size = NGUIText.CalculatePrintedSize(text);
        return Mathf.Max(0, Mathf.CeilToInt(size.x));
    }

    private static string WrapToWidth(string content, int wrapWidth)
    {
        int previousWidth = NGUIText.rectWidth;
        NGUIText.rectWidth = wrapWidth;
        try
        {
            if (WrapTextMethod != null)
            {
                object result = InvokeWrap(content);
                if (result is string wrapped && !string.IsNullOrEmpty(wrapped))
                {
                    return wrapped;
                }
            }

            return FallbackWrap(content, wrapWidth);
        }
        finally
        {
            NGUIText.rectWidth = previousWidth;
        }
    }

    private static object InvokeWrap(string content)
    {
        ParameterInfo[] parameters = WrapTextMethod.GetParameters();
        object[] args = new object[parameters.Length];
        int outIndex = -1;
        for (int i = 0; i < parameters.Length; i++)
        {
            Type type = parameters[i].ParameterType;
            if (type == typeof(string) && !parameters[i].IsOut)
            {
                args[i] = content;
            }
            else if (type.IsByRef && type.GetElementType() == typeof(string))
            {
                args[i] = string.Empty;
                outIndex = i;
            }
            else if (type == typeof(bool))
            {
                args[i] = false;
            }
            else
            {
                args[i] = type.IsValueType ? Activator.CreateInstance(type) : null;
            }
        }

        WrapTextMethod.Invoke(null, args);
        if (outIndex >= 0)
        {
            return args[outIndex] as string;
        }

        return args.Length > 0 ? args[0] : content;
    }

    private static string FallbackWrap(string content, int wrapWidth)
    {
        if (MeasureWidth(content) <= wrapWidth)
        {
            return content;
        }

        StringBuilder builder = new StringBuilder(content.Length + 8);
        int start = 0;
        while (start < content.Length)
        {
            int low = 1;
            int high = content.Length - start;
            int fit = 1;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                string candidate = content.Substring(start, mid);
                if (MeasureWidth(StripBbCode(candidate)) <= wrapWidth)
                {
                    fit = mid;
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            int take = FitBreak(content, start, fit);
            if (builder.Length > 0)
            {
                builder.Append('\n');
            }

            builder.Append(content, start, take);
            start += take;
            while (start < content.Length && content[start] == ' ')
            {
                start++;
            }
        }

        return builder.ToString();
    }

    private static int FitBreak(string content, int start, int fit)
    {
        if (start + fit >= content.Length)
        {
            return fit;
        }

        int space = content.LastIndexOf(' ', start + fit - 1, fit);
        if (space > start)
        {
            return space - start;
        }

        return Math.Max(1, fit);
    }

    private static string StripBbCode(string text)
    {
        if (text.IndexOf('[') < 0)
        {
            return text;
        }

        StringBuilder builder = new StringBuilder(text.Length);
        bool inTag = false;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '[')
            {
                inTag = true;
                continue;
            }

            if (c == ']' && inTag)
            {
                inTag = false;
                continue;
            }

            if (!inTag)
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }

    private static MethodInfo FindWrapText()
    {
        MethodInfo[] methods = typeof(NGUIText).GetMethods(BindingFlags.Public | BindingFlags.Static);
        MethodInfo best = null;
        int bestScore = int.MinValue;
        for (int i = 0; i < methods.Length; i++)
        {
            MethodInfo method = methods[i];
            if (method.Name != "WrapText")
            {
                continue;
            }

            ParameterInfo[] parameters = method.GetParameters();
            int score = 0;
            bool hasString = false;
            for (int p = 0; p < parameters.Length; p++)
            {
                Type type = parameters[p].ParameterType;
                if (type == typeof(string) && !parameters[p].IsOut)
                {
                    hasString = true;
                    score += 2;
                }
                else if (type.IsByRef && type.GetElementType() == typeof(string))
                {
                    score += 4;
                }
            }

            if (!hasString)
            {
                continue;
            }

            if (score > bestScore)
            {
                bestScore = score;
                best = method;
            }
        }

        return best;
    }
}
