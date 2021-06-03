// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM
// ***************************************************************************
// This is the public API file - every property or public method belongs here
// ***************************************************************************
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using WPM.Poly2Tri;

namespace WPM {
    public static class Conversion {

        const float EARTH_RADIUS = 6371000f;

        #region Public Conversion API area

        /// <summary>
        /// Returns UV texture coordinates from latitude and longitude
        /// </summary>
        public static Vector2 GetUVFromLatLon(float lat, float lon) {
            Vector2 p;
            p.x = (lon + 180f) / 360f;
            p.y = (lat + 90f) / 180f;
            return p;
        }

        /// <summary>
        /// Returns UV texture coordinates from sphere coordinates
        /// </summary>
        public static Vector2 GetUVFromSpherePoint(Vector3 p) {
            float lat, lon;
            GetLatLonFromSpherePoint(p, out lat, out lon);
            return GetUVFromLatLon(lat, lon);
        }

        /// <summary>
        /// Converts latitude/longitude/altitude to sphere coordinates, optionally passing altitude in meters.
        /// Altitude is given in real world meters (Earth radius is 6371000 meters)
        /// </summary>
        public static Vector3 GetSpherePointFromLatLon(double lat, double lon, double altitude = 0) {
            double phi = lat * 0.0174532924; //Mathf.Deg2Rad;
            double theta = (lon + 90.0) * 0.0174532924; //Mathf.Deg2Rad;
            double cosPhi = Math.Cos(phi);
            double h = 0.5 * (altitude + EARTH_RADIUS) / EARTH_RADIUS;
            double x = cosPhi * Math.Cos(theta) * h;
            double y = Math.Sin(phi) * h;
            double z = cosPhi * Math.Sin(theta) * h;
            return new Vector3((float)x, (float)y, (float)z);
        }

        /// <summary>
        /// Converts latitude/longitude/altitude to sphere coordinates, optionally passing altitude in meters.
        /// Altitude is given in real world meters (Earth radius is 6371000 meters)
        /// </summary>
        public static Vector3 GetSpherePointFromLatLon(float lat, float lon, float altitude = 0f) {
            float phi = lat * Mathf.Deg2Rad;
            float theta = (lon + 90.0f) * Mathf.Deg2Rad;
            float cosPhi = Mathf.Cos(phi);
            float h = 0.5f * (altitude + EARTH_RADIUS) / EARTH_RADIUS;
            float x = cosPhi * Mathf.Cos(theta) * h;
            float y = Mathf.Sin(phi) * h;
            float z = cosPhi * Mathf.Sin(theta) * h;
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Converts latitude/longitude/altitude to sphere coordinates
        /// </summary>
        public static Vector3 GetSpherePointFromLatLon(Vector2 latLon, float altitude = 0) {
            return GetSpherePointFromLatLon(latLon.x, latLon.y, altitude);
        }

        /// <summary>
        /// Convertes sphere to latitude/longitude coordinates
        /// </summary>
        public static void GetLatLonFromSpherePoint(Vector3 p, out double lat, out double lon) {
            double phi = Math.Asin(p.y * 2.0);
            double theta = Math.Atan2(p.x, p.z);
            lat = phi * Mathf.Rad2Deg;
            lon = -theta * Mathf.Rad2Deg;
        }


        /// <summary>
        /// Convertes sphere to latitude/longitude coordinates
        /// </summary>
        public static void GetLatLonFromSpherePoint(Vector3 p, out float lat, out float lon) {
            p.Normalize();
            float phi = Mathf.Asin(p.y);
            float theta = Mathf.Atan2(p.x, p.z);
            lat = phi * Mathf.Rad2Deg;
            lon = -theta * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Convertes sphere to latitude/longitude coordinates
        /// </summary>
        public static void GetLatLonFromSpherePoint(Vector3 p, out Vector2 latLon) {
            float lat, lon;
            GetLatLonFromSpherePoint(p, out lat, out lon);
            latLon = new Vector2(lat, lon);
        }

        public static Vector2 GetLatLonFromBillboard(Vector2 position) {
            const float mapWidth = 200.0f;
            const float mapHeight = 100.0f;
            float lon = (position.x + mapWidth * 0.5f) * 360f / mapWidth - 180f;
            float lat = position.y * 180f / mapHeight;
            return new Vector2(lat, lon);
        }


        /// <summary>
        /// Gets the lat lon from UV coordinates (UV ranges from 0 to 1)
        /// </summary>
        /// <returns>The lat lon from U.</returns>
        /// <param name="uv">Uv.</param>
        public static Vector2 GetLatLonFromUV(Vector2 uv) {
            float lon = uv.x * 360f - 180f;
            float lat = (uv.y - 0.5f) * 2f * 90f;
            return new Vector2(lat, lon);
        }

        public static Vector2 GetBillboardPointFromLatLon(Vector2 latlon) {
            Vector2 p;
            float mapWidth = 200.0f;
            float mapHeight = 100.0f;
            p.x = (latlon.y + 180) * (mapWidth / 360f) - mapWidth * 0.5f;
            p.y = latlon.x * (mapHeight / 180f);
            return p;
        }

        public static Rect GetBillboardRectFromLatLonRect(Rect latlonRect) {
            Vector2 min = GetBillboardPointFromLatLon(latlonRect.min);
            Vector2 max = GetBillboardPointFromLatLon(latlonRect.max);
            return new Rect(min.x, min.y, Math.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));
        }

        public static Rect GetUVRectFromLatLonRect(Rect latlonRect) {
            Vector2 min = GetUVFromLatLon(latlonRect.min.x, latlonRect.min.y);
            Vector2 max = GetUVFromLatLon(latlonRect.max.x, latlonRect.max.y);
            return new Rect(min.x, min.y, Math.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));
        }

