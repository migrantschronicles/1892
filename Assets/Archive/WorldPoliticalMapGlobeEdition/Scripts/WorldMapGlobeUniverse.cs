// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM
// ***************************************************************************
// This is the public API file - every property or public method belongs here
// ***************************************************************************

using UnityEngine;

namespace WPM {

    public enum SKYBOX_STYLE {
		UserDefined = 0,
		Basic = 1,
		MilkyWay = 2,
		DualSkybox = 3
	}

	/* Public WPM Class */
	public partial class WorldMapGlobe : MonoBehaviour {

		[SerializeField]
		Vector3 _earthScenicLightDirection = new Vector4 (-0.5f, 0.5f, -1f);

		public Vector3 earthScenicLightDirection {
			get { return _earthScenicLightDirection; }
			set {
				if (value != _earthScenicLightDirection) {
					_earthScenicLightDirection = value;
					isDirty = true;
					DrawAtmosphere ();
				}
			}
		}

		[SerializeField]
		Transform _sun;

		public Transform sun {
			get { return _sun; }
			set {
				if (value != _sun) {
					_sun = value;
					isDirty = true;
					RestyleEarth ();
				}
			}
		}


		[SerializeField]
		bool _showMoon = false;

		public bool showMoon {
			get { return _showMoon; }
			set {
				if (_showMoon != value) {
					_showMoon = value;
					isDirty = true;
					UpdateMoon ();
				}
			}
		}

		
		[SerializeField]
		bool _moonAutoScale = true;

		public bool moonAutoScale {
			get { return _moonAutoScale; }
			set {
				if (_moonAutoScale != value) {
					_moonAutoScale = value;
					isDirty = true;
					UpdateMoon ();
				}
			}
		}


		[SerializeField]
		SKYBOX_STYLE _skyboxStyle = SKYBOX_STYLE.UserDefined;

		public SKYBOX_STYLE skyboxStyle {
			get { return _skyboxStyle; }
			set {
				if (_skyboxStyle != value) {
					_skyboxStyle = value;
					isDirty = true;
					UpdateSkybox ();
				}
			}
		}

		[SerializeField]
		float _skyboxEnvironmentTransitionAltitudeMin = 1000;

		public float skyboxEnvironmentTransitionAltitudeMin {
			get { return _skyboxEnvironmentTransitionAltitudeMin; }
			set { if (_skyboxEnvironmentTransitionAltitudeMin != value) {
					_skyboxEnvironmentTransitionAltitudeMin = value;
					isDirty = true;
                }
			}
        }


		[SerializeField]
		float _skyboxEnvironmentTransitionAltitudeMax = 1100;

		public float skyboxEnvironmentTransitionAltitudeMax {
			get { return _skyboxEnvironmentTransitionAltitudeMax; }
			set {
				if (_skyboxEnvironmentTransitionAltitudeMax != value) {
					_skyboxEnvironmentTransitionAltitudeMax = value;
					isDirty = true;
				}
			}
		}

		[SerializeField]
		Texture2D _skyboxEnvironmentTextureHDR;

		public Texture2D skyboxEnvironmentTextureHDR {
			get { return _skyboxEnvironmentTextureHDR; }
			set {
				if (_skyboxEnvironmentTextureHDR != value) {
					_skyboxEnvironmentTextureHDR = value;
					UpdateSkybox();
					isDirty = true;
				}
			}
		}

		[SerializeField]
		bool _syncTimeOfDay = false;

		public bool syncTimeOfDay {
			get { return _syncTimeOfDay; }
			set {
				if (_syncTimeOfDay != value) {
					_syncTimeOfDay = value;
					if (_syncTimeOfDay) {
						if (!_earthStyle.isScatter () && !_earthStyle.isScenic ()) {
							earthStyle = EARTH_STYLE.NaturalHighResScenicScatterCityLights;
						}
					} else {
						TiltGlobe ();
					}
					isDirty = true;
				}
			}
		}


	}

}