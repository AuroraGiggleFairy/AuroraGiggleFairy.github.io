using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionRagdoll : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float duration = 2.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cvarRef;

	[PublicizedFrom(EAccessModifier.Private)]
	public string refCvarName = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public float force;

	[PublicizedFrom(EAccessModifier.Private)]
	public float scaleY;

	[PublicizedFrom(EAccessModifier.Private)]
	public float massScale;

	public override void Execute(MinEventParams _params)
	{
		DamageResponse dmResponse = DamageResponse.New(_fatal: false);
		dmResponse.StunDuration = duration;
		dmResponse.Strength = (int)force;
		if (cvarRef && targets.Count > 0)
		{
			dmResponse.StunDuration = targets[0].Buffs.GetCustomVar(refCvarName);
		}
		Vector3 vector = _params.StartPosition;
		if (vector.y == 0f)
		{
			vector = _params.Self.position;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			EntityAlive entityAlive = targets[i];
			if (entityAlive.AttachedToEntity != null)
			{
				entityAlive.Detach();
			}
			Vector3 vector2 = entityAlive.position - vector;
			if (scaleY == 0f)
			{
				vector2.y = 0f;
				dmResponse.Source = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Bashing, vector2.normalized);
			}
			else
			{
				vector2.y = _params.Self.GetLookVector().y * scaleY;
				float num = force;
				if (massScale > 0f)
				{
					num *= EntityClass.list[entityAlive.entityClass].MassKg * massScale;
				}
				dmResponse.Source = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Falling, vector2.normalized * num);
			}
			entityAlive.DoRagdoll(dmResponse);
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			switch (_attribute.Name.LocalName)
			{
			case "duration":
				if (_attribute.Value.StartsWith("@"))
				{
					cvarRef = true;
					refCvarName = _attribute.Value.Substring(1);
				}
				else
				{
					duration = StringParsers.ParseFloat(_attribute.Value);
				}
				return true;
			case "force":
				force = StringParsers.ParseFloat(_attribute.Value);
				return true;
			case "massScale":
				massScale = StringParsers.ParseFloat(_attribute.Value);
				return true;
			case "scaleY":
				scaleY = StringParsers.ParseFloat(_attribute.Value);
				return true;
			}
		}
		return flag;
	}
}
