***************************************
* WORLD POLITICAL MAP - GLOBE EDITION *
*  Created by Ramiro Oliva (Kronnect) *
*             README FILE             *
***************************************


How to use this asset
---------------------
Firstly, you should run the Demo Scene provided to get an idea of the overall functionality.
Later, you should read the documentation and experiment with the API/prefabs.


Demo Scene
----------
There're several demo scenes, located in "Demo" folder. Just go there from Unity, start with "GeneralDemo" scene and run it.


Documentation/API reference
---------------------------
The PDF is located in the Doc folder. It contains instructions on how to use the prefab and the API so you can control it from your code.


Support
-------
Please read the documentation PDF and browse/play with the demo scene and sample source code included before contacting us for support :-)

* Support: contact@kronnect.com
* Website-Forum: https://kronnect.com/support
* Twitter: @KronnectGames
* Facebook: http://facebook.com/Kronnect


Version history
---------------

Current version
  - Added Camera AutoRotation Speed parameter
  - [Fix] Fixed an issue with ProvincesMerge when a smaller province region is surrounded by another neighbour

Version 15.0
  - Added "Highlight While Dragging" option to inspector. This option allows customization of this behaviour.
  - API: added ProvincesMerge(List<Province>...)
  - [Fix] Fixed issue in playmode when toggline country names visibility in blended rendering mode
  - [Fix] Fixed default position of some country labels

Version 14.9.2
  - Tile servers list update: added MapBox (satellite, traffic, terrain, terrain-rgb, incidents, streets, countries)

Version 14.9.1
  - Tile servers list update: added Google Maps (satellite + relief), ESRI (topo, streets, satellite, national geo style), Maps-For-Free, USGS satellite and OpenStreeMaps Hiking map.
  - Tile servers list update: removed MapBox satellite.
  - Tile system: optimized system reload when switching tile server

Version 14.9
  - Updated "Earth Grafiti" demo scene. Added "Pen Strength"
  - Updated cities population
  - Updated support of latest Oculus VR integration plugin
  - Added compatibility with Unity 2020.2

Version 14.8
  - Added Dual Skybox option which seamlessly switched between starfield and environment cubemap based on altitude
  - API: added DrawProvinces(List<Country>). Draws provinces of a list of given countries
  - [Fix] Fixed latLonCenter returning (0,0) for a province if it has not been drawn
  - [Fix] Fixed province coloring conflict with highlight

Version 14.7
  - Added "Cursor Style" option. Currently can switch from legacy behaviour to a latitude/longitude cursor
  - Added "Zoom At Mouse Pos". When enabled and using the mouse wheel or pinch in/out gesture, will move camera in the direction of the cursor and not the center of viewport
  - "Drag Damping Duration" parameter is now available when using Constand Drag Speed
  - Fog of War: added Noise parameter
  - Split drag/rotation speed so different values can be used depending on rotation/zoom modes (Earth or Camera rotates/moves)
  - UI elements no longer block mouse drag operation

Version 14.6.1
  - [Fix] Fixed trembling drag when zoomed very high with very big globe scale
  - [Fix] Fixed fog of war shaders issue with Universal Rendering Pipeline
  - [Fix] Tile system: visibility tests now take into account customized camera viewport rect
  - [Fix] Fixed province names in Norway (Østfold) and Azerbaijan (Ağdaş)
 
Version 14.6
  - Optimized heap allocations during city and province data loading to avoid memory pressure on mobile devices
  - API: added OnRaycast event. Allows user to override default raycasting to determine cursor position
  - [Fix] Fixed province name typos due to incorrect encoding handling
  - [Fix] Fixed country labels disappearing in world space mode when adding polygon markers

Version 14.5
  - Updated Oculus VR integration support
  - Added "Zoom Constant Speed" (similar to Drag Constant Speed but related to zooming)
  - Improved rotation precision

Version 14.4.1
  - Added City Lights Brighness setting to inspector
  - Improved cursor precision
  - [Fix] Fixes to orbit mode changes when using keep straight option
  - [Fix] Fixes to tile occlusion detection
  - [Fix] Fixed cursor dash line style

Version 14.4
  - Map Editor: country and province regions are now listed with additional commands to extract / create entities from them

Version 14.3
  - Added HDR support to inspector color pickers
  - Added "Max Drag Duration" parameter to inspector
  - API: Map Editor: added CountryTransferAsProvince method
  - [Fix] Fixed an unpack prefab issue that renders the globe in pink for a frame inside Unity Editor

Version 14.2
  - Optimized thick/custom country outline materials and shaders
  - [Fix] Fixed "Keep Straight" disabled when orbiting camera
  - [Fix] Map Editor: fixed error when saving changes if user has removed provinces geodata file

