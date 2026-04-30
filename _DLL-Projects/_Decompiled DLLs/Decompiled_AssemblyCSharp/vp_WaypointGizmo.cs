using System;
using UnityEngine;

public class vp_WaypointGizmo : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Color m_GizmoColor = new Color(1f, 1f, 1f, 0.4f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Color m_SelectedGizmoColor = new Color32(160, byte.MaxValue, 100, 100);

	public void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = m_GizmoColor;
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
		Gizmos.color = new Color(0f, 0f, 0f, 1f);
		Gizmos.DrawLine(Vector3.zero, Vector3.forward);
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = m_SelectedGizmoColor;
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
		Gizmos.color = new Color(0f, 0f, 0f, 1f);
		Gizmos.DrawLine(Vector3.zero, Vector3.forward);
	}
}
