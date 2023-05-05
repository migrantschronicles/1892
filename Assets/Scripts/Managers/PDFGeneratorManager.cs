using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PDFGeneratorManager : MonoBehaviour
{
    [SerializeField]
    private Camera pdfCamera;
    [SerializeField]
    private Canvas pdfCanvas;
    [SerializeField]
    private Sprite diaryBackgroundDiary;
    [SerializeField]
    private Sprite diaryBackgroundMap;
    [SerializeField]
    private Image diaryBackground;
    [SerializeField]
    private Image background;
    [SerializeField]
    private GameObject mapContent;
    [SerializeField]
    private GameObject diaryContent;
    [SerializeField]
    private GameObject leftPage;
    [SerializeField]
    private GameObject rightPage;
    [SerializeField]
    private DiaryPages diaryPages;

    public Texture2D TakeMapScreenshot()
    {
        pdfCamera.gameObject.SetActive(true);
        pdfCanvas.gameObject.SetActive(true);
        diaryBackground.sprite = diaryBackgroundMap;

        mapContent.SetActive(true);
        MapRoute[] routes = mapContent.GetComponentsInChildren<MapRoute>(true);
        foreach(MapRoute route in routes)
        {
            route.UpdateImage();
        }

        MapShipRoute[] shipRoutes = mapContent.GetComponentsInChildren<MapShipRoute>(true);
        foreach(MapShipRoute shipRoute in shipRoutes)
        {
            shipRoute.UpdateElement();
        }

        MapLocationMarker[] locations = mapContent.GetComponentsInChildren<MapLocationMarker>(true);
        foreach(MapLocationMarker location in locations)
        {
            location.UpdateIcon();
            location.UpdateName();
        }

        Texture2D screenshot = TakeScreenshot(944, 590);

        mapContent.SetActive(false);
        pdfCamera.gameObject.SetActive(false);
        pdfCanvas.gameObject.SetActive(false);

        return screenshot;
    }

    public Texture2D TakeDiaryScreenshot(DiaryEntryData data)
    {
        pdfCamera.gameObject.SetActive(true);
        pdfCanvas.gameObject.SetActive(true);
        diaryBackground.sprite = diaryBackgroundDiary;

        diaryContent.SetActive(true);
        GameObject leftPageContent = diaryPages.CreatePageContent(data, data.leftPage, leftPage.transform, false);
        GameObject rightPageContent = diaryPages.CreatePageContent(data, data.rightPage, rightPage.transform, false);

        Texture2D renderedTexture = TakeScreenshot(816, 510);

        diaryContent.SetActive(false);
        pdfCamera.gameObject.SetActive(false);
        pdfCanvas.gameObject.SetActive(false);

        System.IO.File.WriteAllBytes(Application.persistentDataPath + "/pdftest.png", renderedTexture.EncodeToPNG());

        Destroy(leftPageContent);
        Destroy(rightPageContent);

        return renderedTexture;
    }

    private Texture2D TakeScreenshot(int outputWidth, int outputHeight)
    {
        RenderTexture mainTexture = new RenderTexture(Screen.width, Screen.height, 16);
        pdfCamera.targetTexture = mainTexture;
        pdfCamera.Render();

        // Set the output size and adjust the image size that is actually rendered (same aspect ratio of screen).
        int targetWidth = outputWidth;
        int targetHeight = outputHeight;
        float sourceAspect = (float)Screen.width / Screen.height;
        float outputAspect = (float)outputWidth / outputHeight;
        if (!Mathf.Approximately(sourceAspect, outputAspect))
        {
            if (outputAspect > sourceAspect)
            {
                targetWidth = (int)(targetHeight * sourceAspect);
            }
            else if (outputAspect < sourceAspect)
            {
                targetHeight = (int)(targetWidth / sourceAspect);
            }
        }

        // Resize the screen texture to the new target size
        RenderTexture resizedTexture = new RenderTexture(targetWidth, targetHeight, 16);
        RenderTexture.active = resizedTexture;
        Graphics.Blit(mainTexture, resizedTexture);

        // Read the render texture into a texture.
        Texture2D renderedTexture = new Texture2D(outputWidth, outputHeight);
        int destX = 0;
        int destY = 0;
        if (!Mathf.Approximately(sourceAspect, outputAspect))
        {
            // Adjust the x and y position where the pixel data in the texture is written to.
            if (outputAspect > sourceAspect)
            {
                destX = (int)((outputWidth - (outputHeight * sourceAspect)) / 2);
            }
            else if (outputAspect < sourceAspect)
            {
                destY = (int)((outputHeight - (outputWidth / sourceAspect)) / 2);
            }

            // Fill the background transparent
            Color[] renderedTextureColors = renderedTexture.GetPixels();
            Color backgroundColor = new Color(0, 0, 0, 0);
            for (int i = 0; i < renderedTextureColors.Length; ++i)
            {
                renderedTextureColors[i] = backgroundColor;
            }
            renderedTexture.SetPixels(renderedTextureColors);
        }
        renderedTexture.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), destX, destY);

        renderedTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);

        RenderTexture.active = null;
        pdfCamera.targetTexture = null;
        mainTexture.Release();

        return renderedTexture;
    }
}