Version 14.1
  - Map Editor: added snap shortcut Shift+S when using the Move Point tool
  - API: added StopAnyNavigation(), isOrbitRotateToActive, isZoomToActive
  - API: added DrawCountryOutline(), ToggleCountryOutline()
  - Callback pattern added to navigation methods. Example: map.FlyTo(..).Then( () => action; );
  - Added safety check when loading/saving mountpoint data
  - Added support for country polygons crossing -180 or 180 longitude
  - Added "Constraint Latitude" option to limit camera/Earth rotation betweeen -lat..lat
  - [Fix] Fixed detection of touches over UI elements on mobile
  - [Fix] Fixed map editor not responding in Unity 2019.x if gizmos are not enabled

Version 14.0
  - New Navigation & Camera Control section in inspector
  - New option: "Show Country Outline" under Provinces section
  - Grid wireframe no longer computed when using just FindPath method
  - Outline width now respects country frontiers width settings
  - Entity coordinates loading optimization
  - API: added GenerateGrid() method
  - Field name change: country/province sphereCenter is now localPosition. MountPoint/City previous unitySphereLocation is now localPosition.
  - [Fix] Fixed shaders compatibility with VR modes

Version 13.5
  - Added rocket prefab to "Fire Rocket!" command in demo scene 1
  - Removed physics dependencies to improve performance (globe no longer uses a collider)
  - Inspector GUI optimizations
  - Improved orbit navigation
  - [Fix] Inverted mode fixes
  - [Fix] Fixed tile lat/lon floating point accuracy issue

Version 13.4
  - Added "Right Drag Behaviour" option (rotate/orbit)
  - Added "Precise Rotation" and "Zoom Level Change": switches Navigation Move mode from Earth to Camera beyond specified zoom level
  - Added "Orbit Tilt Max Distance", "Orbit Tilt Min Distance": controls distance to Earth surface at which orbitting is possible
  - Improved sky scattering effect when camera is below atmosphere outer radius
  - Path Finding: added "HiddenCell" filter to FindPath method
  - Path Finding: added "Max Path Cost" option
  - API: added "IsCellVisible"
  - API: added orbitIdealYaw, orbitIdealPitch
  - [Fix] Fixes to tile map system for big Earth scales

Version 13.3
  - Added Tile Background Color: default color for rendering background Earth. For best results, assign a color that matches the current tile theme/style.
  - Exposed camera property in the inspector under Other Settings section
  - [Fix] Fixed tile mode bug: world texture could appear briefly in some small tiles

Version 13.2
  - API: added AddPolygon3D. Draws a polygon over the globe with optional fill color.

Version 13.1
  - Pan and zoom changes now takes into account framerate to ensure interaction is device-independent
  - API: new ToggleCountrySurface overloads which accept custom outline color
  - [Fix] API: SetSimulatedButtonClick now triggers events like OnCountryClick. Use SetSimulatedButtonClick(0) for left button, 1 for right button
  - [Fix] Inverted mode fixes

Version 13.0
  - Added Attributes to map entities (country, province, city, mountpoint). Attributes are stored inside a JSON public field of the objects.
  - New demo scene 15: "Custom Attributes". Run and check console output
  - Map Editor: custom attributes can now be added to countries, provinces, cities and mount points and saved from the Map Editor
  - Tile map: lower zoom levels are reused while new tiles are downloaded
  - Sphere Overlay Layer is now hidden completely if not used to improve performance. Additional changes to avoid issues with certain mobile devices.
  - API: added GetCities(attribute_predicate, results)
  - API: added GetCitiesAttributes / SetCitiesAttributes
  - API: added GetCountries(attribute_predicate, results)
  - API: added GetCountriesAttributes / SetCountriesAttributes
  - API: added GetProvinces(attribute_predicate, results)
  - API: added GetProvincesAttributes / SetProvincesAttributes
  
Version 12.4
  - API: added GetMountPoints(countryIndex), GetMountPoints(countryIndex, provinceIndex), GetMountPoints(region)
  - Improved backface clipping
  - [Fix] Fixed globe rotation when pinching on mobile device and allowUserRotation/allowUserZoom are disabled

Version 12.3.1
  - [Fix] Fixes and improvements to country/continent deletion in Map Editor

Version 12.3
  - General support for LWRP 5.16+ (fog of war is not supported due to lack of multi-pass shader support in LWRP)
  - Minimum Unity version required upped to 2017.4
  - Tweaked atmosphere and glow shaders to improve visuals in linear color space. Due to changes, Earth, atmosphere or glow settings need to be adjusted.
  - API: added GetZoomLevel(altitudeInKm)

Version 12.2
  - API: added OnCountryPointerUp, OnProvincePointerUp, OnCityPointerUp events
  - Change: Texture Resolution has been renamed to "Overlay Resolution" and a new "Not Used" option has been added to save memory if label are rendered in world space and no tickers or markers are used
  - [Fix] Fixed OnClick event issue

Version 12.1
  - Added specular shining option to Scenic and Scenic Scatter styles (enable it in inspector)
  - API: added OnCountryPointerDown, OnProvincePointerDown, OnCityPointerDown events

