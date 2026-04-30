using System.Text;
using System.Text.RegularExpressions;

public class TextEllipsisAnimator
{
	public enum AnimationMode
	{
		Off,
		Final,
		All
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] ellipsisEndings = new string[4] { "[00]...", ".[00]..", "..[00].", "..." };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] ellipsisEndingsChinese = new string[3] { "[00]……", "…[00]…", "……" };

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool isChinese;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string lastEllipsisState;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Regex ellipsisBreakPatternEastern = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex ellipsisBreakPattern = new Regex("(\\b\\w+\\b)(\\n\\.\\.\\.|\\.\\n\\.\\.|\\.\\.\\n\\.)");

	[PublicizedFrom(EAccessModifier.Private)]
	public const char ellipsisChar = '…';

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label xuiLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public UILabel uiLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentEllipsisState;

	[PublicizedFrom(EAccessModifier.Private)]
	public float ellipsisTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public int cycles;

	[PublicizedFrom(EAccessModifier.Private)]
	public string baseString;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalEllipsis;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] animatedStrings = new string[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public string mostRecentAlphaBB = "[FF]";

	[PublicizedFrom(EAccessModifier.Private)]
	public StringBuilder sb;

	public int animationStates
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!isChinese)
			{
				return 4;
			}
			return 3;
		}
	}

	public TextEllipsisAnimator(string _input, UILabel _uiLabel)
	{
		uiLabel = _uiLabel;
		isChinese = Localization.language == "schinese" || Localization.language == "tchinese";
		lastEllipsisState = (isChinese ? ellipsisEndingsChinese[2] : ellipsisEndings[3]);
		Init(_input);
	}

	public TextEllipsisAnimator(string _input, XUiV_Label _label)
	{
		uiLabel = _label.Label;
		xuiLabel = _label;
		isChinese = Localization.language == "schinese" || Localization.language == "tchinese";
		lastEllipsisState = (isChinese ? ellipsisEndingsChinese[2] : ellipsisEndings[3]);
		Init(_input);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init(string _input)
	{
		currentEllipsisState = animationStates - 1;
		sb = new StringBuilder();
		if (ellipsisBreakPatternEastern == null)
		{
			ellipsisBreakPatternEastern = (isChinese ? new Regex("([\\u3008-\\u9FFF])(\\u2026\\n\\u2026)") : new Regex("([\\u3008-\\u9FFF])(\\.\\n\\.\\.|\\.\\.\\n\\.)"));
		}
		SetBaseString(_input);
	}

	public void GetNextAnimatedString(float _dt)
	{
		if (totalEllipsis != 0 && baseString != null && !(uiLabel == null))
		{
			ellipsisTimer += _dt;
			if (ellipsisTimer >= 0.5f)
			{
				currentEllipsisState = (currentEllipsisState + 1) % animationStates;
				ellipsisTimer = 0f;
				UpdateLabel();
			}
		}
	}

	public void SetBaseString(string _input, AnimationMode _mode = AnimationMode.All)
	{
		if (_input == null || uiLabel == null || (xuiLabel != null && !xuiLabel.SupportBbCode) || _mode == AnimationMode.Off)
		{
			baseString = null;
			return;
		}
		sb.Clear();
		mostRecentAlphaBB = "[FF]";
		cycles = 0;
		totalEllipsis = 0;
		sb.Append(_input);
		switch (_mode)
		{
		case AnimationMode.Final:
		{
			int num = sb.Length - 1;
			while (num > 0 && char.IsWhiteSpace(sb[num]))
			{
				sb.Remove(num, 1);
				num--;
			}
			while (num >= 3 && sb[num] == '.' && sb[num - 1] == '.' && sb[num - 2] == '.')
			{
				sb.Remove(num - 2, 3);
				num -= 3;
				totalEllipsis = 1;
			}
			while (num >= 1 && sb[num] == '…')
			{
				sb.Remove(num, 1);
				num--;
				totalEllipsis = 1;
			}
			while (num > 0 && char.IsWhiteSpace(sb[num]))
			{
				sb.Remove(num, 1);
				num--;
			}
			if (totalEllipsis > 0)
			{
				sb.Append(lastEllipsisState);
			}
			break;
		}
		case AnimationMode.All:
		{
			for (int i = 0; i < sb.Length; i++)
			{
				if (sb[i] == '[' && i + 3 < sb.Length && sb[i + 3] == ']' && IsHexDigit(sb[i + 1]) && IsHexDigit(sb[i + 2]))
				{
					mostRecentAlphaBB = sb[i].ToString() + sb[i + 1] + sb[i + 2] + sb[i + 3];
					i += 3;
				}
				if (sb[i] == '…')
				{
					int j;
					for (j = 0; i + j + 1 < sb.Length && sb[i + j + 1] == '…'; j++)
					{
					}
					if (j > 0)
					{
						sb.Remove(i + 1, j);
					}
					sb.Remove(i, 1);
					sb.Insert(i, lastEllipsisState + mostRecentAlphaBB);
					i += lastEllipsisState.Length - 1 + 4;
					totalEllipsis++;
				}
			}
			for (int k = 0; k < sb.Length - 2; k++)
			{
				if (sb[k] == '[' && k + 3 < sb.Length && sb[k + 3] == ']' && IsHexDigit(sb[k + 1]) && IsHexDigit(sb[k + 2]))
				{
					mostRecentAlphaBB = sb[k].ToString() + sb[k + 1] + sb[k + 2] + sb[k + 3];
					k += 3;
				}
				if (sb[k] == '.' && sb[k + 1] == '.' && sb[k + 2] == '.')
				{
					sb.Remove(k, 3);
					sb.Insert(k, lastEllipsisState + mostRecentAlphaBB);
					k += 3 - lastEllipsisState.Length + 4;
					totalEllipsis++;
				}
			}
			break;
		}
		}
		baseString = sb.ToString();
		xuiLabel?.SetText(baseString);
		int processedMatches = 0;
		for (int l = 0; l < totalEllipsis; l++)
		{
			xuiLabel?.UpdateData();
			if (xuiLabel != null && xuiLabel.Text == xuiLabel.Label.processedText)
			{
				break;
			}
			bool hadEastern = false;
			int j2 = 0;
			baseString = ellipsisBreakPatternEastern.Replace(uiLabel.processedText, [PublicizedFrom(EAccessModifier.Internal)] (Match m) =>
			{
				if (j2 == processedMatches)
				{
					hadEastern = true;
					int num2 = processedMatches;
					processedMatches = num2 + 1;
					return m.Groups[1].Value + "\n" + lastEllipsisState;
				}
				j2++;
				return m.Value;
			});
			if (hadEastern)
			{
				continue;
			}
			j2 = 0;
			baseString = ellipsisBreakPattern.Replace(uiLabel.processedText, [PublicizedFrom(EAccessModifier.Internal)] (Match m) =>
			{
				if (j2 == processedMatches)
				{
					int num2 = processedMatches;
					processedMatches = num2 + 1;
					return "\n" + m.Groups[1].Value + lastEllipsisState;
				}
				j2++;
				return m.Value;
			});
			xuiLabel?.SetText(baseString);
		}
		UpdateLabel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLabel()
	{
		if (totalEllipsis > 0)
		{
			if (cycles < animationStates)
			{
				cycles++;
				if (isChinese)
				{
					animatedStrings[currentEllipsisState] = baseString.Replace(lastEllipsisState, ellipsisEndingsChinese[currentEllipsisState]);
				}
				else
				{
					animatedStrings[currentEllipsisState] = baseString.Replace(lastEllipsisState, ellipsisEndings[currentEllipsisState]);
				}
			}
			if (xuiLabel != null)
			{
				xuiLabel.Text = animatedStrings[currentEllipsisState];
				xuiLabel.UpdateData();
			}
			else
			{
				uiLabel.text = animatedStrings[currentEllipsisState];
			}
		}
		else if (xuiLabel != null)
		{
			xuiLabel.SetText(baseString);
		}
		else
		{
			uiLabel.text = baseString;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsHexDigit(char c)
	{
		if (!char.IsDigit(c))
		{
			if (char.ToLower(c) >= 'a')
			{
				return char.ToLower(c) <= 'f';
			}
			return false;
		}
		return true;
	}
}
