using System;
using UnityEngine.Scripting;

[Preserve]
public class RewardSkill : BaseReward
{
	public override void SetupReward()
	{
		string arg = Localization.Get("RewardSkill_keyword");
		base.Description = $"{base.ID} {arg}";
		base.ValueText = base.Value;
		base.Icon = base.OwnerQuest.OwnerJournal.OwnerPlayer.Progression.GetProgressionValue(base.ID).ProgressionClass.Icon;
	}

	public override void GiveReward(EntityPlayer player)
	{
		ProgressionValue progressionValue = player.Progression.GetProgressionValue(base.ID);
		int num = Convert.ToInt32(base.Value);
		if (progressionValue != null)
		{
			if (progressionValue.Level + num > progressionValue.ProgressionClass.MaxLevel)
			{
				progressionValue.Level = progressionValue.ProgressionClass.MaxLevel;
			}
			else
			{
				progressionValue.Level += num;
			}
			if (progressionValue.ProgressionClass.IsPerk)
			{
				player.MinEventContext.ProgressionValue = progressionValue;
				player.FireEvent(MinEventTypes.onPerkLevelChanged);
			}
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntitySetSkillLevelServer>().Setup(player.entityId, progressionValue.Name, progressionValue.Level));
			}
		}
	}

	public override BaseReward Clone()
	{
		RewardSkill rewardSkill = new RewardSkill();
		CopyValues(rewardSkill);
		return rewardSkill;
	}
}