Version 12.0
  - API: added GetProvinces(country)
  - Added labelFontSize, labelFontSizeOverride to Country class
  - Decorators: added option to specify label size per country
  - [Fix] Fixed text encoding of some provinces in Turkey, Azerbaijan and Poland
  - [Fix] Fixed OnCityClick event not firing when dragging the globe

Version 11.9
  - Improved zoom damping
  - Improvements to tile system rendering. Fade effect now used when zooming in/out between levels and not only when tile is loaded for the first time.
  - Map Editor: automatic split of region drawn across map edge (West/East)
  - API: added OnTileURLRequest event which can be used to modify tile url just before starting the download

Version 11.8
  - Forced LOD 0 in 16K shaders to avoid seams
  - Virus plague demo scene: support for 16K styles
  - Map Editor: city lat/lon is now shown in inspector and can be updated
  - Added cities and capitals to some countries
  - VR: added support for Oculus GO controller

Version 11.7
  - "Pinch and throw" gesture implemented when "Constand Drag Speed" is enabled
  - Added label shadow offset parameter to Inspector
  - Added province highlight max screen size parameter to Inspector
  - Multiple instances of Globe are now allowed in same scene
  - API: added GetCityFullName

Version 11.6 2019-JAN-22
  - Zoom tilt option: keep centered
  - Added Zoom Level to inspector (only usable at Editor time; at runtime use SetZoomLevel method)

Version 11.5 2019-JAN-13
  - Added Damping Duration option (duration of the damping rotation after a drag until the Earth stops completely)
  - Improved visibility of clouds
  - Added GetCityRandom, GetCityIndexRandom methods
  - [Fix] Disabled/added workaround for code not compatible with HoloLens

Version 11.4 2018-DEC-16
  - API: added GetZoomExtents(rect)
  - API: added GetCountryCapital(countryName), GetCountryCapitalIndex(countryName) methods
  - [Fix] Fixed OnClick event not firing on some Android devices
  - [Fix] Fixed prefab & www compatibility issues with Unity 2018.3 beta

Version 11.3 2018-OCT-25
  - Improved rotation constraint option
  - [Fix] Fixed single-sided line markers
  - [Fix] Fixed globe flickering when rotation mode is set to camera and keep straight and zoom tilt are used

Version 11.2 2018-SEP-28
  - Added Enable Province Highlight option
  - Reduced lag when disabling the Show Outline option
  - Added OnTileRequest event
  - Map Editor: country label visibility now is saved to geodata files when saving changes from editor
  - Improved performance of polygon triangulation
  - [Fix] Fixed distance calculation method in Calculator component

Version 11.1 2018-SEP-17
  - Support for offline maps (tile system)
  - New tile downloader assistant
  - API: Added AddText method for adding labels over the globe
  - API: Added GetCountryMainRegionZoomExtents, GetCountryRegionZoomExtents, GetProvinceMainRegionZoomExtents, GetProvinceRegionZoomExtents, GetRegionZoomExtents
  - Updated demo scene City Travel (uses a real model with proper heading orientation)	
  - [Fix] Fixed an issue when creating one country after deleting all continents

Version 11.0 2018-SEP-09
  - Added MiniMap prefab

Version 10.10.3 2018-SEP-04
  - Added check to force loading of data if API is called during Awake or it's not ready to use yet
  - [Fix] Fixed issue with ToggleCountrySurface method
  - [Fix] Fixed issue with province saving from Map Editor

Version 10.10.1 2018-JUL-13
  - [Fix] Fixed issues with Inverted Mode (clipping + VR compatibility)

Version 10.10 2018-JUL-11
  - New demo scene 14: virus plague!
  - Map Editor: improved region splitting tool
  - New demo scene 13: demo of objects inside the globe being clipped with precision
  - Added geodata folder name parameter to inspector to support multiple geodata folders
  - API: Added OnLineDrawingEnd event to line marker animator script
  - [Fix] Fixed tile system issue on Unity 2017+ and iOS
  - [Fix] Fixed issue with decorators not being saved with the scene

Version 10.9.2 2018-JUN-22
  - [Fix] Fixed label rendering issue when render method is set to blend and minimum size is changed
  - [Fix] Fixed highlight issue when highlight all regions is enabled
  - [Fix] Fixed encoding issue with some city names

Version 10.9.1 2018-JUN-1
  - [Fix] Support for tile system mode and WebGL platform
  - [Fix] Fixed cloud distortion issue when camera rotation is enabled
  - [Fix] Fixed orientation of rotation with two fingers on mobile when camera rotation is enabled
  - [Fix] Fixed transparency being ignored in textured regions

Version 10.9 2018-ABR-21
  - Added FIPS 10 4, ISO A3, ISO A2, ISO N3 codes to countries geodata file
  - API: Added GetCountryIndexByFIPS10_4, GetCountryIndexByISO_A3, GetCountryIndexByISO_A2, GetCountryIndexByISO_N3
  - Added Drag Threshold parameter to prevent erratic drag on certain mobile devices
  - Added support for rotation with two fingers on mobile
  - Cosmetic changes to WPM Inspector (also makes it more lightweight)
  - [Fix] Fixed drag on mobile when Constant Drag Speed is enabled

