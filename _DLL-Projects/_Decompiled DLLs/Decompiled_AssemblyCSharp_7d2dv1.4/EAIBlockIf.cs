using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class EAIBlockIf : EAIBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum eType
	{
		None,
		Alert,
		Investigate
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum eOp
	{
		None,
		e,
		ne
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct Condition
	{
		public eType type;

		public eOp op;

		public float value;
	}

	public bool canExecute;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Condition> conditions;

	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		MutexBits = 1;
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
		conditions = new List<Condition>();
		if (!data.TryGetValue("condition", out var _value))
		{
			return;
		}
		string[] array = _value.Split(' ');
		for (int i = 0; i < array.Length; i += 3)
		{
			Condition item = new Condition
			{
				type = EnumUtils.Parse<eType>(array[i], _ignoreCase: true)
			};
			if (item.type == eType.None)
			{
				Log.Warning("{0} BlockIf type None", theEntity.EntityName);
			}
			item.op = EnumUtils.Parse<eOp>(array[i + 1], _ignoreCase: true);
			if (item.op == eOp.None)
			{
				Log.Warning("{0} BlockIf op None", theEntity.EntityName);
			}
			item.value = StringParsers.ParseFloat(array[i + 2]);
			conditions.Add(item);
		}
	}

	public override bool CanExecute()
	{
		int count = conditions.Count;
		for (int i = 0; i < count; i++)
		{
			Condition condition = conditions[i];
			float v = 0f;
			switch (condition.type)
			{
			case eType.Alert:
				v = (theEntity.IsAlert ? 1 : 0);
				break;
			case eType.Investigate:
				v = (theEntity.HasInvestigatePosition ? 1 : 0);
				break;
			}
			if (Compare(condition.op, v, condition.value))
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool Compare(eOp op, float v1, float v2)
	{
		return op switch
		{
			eOp.e => v1 == v2, 
			eOp.ne => v1 != v2, 
			_ => false, 
		};
	}

	public override bool Continue()
	{
		return CanExecute();
	}
}
