// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM
// ***************************************************************************
// This is the public API file - every property or public method belongs here
// ***************************************************************************
using UnityEngine;
using System;

namespace WPM {
    public enum NAVIGATION_MODE {
        EARTH_ROTATES = 0,
        CAMERA_ROTATES = 1
    }

    public enum ZOOM_MODE {
        EARTH_MOVES = 0,
        CAMERA_MOVES = 1
    }

    public enum ROTATION_AXIS_ALLOWED {
        BOTH_AXIS = 0,
        X_AXIS_ONLY = 1,
        Y_AXIS_ONLY = 2
    }

    public enum DRAG_BEHAVIOUR {
        None = 0,
        Rotate = 1,
        CameraOrbit = 2
    }

    public delegate void GlobeClickEvent(Vector3 sphereLocation, int mouseButtonIndex);
    public delegate void GlobeEvent(Vector3 sphereLocation);
    public delegate void RectangleSelectionEvent(Vector3 startPosition, Vector3 endPosition, bool finishedSelection);
    public delegate void ZoomEvent(float zoomLevel);
    public delegate void SimpleEvent();
    public delegate Ray RaycastEvent();

    /* Public WPM Class */
    public partial class WorldMapGlobe : MonoBehaviour {

        public event GlobeClickEvent OnClick;
        public event GlobeClickEvent OnMouseDown;
        public event GlobeClickEvent OnMouseRelease;
        public event GlobeEvent OnDrag;
        public event GlobeEvent OnFlyStart;
        public event GlobeEvent OnFlyEnd;

        public event SimpleEvent OnOrbitRotateStart;
        public event SimpleEvent OnOrbitRotateEnd;
        public event ZoomEvent OnZoomStart;
        public event ZoomEvent OnZoomEnd;

        public event RaycastEvent OnRaycast;

        /// <summary>
        /// Returns true is mouse has entered the Earth's collider.
        /// </summary>
        public bool mouseIsOver {
            get {
                return _mouseIsOver || _earthInvertedMode;
            }
            set {
                _mouseIsOver = value;
            }
        }

        [SerializeField]
        bool
            _VREnabled;