Version 10.8.1 2018-ABR-9
  - [Fix] Fixes for HoloLens platform

Version 10.8 2018-MAR-21
  - Added support for real-time weather layers by AerisWeather
  - Added support for transparent layers
  - Performance optimizations
  - [Fix] Fixed issue with cities icon scale with large globe scales

Version 10.7 2018-FEB-28
  - Add Overlay Layer option in Other Settings (can be used to apply selective bloom effects on labels/markers using Beautify)
  - Improved rotation around X-Axis when rotation mode is set to Camera rotates
  - Reduced inspector width
  - [Fix] Fixed rare normals issue when generating some surfaces
  - [Fix] Fixed province remain highlighted when cursor exits province in same country with hidden provinces
  - [Fix] Hexagonal grid is now compatible with wireframe mode (hide Earth + double sided option)

Version 10.6 2018-JAN-25
  - Improved Scenic & Scatter Earth styles. New parameters exposed.
    - Perspective cloud shadows (honors Sun light direction)
    - Scenic style refactored and no longers require globe scale = 1.
    - New bump mapping option
  - Added optional navigation bounce effect (navigation bounce in inspector, API: navigationBounceIntensity)
  - Lines now support textures. Example: build road. Demo scene 10 (Hexagonal Grid)
  - New variable width frontiers for countries
  - New option to include all country regions when highlighting
  - New "Double Sided" option when Show Earth is off which renders full wireframe
  - Overlay layer no longer forces a default layer number so you can change it to use any layer (useful for masking bloom effect on country labels with Beautify)
  - Map Editor: improvements to polygon operations
  - API: new CountryTransferCell, ProvinceTransferCel : merges a cell border with a country or province
  - API: new GellCells function : returns cells inside a country with options
  - API: BlinkCountry/BlinkProvince: added smoothBlink parameter
  - [Fix] Fixed hexagonal grid material dictionary look up bug which causes performance issues when coloring/texturing many cells
  - [Fix] Fixed hexagonal grid material transparency issue
  - [Fix] Map Editor: fixed camera out of frustum console error when using the Toggle Zoom button
  - [Fix] Fixed Time of Day sync formula error
  - [Fix] Negative scaling on country labels was breaking dynamic batching on labels

Version 10.5 2018-JAN-03
  - Support for texturing provinces in ToggleProvinceRegion methods
  - Change: country highlight now shows behind any colored or textures province
  - Added a security check when assigning a new Font to prevent issues with Dynamic Fonts
  - WPM Globe Edition now requires Unity 5.5.0 or later
  - API change: GetCityIndex(provinceIndex, cityName) refactored to GetCityIndexInProvince(provinceIndex, cityName)
  - API: added GetCityIndexInProvince and GetCityIndexInCountry
  - FaceToCamera script now supports Camera Rotate mode (included in demo scene 12)
  - [Fix] Fixed color issue with provinces sharing the same color than others incorrectly
  - [Fix] Fixed FlyTo rotation issue when globe scale is big and camera rotation is set to Camera Rotates

Version 10.4.1 2017-DEC-19
  - Starting camera rotation is now respected when navigation mode is set to Camera Rotates
  - API additions/change: GetCityIndex now filters by province index instead of country index
  - Map Editor: added new option under gear icon to export list of entities to plain text files
  - [Fix] Fixed text encoding when saving changes from map editor
  - [Fix] Colored provinces now appear properly over colored countries
  - [Fix] Fixed province names with special characters

Version 10.4 2017-DEC-14
  - Map Editor: added new option to convert a province into a new country
  - Added "Sync Time of Day"
  - Added support for elevation in Conversion methods
  - AddLine now supports elevation
  - Added "Main Camera" property in inspector
  - Added real skybox 6x2K resolution cubemap derived from Tycho star catalogue (in new Universe section in inspector)
  - Added Moon option (in new Universe section in inspector)
  - Added Ambient Light option to Scenic Scatter styles
  - Behaviour change: OnProvinceBeforeEnter now keeps country highlighted if province highlighted is cancelled
  - Behaviour change: provinces of countries with allowShowProvinces set to false no longer highlight
  - Province: added "hidden" attribute
  - [Fix] VR: Added support for Stereo non-headmounted mode
  - [Fix] Fixed British Columbia borders issue

Version 10.3 2017-NOV-30
  - New "Allowed Axis" option to restrict rotation axis
  - Map Editor: added new gear icon menu option to find/fix repeated province names
  - Inverted Mode: added some lacking features wrt normal mode
  - [Fix] Fixed some provinces names
 
Version 10.2.1 2017-NOV-03
  - API: FlyToCountry, FlyToProvince, FlyToCity no accept destination zoom level
  - [Fix] Fixed 'show states names' example to prevent text showing on the back of the globe