        /// <summary>
        /// Converts latitude/longitude to sphere coordinates
        /// </summary>
        public static Vector3 GetSpherePointFromLatLon(PolygonPoint point) {
            return GetSpherePointFromLatLon(point.X, point.Y, 0);
        }


        /// <summary>
        /// Converts latitude/longitude/altitude to sphere coordinates
        /// Altitude is given in real world meters (Earth radius is 6371000 meters)
        /// </summary>
        public static Vector3 GetSpherePointFromLatLon(PolygonPoint point, float altitude) {
            return GetSpherePointFromLatLon(point.X, point.Y, altitude);
        }


        /// <summary>
        /// Convertes sphere to latitude/longitude coordinates
        /// </summary>
        public static Vector2 GetLatLonFromSpherePoint(Vector3 p) {
            p.Normalize();
            float phi = Mathf.Asin(p.y);
            float theta = Mathf.Atan2(p.x, p.z);
            return new Vector2(phi * Mathf.Rad2Deg, -theta * Mathf.Rad2Deg);
        }

        public static Vector2 ConvertToTextureCoordinates(Vector3 p, int width, int height) {
            float phi = Mathf.Asin(p.y * 2f);
            float theta = Mathf.Atan2(p.x, p.z);
            float lonDec = -theta * Mathf.Rad2Deg;
            float latDec = phi * Mathf.Rad2Deg;
            Vector2 o;
            o.x = (lonDec + 180) * width / 360.0f;
            o.y = latDec * (height / 180.0f) + height / 2.0f;
            return o;
        }

        public static int ConvertToTextureColorIndex(Vector3 p, int widthMinusOne, int heightMinusOne) {
            const float invPI = 1.0f / Mathf.PI;
            const float PI2 = Mathf.PI * 2.0f;

            float phi = Mathf.Asin(p.y * 2.0f);
            float theta = Mathf.Atan2(p.x, p.z);
            float tx = (-theta + Mathf.PI) / PI2;
            float ty = phi * invPI + 0.5f;
            return ((int)(ty * heightMinusOne)) * (widthMinusOne + 1) + (int)(tx * widthMinusOne);
        }

        public static Vector2 GetBillboardPosFromSpherePoint(Vector3 p) {
            float u = 1.25f - (Mathf.Atan2(p.z, -p.x) / (2.0f * Mathf.PI) + 0.5f);
            if (u > 1)
                u -= 1.0f;
            float v = Mathf.Asin(p.y * 2.0f) / Mathf.PI;
            return new Vector2(u * 2.0f - 1.0f, v) * 100.0f;
        }

        /// <summary>
        /// Returns the distance in meters between two sphere positions
        /// </summary>
        /// <param name="position1">Position1.</param>
        /// <param name="position2">Position2.</param>
        public static float Distance(Vector3 position1, Vector3 position2) {
            Vector2 latlon1 = Conversion.GetLatLonFromSpherePoint(position1);
            Vector2 latlon2 = Conversion.GetLatLonFromSpherePoint(position2);
            return Distance(latlon1.x, latlon1.y, latlon2.x, latlon2.y);
        }

        /// <summary>
        /// Returns distance in meters between two lat/lon coordinates
        /// </summary>
        public static float Distance(float latDec1, float lonDec1, float latDec2, float lonDec2) {
            const float R = 6371000; // metres
            float phi1 = latDec1 * Mathf.Deg2Rad;
            float phi2 = latDec2 * Mathf.Deg2Rad;
            float deltaPhi = (latDec2 - latDec1) * Mathf.Deg2Rad;
            float deltaLambda = (lonDec2 - lonDec1) * Mathf.Deg2Rad;

            float a = Mathf.Sin(deltaPhi / 2) * Mathf.Sin(deltaPhi / 2) +
                Mathf.Cos(phi1) * Mathf.Cos(phi2) *
                Mathf.Sin(deltaLambda / 2) * Mathf.Sin(deltaLambda / 2);
            float c = 2.0f * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1.0f - a));
            return c * R;
        }

        /// <summary>
        /// Get tile coordinate which contains a given latitude/longitude
        /// </summary>
        /// <param name="zoomLevel">Zoom level.</param>
        /// <param name="lat">Lat.</param>
        /// <param name="lon">Lon.</param>
        /// <param name="xtile">Xtile.</param>
        /// <param name="ytile">Ytile.</param>
        public static void GetTileFromLatLon(int zoomLevel, float lat, float lon, out int xtile, out int ytile) {
            lat = Mathf.Clamp(lat, -80f, 80f);
            xtile = (int)((lon + 180.0) / 360.0 * (1 << zoomLevel));
            ytile = (int)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) + 1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoomLevel));
        }

        /// <summary>
        /// Gets latitude/longitude of top/left corner for a given map tile
        /// </summary>
        /// <returns>The lat lon from tile.</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="zoomLevel">Zoom level.</param>
        public static Vector2 GetLatLonFromTile(float x, float y, int zoomLevel) {
            double n = Math.PI - 2.0 * Math.PI * y / (1 << zoomLevel);
            double lat = 180.0 / Math.PI * Math.Atan(Math.Sinh(n));
            double lon = x / (double)(1 << zoomLevel) * 360.0 - 180.0;
            return new Vector2((float)lat, (float)lon);
        }


        #endregion

    }

}