using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class TemporaryObject : MonoBehaviour
{
	public float life = 2f;

	public bool destroyMaterials;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine coroutine;

	public void SetLife(float _life)
	{
		life = _life;
		float num = Utils.FastMax(_life - 1f, 0.1f);
		float num2 = 0.1f;
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			float duration = componentsInChildren[i].main.duration;
			if (duration > num2)
			{
				num2 = duration;
			}
		}
		float num3 = num / num2;
		float num4 = num2 * 0.5f;
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			ParticleSystem.MainModule main = particleSystem.main;
			float duration2 = main.duration;
			if (duration2 >= num4)
			{
				particleSystem.Stop(withChildren: false);
				main.duration = duration2 * num3;
				ParticleSystem.MinMaxCurve startDelay = main.startDelay;
				switch (startDelay.mode)
				{
				case ParticleSystemCurveMode.Constant:
					startDelay.constant *= num3;
					break;
				case ParticleSystemCurveMode.TwoConstants:
					startDelay.constantMin *= num3;
					startDelay.constantMax *= num3;
					break;
				default:
					startDelay.curveMultiplier *= num3;
					break;
				}
				main.startDelay = startDelay;
				ParticleSystem.MinMaxCurve startLifetime = main.startLifetime;
				switch (startLifetime.mode)
				{
				case ParticleSystemCurveMode.Constant:
					startLifetime.constant *= num3;
					break;
				case ParticleSystemCurveMode.TwoConstants:
					startLifetime.constantMin *= num3;
					startLifetime.constantMax *= num3;
					break;
				default:
					startLifetime.curveMultiplier *= num3;
					break;
				}
				main.startLifetime = startLifetime;
				particleSystem.Play(withChildren: false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		coroutine = StartCoroutine(DestroyLater());
	}

	public void Restart()
	{
		base.gameObject.SetActive(value: true);
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
		}
		Start();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator DestroyLater()
	{
		yield return new WaitForSeconds(life);
		if (destroyMaterials)
		{
			Utils.CleanupMaterialsOfRenderers(base.transform.GetComponentsInChildren<Renderer>());
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	[Conditional("DEBUG_TEMPOBJ")]
	public void LogTO(string _format = "", params object[] _args)
	{
		_format = $"{GameManager.frameCount} TemporaryObject {base.gameObject.GetGameObjectPath()}, id {base.gameObject.GetInstanceID()}, life {life}, {_format}";
		Log.Warning(_format, _args);
	}
}
