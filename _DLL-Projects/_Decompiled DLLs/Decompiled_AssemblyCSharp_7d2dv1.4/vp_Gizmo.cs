using UnityEngine;

public class vp_Gizmo : MonoBehaviour
{
	public Color gizmoColor = new Color(1f, 1f, 1f, 0.4f);

	public Color selectedGizmoColor = new Color(1f, 1f, 1f, 0.4f);

	public void OnDrawGizmos()
	{
		Vector3 center = GetComponent<Collider>().bounds.center;
		Vector3 size = GetComponent<Collider>().bounds.size;
		Gizmos.color = gizmoColor;
		Gizmos.DrawCube(center, size);
		Gizmos.color = new Color(0f, 0f, 0f, 1f);
		Gizmos.DrawLine(Vector3.zero, Vector3.forward);
	}

	public void OnDrawGizmosSelected()
	{
		Vector3 center = GetComponent<Collider>().bounds.center;
		Vector3 size = GetComponent<Collider>().bounds.size;
		Gizmos.color = selectedGizmoColor;
		Gizmos.DrawCube(center, size);
		Gizmos.color = new Color(0f, 0f, 0f, 1f);
		Gizmos.DrawLine(Vector3.zero, Vector3.forward);
	}
}
