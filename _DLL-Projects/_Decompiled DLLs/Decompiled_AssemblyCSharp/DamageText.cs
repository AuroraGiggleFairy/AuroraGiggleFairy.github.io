using System;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
	public static bool Enabled;

	public float TimeDuration = 1.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public TextMeshPro textMeshPro;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 worldPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 velocity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float velocityDecayDelay = 0.2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform cameraT;

	public static void Create(string _text, Color _color, Vector3 _worldPos, Vector3 _velocity, float _scale = 1f)
	{
		GameObject obj = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/DamageText"));
		DamageText component = obj.GetComponent<DamageText>();
		TextMeshPro textMeshPro = (component.textMeshPro = obj.GetComponent<TextMeshPro>());
		textMeshPro.text = _text;
		textMeshPro.color = _color;
		textMeshPro.rectTransform.localScale = new Vector3(_scale, _scale, _scale);
		component.worldPos = _worldPos;
		component.velocity = _velocity;
		component.cameraT = Camera.main.transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		float deltaTime = Time.deltaTime;
		TimeDuration -= deltaTime;
		if (TimeDuration <= 0f || !cameraT)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		textMeshPro.alpha = Utils.FastLerp(0f, 1f, TimeDuration * 2f);
		velocityDecayDelay -= deltaTime;
		if (velocityDecayDelay <= 0f)
		{
			velocityDecayDelay = 0.1f;
			velocity *= 0.8f;
		}
		worldPos += velocity * deltaTime;
		base.transform.SetPositionAndRotation(Vector3.MoveTowards(worldPos - Origin.position, cameraT.position + cameraT.forward * 0.18f, 0.25f), cameraT.rotation);
	}
}