Version 10.2 2017-OCT-06
  - New demo scene 12: UI Canvas demo
  - Path Finding: per edge costs. New APIs (see manual)
  - Demo scene 11 (Hexagonal Grid and Path Finding) updated with new examples
  - [Fix] Added CultureInfo.InvariantCulture to all float parsing instructions to support foreign languages
  
Version 10.1 2017-SEP-03 Release
  - Tickers: new options to keep texts centered on the screen irrespective of globe rotation (API: horizontalOffsetAutomatic and stayOnCenter properties)
  - Tickers: added Text Anchor property to control text alignment (defaults to MiddleCenter)
  - [Fix] Tweaked some shaders z-buffer offset
  
Version 10.0 2017-AUG-11
  - New feature: fog of war! New Demo scene 11.
  - Added new menu entry to quickly create a Globe gameObject (under top menu GameObject / 3D Object)
  - API: Added allowShowProvinces field to Country object
  - Improved performance of outline shader, now slightly modified to produce a relief effect
  - Highlight shader now can tint existing surfaces with applied textures
  - Outline now accepts a secondary color (Outline Edge Color) which adds a "relief touch" when several countries are colored
  - [Fix] Fixed country highlight glitch when using decorator
  
Version 9.3.2 2017.07.31
  - Hexagonal grid now accepts colors with transparency
  - Change: AddLine method now returns a LineMarkerAnimator reference instead of the GameObject reference. To get the line GameObject reference just use the .gameObject property of the LineMarkerAnimator.
  - Tile System: added "Custom" server option
  - Performance optimizations to tile system navigation
  - [Fix] Fixed tile system navigation issues on Unity 2017.1
  - [Fix] Fixed Earth rotation when switching between Camera Rotates and Earth Rotates mode.
  - [Fix] Fixed camera rotation when zoom mode set to Earth Rotates and zoom tilt is used
  
Version 9.3.1 2017.07.03
  - Improvements to Tile System

Version 9.3 2017.07.1
  - New zoom mode (Earth moves or Camera moves)
  - Improved zoom step
  - Improved Tile mode
  - Updated demo scene 9 SlippyMap
  - Tile System change: no longer an Earth style, it now has its own toggle setting
  - Tile System: added Tile Keep Alive parameter to optimize memory usage

Version 9.2.1 2017.06.26
  - API: Added GetCellNeighbours / GetCellNeighboursIndices
  - [Fix] Optimized scenic glow shader and prevents clipping with animated lines

Version 9.2 2017.05.31
  - VR: support for Samsung Gear VR pointer (laser)
  - New Sun property to automatically sync atmosphere with Sun light direction
  - Hexagonal grid: added rotation shift option
  - API: Added OnFlyStart / OnFlyEnd events
  - API: Added destinationZoomLevel optional parameter to FlyTo() methods
  - [Fix] Fixed GetZoomLevel() not matching SetZoomLevel() values
  - [Fix] Fixed horizon white line in Scenic 16K styles

Version 9.1.1 2017.05.16
  - [Fix] Fixed Windows Store App build error

Version 9.1 2017.05.15
  - New: Hexagonal Grid feature with water mask, highlighting and cell colorization. New demo scene #10!
  - New: PathFinding support for the grid
  - Added zoom damping to control speed of deceleration when user stops applying zoom
  
Version 9.0.2 2017.05.08
  - Improved global performance
  - Markers added to the globe with colliders now receive a rigidbody component to enable interaction
  - VR: Support for Google VR pointer and controller touch
  - Map Editor: added option to merge provinces
  - [Fix] Country labels with World Space render method disappear when adding tickers to the globe
  - [Fix] Fixed wrong target sovereign country name shown in transfer dialog (countries and provinces)
  - [Fix] Immediate changes in inspector does not mark scene as dirty (pending save)
  - [Fix] Added workaround to prevent Unity Editor hanging when enabling "Draw All Provinces" with World Map Globe prefab in the scene
  - [Fix] Fixed shaking issue when zoom tilt and keep straight options are enabled
  - [Fix] Map Editor: Unity orbiting keys were being supressed while map editor was selected
  
Versino 9.0.1 2017.04.10
  - Improved tile download system (fewer tiles are now downloads -> less memory comsuption)
  - Added Preload Main Tiles option in inspector to show all tiles for main zoom level from start (only available if they have been downloaded and stored in local cache)

Version 9.0 2017.03.13
  - New Earth style: Tiled + dedicated section to setup integration with online map systems
  - New demo scene 14 showing the new tile system
  - Map Editor: new contexual option to equalize number of provinces across countries
  - Map Editor: new 'Fix Orphan Cities" to automatically assign cities to countries and provinces
  - Map Editor: city province field is now shown when selecting a city
  - Improved globe drag when using WASD keys
  - Added rightClickRotates and rightClickRotatingClockwise properties
  - Improved precision of high definition frontiers projection (EPSG:4326)
  - Country and province borders now use dedicated materials based on color alpha to improve performance
  - Improved cursor: pattern now remains invariable to zoom level
  - Added countryHighlightMaxScreenAreaSize to provide a maximum screen area size for highlighted countries
  - [Fix] Fixed namespace conflict with PUN+
  - [Fix] Fixed jitter when reaching minimum zoom
  
