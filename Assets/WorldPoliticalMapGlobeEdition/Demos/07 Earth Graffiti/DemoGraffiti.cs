using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WPM {
    public class DemoGraffiti : MonoBehaviour {

        WorldMapGlobe map;
        bool enableRotation;
        GUIStyle labelStyle, buttonStyle, sliderStyle, sliderThumbStyle;
        ColorPicker colorPicker;
        Color32[] colors;
        Texture2D earthTex;
        Color32 penColor = Color.cyan;
        int penWidth = 5;
        int penStrength = 128;
        float lastTextureUpdateTime;
        bool needTextureUpdate;

        void Start() {

            labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.white;
            buttonStyle = new GUIStyle(labelStyle);
            buttonStyle.alignment = TextAnchor.MiddleLeft;
            buttonStyle.normal.background = Texture2D.whiteTexture;
            buttonStyle.normal.textColor = Color.white;
            sliderStyle = new GUIStyle();
            sliderStyle.normal.background = Texture2D.whiteTexture;
            sliderStyle.fixedHeight = 4.0f;
            sliderThumbStyle = new GUIStyle();
            sliderThumbStyle.normal.background = Resources.Load<Texture2D>("thumb");
            sliderThumbStyle.overflow = new RectOffset(0, 0, 8, 0);
            sliderThumbStyle.fixedWidth = 20.0f;
            sliderThumbStyle.fixedHeight = 12.0f;
            colorPicker = gameObject.GetComponent<ColorPicker>();


            // setup GUI resizer - only for the demo
            GUIResizer.Init(800, 500);

            // Get map instance to Globe API methods
            map = WorldMapGlobe.instance;
            map.OnMouseDown += (cursorLocation, buttonIndex) => { if (buttonIndex==0) PaintEarth(cursorLocation); };
            map.OnDrag += PaintEarth;

            ResetTexture();
        }

        void OnGUI() {
            GUIResizer.AutoResize();
            GUI.Label(new Rect(10, 10, 350, 30), "Click and drag over the Earth to paint", labelStyle);

            enableRotation = GUI.Toggle(new Rect(10, 40, 350, 30), enableRotation, "Enable Earth rotation");
            map.allowUserRotation = enableRotation;

            GUI.Label(new Rect(10, 70, 85, 30), "  Pen Width", labelStyle);
            penWidth = (int)GUI.HorizontalSlider(new Rect(110, 75, 100, 20), penWidth, 2, 50, sliderStyle, sliderThumbStyle);

            GUI.Label(new Rect(10, 95, 85, 30), "  Pen Alpha", labelStyle);
            penStrength = (int)GUI.HorizontalSlider(new Rect(110, 100, 100, 20), penStrength, 1, 255, sliderStyle, sliderThumbStyle);

            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.5f);

            if (GUI.Button(new Rect(10, 120, 160, 30), "  Change Pen Color", buttonStyle)) {
                colorPicker.showPicker = true;
            }

            if (GUI.Button(new Rect(10, 145, 90, 30), "  Reset Texture", buttonStyle)) {
                ResetTexture();
            }


            if (colorPicker.showPicker) {
                penColor = colorPicker.setColor;
            }
        }

        void PaintEarth(Vector3 cursorLocation) {
            if (enableRotation)
                return;

            // Convert cursor location to texture coordinates
            Vector2 uv = Conversion.GetUVFromSpherePoint(cursorLocation);

            // Paints thick pixel on texture position
            float srcAlpha = 1f - penStrength / 255f;
            int x = (int)(uv.x * earthTex.width);
            int y = (int)(uv.y * earthTex.height);
            for (int j = -penWidth; j < penWidth; j++) {
                int jj = (y + j) * earthTex.width;
                for (int k = -penWidth; k < penWidth; k++) {
                    int colorIndex = jj + x + k;
                    if (colorIndex < 0 || colorIndex >= colors.Length)
                        continue;
                    Color32 currentColor = colors[colorIndex];

                    float r = (float)(j * j + k * k) / (penWidth * penWidth);   // for smooth drawing
                    if (r < srcAlpha)
                        r = srcAlpha;
                    else if (r > 1f)
                        r = 1f;

                    colors[colorIndex].r = (byte)(currentColor.r * r + penColor.r * (1f - r));
                    colors[colorIndex].g = (byte)(currentColor.g * r + penColor.g * (1f - r));
                    colors[colorIndex].b = (byte)(currentColor.b * r + penColor.b * (1f - r));
                }
            }
            needTextureUpdate = true;
        }

        void Update() {
            if (needTextureUpdate && Time.time - lastTextureUpdateTime > 0.1f) {
                needTextureUpdate = false;
                lastTextureUpdateTime = Time.time;
                earthTex.SetPixels32(colors);
                earthTex.Apply();
            }
        }

        void ResetTexture() {

            map.ReloadEarthTexture();

            // Get current pixels
            earthTex = map.earthTexture;
            colors = earthTex.GetPixels32();

        }




    }
}
