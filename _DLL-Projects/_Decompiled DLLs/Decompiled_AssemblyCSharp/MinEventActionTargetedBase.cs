using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class MinEventActionTargetedBase : MinEventActionBase
{
	public enum TargetTypes
	{
		self,
		other,
		selfAOE,
		otherAOE,
		positionAOE,
		selfOtherPlayers
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static FastTags<TagGroup.Global> ally = FastTags<TagGroup.Global>.Parse("ally");

	[PublicizedFrom(EAccessModifier.Protected)]
	public static FastTags<TagGroup.Global> party = FastTags<TagGroup.Global>.Parse("party");

	[PublicizedFrom(EAccessModifier.Protected)]
	public static FastTags<TagGroup.Global> enemy = FastTags<TagGroup.Global>.Parse("enemy");

	public TargetTypes targetType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public FastTags<TagGroup.Global> targetTags = FastTags<TagGroup.Global>.none;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<EntityAlive> targets = new List<EntityAlive>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public float maxRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityAlive> entsInRange;

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		targets.Clear();
		if (targetType == TargetTypes.self)
		{
			if (_params.Self != null && isValidTarget(_params.Self, null))
			{
				targets.Add(_params.Self);
				return singleTargetCheck(_params);
			}
		}
		else if (targetType == TargetTypes.other)
		{
			if (_params.Others != null)
			{
				for (int i = 0; i < _params.Others.Length; i++)
				{
					if (isValidTarget(_params.Self, _params.Others[i]))
					{
						MinEventParams minEventParams = new MinEventParams();
						MinEventParams.CopyTo(_params, minEventParams);
						minEventParams.Other = _params.Others[i];
						if (singleTargetCheck(minEventParams))
						{
							targets.Add(_params.Others[i]);
						}
					}
				}
			}
			else if (_params.Other != null && isValidTarget(_params.Self, _params.Other) && singleTargetCheck(_params))
			{
				targets.Add(_params.Other);
			}
		}
		else if (targetType == TargetTypes.selfAOE)
		{
			if (_params.Self != null)
			{
				entsInRange = GameManager.Instance.World.GetLivingEntitiesInBounds(_params.Self, new Bounds(_params.Self.position, Vector3.one * (maxRange * 2f)));
				for (int j = 0; j < entsInRange.Count; j++)
				{
					if (entsInRange[j] != null && isValidTarget(_params.Self, entsInRange[j]))
					{
						MinEventParams minEventParams2 = new MinEventParams();
						MinEventParams.CopyTo(_params, minEventParams2);
						minEventParams2.Other = entsInRange[j];
						if (singleTargetCheck(minEventParams2))
						{
							targets.Add(entsInRange[j]);
						}
					}
				}
				entsInRange.Clear();
			}
		}
		else if (targetType == TargetTypes.selfOtherPlayers)
		{
			if (_params.Self != null)
			{
				List<EntityPlayer> players = GameManager.Instance.World.GetPlayers();
				for (int k = 0; k < players.Count; k++)
				{
					EntityPlayer entityPlayer = players[k];
					if (!(entityPlayer == _params.Self) && isValidTarget(_params.Self, entityPlayer))
					{
						MinEventParams minEventParams3 = new MinEventParams();
						MinEventParams.CopyTo(_params, minEventParams3);
						minEventParams3.Other = entityPlayer;
						if (singleTargetCheck(minEventParams3))
						{
							targets.Add(entityPlayer);
						}
					}
				}
			}
		}
		else if (targetType == TargetTypes.otherAOE)
		{
			if (_params.Other != null)
			{
				entsInRange = GameManager.Instance.World.GetLivingEntitiesInBounds(_params.Self, new Bounds(_params.Other.position, Vector3.one * (maxRange * 2f)));
				for (int l = 0; l < entsInRange.Count; l++)
				{
					if (entsInRange[l] != null && isValidTarget(_params.Self, entsInRange[l]))
					{
						MinEventParams minEventParams4 = new MinEventParams();
						MinEventParams.CopyTo(_params, minEventParams4);
						minEventParams4.Other = entsInRange[l];
						if (singleTargetCheck(minEventParams4))
						{
							targets.Add(entsInRange[l]);
						}
					}
				}
				entsInRange.Clear();
			}
		}
		else if (targetType == TargetTypes.positionAOE)
		{
			entsInRange = GameManager.Instance.World.GetLivingEntitiesInBounds(null, new Bounds(_params.Position, Vector3.one * (maxRange * 2f)));
			for (int m = 0; m < entsInRange.Count; m++)
			{
				EntityAlive entityAlive = entsInRange[m];
				if (entityAlive != null && isValidTarget(_params.Self, entityAlive) && (!(entityAlive is EntityPlayer) || !((entityAlive.getHipPosition() - _params.Position).sqrMagnitude > maxRange * maxRange)))
				{
					MinEventParams minEventParams5 = new MinEventParams();
					MinEventParams.CopyTo(_params, minEventParams5);
					minEventParams5.Other = entityAlive;
					if (singleTargetCheck(minEventParams5))
					{
						targets.Add(entityAlive);
					}
				}
			}
			entsInRange.Clear();
		}
		return targets.Count > 0;
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			switch (_attribute.Name.LocalName)
			{
			case "target":
				targetType = EnumUtils.Parse<TargetTypes>(_attribute.Value, _ignoreCase: true);
				return true;
			case "range":
				maxRange = StringParsers.ParseFloat(_attribute.Value);
				return true;
			case "target_tags":
				targetTags = FastTags<TagGroup.Global>.Parse(_attribute.Value);
				return true;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool singleTargetCheck(MinEventParams _tempParams)
	{
		if (Requirements != null)
		{
			return Requirements.IsValid(_tempParams);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isValidTarget(EntityAlive _self, EntityAlive _other)
	{
		if (targetTags.IsEmpty)
		{
			return true;
		}
		if (_self == null && _other != null && _other.HasAnyTags(targetTags))
		{
			return true;
		}
		if (_other == null && _self.HasAnyTags(targetTags))
		{
			return true;
		}
		if (_other.HasAnyTags(targetTags))
		{
			return true;
		}
		if (targetTags.Test_AnySet(party) && _self as EntityPlayer != null && _other as EntityPlayer != null && (_self as EntityPlayer).Party != null && (_self as EntityPlayer).Party.ContainsMember(_other as EntityPlayer))
		{
			return true;
		}
		if (targetTags.Test_AnySet(ally))
		{
			if (_self as EntityPlayer != null && _other as EntityPlayer != null && (_self as EntityPlayer).IsFriendsWith(_other as EntityPlayer))
			{
				return true;
			}
			if (_self as EntityEnemy != null && _other as EntityEnemy != null)
			{
				return true;
			}
			if (FactionManager.Instance != null)
			{
				if (_self as EntityPlayer != null && _other as EntityNPC != null)
				{
					if (FactionManager.Instance.GetRelationshipTier(_self, _other) == FactionManager.Relationship.Like)
					{
						return true;
					}
					if (FactionManager.Instance.GetRelationshipTier(_self, _other) == FactionManager.Relationship.Love)
					{
						return true;
					}
				}
				if (_self as EntityNPC != null && _other as EntityPlayer != null)
				{
					if (FactionManager.Instance.GetRelationshipTier(_self, _other) == FactionManager.Relationship.Like)
					{
						return true;
					}
					if (FactionManager.Instance.GetRelationshipTier(_self, _other) == FactionManager.Relationship.Love)
					{
						return true;
					}
				}
				if (_self as EntityNPC != null && _other as EntityNPC != null)
				{
					if (FactionManager.Instance.GetRelationshipTier(_self, _other) == FactionManager.Relationship.Like)
					{
						return true;
					}
					if (FactionManager.Instance.GetRelationshipTier(_self, _other) == FactionManager.Relationship.Love)
					{
						return true;
					}
				}
			}
		}
		if (targetTags.Test_AnySet(enemy))
		{
			if (_self as EntityEnemy != null && _other as EntityPlayer != null)
			{
				return true;
			}
			if (_self as EntityPlayer != null && _other as EntityEnemy != null)
			{
				return true;
			}
			if (_self as EntityEnemy != null && _other as EntityNPC != null)
			{
				return true;
			}
			if (_self as EntityNPC != null && _other as EntityEnemy != null)
			{
				return true;
			}
			if (FactionManager.Instance != null)
			{
				if (_self as EntityPlayer != null && _other as EntityPlayer != null)
				{
					if (FactionManager.Instance.GetRelationshipTier(_self, _other) == FactionManager.Relationship.Hate)
					{
						return true;
					}
					if (FactionManager.Instance.GetRelationshipTier(_self, _other) == FactionManager.Relationship.Dislike)
					{
						return true;
					}
				}
				if (_self as EntityPlayer != null && _other as EntityNPC != null)
				{
					if (FactionManager.Instance.GetRelationshipTier(_self, _other) == FactionManager.Relationship.Hate)
					{
						return true;
					}
					if (FactionManager.Instance.GetRelationshipTier(_self, _other) == FactionManager.Relationship.Dislike)
					{
						return true;
					}
				}
				if (_self as EntityNPC != null && _other as EntityPlayer != null)
				{
					if (FactionManager.Instance.GetRelationshipTier(_self, _other) == FactionManager.Relationship.Hate)
					{
						return true;
					}
					if (FactionManager.Instance.GetRelationshipTier(_self, _other) == FactionManager.Relationship.Dislike)
					{
						return true;
					}
				}
				if (_self as EntityNPC != null && _other as EntityNPC != null)
				{
					if (FactionManager.Instance.GetRelationshipTier(_self, _other) == FactionManager.Relationship.Hate)
					{
						return true;
					}
					if (FactionManager.Instance.GetRelationshipTier(_self, _other) == FactionManager.Relationship.Dislike)
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