Version 8.0 - 2017.02.09
  - Support for enclaves at country and province levels
  - Redesigned Editor inspector
  - Map Editor: improved region transfer system
  - Map Editor: transfer country field now shows neighbours at top of list
  - Automatic country label fading: optimized performance
  - Optimized performance of country selection with mouse
  - AddLine: new reuseMaterial parameter to improve performance avoiding material instantiation
  - Optimized line animation system

Version 7.3 - 2017.01.10
  - Added Brightness & Contrast sliders to Earth scenic styles
  - Convenience: Added VR Enabled setting in inspector to avoid having to edit WPMInternal script to uncomment a macro
  - Compatibility with Windows Store platform
  - [Fix] Fixed minor compatibility issues with Unity 5.5
  - [Fix] Map Editor: fixed province issue when renaming country
  - [Fix] Fixed country label wrong positioning on Microsoft HoloLens
  - [Fix] Fixed location precision for cities
  - [Fix] Fixed collider expensive delayed cost warnings in profiler
  
Version 7.2 - 2016.11.25
  - Rectangle selection support. New demo scene 8.
  - New marker type: quad (API: AddMarker).
  - New API: GetVisibleCountries(rect), GetVisibleProvinces(rect), GetVisibleCities(rect), GetVisibleMountPoints(rect)
  - Added Hide/Show method for fast hiding/showing globe (faster than enabling/disabling gameobject)
  - Added brightness & contrast properties to Scenic shaders
  - VR: compatibility in normal mode (before, only worked with Inverted Mode)
  - VR: compatibility with Google VR
  - [Fix] Fixed Unlit Earth Style in deferred rendering
  
  
Version 7.1 - 2016.09.23
  - New demo scene 5: sorting cities
  - New demo scene 6: city travel / path traversal
  - New demo scene 7: Earth graffiti!
  - New Earth styles: 2K Standard Shader, 8K Standard Shader.
  - New City Combine Meshes option for best performance when lot of cities are visible
  - Unlit styles now receive shadows
  - Demo scene 1: added new button "Fire Bullet!"
  - [Fix] Frontiers, Mount Points, Cursor, Latitude Lines and other objects created dynamically now use the same layer than globe
  - [Fix] Fixed RespectOtherUI behaviour on mobile
  - [Fix] Fixed drag issue on Windows 10

Version 7.0 - 2016.08.18
  - New Earth styles supporting multi-tile texture up to 16K (4x8K): 16K Scenic, 16K Scenic + CityLights, 16K Scatter and 16K Scatter + City Lights
  - Performance improvement of Automatic Labels Fade feature


Version 6.1 - 2016.08.15

New Features:
  - Option to follow device GPS coordinates on the map
  
Improvements:
  - API: methods used to colorize countries now can also add a colored outline
  - Map Editor: added Update button next to country continent
  
Fixes:
  - Fixed camera bug when calling FlyTo() while a rotation operation was being executed in CameraRotate mode
  - Fixed Upright Labels option when inverted view is enabled
  - Map Editor: fixed issue with target country being deselected when transferring countries
  

Version 6.0 - 2016.06.29

New Features:
  - New Scenic City Lights and Scenic Scatter City Lights styles
  - New Zoom Tilt option to skew view when approaching Earth
  - New Upright Labels option to ensure country labels are always easily readable
  
Improvements:
  - New APIs: GetCountryIndex(spherePosition), GetCountryNearToPosition(spherePosition), GetProvinceIndex(spherePosition), GetProvinceNearToPosition(spherePosition)
  - New APIs: GetVisibleCountries, GetVisibleProvinces, GetVisibleCities, GetVisibleMountPoints.
  - New events: OnCountryBeforeEnter, OnProvinceBeforeEnter
  - Can use transparent colors when coloring countries or provinces
  - Animated lines general optimization

Fixes:
  - Fixed bug when flying to target location with globe scale >1 and navigation mode set to Camera Rotates.
  - Improved runtime performance and reduced garbage collection
  

Version 5.4 - 2016.05.24

New Features:
  - New label rendering method: World Space
  - New global default font setting for all labels

Improvements:
  - New APIs: GetCountryRegionSurfaceGameObject and GetProvinceRegionSurfaceGameObject for retrieving the colored surface (game object) of any country or province (for example to read the current color)
  
Fixes:
  - Fixed issues with country transfer options in Map Editor
  - Fixed inland frontiers line renderer which was scaling too much at short distances  
  - Fixed duplicate province names

Version 5.3 - 2016.04.27

New features:
  - Now, by default, coastal frontiers won't be drawn, which is useful in combination with inland frontiers (avoid overdraw, increase performance) (API: showCoastalFrontiers)

