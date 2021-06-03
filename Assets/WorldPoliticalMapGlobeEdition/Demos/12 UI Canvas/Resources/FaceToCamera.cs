using UnityEngine;

namespace WPM {

    public class FaceToCamera : MonoBehaviour {

        WorldMapGlobe map;

        void Start() {
            map = WorldMapGlobe.instance;
            Update();
        }

        void Update() {
            // Check if element is inside the front hemisphere
            float d = Vector3.Dot((Camera.main.transform.position - map.transform.position).normalized, (transform.position - map.transform.position).normalized);

            // Lerps between two rotations: when panel is behind globe, make it rotate outside the globe so it does not cross it.
            // and when panel is approaching the camera, make it rotate towards the camera.
            transform.LookAt(map.transform.position, Vector3.up);
            d = Mathf.Clamp01(d);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(transform.position - Camera.main.transform.position, Camera.main.transform.up), d);
        }
    }

}

