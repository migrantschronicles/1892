using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WPM
{
	public class AnimateSphereAround : MonoBehaviour
	{

		Vector3 originalPosition, destination;
		float startTime, duration;

		void Start ()
		{
			SetRandomDestination();
		}

		void Update() {
			float t = (Time.time - startTime) / duration;
			if (t>1f) t = 1f;
			Vector3 pos = Vector3.Lerp(originalPosition, destination, t).normalized;
			transform.localPosition = pos * 0.5f;
			// Add offset to avoid clipping with terrain
			transform.localPosition += pos * transform.localScale.x * 0.5f;

			if (t>=1f) SetRandomDestination();
		}

		void SetRandomDestination() {
			originalPosition = transform.localPosition;
			destination = Random.onUnitSphere;
			duration = Random.Range(5, 10);
			startTime = Time.time;
		}
	

	}
}
