// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace WPM {
    public partial class WorldMapGlobe : MonoBehaviour {


        #region Internal variables

        const int RENDER_QUEUE_OPAQUE = 2000;
        const int RENDER_QUEUE_TRANSPARENT = 3000;
        const string EARTH_ATMOSPHERE_GO_NAME = "WorldMapGlobeAtmosphere";
        const string EARTH_GLOBE_GO_NAME = "WorldMapGlobeEarth";
        const string SKW_BUMPMAP = "WPM_BUMPMAP_ENABLED";
        const string SKW_CLOUDSHADOWS = "WPM_CLOUDSHADOWS_ENABLED";
        const string SKW_SPECULAR = "WPM_SPECULAR_ENABLED";
        const string SKW_INVERTED = "WPM_INVERTED";

        Material earthGlowMat, earthGlowScatterMat;
        Renderer _skyRenderer, _earthRenderer;
        Vector3 m_waveLength = new Vector3(0.65f, 0.57f, 0.475f);
        // Wave length of sun light
        float m_ESun = 20.0f;
        // Sun brightness constant
        float m_kr = 0.0025f;
        // Rayleigh scattering constant
        float m_km = 0.0010f;
        // Mie scattering constant
        float m_g = -0.990f;
        // The Mie phase asymmetry factor, must be between 0.999 to -0.999

        //Dont change these
        const float m_outerScaleFactor = 1.025f;
        // Difference between inner and ounter radius. Must be 2.5%
        float m_innerRadius;
        // Radius of the ground sphere
        float m_outerRadius;
        // Radius of the sky sphere
        float m_scaleDepth = 0.25f;
        // The scale depth (i.e. the altitude at which the atmosphere's average density is found)

        #endregion

        Renderer skyRenderer {
            get {
                if (_skyRenderer == null) {
                    Transform tsky = transform.Find(EARTH_ATMOSPHERE_GO_NAME);
                    if (tsky != null)
                        _skyRenderer = tsky.GetComponent<MeshRenderer>() ?? tsky.gameObject.AddComponent<MeshRenderer>();
                }
                return _skyRenderer;
            }
        }

        Renderer earthRenderer {
            get {
                if (_earthRenderer == null) {
                    Transform tearth = transform.Find(EARTH_GLOBE_GO_NAME);
                    if (tearth != null)
                        _earthRenderer = tearth.GetComponent<MeshRenderer>() ?? tearth.gameObject.AddComponent<MeshRenderer>();
                }
                return _earthRenderer;
            }
        }

        #region Drawing stuff

        void RestyleEarth() {
            if (gameObject == null || earthRenderer == null)
                return;

            string materialName;

            earthRenderer.enabled = _showWorld;
            switch (_earthStyle) {
                case EARTH_STYLE.Alternate1:
                    materialName = "Earth2";
                    break;
                case EARTH_STYLE.Alternate2:
                    materialName = "Earth4";
                    break;
                case EARTH_STYLE.Alternate3:
                    materialName = "Earth5";
                    break;
                case EARTH_STYLE.SolidColor:
                    materialName = "EarthSolidColor";
                    break;
                case EARTH_STYLE.NaturalHighRes:
                    materialName = "EarthHighRes";
                    break;
                case EARTH_STYLE.Scenic:
                    materialName = "EarthScenic";
                    break;
                case EARTH_STYLE.ScenicCityLights:
                    materialName = "EarthScenicCityLights";
                    break;
                case EARTH_STYLE.NaturalHighResScenic:
                    materialName = "EarthHighResScenic";
                    break;
                case EARTH_STYLE.NaturalHighResScenicCityLights:
                    materialName = "EarthHighResScenicCityLights";
                    break;
                case EARTH_STYLE.NaturalHighResScenicScatter:
                    materialName = "EarthHighResScenicScatter";
                    _earthGlowScatter = true;
                    break;
                case EARTH_STYLE.NaturalHighResScenicScatterCityLights:
                    materialName = "EarthHighResScenicScatterCityLights";
                    _earthGlowScatter = true;
                    break;
                case EARTH_STYLE.NaturalHighRes16K:
                    materialName = "EarthHighRes16K";
                    break;
                case EARTH_STYLE.NaturalHighRes16KScenic:
                    materialName = "EarthHighRes16KScenic";
                    break;
                case EARTH_STYLE.NaturalHighRes16KScenicCityLights:
                    materialName = "EarthHighRes16KScenicCityLights";
                    break;
                case EARTH_STYLE.NaturalHighRes16KScenicScatter:
                    materialName = "EarthHighRes16KScenicScatter";
                    _earthGlowScatter = true;
                    break;
                case EARTH_STYLE.NaturalHighRes16KScenicScatterCityLights:
                    materialName = "EarthHighRes16KScenicScatterCityLights";
                    _earthGlowScatter = true;
                    break;
                case EARTH_STYLE.Custom:
                    materialName = "EarthCustom";
                    break;
                case EARTH_STYLE.StandardShader2K:
                    materialName = "EarthStandardShader";
                    break;
                case EARTH_STYLE.StandardShader8K:
                    materialName = "EarthStandardShaderHighRes";
                    break;
                default:
                    materialName = "Earth";
                    break;
            }

            if (earthRenderer.sharedMaterial == null || !earthRenderer.sharedMaterial.name.Equals(materialName)) {
                Material earthMaterial = Instantiate(Resources.Load<Material>("Materials/" + materialName));
                earthMaterial.hideFlags = HideFlags.DontSave;
                if (_earthStyle == EARTH_STYLE.SolidColor) {
                    earthMaterial.color = _earthColor;
                }
                earthMaterial.name = materialName;
                earthRenderer.material = earthMaterial;
            }

            if (_showTiles) {
                if (backFacesRendererMat != null) {
                    backFacesRendererMat.color = _tileBackgroundColor;
                }
                    if (tilesRoot == null) {
                        InitTileSystem();
                    } else if (Time.frameCount > 1) { // avoids reseting tiles multiple times during first frame
                        ResetTiles();
                    }
                if (Application.isPlaying) {
                    if (!_tileTransparentLayer) {
                        earthRenderer.enabled = false;
                        return;
                    }
                } else if (tilesRoot != null) {
                    tilesRoot.gameObject.SetActive(false);
                }
            } else if (backFacesRendererMat != null) {
                backFacesRendererMat.color = Color.black;
            }
            if (_sun != null) {
                _earthScenicLightDirection = -_sun.forward;
            }
            DrawAtmosphere();

            Drawing.ReverseSphereNormals(earthRenderer.gameObject, _earthInvertedMode, _earthHighDensityMesh);
            if (cursorMat != null) {
                if (_earthInvertedMode) {
                    cursorMat.EnableKeyword(SKW_INVERTED);
                } else {
                    cursorMat.DisableKeyword(SKW_INVERTED);
                }
            }
            if (_earthInvertedMode) {
                _navigationMode = NAVIGATION_MODE.EARTH_ROTATES;
            }
            float sx = transform.localScale.x;
            if ((_earthInvertedMode && sx > 0) || (!_earthInvertedMode && sx < 0)) {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
            lastRestyleEarthNormalsScaleCheck = transform.lossyScale;
            if (backFacesRendererMat) {
                backFacesRendererMat.SetFloat("_Inverted", _earthInvertedMode ? 1 : 0);
            }

            UpdateMaterialBrightness();
            UpdateMaterialsZWrite();
        }

        void UpdateMaterialBrightness() {
            if (earthRenderer == null || _earthRenderer.sharedMaterial == null)
                return;
            Material mat = _earthRenderer.sharedMaterial;
            mat.SetFloat("_Brightness", _brightness);
            mat.SetFloat("_Contrast", _contrast);
            mat.SetFloat("_AmbientLight", _ambientLight);
            mat.SetFloat("_CityLightsBrightness", _citiesBrightness);
        }

        void UpdateMaterialsZWrite() {
            // Materials that are transparent in nature switches the render queue depending on the Double Sided option.
            // It that option is disabled, then materials render in opaque queue - if enabled, materials are moved to the transparent queue which does not need to write to zbuffer
            // Materials that are opaque in nature, just gets the zwrite on/off toggle depending on the Double Sided option.
            int zwriteValue = (_showWorld || !_showBackSide) ? 0 : 1;
            if (frontiersMatThinAlpha != null) {
                frontiersMatThinAlpha.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 8 : RENDER_QUEUE_OPAQUE - 8;
            }
            if (frontiersMatThinOpaque != null) {
                frontiersMatThinOpaque.SetInt("_ZWrite", zwriteValue);
            }
            if (frontiersMatThickAlpha != null) {
                frontiersMatThickAlpha.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 8 : RENDER_QUEUE_OPAQUE - 8;
            }
            if (frontiersMatThickOpaque != null) {
                frontiersMatThickOpaque.SetInt("_ZWrite", zwriteValue);
            }
            if (inlandFrontiersMatOpaque != null) {
                inlandFrontiersMatOpaque.SetInt("_ZWrite", zwriteValue);
            }
            if (inlandFrontiersMatAlpha != null) {
                inlandFrontiersMatAlpha.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 10 : RENDER_QUEUE_OPAQUE - 10;
            }
            if (gridMatAlpha != null) {
                gridMatAlpha.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 11 : RENDER_QUEUE_OPAQUE - 11;
            }
            if (gridMatNonAlpha != null) {
                gridMatNonAlpha.SetInt("_ZWrite", zwriteValue);
            }
            if (gridMatMasked != null) {
                gridMatMasked.SetInt("_ZWrite", zwriteValue);
            }
            if (gridMatOverlay != null) {
                gridMatOverlay.SetInt("_ZWrite", zwriteValue);
            }
            if (cursorMat != null) {
                cursorMat.SetInt("_ZWrite", zwriteValue);
            }
            UpdateOutlineMatProperties();
            if (outlineMatCurrent != null) {
                outlineMatCurrent.SetInt("_ZWrite", zwriteValue);
            }
            if (provincesMatOpaque != null) {
                provincesMatOpaque.SetInt("_ZWrite", zwriteValue);
            }
            if (provincesMatAlpha != null) {
                provincesMatAlpha.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 8 : RENDER_QUEUE_OPAQUE - 8;
            }
            if (labelsFont != null) {
                labelsFont.material.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 5 : RENDER_QUEUE_OPAQUE - 5;
            }
            if (labelsShadowMaterial != null) {
                labelsShadowMaterial.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 6 : RENDER_QUEUE_OPAQUE - 6;
            }
            if (hudMatCountry != null) {
                hudMatCountry.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 16 : RENDER_QUEUE_OPAQUE - 16;
            }
            if (hudMatProvince != null) {
                hudMatProvince.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 11 : RENDER_QUEUE_OPAQUE - 11;
            }
            if (hexaGridHighlightMaterial != null) {
                hexaGridHighlightMaterial.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 10 : RENDER_QUEUE_OPAQUE - 10;
            }
            if (cellColoredMatNonAlpha != null) {
                cellColoredMatNonAlpha.SetInt("_ZWrite", zwriteValue);
            }
            if (cellColoredMatAlpha != null) {
                cellColoredMatAlpha.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 14 : RENDER_QUEUE_OPAQUE - 14;
            }
            if (cellTexturedMatNonAlpha != null) {
                cellTexturedMatNonAlpha.SetInt("_ZWrite", zwriteValue);
            }
            if (cellTexturedMatAlpha != null) {
                cellTexturedMatAlpha.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 14 : RENDER_QUEUE_OPAQUE - 14;
            }
            if (citiesNormalMat != null) {
                citiesNormalMat.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 4 : RENDER_QUEUE_OPAQUE - 4;
            }
            if (citiesRegionCapitalMat != null) {
                citiesRegionCapitalMat.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 4 : RENDER_QUEUE_OPAQUE - 4;
            }
            if (citiesCountryCapitalMat != null) {
                citiesCountryCapitalMat.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 4 : RENDER_QUEUE_OPAQUE - 4;
            }
            if (countryColoredMat != null) {
                countryColoredMat.SetInt("_ZWrite", zwriteValue);
                foreach (KeyValuePair<Color, Material> kv in countryColoredMatCache) {
                    if (kv.Value.shader == countryColoredMat.shader) {
                        kv.Value.SetInt("_ZWrite", zwriteValue);
                    }
                }
            }
            if (countryColoredAlphaMat != null) {
                countryColoredAlphaMat.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 17 : RENDER_QUEUE_OPAQUE - 17;
                foreach (KeyValuePair<Color, Material> kv in countryColoredMatCache) {
                    if (kv.Value.shader == countryColoredAlphaMat.shader) {
                        kv.Value.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 17 : RENDER_QUEUE_OPAQUE - 17;
                    }
                }
            }
            if (countryTexturizedMat != null) {
                countryTexturizedMat.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 17 : RENDER_QUEUE_OPAQUE - 17;
                foreach (KeyValuePair<Color, Material> kv in countryColoredMatCache) {
                    if (kv.Value.shader == countryTexturizedMat.shader) {
                        kv.Value.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 17 : RENDER_QUEUE_OPAQUE - 17;
                    }
                }
            }
            if (provinceColoredMat != null) {
                provinceColoredMat.SetInt("_ZWrite", zwriteValue);
                foreach (KeyValuePair<Color, Material> kv in provinceColoredMatCache) {
                    if (kv.Value.shader == provinceColoredMat.shader) {
                        kv.Value.SetInt("_ZWrite", zwriteValue);
                    }
                }
            }
            if (provinceColoredAlphaMat != null) {
                provinceColoredAlphaMat.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 15 : RENDER_QUEUE_OPAQUE - 15;
                foreach (KeyValuePair<Color, Material> kv in provinceColoredMatCache) {
                    if (kv.Value.shader == provinceColoredAlphaMat.shader) {
                        kv.Value.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 15 : RENDER_QUEUE_OPAQUE - 15;
                    }
                }
            }
            if (provinceTexturizedMat != null) {
                provinceTexturizedMat.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 15 : RENDER_QUEUE_OPAQUE - 15;
                foreach (KeyValuePair<Color, Material> kv in provinceColoredMatCache) {
                    if (kv.Value.shader == provinceTexturizedMat.shader) {
                        kv.Value.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 15 : RENDER_QUEUE_OPAQUE - 15;
                    }
                }
            }
            if (markerMatOther != null) {
                markerMatOther.renderQueue = zwriteValue == 1 ? RENDER_QUEUE_TRANSPARENT - 7 : RENDER_QUEUE_OPAQUE - 7;
            }

            if (backFacesRenderer != null) {
                backFacesRenderer.enabled = zwriteValue == 0;
            }
        }

        void DrawAtmosphere() {
            if (_showFogOfWar) {
                if (skyRenderer.enabled)
                    skyRenderer.enabled = false;
            } else {
                if (skyRenderer != null) {
                    bool glowEnabled = _showWorld && !_earthInvertedMode && _earthScenicGlowIntensity > 0;
                    if (skyRenderer.enabled != glowEnabled) {
                        skyRenderer.enabled = glowEnabled;
                    }
                    if (glowEnabled) {
                        if (_earthGlowScatter) {
                            if (skyRenderer.sharedMaterial != earthGlowScatterMat) {
                                skyRenderer.sharedMaterial = earthGlowScatterMat;
                            }
                            skyRenderer.transform.localScale = Misc.Vector3one * 1.025f;
                            // Updates sky shader params
                            UpdateGlowScatterMaterial();
                        } else {
                            if (skyRenderer.sharedMaterial != earthGlowMat) {
                                skyRenderer.sharedMaterial = earthGlowMat;
                                skyRenderer.transform.localScale = Misc.Vector3one * 1.17f;
                            }
                            if (_sun != null) {
                                _earthScenicLightDirection = -_sun.forward;
                            }
                            if (skyRenderer.sharedMaterial != null) {
                                skyRenderer.sharedMaterial.SetVector("_SunLightDirection", _earthScenicLightDirection.normalized);
                            }
                            UpdateGlowScenicMaterial();
                        }
                    }
                }
            }

            // Updates shader params
            if (_earthStyle.isScatter()) {
                UpdateEarthScatterMaterial();
            } else if (_earthStyle.isScenic()) {
                UpdateEarthScenicMaterial();
            }
            Transform t = transform;
            Shader.SetGlobalVector("_WPM_GlobePos", new Vector4(t.position.x, t.position.y, t.position.z, radius * radius));
        }


        void UpdateGlowScatterMaterial() {
            if (skyRenderer == null)
                return;
            Material skyMat = skyRenderer.sharedMaterial;
            if (skyMat == null)
                return;
            UpdateScatterMat(skyMat);
            skyRenderer.sharedMaterial.SetFloat("_GlowIntensity", _earthScenicGlowIntensity);
        }

        void UpdateEarthScatterMaterial() {
            if (earthRenderer == null)
                return;
            Material groundMat = earthRenderer.sharedMaterial;
            if (groundMat == null)
                return;
            UpdateScatterMat(groundMat);
            groundMat.SetFloat("_CloudSpeed", _cloudsSpeed);
            groundMat.SetFloat("_CloudAlpha", _cloudsAlpha);
            groundMat.SetFloat("_CloudShadowStrength", _cloudsShadowStrength);
            groundMat.SetFloat("_CloudElevation", _cloudsElevation);
            groundMat.SetFloat("_BumpAmount", _earthBumpMapIntensity);
            if (_earthBumpMapEnabled) {
                groundMat.EnableKeyword(SKW_BUMPMAP);
            } else {
                groundMat.DisableKeyword(SKW_BUMPMAP);
            }
            if (_cloudsShadowEnabled) {
                groundMat.EnableKeyword(SKW_CLOUDSHADOWS);
            } else {
                groundMat.DisableKeyword(SKW_CLOUDSHADOWS);
            }
            if (_earthSpecularEnabled) {
                groundMat.EnableKeyword(SKW_SPECULAR);
                groundMat.SetFloat("_SpecularPower", _earthSpecularPower);
                groundMat.SetFloat("_SpecularIntensity", _earthSpecularIntensity);
            } else {
                groundMat.DisableKeyword(SKW_SPECULAR);
            }
        }

        void UpdateScatterMat(Material mat) {

            if (mat == null)
                return;

            m_innerRadius = radius;
            //The outer sphere must be 2.5% larger that the inner sphere
            m_outerRadius = m_outerScaleFactor * radius;

            Vector3 invWaveLength4 = new Vector3(1.0f / Mathf.Pow(m_waveLength.x, 4.0f), 1.0f / Mathf.Pow(m_waveLength.y, 4.0f), 1.0f / Mathf.Pow(m_waveLength.z, 4.0f));
            float scale = 1.0f / (m_outerRadius - m_innerRadius);

            mat.SetFloat("fOuterRadius", m_outerRadius);
            mat.SetFloat("fOuterRadius2", m_outerRadius * m_outerRadius);
            mat.SetFloat("fInnerRadius", m_innerRadius);
            mat.SetFloat("fInnerRadius2", m_innerRadius * m_innerRadius);
            mat.SetFloat("fKrESun", m_kr * m_ESun);
            mat.SetFloat("fKmESun", m_km * m_ESun);
            mat.SetFloat("fKr4PI", m_kr * 4.0f * Mathf.PI);
            mat.SetFloat("fKm4PI", m_km * 4.0f * Mathf.PI);
            mat.SetFloat("fScale", scale);
            mat.SetFloat("fScaleDepth", m_scaleDepth);
            mat.SetFloat("fScaleOverScaleDepth", scale / m_scaleDepth);
            mat.SetVector("g", new Vector4(m_g, m_g * m_g, 0, 0));
            mat.SetVector("v3InvWavelength", invWaveLength4);
            if (_sun != null)
                _earthScenicLightDirection = -_sun.forward;
            mat.SetVector("_SunLightDirection", _earthScenicLightDirection.normalized);
            mat.SetFloat("_GlowIntensity", _earthScenicGlowIntensity);
            mat.SetFloat("_AtmosphereAlpha", _atmosphereScatterAlpha);
            mat.SetFloat("_Brightness", _brightness);
        }

        void UpdateGlowScenicMaterial() {
            if (skyRenderer == null)
                return;
            Material skyMat = skyRenderer.sharedMaterial;
            if (skyMat == null)
                return;
            skyMat.SetColor("_GlowColor", _earthScenicGlowColor);
            skyMat.SetFloat("_GlowIntensity", _earthScenicGlowIntensity);
            skyMat.SetFloat("_GlowFallOff", _atmosphereFallOff);
            skyMat.SetFloat("_GlowGrow", _atmosphereThickness);
        }

        void UpdateEarthScenicMaterial() {
            if (earthRenderer == null)
                return;
            Material groundMat = earthRenderer.sharedMaterial;
            if (groundMat == null)
                return;
            groundMat.SetVector("_SunLightDirection", _earthScenicLightDirection.normalized);
            groundMat.SetFloat("_ScenicIntensity", _earthScenicAtmosphereIntensity);
            groundMat.SetFloat("_CloudSpeed", _cloudsSpeed);
            groundMat.SetFloat("_CloudAlpha", _cloudsAlpha);
            groundMat.SetFloat("_CloudShadowStrength", _cloudsShadowStrength);
            groundMat.SetFloat("_CloudElevation", _cloudsElevation);
            groundMat.SetColor("_AtmosphereColor", _atmosphereColor);
            groundMat.SetFloat("_AtmosphereAlpha", _atmosphereAlpha);
            groundMat.SetFloat("_AtmosphereFallOff", _atmosphereFallOff);
            groundMat.SetFloat("_BumpAmount", _earthBumpMapIntensity);
            if (_earthBumpMapEnabled) {
                groundMat.EnableKeyword(SKW_BUMPMAP);
            } else {
                groundMat.DisableKeyword(SKW_BUMPMAP);
            }
            if (_cloudsShadowEnabled) {
                groundMat.EnableKeyword(SKW_CLOUDSHADOWS);
            } else {
                groundMat.DisableKeyword(SKW_CLOUDSHADOWS);
            }
            if (_earthSpecularEnabled) {
                groundMat.EnableKeyword(SKW_SPECULAR);
                groundMat.SetFloat("_SpecularPower", _earthSpecularPower);
                groundMat.SetFloat("_SpecularIntensity", _earthSpecularIntensity);
            } else {
                groundMat.DisableKeyword(SKW_SPECULAR);
            }
        }

        #endregion


    }

}