        /// <summary>
        /// Sets or returns VR mode compatibility
        /// </summary>
        public bool VREnabled {
            get {
                return _VREnabled;
            }
            set {
                if (_VREnabled != value) {
                    _VREnabled = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        [Range(1.0f, 16.0f)]
        float
            _navigationTime = 4.0f;

        /// <summary>
        /// The navigation time in seconds.
        /// </summary>
        public float navigationTime {
            get {
                return _navigationTime;
            }
            set {
                if (_navigationTime != value) {
                    _navigationTime = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        [Range(0f, 1f)]
        float
            _navigationBounceIntensity;

        /// <summary>
        /// The bouncing intensity when flying from one coordinate to another.
        /// </summary>
        public float navigationBounceIntensity {
            get {
                return _navigationBounceIntensity;
            }
            set {
                if (_navigationBounceIntensity != value) {
                    _navigationBounceIntensity = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        NAVIGATION_MODE
            _navigationMode = NAVIGATION_MODE.EARTH_ROTATES;

        /// <summary>
        /// Changes the navigation mode so it's the Earth or the Camera which rotates when FlyToxxx methods are called.
        /// </summary>
        public NAVIGATION_MODE navigationMode {
            get {
                return _navigationMode;
            }
            set {
                if (_navigationMode != value) {
                    _navigationMode = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        bool
            _allowUserKeys;

        /// <summary>
        /// Whether WASD keys can rotate the globe.
        /// </summary>
        /// <value><c>true</c> if allow user keys; otherwise, <c>false</c>.</value>
        public bool allowUserKeys {
            get { return _allowUserKeys; }
            set {
                if (value != _allowUserKeys) {
                    _allowUserKeys = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        bool _allowHighlightWhileDragging = true;

        /// <summary>
        /// Allows country/province highlighting while dragging cursor
        /// </summary>
        public bool allowHighlightWhileDragging {
            get { return _allowHighlightWhileDragging; }
            set {
                if (value != _allowHighlightWhileDragging) {
                    _allowHighlightWhileDragging = value;
                    isDirty = true;
                }
            }
        }



        [SerializeField]
        bool _dragConstantSpeed;

        public bool dragConstantSpeed {
            get { return _dragConstantSpeed; }
            set {
                if (value != _dragConstantSpeed) {
                    _dragConstantSpeed = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        float _dragConstantSpeedDampingMultiplier = 0.1f;

        public float dragConstantSpeedDampingMultiplier {
            get { return _dragConstantSpeedDampingMultiplier; }
            set {
                if (value != _dragConstantSpeedDampingMultiplier) {
                    _dragConstantSpeedDampingMultiplier = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        float _dragDampingDuration = 0.5f;

        public float dragDampingDuration {
            get { return _dragDampingDuration; }
            set {
                if (value != _dragDampingDuration) {
                    _dragDampingDuration = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        bool
            _keepStraight;

        public bool keepStraight {
            get { return _keepStraight; }
            set {
                if (value != _keepStraight) {
                    _keepStraight = value;
                    if (_keepStraight) {
                        yaw = 0;
                    }
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        bool
            _allowUserRotation = true;

        public bool allowUserRotation {
            get { return _allowUserRotation; }
            set {
                if (value != _allowUserRotation) {
                    _allowUserRotation = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        ROTATION_AXIS_ALLOWED
            _rotationAxisAllowed = ROTATION_AXIS_ALLOWED.BOTH_AXIS;

        public ROTATION_AXIS_ALLOWED rotationAxisAllowed {
            get { return _rotationAxisAllowed; }
            set {
                if (value != _rotationAxisAllowed) {
                    _rotationAxisAllowed = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        bool
            _centerOnRightClick = true;

        public bool centerOnRightClick {
            get { return _centerOnRightClick; }
            set {
                if (value != _centerOnRightClick) {
                    _centerOnRightClick = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        DRAG_BEHAVIOUR _rightButtonDragBehaviour = DRAG_BEHAVIOUR.Rotate;

        public DRAG_BEHAVIOUR rightButtonDragBehaviour {
            get { return _rightButtonDragBehaviour; }
            set {
                if (value != _rightButtonDragBehaviour) {
                    _rightButtonDragBehaviour = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        Transform _orbitTarget;

        public Transform orbitTarget {
            get { return _orbitTarget; }
            set {
                if (value != _orbitTarget) {
                    _orbitTarget = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        bool
            _orbitSingleAxis;

        public bool orbitSingleAxis {
            get { return _orbitSingleAxis; }
            set {
                if (value != _orbitSingleAxis) {
                    _orbitSingleAxis = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        bool
            _pitchAdjustByDistance;

        public bool pitchAdjustByDistance {
            get { return _pitchAdjustByDistance; }
            set {
                if (value != _pitchAdjustByDistance) {
                    _pitchAdjustByDistance = value;
                    isDirty = true;
                }
            }
        }


        [NonSerialized]
        public bool lockPan, lockYaw, lockPitch;


        [SerializeField]
        bool
            _rightClickRotatingClockwise;

        public bool rightClickRotatingClockwise {
            get { return _rightClickRotatingClockwise; }
            set {
                if (value != _rightClickRotatingClockwise) {
                    _rightClickRotatingClockwise = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        bool
            _respectOtherUI = true;

        /// <summary>
        /// When enabled, will prevent globe interaction if pointer is over an UI element
        /// </summary>
        public bool respectOtherUI {
            get { return _respectOtherUI; }
            set {
                if (value != _respectOtherUI) {
                    _respectOtherUI = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        bool
            _allowUserZoom = true;

        public bool allowUserZoom {
            get { return _allowUserZoom; }
            set {
                if (value != _allowUserZoom) {
                    _allowUserZoom = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        float
            _zoomMaxDistance = 10;

        public float zoomMaxDistance {
            get { return _zoomMaxDistance; }
            set {
                if (value != _zoomMaxDistance) {
                    _zoomMaxDistance = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        float
            _zoomMinDistance = 0;

        public float zoomMinDistance {
            get { return _zoomMinDistance; }
            set {
                if (value != _zoomMinDistance) {
                    _zoomMinDistance = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        ZOOM_MODE
            _zoomMode = ZOOM_MODE.CAMERA_MOVES;

        /// <summary>
        /// Changes the zoom mode so it's the Earth or the Camera which moves towards each other when zooming in/out
        /// </summary>
        public ZOOM_MODE zoomMode {
            get {
                return _zoomMode;
            }
            set {
                if (_zoomMode != value) {
                    _zoomMode = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        bool
            _invertZoomDirection;

        public bool invertZoomDirection {
            get { return _invertZoomDirection; }
            set {
                if (value != _invertZoomDirection) {
                    _invertZoomDirection = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        bool
            _zoomAtMousePosition;

        public bool zoomAtMousePosition {
            get { return _zoomAtMousePosition; }
            set {
                if (value != _zoomAtMousePosition) {
                    _zoomAtMousePosition = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        bool _zoomConstantSpeed;

        public bool zoomConstantSpeed {
            get { return _zoomConstantSpeed; }
            set {
                if (value != _zoomConstantSpeed) {
                    _zoomConstantSpeed = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        [Range(0.1f, 3)]
        float
            _mouseDragSensitivity = 0.5f;

        public float mouseDragSensitivity {
            get { return _mouseDragSensitivity; }
            set {
                if (value != _mouseDragSensitivity) {
                    _mouseDragSensitivity = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        [Range(0.1f, 3)]
        float
            _cameraRotationSensibility = 0.5f;

        public float cameraRotationSensibility {
            get { return _cameraRotationSensibility; }
            set {
                if (value != _cameraRotationSensibility) {
                    _cameraRotationSensibility = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        bool
            _dragOnScreenEdges;

        public bool dragOnScreenEdges {
            get { return _dragOnScreenEdges; }
            set {
                if (value != _dragOnScreenEdges) {
                    _dragOnScreenEdges = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        float _dragMaxDuration;

        public float dragMaxDuration {
            get { return _dragMaxDuration; }
            set {
                if (value != _dragMaxDuration) {
                    _dragMaxDuration = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        [Range(0, 0.3f)]
        float _dragOnScreenEdgesMarginPercentage = 0.05f;

        public float dragOnScreenEdgesMarginPercentage {
            get { return _dragOnScreenEdgesMarginPercentage; }
            set {
                if (value != _dragOnScreenEdgesMarginPercentage) {
                    _dragOnScreenEdgesMarginPercentage = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        [Range(0.1f, 3)]
        float
            _dragOnScreenEdgesSpeed = 1f;

        public float dragOnScreenEdgesSpeed {
            get { return _dragOnScreenEdgesSpeed; }
            set {
                if (value != _dragOnScreenEdgesSpeed) {
                    _dragOnScreenEdgesSpeed = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        float
            _orbitSmoothSpeed = 5f;

        public float orbitSmoothSpeed {
            get { return _orbitSmoothSpeed; }
            set {
                if (value != _orbitSmoothSpeed) {
                    _orbitSmoothSpeed = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        float
            _pitchMaxDistanceTilt = 1500f;

        public float pitchMaxDistanceTilt {
            get { return _pitchMaxDistanceTilt; }
            set {
                if (value != _pitchMaxDistanceTilt) {
                    _pitchMaxDistanceTilt = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        float
            _pitchMinDistanceTilt = 30f;

        public float pitchMinDistanceTilt {
            get { return _pitchMinDistanceTilt; }
            set {
                if (value != _pitchMinDistanceTilt) {
                    _pitchMinDistanceTilt = value;
                    isDirty = true;
                }
            }
        }



        [SerializeField]
        bool _orbitTiltOnZoomIn = true;

        public bool orbitTiltOnZoomIn {
            get { return _orbitTiltOnZoomIn; }
            set {
                if (value != _orbitTiltOnZoomIn) {
                    _orbitTiltOnZoomIn = value;
                    isDirty = true;
                }
            }
        }



        [SerializeField]
        [Range(0.1f, 3)]
        float
            _mouseWheelSensitivity = 0.5f;

        public float mouseWheelSensitivity {
            get { return _mouseWheelSensitivity; }
            set {
                if (value != _mouseWheelSensitivity) {
                    _mouseWheelSensitivity = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        [Range(0.1f, 3)]
        float
            _cameraZoomSpeed = 0.5f;

        public float cameraZoomSpeed {
            get { return _cameraZoomSpeed; }
            set {
                if (value != _cameraZoomSpeed) {
                    _cameraZoomSpeed = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        int
            _mouseDragThreshold = 0;

        public int mouseDragThreshold {
            get { return _mouseDragThreshold; }
            set {
                if (_mouseDragThreshold != value) {
                    _mouseDragThreshold = value;
                    isDirty = true;
                }
            }
        }



        [SerializeField]
        float
            _zoomDamping = 0.6f;

        /// <summary>
        /// Speed for the zoom deceleration once user stops applying zoom.
        /// </summary>
        public float zoomDamping {
            get { return _zoomDamping; }
            set {
                if (_zoomDamping != value) {
                    _zoomDamping = value;
                }
            }
        }



        /// <summary>
        /// Get/sets the constraint position.
        /// </summary>
        public Vector3 constraintPosition = new Vector3(0.5f, 0, 0);

        /// <summary>
        /// Gets/sets the constraint angle in degrees (when enabled, the constraint won't allow the user to rotate the globe beyond this angle and contraintPosition).
        /// Useful to stick around certain locations.
        /// </summary>
        public float constraintPositionAngle = 15f;

        /// <summary>
        /// Enabled/disables constraint option.
        /// </summary>
        public bool constraintPositionEnabled;


        /// <summary>
        /// Enables latitude constraint
        /// </summary>
        public bool constraintLatitudeEnabled;

        public float constraintLatitudeMaxAngle = 90;
        public float constraintLatitudeMinAngle = -90;



        [SerializeField]
        bool
            _followDeviceGPS;

        /// <summary>
        /// Sets if map should be centered on current device GPS coordinates
        /// </summary>
        public bool followDeviceGPS {
            get { return _followDeviceGPS; }
            set {
                if (value != _followDeviceGPS) {
                    _followDeviceGPS = value;
                    if (_followDeviceGPS) {
                        StartCoroutine(CheckGPS());
                    }
                    isDirty = true;
                }
            }
        }



        [SerializeField]
        bool _enableOrbit;

        /// <summary>
        /// If camera orbit (pitch / yaw) controls are enabled
        /// </summary>
        public bool enableOrbit {
            get { return _enableOrbit; }
            set {
                if (value != _enableOrbit) {
                    _enableOrbit = value;
                    if (_enableOrbit) {
                        CheckCameraPivot();
                    } else {
                        OrbitReset();
                    }
                    isDirty = true;
                }
            }
        }



        [SerializeField]
        bool _orbitInvertDragDirection;

        /// <summary>
        /// Invert drag directino
        /// </summary>
        public bool orbitInvertDragDirection {
            get { return _orbitInvertDragDirection; }
            set {
                if (value != _orbitInvertDragDirection) {
                    _orbitInvertDragDirection = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        float _yaw;

        /// <summary>
        /// Camera yaw in degrees
        /// </summary>
        public float yaw {
            get { return _yaw; }
            set {
                if (value != _yaw) {
                    _yaw = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        float _pitch;

        /// <summary>
        /// Camera pitch in degrees
        /// </summary>
        public float pitch {
            get { return _pitch; }
            set {
                if (value != _pitch) {
                    _pitch = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        float _maxPitch = 70;

        /// <summary>
        /// Maximum allowed pitch in degrees
        /// </summary>
        public float maxPitch {
            get { return _maxPitch; }
            set {
                if (value != _maxPitch) {
                    _maxPitch = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        float _targetElevation;

        /// <summary>
        /// Target elevation in km
        /// </summary>
        public float targetElevation {
            get { return _targetElevation; }
            set {
                if (value != _targetElevation) {
                    _targetElevation = value;
                    isDirty = true;
                }
            }
        }



        #region Public API area

        /// <summary>
        /// Sets the zoom level
        /// </summary>
        /// <param name="zoomLevel">Value from 0 to 1</param>
        public void SetZoomLevel(float zoomLevel) {
            float f = GetZoomLevelDistance(zoomLevel);
            SetCameraDistance(f);
        }

        /// <summary>
        /// Returns the distance of the camera or pivot to the center of the globe
        /// </summary>
        public float GetCameraDistance() {
            return Vector3.Distance(pivotTransform.position, transform.position);
        }


        /// <summary>
        /// Returns the distance of the camera to the surface of the globe
        /// </summary>
        public float GetCameraDistanceToSurface() {
            return GetCameraDistance() - radius;
        }

        /// <summary>
        /// Sets camera distance to surface of the globe
        /// </summary>
        /// <param name="distanceToSurface"></param>
        public void SetCameraDistanceToSurface(float distanceToSurface) {
            SetCameraDistance(distanceToSurface + radius);
        }

        /// <summary>
        /// Sets the camera distance to the center of the globe
        /// </summary>
        public void SetCameraDistance(float distanceToCenter) {
            Camera cam = mainCamera;

            if (_earthInvertedMode) {
                cam.fieldOfView = Mathf.Lerp(MIN_FIELD_OF_VIEW, MAX_FIELD_OF_VIEW, distanceToCenter / radius);
                return;
            }
            
            // Gets the max distance from the map
            float minRadius = GetMinEarthRadius();

            Vector3 dir = (pivotTransform.position - transform.position).normalized;
            float distance = Mathf.Max(distanceToCenter, minRadius);

            if (_zoomMode == ZOOM_MODE.EARTH_MOVES) {
                // it's the Earth which get closer to the camera
                transform.position = pivotTransform.position - dir * distance;
            } else {
                pivotTransform.position = transform.position + dir * distance;
            }
        }

        /// <summary>
        /// Gets the current zoom level (0..1) 
        /// </summary>
        public float GetZoomLevel() {
            // Gets the max distance from the map
            Camera cam = mainCamera;
            if (cam == null) return 1;
            float zoomLevel;
            if (_earthInvertedMode) {
                float fv = cam.fieldOfView;
                zoomLevel = (fv - MIN_FIELD_OF_VIEW) / (MAX_FIELD_OF_VIEW - MIN_FIELD_OF_VIEW);
            } else {
                // Takes the distance from the focus point and adjust it according to the zoom level
                float frustumDistance = GetFrustumDistance(cam);
                float dist = Vector3.Distance(transform.position, pivotTransform.position);
                float minRadius = GetMinEarthRadius();
                zoomLevel = (dist - minRadius) / (frustumDistance - minRadius);
                if (zoomLevel < 0) zoomLevel = 0;
            }
            return zoomLevel;
        }


        /// <summary>
        /// Compute zoom level based on altitude in kilometers
        /// </summary>
        /// <returns>The zoom level.</returns>
        /// <param name="altitudeInMeters">Altitude in meters.</param>
        public float GetZoomLevel(float altitudeInKm) {
            const float EARTH_RADIUS_KM = 6371f;
            float distanceWS = radius * ((altitudeInKm + EARTH_RADIUS_KM) / EARTH_RADIUS_KM);
            float frustumDistance = GetFrustumDistance(mainCamera);
            float minRadius = GetMinEarthRadius();
            float zoomLevel = (distanceWS - minRadius) / (frustumDistance - minRadius);
            if (zoomLevel < 0) {
                zoomLevel = 0;
            }
            return zoomLevel;
        }

        /// <summary>
        /// Compute zoom level based on a world space position
        /// </summary>
        public float GetZoomLevel(Vector3 position) {
            float distanceToSurfaceWS = Vector3.Distance(position, transform.position) - radius;
            float maxDistance = GetZoomLevelDistance(1f);
            float zoomLevel = (distanceToSurfaceWS - _zoomMinDistance) / (maxDistance - _zoomMinDistance);
            if (zoomLevel < 0)
                zoomLevel = 0;
            return zoomLevel;
        }

        /// <summary>
        /// Gets the distance to the globe center for a given zoom level (0..1)
        /// </summary>
        public float GetZoomLevelDistance(float zoomLevel) {
            Camera cam = mainCamera;
            if (_earthInvertedMode) {
                return Mathf.Lerp(MIN_FIELD_OF_VIEW, MAX_FIELD_OF_VIEW, zoomLevel);
            } else {
                float frustumDistance = GetFrustumDistance(cam);
                float minRadius = GetMinEarthRadius();
                return minRadius + (frustumDistance - minRadius) * zoomLevel;
            }
        }

        /// <summary>
        /// Starts zooming to specified zoom level using navigation time as duration
        /// </summary>
        /// <param name="destinationZoomLevel"></param>
        public CallbackHandler ZoomTo(float destinationZoomLevel) {
            return ZoomTo(destinationZoomLevel, _navigationTime);
        }

        /// <summary>
        /// Starts zooming to specified zoom level
        /// </summary>
        /// <param name="destinationZoomLevel"></param>
        public CallbackHandler ZoomTo(float destinationZoomLevel, float duration) {
            zoomToStartTime = Time.time;
            zoomToDuration = duration;
            zoomToStartDistance = GetCameraDistance();
            zoomToEndDistance = GetZoomLevelDistance(destinationZoomLevel);
            zoomToActive = true;
            zoomComplete = false;

            if (duration == 0) {
                ZoomToDestination();
            } else {
                if (OnZoomStart != null) {
                    OnZoomStart(GetZoomLevel());
                }
            }
            return new CallbackHandlerForZoomTo(this);
        }

        /// <summary>
        /// Starts navigation to target location in local spherical coordinates.
        /// </summary>
        public CallbackHandler FlyToLocation(Vector3 destination) {
            return FlyToLocation(destination, _navigationTime, 0, _navigationBounceIntensity);
        }

        /// <summary>
        /// Navigates to target latitude and longitude using navigationTime duration.
        /// </summary>
        public CallbackHandler FlyToLocation(Vector2 latlon) {
            return FlyToLocation(latlon.x, latlon.y, _navigationTime, _navigationBounceIntensity);
        }

        /// <summary>
        /// Navigates to target latitude and longitude using navigationTime duration.
        /// </summary>
        public CallbackHandler FlyToLocation(float latitude, float longitude) {
            return FlyToLocation(latitude, longitude, _navigationTime, _navigationBounceIntensity);
        }

        /// <summary>
        /// Navigates to target latitude and longitude using given duration.
        /// </summary>
        public CallbackHandler FlyToLocation(Vector2 latlon, float duration, float destinationZoomLevel = 0) {
            Vector3 destination = Conversion.GetSpherePointFromLatLon(latlon.x, latlon.y);
            return FlyToLocation(destination, duration, destinationZoomLevel, _navigationBounceIntensity);
        }


        /// <summary>
        /// Navigates to target latitude and longitude using given duration.
        /// </summary>
        public CallbackHandler FlyToLocation(float latitude, float longitude, float duration, float destinationZoomLevel = 0) {
            Vector3 destination = Conversion.GetSpherePointFromLatLon(latitude, longitude);
            return FlyToLocation(destination, duration, destinationZoomLevel, _navigationBounceIntensity);
        }

        /// <summary>
        /// Starts navigation to target location in local spherical coordinates.
        /// </summary>
        /// <param name="destination">Destination in spherical coordinates.</param>
        /// <param name="duration">Duration.</param>
        /// <param name="destinationZoomLevel">Destination zoom level. A value of 0 will keep current zoom level.</param>
        /// <param name="bounceIntensity">Bounce intensity. 0 uses a linear transition between zoom levels. A value > 0 will produce a jump effect.</param>
        /// <param name="cameraDistance">Distance in world space units to the surface.</param> 
        public CallbackHandler FlyToLocation(Vector3 destination, float duration, float destinationZoomLevel = 0, float bounceIntensity = 0, float cameraDistance = 0) {

            flyToGlobeStartPosition = Misc.Vector3one * -1000;
            flyToEndDestination = destination;
            if (_navigationMode == NAVIGATION_MODE.EARTH_ROTATES || _earthInvertedMode) {
                flyToStartQuaternion = transform.rotation;
            } else {
                flyToStartQuaternion = pivotTransform.rotation;
            }
            flyToDuration = duration;
            flyToActive = true;
            flyToComplete = false;
            flyToStartTime = Time.time;
            flyToCameraStartPosition = (pivotTransform.position - transform.position).normalized;
            flyToCameraStartUpVector = pivotTransform.up;
            flyToCameraEndPosition = transform.TransformDirection(destination);
            flyToCameraStartRotation = pivotTransform.rotation;
            Vector3 lookDir = transform.position - pivotTransform.position;
            if (lookDir.sqrMagnitude > 0) {
                flyToCameraEndRotation = Quaternion.LookRotation(lookDir.normalized, flyToCameraStartUpVector); // transform.up);
            } else {
                flyToCameraEndRotation = flyToCameraStartRotation;
            }
            flyToBounceIntensity = bounceIntensity;
            flyToMode = _navigationMode;
            if (destinationZoomLevel > 0 || bounceIntensity > 0) {
                flyToCameraDistanceStart = GetCameraDistance();
                flyToCameraDistanceEnd = destinationZoomLevel > 0 ? GetZoomLevelDistance(destinationZoomLevel) : flyToCameraDistanceStart;
                flyToCameraDistanceEnabled = true;
            } else if (cameraDistance > 0) {
                flyToCameraDistanceStart = GetCameraDistance();
                flyToCameraDistanceEnd = cameraDistance;
                flyToCameraDistanceEnabled = true;
            } else {
                flyToCameraDistanceEnabled = false;
            }

            if (duration == 0) {
                NavigateToDestination();
            } else {
                if (OnFlyStart != null) {
                    OnFlyStart(GetCurrentMapLocation());
                }
            }
            return new CallbackHandlerForFlyTo(this);
        }

        /// <summary>
        /// Returns whether a FlyToXXX() operation is executing
        /// </summary>
        public bool isFlyingToActive {
            get { return flyToActive; }
        }


        /// <summary>
        /// Returns true if latest FlyToXXX() operation was completed successfully (ie. not interrupted)
        /// </summary>
        public bool isFlyingComplete {
            get { return flyToComplete; }
        }

        /// <summary>
        /// Returns true if a ZoomTo() operation is executing
        /// </summary>
        public bool isZoomToActive {
            get { return zoomToActive; }
        }

        /// <summary>
        /// Returns true if latest ZoomTo() operation was completed successfully (ie. not interrupted)
        /// </summary>
        public bool isZoomComplete {
            get { return zoomComplete; }
        }

        /// <summary>
        /// Returns true if a OrbitRotateTo() operation is executing
        /// </summary>
        public bool isOrbitRotateToActive {
            get { return orbitRotateToActive; }
        }

        /// <summary>
        /// Returns true if latest OrbitRotateTo() operation was completed successfully (ie. not interrupted)
        /// </summary>
        public bool isOrbitRotateToComplete {
            get { return orbitRotateToComplete; }
        }


        /// <summary>
        /// Stops any FlyTo(), ZoomTo() or OrbitRotateTo() operation
        /// </summary>
        public void StopAnyNavigation() {
            StopNavigation(false);
            StopZooming(false);
            StopRotateTo(false);
        }

        /// <summary>
        /// Stops current FlyTo() operation
        /// </summary>
        public void StopNavigation() {
            StopNavigation(false);
        }

        /// <summary>
        /// Stops current RotateTo() operation
        /// </summary>
        public void StopRotateTo() {
            StopRotateTo(false);
        }


        /// <summary>
        /// Stops current ZoomTo operation
        /// </summary>
        public void StopZooming() {
            StopZooming(false);
        }


        /// <summary>
        /// Signals a simulated mouse button click.
        /// </summary>
        public void SetSimulatedMouseButtonClick(int buttonIndex) {
            simulatedMouseButtonClick = buttonIndex;
        }

        /// <summary>
        /// Signals a simulated mouse button press. Use this for dragging purposes.
        /// </summary>
        public void SetSimulatedMouseButtonPressed(int buttonIndex) {
            simulatedMouseButtonPressed = buttonIndex;
        }

        /// <summary>
        /// Signals a simulated mouse button release.
        /// </summary>
        public void SetSimulatedMouseButtonRelease(int buttonIndex) {
            simulatedMouseButtonRelease = buttonIndex;
        }

        /// <summary>
        /// Returns the sphere coordinates of the center of the currently visible map
		/// </summary>
		public Vector3 GetCurrentMapLocation(bool worldSpace = false) {
            Camera cam = mainCamera;
            Ray ray = new Ray(pivotTransform.position, cam.transform.forward);
            Vector3 hitPos;
            if (GetGlobeIntersection(ray, out hitPos)) { 
                if (worldSpace) {
                    return hitPos;
                }
                return transform.InverseTransformPoint(hitPos);
            }
            return worldSpace ? transform.TransformPoint(_cursorLocation) : _cursorLocation; // fallback
        }


        /// <summary>
        /// Initiates a rectangle selection operation.
        /// </summary>
        /// <returns>The rectangle selection.</returns>
        public GameObject RectangleSelectionInitiate(RectangleSelectionEvent rectangleSelectionCallback, Color rectangleFillColor, Color rectangleBorderColor, float borderWidth = 0.2f) {
            RectangleSelectionCancel();
            GameObject rectangle = mAddMarkerQuad(MARKER_TYPE.QUAD, Vector3.zero, Vector3.zero, rectangleFillColor, rectangleBorderColor, borderWidth);
            RectangleSelection rs = rectangle.AddComponent<RectangleSelection>();
            rs.map = this;
            rs.callback = rectangleSelectionCallback;
            rs.fillColor = rectangleFillColor;
            rs.borderColor = rectangleBorderColor;
            rs.borderWidth = borderWidth;
            return rectangle;
        }

        /// <summary>
        /// Cancel any rectangle selection operation in progress
        /// </summary>
        public void RectangleSelectionCancel() {
            if (overlayMarkersLayer == null)
                return;
            RectangleSelection[] rrss = overlayMarkersLayer.GetComponentsInChildren<RectangleSelection>(true);
            for (int k = 0; k < rrss.Length; k++) {
                Destroy(rrss[k].gameObject);
            }
        }

        /// <summary>
        /// Returns true if a rectangle selection is occuring
        /// </summary>
        public bool rectangleSelectionInProgress {
            get {
                if (overlayMarkersLayer == null)
                    return false;
                RectangleSelection rs = overlayMarkersLayer.GetComponentInChildren<RectangleSelection>();
                return rs != null;
            }
        }

        #endregion


    }

}