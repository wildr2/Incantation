using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util : MonoBehaviour
{
	public static void DebugDrawCross(Vector3 position, Color? color=null, float duration=0.0f, float size=0.1f, bool drawOnTop=false)
	{
		float halfSize = size / 2f;
		Color _color = color != null ? (Color)color : Color.red;
 
		// Horizontal.
		Debug.DrawLine(position + Vector3.left * halfSize, position + Vector3.right * halfSize, _color, duration, !drawOnTop);
		Debug.DrawLine(position + Vector3.back * halfSize, position + Vector3.forward * halfSize, _color, duration, !drawOnTop);

		// Vertical.
		Debug.DrawLine(position + Vector3.up * halfSize, position + Vector3.down * halfSize, _color, duration, !drawOnTop);
		Debug.DrawLine(position + Vector3.left * halfSize, position + Vector3.right * halfSize, _color, duration, !drawOnTop);
	}

	public static Vector3 IntersectPlaneRay(Ray ray, Vector3 planePoint, Vector3 planeNormal)
	{
		float rayDistance;
		Plane plane = new Plane(planeNormal, planePoint);

		if (plane.Raycast(ray, out rayDistance))
		{
			return ray.GetPoint(rayDistance);
		}

		// No intersection.
		return Vector3.zero;
	}

	public static bool IntersectSphereLine(Vector3 center, float radius, Vector3 line0, Vector3 line1, out Vector3? intersection0, out Vector3? intersection1)
	{
		intersection0 = null;
		intersection1 = null;

		Vector3 line = line1 - line0;
		Vector3 lineToSphere = center - line0;

		float tClosest = Vector3.Dot(lineToSphere, line) / line.sqrMagnitude;

		Vector3 closestPoint = line0 + tClosest * line;
		Vector3 closestPointToCenter = closestPoint - center;
		float distanceSquared = closestPointToCenter.sqrMagnitude;

		if (distanceSquared > radius * radius)
		{
			// Line misses the sphere.
			return false;
		}

		float offset = Mathf.Sqrt(radius * radius - distanceSquared);
		Vector3 lineDir = line.normalized;
		intersection0 = closestPoint - offset * lineDir;
		intersection1 = closestPoint + offset * lineDir;

		float t0 = Vector3.Dot((Vector3)intersection0 - line0, line) / line.sqrMagnitude;
		float t1 = Vector3.Dot((Vector3)intersection1 - line0, line) / line.sqrMagnitude;

		if (t0 < 0.0f || t0 > 1.0f)
		{
			intersection0 = null;
		}
		if (t1 < 0.0f || t1 > 1.0f)
		{
			intersection1 = null;
		}

		return true;
	}

	public static bool Periodic(double periodSeconds, double time=-1, double deltaTime=-1)
	{
		double t = time >= 0 ? time : Time.timeAsDouble;
		double dt = deltaTime >= 0 ? deltaTime : Time.deltaTime;
		double recentPeriodTime = (int)(t / periodSeconds) * periodSeconds;
		return (t - dt) < recentPeriodTime;
	}

	public static bool PeriodicOnOff(float periodSeconds, float time=-1)
	{
		float t = time >= 0 ? time : Time.time;
		return (int)(t / (periodSeconds * 0.5f)) % 2 == 0;
	}

	public static float Modulo(float x, float m)
	{
		return (x % m + m) % m;
	}

	public static float Map(float inputMin, float inputMax, float outputMin, float outputMax, float value)
	{
		if (inputMin > inputMax)
		{
			return Map(inputMax, inputMin, outputMax, outputMin, value);
		}
		if (value < inputMin)
		{
			return outputMin;
		}
		else if (value > inputMax)
		{
			return outputMax;
		}
		else
		{
			return (value - inputMin) / (inputMax - inputMin) * (outputMax - outputMin) + outputMin;
		}
	}

	public static float WrappedDelta(float a, float b, float min, float max, bool forwardOnly=false)
	{
		if (forwardOnly && a > b)
		{
			return (max - a) + (b - min);
		}

		float range = max - min;
		float halfRange = range / 2.0f;

		float delta = (b - a) % range;

		if (delta > halfRange)
		{
			delta -= range;
		}
		else if (delta < -halfRange)
		{
			delta += range;
		}

		return delta;
	}

	public static float WrappedLerp(float a, float b, float t, float min, float max)
	{
		float wa = Wrap(a, min, max);
		float wb = Wrap(b, min, max);

		return wa < wb ? Mathf.Lerp(wa, wb, t) :
			Wrap(Mathf.Lerp(wa, wa + Util.WrappedDelta(wa, wb, min, max, true), t), min, max);
	}

	public static float Wrap(float value, float min, float max)
	{
		float range = max - min;
		float wrapped = (value - min) % range;
		return (wrapped < 0 ? wrapped + range : wrapped) + min;
	}

	public static float RoundToNearestFactor(float value, float factor)
	{
		if (factor == 0)
		{
			// Avoid division by zero
			return value;
		}

		float roundedValue = Mathf.Round(value / factor) * factor;
		return roundedValue;
	}

	public static Vector3 Flatten(Vector3 vector)
	{
		vector.y = 0.0f;
		return vector;
	}

	public static float CalculateDistanceToBoxSurface(Vector3 point, Vector3 boxCenter, Vector3 boxSize)
	{
		// Calculate the half extents of the box
		Vector3 halfExtents = boxSize / 2.0f;

		// Calculate the difference between the point and the box center
		Vector3 difference = point - boxCenter;

		// Clamp the difference vector to the half extents of the box
		Vector3 clampedDifference = new Vector3(
			Mathf.Clamp(difference.x, -halfExtents.x, halfExtents.x),
			Mathf.Clamp(difference.y, -halfExtents.y, halfExtents.y),
			Mathf.Clamp(difference.z, -halfExtents.z, halfExtents.z)
		);

		// Calculate the closest point on the surface of the box
		Vector3 closestPoint = boxCenter + clampedDifference;

		// Calculate the distance between the original point and the closest point
		float distance = Vector3.Distance(point, closestPoint);

		return distance;
	}

	public static Vector3 SquashStretch(Vector3 scale, float squash, float stretchFactor=1)
	{
		float yScale = 1.0f - squash;
		float xzScale = (1.0f + squash) * stretchFactor;
		scale.y *= yScale;
		scale.x *= xzScale;
		scale.z *= xzScale;
		return scale;
	}

	public static Color SetAlpha(Color color, float a)
	{
		return new Color(color.r, color.g, color.b, a);
	}

	public static Vector3 GetArrowKeysXZDir()
	{
		Vector3 dir = new Vector3();
		dir.x = (Input.GetKey(KeyCode.LeftArrow) ? -1.0f : 0.0f) + (Input.GetKey(KeyCode.RightArrow) ? 1.0f : 0.0f);
		dir.z = (Input.GetKey(KeyCode.DownArrow) ? -1.0f : 0.0f) + (Input.GetKey(KeyCode.UpArrow) ? 1.0f : 0.0f);
		return dir.normalized;
	}

	public static int GetNumericKeyInput()
	{
		for (int i = 0; i <= 9; i++)
		{
			if (Input.GetKey(i.ToString()))
			{
				return i;
			}
		}

		return -1;
	}

	public static int GetNumericKeyInputDown()
	{
		for (int i = 0; i <= 9; i++)
		{
			if (Input.GetKeyDown(i.ToString()))
			{
				return i;
			}
		}

		return -1;
	}

	public static int GetNumericKeyInputUp()
	{
		for (int i = 0; i <= 9; i++)
		{
			if (Input.GetKeyUp(i.ToString()))
			{
				return i;
			}
		}

		return -1;
	}

	public static void Shuffle<T>(IList<T> list)
	{
		int n = list.Count;
		for (int i = 0; i < (n - 1); ++i)
		{
			int r = i + Random.Range(0, n - i);
			T t = list[r];
			list[r] = list[i];
			list[i] = t;
		}
	}

	public static Vector2 RandomDir2D()
	{
		float angle = Random.Range(0.0f, Mathf.PI * 2.0f);
		return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
	}

	public static char RandomLetter()
	{
		return (char)Random.Range('a', 'z' + 1);
	}

	public static int GetEnumCount<T>() where T : System.Enum
	{
		return System.Enum.GetValues(typeof(T)).Length;
	}
}

public static class Extensions
{
	public static void SetXZ(this Transform transform, Vector3 xz)
	{
		transform.position = new Vector3(xz.x, transform.position.y, xz.z);
	}

	public static void SetY(this Transform transform, float y)
	{
		transform.position = new Vector3(transform.position.x, y, transform.position.z);
	}
}

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	protected static T instance;
	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				instance = FindObjectOfType<T>();
			}
			return instance;
		}
	}
}
