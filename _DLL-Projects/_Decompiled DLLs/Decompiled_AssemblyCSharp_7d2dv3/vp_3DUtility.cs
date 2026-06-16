using UnityEngine;

public static class vp_3DUtility
{
	public static Vector3 HorizontalVector(Vector3 value)
	{
		value.y = 0f;
		return value;
	}

	public static Vector3 RandomHorizontalDirection()
	{
		return (Random.rotation * Vector3.up).normalized;
	}

	public static bool OnScreen(Camera camera, Renderer renderer, Vector3 worldPosition, out Vector3 screenPosition)
	{
		screenPosition = Vector2.zero;
		if (camera == null || renderer == null || !renderer.isVisible)
		{
			return false;
		}
		screenPosition = camera.WorldToScreenPoint(worldPosition);
		if (screenPosition.z < 0f)
		{
			return false;
		}
		return true;
	}

	public static bool InLineOfSight(Vector3 from, Transform target, Vector3 targetOffset, int layerMask)
	{
		Physics.Linecast(from, target.position + targetOffset, out var hitInfo, layerMask);
		if (hitInfo.collider == null || hitInfo.collider.transform.root == target)
		{
			return true;
		}
		return false;
	}

	public static bool WithinRange(Vector3 from, Vector3 to, float range, out float distance)
	{
		distance = Vector3.Distance(from, to);
		if (distance > range)
		{
			return false;
		}
		return true;
	}

	public static float DistanceToRay(Ray ray, Vector3 point)
	{
		return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
	}

	public static float LookAtAngle(Vector3 fromPosition, Vector3 fromForward, Vector3 toPosition)
	{
		if (!(Vector3.Cross(fromForward, (toPosition - fromPosition).normalized).y < 0f))
		{
			return Vector3.Angle(fromForward, (toPosition - fromPosition).normalized);
		}
		return 0f - Vector3.Angle(fromForward, (toPosition - fromPosition).normalized);
	}

	public static float LookAtAngleHorizontal(Vector3 fromPosition, Vector3 fromForward, Vector3 toPosition)
	{
		return LookAtAngle(HorizontalVector(fromPosition), HorizontalVector(fromForward), HorizontalVector(toPosition));
	}

	public static float AngleAroundAxis(Vector3 dirA, Vector3 dirB, Vector3 axis)
	{
		dirA -= Vector3.Project(dirA, axis);
		dirB -= Vector3.Project(dirB, axis);
		return Vector3.Angle(dirA, dirB) * (float)((!(Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0f)) ? 1 : (-1));
	}

	public static Quaternion GetBoneLookRotationInWorldSpace(Quaternion originalRotation, Quaternion parentRotation, Vector3 worldLookDir, float amount, Vector3 referenceLookDir, Vector3 referenceUpDir, Quaternion relativeWorldSpaceDifference)
	{
		Vector3 vector = Quaternion.Inverse(parentRotation) * worldLookDir.normalized;
		vector = Quaternion.AngleAxis(AngleAroundAxis(referenceLookDir, vector, referenceUpDir), referenceUpDir) * Quaternion.AngleAxis(AngleAroundAxis(vector - Vector3.Project(vector, referenceUpDir), vector, Vector3.Cross(referenceUpDir, vector)), Vector3.Cross(referenceUpDir, referenceLookDir)) * referenceLookDir;
		Vector3 tangent = referenceUpDir;
		Vector3.OrthoNormalize(ref vector, ref tangent);
		Vector3 normal = vector;
		Vector3 tangent2 = tangent;
		Vector3.OrthoNormalize(ref normal, ref tangent2);
		return Quaternion.Lerp(Quaternion.identity, parentRotation * Quaternion.LookRotation(normal, tangent2) * Quaternion.Inverse(parentRotation * Quaternion.LookRotation(referenceLookDir, referenceUpDir)), amount) * originalRotation * relativeWorldSpaceDifference;
	}

	public static GameObject DebugPrimitive(PrimitiveType primitiveType, Vector3 scale, Color color, Vector3 pivotOffset, Transform parent = null)
	{
		GameObject gameObject = null;
		Material material = new Material(Shader.Find("Transparent/Diffuse"));
		material.color = color;
		GameObject gameObject2 = GameObject.CreatePrimitive(primitiveType);
		gameObject2.GetComponent<Collider>().enabled = false;
		gameObject2.GetComponent<Renderer>().material = material;
		gameObject2.transform.localScale = scale;
		gameObject2.name = "Debug" + gameObject2.name;
		if (pivotOffset != Vector3.zero)
		{
			gameObject = new GameObject(gameObject2.name);
			gameObject2.name = gameObject2.name.Replace("Debug", "");
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.transform.localPosition = pivotOffset;
		}
		if (parent != null)
		{
			if (gameObject == null)
			{
				gameObject2.transform.parent = parent;
				gameObject2.transform.localPosition = Vector3.zero;
			}
			else
			{
				gameObject.transform.parent = parent;
				gameObject.transform.localPosition = Vector3.zero;
			}
		}
		if (!(gameObject != null))
		{
			return gameObject2;
		}
		return gameObject;
	}

	public static GameObject DebugPointer(Transform parent = null)
	{
		return DebugPrimitive(PrimitiveType.Sphere, new Vector3(0.01f, 0.01f, 3f), new Color(1f, 1f, 0f, 0.75f), Vector3.forward, parent);
	}

	public static GameObject DebugBall(Transform parent = null)
	{
		return DebugPrimitive(PrimitiveType.Sphere, Vector3.one * 0.25f, new Color(1f, 0f, 0f, 0.5f), Vector3.zero, parent);
	}
}
