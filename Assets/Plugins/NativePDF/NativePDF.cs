using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace NativePDFNamespace
{
    public class NativePDF
    {
        public class TextSettings
        {
            public TextSettings(FontColor fontColor, int fontSize, bool underline = false)
            {
                this.FontColor = fontColor;
                this.FontSize = fontSize;
                this.Underline = underline;
            }

            public override string ToString()
            {
                return $"({FontColor},{FontSize},{Underline})";
            }
            public FontColor FontColor;
            public int FontSize;
            public bool Underline;
        }

        public class Typeface
        {
            public Typeface(string name)
            {
                this.Name = name;
            }
            public string Name;
        }

        public class FontColor
        {
            public static FontColor Red { get => _red; }
            public static FontColor Green { get => _green; }
            public static FontColor Blue { get => _blue; }
            public static FontColor White { get => _white; }
            public static FontColor Black { get => _black; }
            public static FontColor Yellow { get => _yellow; }
            public static FontColor Cyan { get => _cyan; }
            public static FontColor Grey { get => _grey; }
            public static FontColor Magenta { get => _magenta; }

            static FontColor _red = new FontColor(255, 0, 0);
            static FontColor _green = new FontColor(0, 255, 0);
            static FontColor _blue = new FontColor(0, 0, 255);
            static FontColor _white = new FontColor(255, 255, 255);
            static FontColor _black = new FontColor(0, 0, 0);
            static FontColor _yellow = new FontColor(255, 235, 4);
            static FontColor _cyan = new FontColor(0, 255, 255);
            static FontColor _grey = new FontColor(128, 128, 128);
            static FontColor _magenta = new FontColor(255, 0, 255);

            public FontColor(int r, int g, int b)
            {
                this.r = r;
                this.g = g;
                this.b = b;
            }

            public override string ToString()
            {
                return $"({r},{g},{b})";
            }
            public int r, g, b;
        }

#if UNITY_ANDROID
        private bool _pageStarted;

        private static AndroidJavaClass _ajc = null;
        private static AndroidJavaClass AJC
        {
            get
            {
                if (_ajc == null)
                    _ajc = new AndroidJavaClass("com.binarysoul.pdflibrary.PdfCreator");

                return _ajc;
            }
        }

        private static AndroidJavaObject _ajobject;
        private static AndroidJavaObject AJObject
        {
            get
            {
                if(_ajobject == null)
                    _ajobject = AJC.CallStatic<AndroidJavaObject>("getInstance");

                return _ajobject;
            }
        }
#elif UNITY_IOS
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _createDocument(int pageWidth, int pageHeight);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _addPage();
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _drawText(string text, float x, float y, string settings);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _drawImage(byte[] imgData, int dataSize, float x, float y, int width, int height);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int _getDocumentData(out IntPtr p);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int _setTypeface(string name);
#endif

#if UNITY_IOS
        IntPtr unmanagedPtr = IntPtr.Zero;
        ~NativePDF()
        {
            if (unmanagedPtr != IntPtr.Zero)
                Marshal.FreeHGlobal(unmanagedPtr);

            unmanagedPtr = IntPtr.Zero;
        }
#endif

        public void CreateDocument(int pageWidth, int pageHeight)
        {

#if UNITY_ANDROID
            AJObject.CallStatic("CreatePdfDocument", new[] { pageWidth, pageHeight });
#elif UNITY_IOS
            _createDocument(pageWidth, pageHeight);
#endif
        }

        public void AddPage()
        {
#if UNITY_ANDROID
            if (_pageStarted)
                FinishPage();

            _pageStarted = true;
            AJObject.CallStatic("StartPage");
#elif UNITY_IOS
            _addPage();
#endif
        }

#if UNITY_ANDROID
        private void FinishPage()
        {
            AJObject.CallStatic("FinishPage");

            _pageStarted = false;
        }
#endif

        public void DrawText(string text, float x, float y, TextSettings textSettings)
        {
#if UNITY_ANDROID
            AJObject.CallStatic("DrawText", new[] { text, x.ToString(), y.ToString(), textSettings.ToString() });
#elif UNITY_IOS
            _drawText(text, x, y, textSettings.ToString());
#endif            
        }

        public void DrawImage(byte[] imageData, int imageDataSize, float x, float y, int width, int height)
        {
#if UNITY_ANDROID
            string base64Data = Convert.ToBase64String(imageData);
            AJObject.CallStatic<string>("DrawImage", new[] { base64Data, x.ToString(), y.ToString(), width.ToString(), height.ToString() });
#elif UNITY_IOS
            _drawImage(imageData, imageData.Length, x, y, width, height);
#endif
        }

        public Typeface LoadTypeface(string path)
        {
            string fileName = String.Empty;
#if UNITY_ANDROID
            AJObject.CallStatic("LoadTypeface", path);
            fileName = Path.GetFileName(path);
#elif UNITY_IOS
            fileName = Path.GetFileNameWithoutExtension(path);
#endif
            return new Typeface(fileName);
        }

        public void SetTypeface(Typeface typeface)
        {
#if UNITY_ANDROID
            AJObject.CallStatic("SetTypeface", typeface.Name);
#elif UNITY_IOS
            _setTypeface(typeface.Name);
#endif
        }

        public byte[] GetDocumentData()
        {
#if UNITY_ANDROID
            if (_pageStarted)
                FinishPage();

            byte[] pdfData = AJObject.CallStatic<byte[]>("GetDocumentAsBytes");
            return pdfData;
#elif UNITY_IOS
            IntPtr unmanagedPtr;
            int len = _getDocumentData(out unmanagedPtr);
            byte[] managedData = new byte[len];
            Marshal.Copy(unmanagedPtr, managedData, 0, len);
            return managedData;
#else
            return null;
#endif
        }
    }
}