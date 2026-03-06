using System.Collections.Generic;
using System.Text;

public sealed class GameStageGroup
{
	public const string cDefaultGroupName = "GroupGenericZombie";

	public readonly GameStageDefinition spawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, GameStageGroup> groups = new Dictionary<string, GameStageGroup>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, GameStageGroup> groupsFullName = new Dictionary<string, GameStageGroup>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static StringBuilder stringBuilder = new StringBuilder();

	public static Dictionary<string, GameStageGroup> Groups => groupsFullName;

	public GameStageGroup(GameStageDefinition _spawner)
	{
		spawner = _spawner;
	}

	public static void AddGameStageGroup(string _fullName, GameStageGroup _group)
	{
		string key = CleanName(_fullName);
		groups.Add(key, _group);
		groupsFullName.Add(_fullName, _group);
	}

	public static GameStageGroup TryGet(string _name)
	{
		if (groups.TryGetValue(_name, out var value))
		{
			return value;
		}
		return null;
	}

	public static void Clear()
	{
		groups.Clear();
		groupsFullName.Clear();
	}

	public static string CleanName(string _name)
	{
		if (_name.Length > 0 && char.IsDigit(_name[0]))
		{
			_name = _name.Substring(1);
		}
		else if (_name.StartsWith("S_"))
		{
			int startIndex = 2;
			if (_name.StartsWith("S_-"))
			{
				startIndex = 3;
			}
			return _name.Substring(startIndex).Replace("_", "");
		}
		return _name;
	}

	public static string MakeDisplayName(string _name)
	{
		bool flag = false;
		foreach (char c in _name)
		{
			if (!char.IsDigit(c))
			{
				if (char.IsUpper(c) && flag)
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append(c);
				flag = true;
			}
		}
		_name = stringBuilder.ToString();
		stringBuilder.Clear();
		return _name;
	}
}
