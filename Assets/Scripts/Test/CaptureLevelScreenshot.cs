using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CaptureLevelScreenshot : MonoBehaviour
{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.S))
        {
            bool blurEnabled = LevelInstance.Instance.IsBlurEnabled;
            if(!blurEnabled)
            {
                LevelInstance.Instance.SetBlurEnabled(true);
            }

            bool interactablesEnabled = LevelInstance.Instance.AreSceneInteractablesEnabled;
            if(interactablesEnabled)
            {
                LevelInstance.Instance.SetSceneInteractablesEnabled(false);
            }

            // Capture
            Camera mainCamera = LevelInstance.Instance.MainCamera;
            Camera uiCamera = LevelInstance.Instance.UICamera;
            var mainCameraData = mainCamera.GetUniversalAdditionalCameraData();
            mainCameraData.cameraStack.Remove(uiCamera);

            Texture2D renderedTexture = PDFGeneratorManager.TakeScreenshot(mainCamera, 1920, 1200);
            if(renderedTexture)
            {
                string path = $"{Application.persistentDataPath}/{LevelInstance.Instance.LocationName}.png";
                System.IO.File.WriteAllBytes(path, renderedTexture.EncodeToPNG());
                Debug.Log($"Screenhot taken to {path}");
            }

            mainCameraData.cameraStack.Add(uiCamera);

            // Cleanup
            if(!blurEnabled)
            {
                LevelInstance.Instance.SetBlurEnabled(false);
            }

            if(interactablesEnabled)
            {
                LevelInstance.Instance.SetSceneInteractablesEnabled(true);
            }
        }
    }
#endif
}