Improvements:
  - New option to draw all province borders (API: drawlAllProvinces)
  - Ability to constraint rotation around a position (APIs: constraintPosition, constraintAngle, constraintPositionEnabled)
  - Increased color contrast of Scenic Scatter style
  - Thicker lines for inland frontiers
  - Added OnLeftClick and OnRightClick events
  - Added CIRCLE_PROJECTION marker type
  
Fixes:
  - Fixed issues in inverted mode and globe scale greater than 2
  - Fixed issue with latitude and longitude lines not being drawn when option enabled
  - Fixed issue with SetSimulatedMouseClick not working with GamePads
  - Fixed Pala (Chad) province
  - Fixed circle drawing when crossing 180 degree longitude
  - Fixed black globe issue under some circumstances


Version 5.2 - 2016.02.16

New features:
  - New high density mesh option and ability to replace the Earth mesh
  - New distance calculator (improved inspector and new API).
  
Improvements:
  - FlyTo is now compatible with simultaneous globe translations
  - New option to prevent interaction with other UI elements (Respect Other UI)
  - Labels now fade in/out automatically depending on camera distance and screen size  
  
Fixes:
  - Fixed new scenic atmosphere scattering style on mobile
  

Version 5.1 - 2016.01.29

New features:
  - New Scenic High Resolution with physically-based Atmosphere Scattering effect
  - New demo scene featuring sprites/billboard positioning

Improvements:
  - Can hide completely individual countries using decorators, Editor or through API (country.hidden property)
  - VR: compatibility with Virtual Reality gaze
  - Performance improvement in city look up functions
  - Increased sphere mesh density for sharper geometry

Fixes:
  - Min population filter now returns to previous value when closing the map editor
  - Fixed cities and markers rotation when inverted mode is enabled
  

Version 5.0 - 2015.12.24

New features:
  - New Camera navigation mode (can rotate camera instead of Earth). Supports user drag, Constant Drag Speed, Keep Straight, FlyTo methods. Updated Scenic Shader. 
  - Mount Points. Allows you to define custom landmarks for special purposes. Mount Points attributes includes a name, position, a type and a collection of tags. Manual updated.
  - Country and region capitals. Different icons + colors, new class filter (cityClassFilter), new "cityClass" property and editor support.
  - Added Hidden GameObjects Tool for dealing with hidden residual gameobjects (located under GameObject main menu)
   
Improvements:
  - New options for country decorator and for country object (now can hide/rotate/offset country label)
  - New line drawing method compatible with inverted mode (still needs some work)
  - Right clicking on a province now centers on that province instead of its country center
  - Improved zoom acceleration in inverted mode
  - (Set/Get)ZoomLevel now works in inverted mode
  - "Constant drag speed" and "Keep straight" now work in inverted mode
  - API: added new events: OnCityClick, OnCountryClick, OnProvinceClick
  - API: added GetCountryUnderSpherePosition and GetProvinceUnderSpherePosition
  - Editor: country's continent is displayed and can be renamed
  - Editor: continent can be destroyed, including countries, provinces and cities
  - Editor: deleting a country now deletes all cities belonging to that country as well
  - Editor: new options to delete a country, a province or all provinces belonging to a country
 
Fixes:
  - Cities were not being visible when inverted mode was enabled
  - Some countries and provinces surrounded by other countries/provinces could not be highlighted
  - Right click to center was not working when inverted mode was enabled


Version 4.2 - 2015.12.02
  
 New features:
  - Number of cities increased from 1249 to 7144
 
 Improvements:
  - Improved performance when cities are visible on the map
  - Improved straightening of globe at current position (right click or new improved API: StraightenGlobe)
  - Added dragConstantSpeed to prevent rotation acceleration
  - Added keepStraight to maintain the globe always straight
  - Added zoom max/min distance
  - Country is now highlighted as well when provinces are shown
  - API: new overload for GetCityIndex to fetch the index of the nearest city around a location (lat/lon or sphere position).
  - API: new events: OnCityEnter, OnCityExit, OnCountryEnter, OnCountryExit, OnProvinceEnter, OnProvinceExit

 Fixes:
  - Fixed geodata issues with Republic of Congo, South Sudan and provinces of British Columbia, Darién, Atlántico Sur, Saskatchewan and Krasnoyarsk
  - Minor fixes regarding province highlighting and some lines crossing Earth
  - Fixed a bug in FlyToxxx() methods when globe is not a 0,0,0 position
  

Version 4.1 - 11/11/2015

 New features:
  - Option to show inland frontiers
  - Improved Scenic shaders including a new 8K + Scenic style (atmosphere falloff + scattering effect)
  
 Improvements:
  - New option to invert zoom direction when using mouse wheel (invertZoomDirection property)
  - New option to automatically hide cursor on the globe if mouse if not over it (cursorAlwaysVisible)
  - New API to obtain the country reference under any sphere position (GetCountryUnderSpherePosition) 
  - New option to mask grid so it only appears over oceans
  - Improved Earth glow
  
 Fixes:
  - Globe interaction is now properly blocked when mouse is hovering an UI element (Canvas, ScrollRect, ...)
  - Labels shadows were not being drawn due to a regression bug


