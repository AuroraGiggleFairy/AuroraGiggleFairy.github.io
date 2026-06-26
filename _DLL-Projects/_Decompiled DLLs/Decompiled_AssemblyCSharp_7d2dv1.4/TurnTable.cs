using UnityEngine;

public class TurnTable : MonoBehaviour
{
	public float _rotationSpeed = 1f;

	public bool _pingPong;

	public float _pingPongDegreeSpan = 90f;

	public float _pingPongDegreeOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (_pingPong)
		{
			float t = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(Time.time * _rotationSpeed, 1f));
			float y = Mathf.Lerp(_pingPongDegreeOffset - _pingPongDegreeSpan / 2f, _pingPongDegreeOffset + _pingPongDegreeSpan / 2f, t);
			base.transform.localRotation = Quaternion.Euler(0f, y, 0f);
		}
		else
		{
			base.transform.localRotation *= Quaternion.Euler(0f, _rotationSpeed * 180f * Time.deltaTime, 0f);
		}
	}
}
