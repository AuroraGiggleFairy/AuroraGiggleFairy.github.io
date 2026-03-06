using System.IO;
using UnityEngine;

public class ProgressionValue
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const byte Version = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public int level;

	[PublicizedFrom(EAccessModifier.Private)]
	public int costForNextLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public int calculatedFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	public float calculatedLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProgressionClass cachedProgressionClass;

	public string Name => name;

	public ProgressionClass ProgressionClass
	{
		get
		{
			if (cachedProgressionClass == null && !Progression.ProgressionClasses.TryGetValue(name, out cachedProgressionClass))
			{
				Log.Error("ProgressionValue ProgressionClasses missing {0}", name);
			}
			return cachedProgressionClass;
		}
	}

	public int CostForNextLevel
	{
		get
		{
			if (ProgressionClass.CurrencyType == ProgressionCurrencyType.SP)
			{
				return ProgressionClass.CalculatedCostForLevel(Level + 1);
			}
			return costForNextLevel;
		}
		set
		{
			if (ProgressionClass.CurrencyType != ProgressionCurrencyType.SP)
			{
				costForNextLevel = value;
			}
		}
	}

	public int Level
	{
		get
		{
			ProgressionClass progressionClass = ProgressionClass;
			if (progressionClass == null)
			{
				return level;
			}
			if (progressionClass.IsSkill)
			{
				return progressionClass.MaxLevel;
			}
			return level;
		}
		set
		{
			calculatedFrame = -1;
			if (ProgressionClass == null)
			{
				level = value;
			}
			else if (ProgressionClass.IsSkill)
			{
				level = ProgressionClass.MaxLevel;
			}
			else
			{
				level = value;
			}
		}
	}

	public float PercToNextLevel => 1f - (float)CostForNextLevel / (float)ProgressionClass.CalculatedCostForLevel(level + 1);

	public float GetCalculatedLevel(EntityAlive _ea)
	{
		if (calculatedFrame == Time.frameCount)
		{
			return calculatedLevel;
		}
		ProgressionClass progressionClass = ProgressionClass;
		if (progressionClass == null)
		{
			return 0f;
		}
		float num = Level;
		PassiveEffects passiveEffects = progressionClass.Type switch
		{
			ProgressionType.Attribute => PassiveEffects.AttributeLevel, 
			ProgressionType.Skill => PassiveEffects.SkillLevel, 
			ProgressionType.Perk => PassiveEffects.PerkLevel, 
			_ => PassiveEffects.None, 
		};
		if (passiveEffects != PassiveEffects.None)
		{
			num = EffectManager.GetValue(passiveEffects, null, num, _ea, null, progressionClass.NameTag);
		}
		num = Mathf.Min(num, ProgressionClass.GetCalculatedMaxLevel(_ea, this));
		num = Mathf.Max(num, progressionClass.MinLevel);
		calculatedFrame = Time.frameCount;
		calculatedLevel = num;
		return num;
	}

	public ProgressionValue()
	{
	}

	public ProgressionValue(string _name)
	{
		name = _name;
	}

	public int CalculatedLevel(EntityAlive _ea)
	{
		return (int)GetCalculatedLevel(_ea);
	}

	public int CalculatedMaxLevel(EntityAlive _ea)
	{
		return ProgressionClass.GetCalculatedMaxLevel(_ea, this);
	}

	public bool IsLocked(EntityAlive _ea)
	{
		return ProgressionClass.GetCalculatedMaxLevel(_ea, this) == 0;
	}

	public void ClearProgressionClassLink()
	{
		cachedProgressionClass = null;
	}

	public bool CanPurchase(EntityAlive _ea, int _level)
	{
		if (_level > ProgressionClass.MaxLevel)
		{
			return false;
		}
		return true;
	}

	public void CopyFrom(ProgressionValue _pv)
	{
		name = _pv.name;
		level = _pv.level;
		costForNextLevel = _pv.costForNextLevel;
	}

	public void Read(BinaryReader _reader)
	{
		_reader.ReadByte();
		name = _reader.ReadString();
		level = _reader.ReadByte();
		costForNextLevel = _reader.ReadInt32();
	}

	public void Write(BinaryWriter _writer, bool _IsNetwork)
	{
		_writer.Write((byte)1);
		_writer.Write(name);
		_writer.Write((byte)level);
		_writer.Write(costForNextLevel);
	}
}
