using UnityEngine;
using System.Collections;

namespace WPM {
	public class TileAnimator : MonoBehaviour {

		public delegate void AnimationEvent (TileInfo ti);

		public event AnimationEvent OnAnimationEnd;
		public bool invert;
		public float duration;
		internal TileInfo ti;
		float startTime;
		bool playing;

		public void Play () {
			ti.renderer.sharedMaterial = ti.parent.transMat;
			ti.SetAlpha (invert ? 1 : 0);
			ti.isAnimating = true;
			ti.animationFinished = false;
			startTime = Time.time;
			playing = true;
			enabled = true;
		}

		public void Stop() {
			playing = false;
			ti.SetAlpha (invert ? 0 : 1);
			ti.animationFinished = true;
			ti.isAnimating = false;
			OnAnimationEnd -= OnAnimationEnd;
			enabled = false;
		}

		void Update () {
			if (!playing)
				return;

			if (ti == null || ti.loadStatus != TILE_LOAD_STATUS.Loaded) {
				Stop ();
				return;
			}

			float t = (Time.time - startTime) / duration;
			if (t >= 1f) {
				t = 1f;
			}
			ti.SetAlpha (invert ? 1f - t : t);
			if (t >= 1) {
				if (OnAnimationEnd != null) {
					OnAnimationEnd (ti);
				}
				ti.renderer.sharedMaterial = ti.parent.opaqueMat;
				Stop ();
			}
		}
	
	}
}