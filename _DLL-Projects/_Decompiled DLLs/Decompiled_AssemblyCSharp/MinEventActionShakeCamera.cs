using System.Collections;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionShakeCamera : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float shakeSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	public float shakeAmplitude;

	[PublicizedFrom(EAccessModifier.Private)]
	public float shakeTime = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public string refCvarNameShakeSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	public string refCvarNameShakeAmplitude;

	[PublicizedFrom(EAccessModifier.Private)]
	public string refCvarNameShakeTime;

	public override void Execute(MinEventParams _params)
	{
		if (targets == null || GameManager.Instance == null)
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i] as EntityPlayerLocal != null)
			{
				if (!string.IsNullOrEmpty(refCvarNameShakeSpeed))
				{
					(targets[i] as EntityPlayerLocal).vp_FPCamera.ShakeSpeed = targets[i].Buffs.GetCustomVar(refCvarNameShakeSpeed);
				}
				else
				{
					(targets[i] as EntityPlayerLocal).vp_FPCamera.ShakeSpeed = shakeSpeed;
				}
				if (!string.IsNullOrEmpty(refCvarNameShakeAmplitude))
				{
					(targets[i] as EntityPlayerLocal).vp_FPCamera.ShakeAmplitude = new Vector3(1f, 1f, 0f) * targets[i].Buffs.GetCustomVar(refCvarNameShakeAmplitude);
				}
				else
				{
					(targets[i] as EntityPlayerLocal).vp_FPCamera.ShakeAmplitude = new Vector3(1f, 1f, 0f) * shakeAmplitude;
				}
				float customVar = shakeTime;
				if (!string.IsNullOrEmpty(refCvarNameShakeTime))
				{
					customVar = (targets[i] as EntityPlayerLocal).Buffs.GetCustomVar(refCvarNameShakeTime);
				}
				if (customVar > 0f)
				{
					GameManager.Instance.StartCoroutine(stopShaking(targets[i] as EntityPlayerLocal, customVar));
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator stopShaking(EntityPlayerLocal target, float time)
	{
		yield return new WaitForSeconds(time);
		if ((bool)target)
		{
			vp_FPCamera vp_FPCamera2 = target.vp_FPCamera;
			if ((bool)vp_FPCamera2)
			{
				vp_FPCamera2.ShakeSpeed = 0f;
				vp_FPCamera2.ShakeAmplitude = Vector3.zero;
			}
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		return base.CanExecute(_eventType, _params);
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			switch (_attribute.Name.LocalName)
			{
			case "shake_speed":
				if (_attribute.Value.StartsWith("@"))
				{
					refCvarNameShakeSpeed = _attribute.Value.Substring(1);
				}
				else
				{
					shakeSpeed = StringParsers.ParseFloat(_attribute.Value);
				}
				break;
			case "shake_amplitude":
				if (_attribute.Value.StartsWith("@"))
				{
					refCvarNameShakeAmplitude = _attribute.Value.Substring(1);
				}
				else
				{
					shakeAmplitude = StringParsers.ParseFloat(_attribute.Value);
				}
				break;
			case "shake_time":
				if (_attribute.Value.StartsWith("@"))
				{
					refCvarNameShakeTime = _attribute.Value.Substring(1);
				}
				else
				{
					shakeTime = StringParsers.ParseFloat(_attribute.Value);
				}
				break;
			}
		}
		return flag;
	}
}
