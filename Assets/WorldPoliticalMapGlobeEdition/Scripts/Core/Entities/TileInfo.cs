using UnityEngine;
using System;

using System.Collections;
using System.Collections.Generic;

namespace WPM {
    public enum TILE_LOAD_STATUS {
        Inactive = 0,
        InQueue = 1,
        Loading = 2,
        Loaded = 3
    }

    public enum TILE_SOURCE {
        Unknown = 0,
        Online = 1,
        Cache = 2,
        Resources = 3
    }

    public class TileInfo {
        public int x, y, zoomLevel;
        public TILE_LOAD_STATUS loadStatus = TILE_LOAD_STATUS.Inactive;
        public bool created;
        public Vector2[] latlons = new Vector2[4];
        public Vector3[] spherePos = new Vector3[4];
        public Vector3[] cornerWorldPos = new Vector3[4];
        public GameObject gameObject;
        public Renderer renderer;
        public Material transMat, opaqueMat;
        public float distToCamera;
        public TileInfo parent;
        public List<TileInfo> children;
        public bool visible;
        public bool insideViewport;
        public bool hasAnimated, animationFinished, isAnimating;
        public float queueTime;
        public TILE_SOURCE source;
        public int subquadIndex;
        public bool debug;
        public Texture2D texture;
        public bool placeholderImageSet;
        public Vector4 parentTextureCoords, worldTextureCoords;
        public float inactiveTime;
        public bool isAddedToInactive;
        public int loadDelay;
        public int stage;

        Texture2D currentEarthTexture;
        TileAnimator anim;

        public TileInfo(int x, int y, int zoomLevel, int subquadIndex, Texture2D currentEarthTexture) {
            this.x = x;
            this.y = y;
            this.zoomLevel = zoomLevel;
            this.subquadIndex = subquadIndex;
            this.currentEarthTexture = currentEarthTexture;
        }

        public void SetAlpha(float t) {
            if (parent.transMat == null)
                return;
            switch (subquadIndex) {
                case 0:
                    parent.transMat.SetFloat("_Alpha", t);
                    break;
                case 1:
                    parent.transMat.SetFloat("_Alpha1", t);
                    break;
                case 2:
                    parent.transMat.SetFloat("_Alpha2", t);
                    break;
                case 3:
                    parent.transMat.SetFloat("_Alpha3", t);
                    break;
            }
        }


        public void StopAnimation() {
            if (isAnimating && anim != null) {
                anim.Stop();
            }
        }

        public void Animate(float duration, TileAnimator.AnimationEvent callback, bool invert = false) {
            if (anim != null) {
                anim.Stop();
            } else {
                anim = gameObject.AddComponent<TileAnimator>();
            }
            anim.invert = invert;
            anim.duration = duration;
            anim.ti = this;
            if (duration == 0) {
                anim.Stop();
                if (callback != null) {
                    callback(this);
                }
            } else {
                anim.OnAnimationEnd -= callback;
                anim.OnAnimationEnd += callback;
                anim.Play();
            }
        }

        public void Reset() {
            if (parent == null)
                return;
            if (renderer != null) {
                renderer.sharedMaterial = parent.transMat;
            }
            StopAnimation();
            SetAlpha(0);
            hasAnimated = animationFinished = isAnimating = false;
            placeholderImageSet = false;
            source = TILE_SOURCE.Unknown;
        }

        public void ClearPlaceholderImage() {
            if (parent == null) {
                return;
            }
            if (renderer != null) {
                renderer.sharedMaterial = parent.transMat;
            }
            parentTextureCoords = worldTextureCoords;
            SetPlaceholderImage(currentEarthTexture);
            parent.placeholderImageSet = false;
            SetAlpha(0);
            hasAnimated = animationFinished = isAnimating = false;
        }

        public void SetPlaceholderImage(Texture2D texture) {
            if (parent == null || parent.transMat == null) {
                return;
            }

            parent.transMat.SetTexture("_ParentTex", texture);
            switch (subquadIndex) {
                case 0:
                    parent.transMat.SetVector("_ParentCoords", parentTextureCoords);
                    break;
                case 1:
                    parent.transMat.SetVector("_ParentCoords1", parentTextureCoords);
                    break;
                case 2:
                    parent.transMat.SetVector("_ParentCoords2", parentTextureCoords);
                    break;
                case 3:
                    parent.transMat.SetVector("_ParentCoords3", parentTextureCoords);
                    break;
            }
        }

        public void SetTexture(Texture2D texture) {

            this.texture = texture;
            TileInfo parent = this.parent != null ? this.parent : this;
            switch (subquadIndex) {
                case 0:
                    parent.transMat.SetTexture("_MainTex", texture);
                    parent.opaqueMat.SetTexture("_MainTex", texture);
                    break;
                case 1:
                    parent.transMat.SetTexture("_MainTex1", texture);
                    parent.opaqueMat.SetTexture("_MainTex1", texture);
                    break;
                case 2:
                    parent.transMat.SetTexture("_MainTex2", texture);
                    parent.opaqueMat.SetTexture("_MainTex2", texture);
                    break;
                case 3:
                    parent.transMat.SetTexture("_MainTex3", texture);
                    parent.opaqueMat.SetTexture("_MainTex3", texture);
                    break;
            }
        }

        public bool loadedFromCache {
            get { return source == TILE_SOURCE.Cache; }
        }


    }

}