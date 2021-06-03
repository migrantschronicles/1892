using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace WPM {
	public class SpherePingPong : MonoBehaviour {

		Vector3 origPosition;

		void Awake () {
			origPosition = transform.position;
		}

		void Update () {
			float t = (Mathf.Sin (Time.time) + 1f) * 0.2f;
			transform.position = origPosition * (0.8f + t);
		}

	}

}