Version 4.0 - 23/10/2015
  New features:
  - Map Editor: new extra component for editing countries, provinces and cities.
  
  Improvements:
  - New APIs for setting/getting normalized zoom factor (SetZoomLevel/GetZoomLevel)
  - New APIs in the Calculator component (prettyCurrentLatLon, toLatCardinal, toLonCardinal, from/toSphereLocation)
  - New API variant for adding circle markers (AddMarker)
  - New API for getting cities from a specified country - GetCities(country)
  - New APIs for getting/setting cities information to a packed string - map.editor.GetCityGeoData/map.ReadCitiesPackedString
  - Option for changing city icon size
  - Can assign custom font to individual country labels
  - Even faster country/province hovering detection
  - Better polygon outline (thicker line and best positioning thanks to new custom shader for outline)
  - Country outline is shown when show provinces mode is activated
  - Improved low-res map generator including Douglas-Peucker implementation + automatic detection of self-crossing polygons
  
 Fixes:
  - Removed requirement of SM3 for the Scenic shader

    
Version 3.2 - 21/09/2015
  New features:
  - New markers and line drawing and animation support
  - New "Scenic" style with custom shader (relief + cloud effects)
  
  Improvements:
  - Pinch in/out support for mobile
  - Improved resolution of high-def frontiers while reducing data file size
  - Single city catalogue with improved size
  - Significant performance improvement in detecting country hover
  - More efficient highlghting system
  - New option in inspector: labels elevation
 
 Fixes:
  - Corrected frontiers distortion
  - Population of cities fixed and approximated to the metro area
 

Version 3.1 - 28/08/2015
  New features:
  - New Inverted Mode view (toggle in the inspector)
  - Bake Earth texture command (available from gear's icon in the inspector title bar)
  
  Improvements:
  - New buttons to straighten and tilt the Earth (also available in API)
  - New option to adjust the drag speed
  - New option to enable rotation using keyboard (WASD)
  - x2 speed increase of colorize/highlight system
  
  Fixes:
  - Fixed bug related to labels drawing when the Earth is rotated on certain angles
  - Fixed colorizing countries when field of view of camera was not default 60
  
  

Version 3.0.1 - 11.08.2015
  New features:
  
  Improvements:
  - Better outline implementation with improved performance)
  
  Fixes:
  - Calculator component: fixed an error in spherical to degree conversion
  - Colorize shader was not showing when Earth was not visible
  - A few countries had parts visible when colorized and Earth rotates



Version 3.0 - 11.08.2015
  New features:
  - New component: World Map Calculator
  - New component: World Map Ticker
  - New component: World Map Decorator
  
  Improvements:
  - Some shaders have been optimized
  - Improved algorithm for centering destinations (produces a straighten view)
  - New option: right click centers on a selected country
  - Lots of internal changes and new APIs
  
  Fixes:
  - Fixed country label positioning bug when some labels overlap
  - Fixed colorizing of some countries which appeared inverted


Version 2.1 - 3.08.2015
  New features:
  - Option to draw country labels with automatic placement
    
  Improvements:
  - Additional high-res (8K) Earth texture
  
  Fixes:
  - Some countries highlight were rendered incorrectly when using high detail frontiers

Version 2.0 - 31/07/2015
  New features:
  - Second detail level for country frontiers
  - New option to draw provinces/states for active country
  - Option to draw an outline around highlighted/colored countries
  - New options to show a cursor over custom/mouse position
  - New options to show latitude/longitude lines
    
  Improvements:
  - Even faster frontier line rendering (+20%)
  - Tweaked triangulation algorithm to improve poly-fill
  - Can locate a country from the inspector
  - Cities are now drawn as small circular dots, instead of small boxes
  - Can change the color of the cities
  - Additional Earth style: CutOut
  
  Fixes:
  - Some new properties were not being correctly saved from Editor
  - Colored countries hide correctly when Earth rotates

Version 1.1 - 25.07.2015
  New features:
  - added 3 new material/textures for Earth
  - extended city catalog (now 1249 cities included!)
  - can filter cities by population
  Improvements: 
  - better frontiers line quality and fasterer render
  - better poly-fill algorithm
  - can change navigation time in Editor
  - moved mouse interactions (rotation/zoom) to the main script and expose that as part of the API
  - reorganized project folder structure
  Fixes:
  - setting navigation time to zero causes error
  - some properties where not being persisted


Version 1.0 - Initial launch 16.07.2015



Credits
-------

All code, data files and images, otherwise specified, is (C) Copyright 2015 Kronnect
Non high-res Earth textures derived from NASA source (Visible Earth)
Flag images: Licensed under Public Domain via Wikipedia
Moon texture is CC-4 Attribution from Solar System Scope (https://www.solarsystemscope.com/textures)

