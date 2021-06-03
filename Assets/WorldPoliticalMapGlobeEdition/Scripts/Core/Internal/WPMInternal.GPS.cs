// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM

using UnityEngine;
using System.Collections;

namespace WPM {

    public partial class WorldMapGlobe : MonoBehaviour {

        #region GPS stuff

        IEnumerator CheckGPS() {
            if (!Application.isPlaying)
                yield break;
            while (_followDeviceGPS) {
                if (!flyToActive && Input.location.isEnabledByUser) {
                    if (Input.location.status == LocationServiceStatus.Stopped) {
                        Input.location.Start();
                    } else if (Input.location.status == LocationServiceStatus.Running) {
                        float latitude = Input.location.lastData.latitude;
                        float longitude = Input.location.lastData.longitude;
                        FlyToLocation(latitude, longitude);
                    }
                }
                yield return new WaitForSeconds(1f);
            }
        }

        void OnApplicationPause(bool pauseState) {

            if (!_followDeviceGPS || !Application.isPlaying)
                return;

            if (pauseState) {
                Input.location.Stop();
            }

        }

        #endregion

    }

}