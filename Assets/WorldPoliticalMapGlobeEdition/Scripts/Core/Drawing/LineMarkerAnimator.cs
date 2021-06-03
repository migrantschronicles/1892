using UnityEngine;
using System.Collections;

namespace WPM {

	public delegate void LineEvent (LineMarkerAnimator lma);

	public class LineMarkerAnimator : MonoBehaviour {

		public Vector3 start, end;

		/// <summary>
		/// Line color.
		/// </summary>
		public Color color;

		/// <summary>
		/// Line Width. Defaults to 0.01
		/// </summary>
		public float lineWidth = 0.01f;

		/// <summary>
		/// The arc elevation.
		/// </summary>
		public float arcElevation;

		/// <summary>
		/// Line drawing duration
		/// </summary>
		public float duration;

        /// <summary>
        /// The number of line points. Increase to improve line resolution. Descrease to improve performance.
        /// </summary>
        public int numPoints = 256;

		/// <summary>
		/// The material used to render the line. See reuseMaterial property.
		/// </summary>
		public Material lineMaterial;

		/// <summary>
		/// Seconds after line is drawn to start fading out effect.
		/// </summary>
		public float autoFadeAfter;

		/// <summary>
		/// Duration for the fadeout effect.
		/// </summary>
		public float fadeOutDuration = 1.0f;

		/// <summary>
		/// If Earth is in inverted mode. Set by WPM internally.
		/// </summary>
		public bool earthInvertedMode;

		/// <summary>
		/// If the provided material should be instantiated. Set this to true to reuse given material and avoid instantiation.
		/// </summary>
		public bool reuseMaterial;

		/// <summary>
		/// This event is called when line fades out
		/// </summary>
		public LineEvent OnLineFadeOut;

		/// <summary>
		/// This event is called when line fades out
		/// </summary>
		public LineEvent OnLineDrawingEnd;

		/// <summary>
		/// The underline Line Renderer component
		/// </summary>
		public LineRenderer lineRenderer {
			get { return _lr; }
		}


		float startTime, startAutoFadeTime;
		float startAltitude, endAltitude;
		LineRenderer _lr;
		Color colorTransparent;
		Vector3[] vertices;

		void OnEnable () {
			_lr = transform.GetComponent<LineRenderer> ();
			if (_lr == null) {
				_lr = gameObject.AddComponent<LineRenderer> ();
			}
			_lr.textureMode = LineTextureMode.Tile;
			_lr.numCornerVertices = 3;
		}


		void Start () {
            // Create the line mesh
            if (numPoints < 2) {
                numPoints = 2;
            }
			startTime = Time.time;
			_lr.useWorldSpace = false;
			_lr.startWidth = lineWidth;
			_lr.endWidth = lineWidth;
			if (!reuseMaterial) {
				lineMaterial = Instantiate (lineMaterial);
			}
			lineMaterial.color = color;
			_lr.material = lineMaterial; // needs to instantiate to preserve individual color so can't use sharedMaterial
			_lr.startColor = color;
			_lr.endColor = color;

			if (vertices == null) {
				startAltitude = start.magnitude;
				endAltitude = end.magnitude;
				vertices = new Vector3[numPoints];
				for (int s = 0; s < numPoints; s++) {
					float t = (float)s / (numPoints - 1);
					float elevation = Mathf.Sin (t * Mathf.PI) * arcElevation;
					Vector3 sPos;
					float h = Mathf.Lerp (startAltitude, endAltitude, t);
					if (earthInvertedMode) {
						sPos = Vector3.Lerp (start, end, t).normalized * h * (1.0f - elevation);
					} else {
						sPos = Vector3.Lerp (start, end, t).normalized * h * (1.0f + elevation);
					}
                    vertices[s] = sPos;
				}
			}

			startAutoFadeTime = float.MaxValue;
			colorTransparent = new Color (color.r, color.g, color.b, 0);

			UpdateLine ();
		}


		public void SetVertices (Vector2[] latlon) {
			if (latlon == null || latlon.Length < 2)
				return;

			Vector3[] spherePoints = new Vector3[latlon.Length];
			for (int k = 0; k < latlon.Length; k++) {
				spherePoints [k] = Conversion.GetSpherePointFromLatLon (latlon [k]);
			}
			startAltitude = spherePoints [0].magnitude;
			endAltitude = spherePoints [latlon.Length - 1].magnitude;

			this.start = spherePoints [0];
			this.end = spherePoints [latlon.Length - 1];

			Vector3 start, end;
			vertices = new Vector3[numPoints];
			for (int s = 0; s < numPoints; s++) {
				float t = (float)s / (numPoints - 1);
				float elevation = Mathf.Sin (t * Mathf.PI) * arcElevation;
				Vector3 sPos;
				float h = Mathf.Lerp (startAltitude, endAltitude, t);

				float t2;
				if (t >= 1f) {
					t2 = 1f;
					start = end = spherePoints [latlon.Length - 1];
				} else {
					float ft = (latlon.Length - 1) * t;
					int index = (int)ft;
					int prev = index;
					int pos = prev + 1;
					if (pos >= latlon.Length) {
						pos = latlon.Length - 1;
					}
					start = spherePoints [prev];
					end = spherePoints [pos];
					t2 = ft - index;
				}
				if (earthInvertedMode) {
					sPos = Vector3.Lerp (start, end, t2).normalized * h * (1.0f - elevation);
				} else {
					sPos = Vector3.Lerp (start, end, t2).normalized * h * (1.0f + elevation);
				}
				vertices [s] = sPos;
			}
		}
	
		// Update is called once per frame
		void Update () {
			if (Time.time >= startAutoFadeTime) {
				UpdateFade ();
			} else {
				UpdateLine ();
			}
		}

		void UpdateLine () {
			float t;
			if (duration == 0)
				t = 1.0f;
			else
				t = (Time.time - startTime) / duration;
			if (t >= 1.0f) {
				t = 1.0f;
				if (autoFadeAfter == 0) {
					Destroy (this); // will destroy this behaviour at the end of the frame
				} else {
					startAutoFadeTime = Time.time;
				}
			}
			SetProgress (t);
		}

		void UpdateFade () {
			float t = Time.time - startAutoFadeTime;
			if (t < autoFadeAfter)
				return;

			t = (t - autoFadeAfter) / fadeOutDuration;
			if (t >= 1.0f) {
				t = 1.0f;
				if (OnLineFadeOut != null)
					OnLineFadeOut (this);
				Destroy (gameObject);
			}

			Color fadeColor = Color.Lerp (color, colorTransparent, t);
			lineMaterial.color = fadeColor;
		}

		void SetProgress (float t) {

			if (vertices.Length < 2)
				return;

			if (t <= 0) {
				_lr.positionCount = 0;
			} else if (t >= 1f) {
				_lr.positionCount = vertices.Length;
				for (int k = 0; k < vertices.Length; k++) {
					_lr.SetPosition (k, vertices [k]);
				}
				if (OnLineDrawingEnd != null) {
					OnLineDrawingEnd (this);
				}
			} else {
				float f = (vertices.Length - 1) * t;
				int lastPos = (int)f + 1;
				_lr.positionCount = lastPos + 1;
				for (int k = 0; k < lastPos; k++) {
					_lr.SetPosition (k, vertices [k]);
				}
				float frac = f - (int)f;
				Vector3 v = Vector3.Lerp (vertices [lastPos - 1], vertices [lastPos], frac);
				_lr.SetPosition (lastPos, v);
			}

		}

	}
}