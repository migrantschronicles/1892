using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using PdfSharp.Pdf;
using sharpPDF;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
#elif UNITY_ANDROID || UNITY_IOS
using static NativePDFNamespace.NativePDF;
using UnityEngine.Networking;
#endif

/**
 * Interface for the pdf document so the pdf can be generated on mobile and windows.
 */
interface IPDFPlatform
{
    int FontSize { get; set; }
    int PageWidth { get; }
    int PageHeight { get; }

    /**
     * Adds a page to the document. New draw operations happen on the new page.
     */
    void AddPage();
    /**
     * Draws text.
     * @param text The string.
     * @param x The x coordinate (left to right).
     * @param y The y coordinate (top to bottom).
     */
    void DrawText(string text, int x, int y);
    /**
     * Draws an image.
     * @param relativePath The relative path to the Application.streamingAssetsPath folder
     * @param x The x coordinate (left to right).
     * @param y The y coordinate (top to bottom).
     * @param width The width.
     * @param height The height.
     */
    void DrawPNG(string relativePath, int x, int y, int width, int height);
    /**
     * Draws an image.
     * @param pngBytes The bytes of the image, png encoded.
     * @param x The x coordinate (left to right).
     * @param y The y coordinate (top to bottom).
     * @param width The width.
     * @param height The height.
     */
    void DrawPNG(byte[] pngBytes, int x, int y, int width, int height);
    /**
     * Saves the document.
     */
    void Close();
    /**
     * Loads a font
     */
    void LoadFont(string fontFile);
    /**
     * Sets a font.
     */
    void SetFont(string fontFile);
}

interface IGeoJSONPlatform
{
    void WriteGeoJSON(string json);
}

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
/**
 * PDF generation happens in 2 phases:
 *  - First all text is drawn with sharpPDF.
 *  - All image draw operations are cached to be rendered in a second pass with PDFsharp.
 */
class WinPDFPlatform : IPDFPlatform
{
    class DrawImageData
    {
        public string relativePath;
        public byte[] pngBytes;
        public int x;
        public int y;
        public int width;
        public int height;
    }

    public int FontSize { get; set; }
    public int PageWidth { get { return 595; } }
    public int PageHeight { get { return 842; } }

    private string outputPath;
    private pdfDocument sharpPdfDocument;
    private pdfPage sharpPdfPage;
    private int pageIndex = -1;
    private List<List<DrawImageData>> drawImages = new List<List<DrawImageData>>();

    public WinPDFPlatform(string path)
    {
        sharpPdfDocument = new pdfDocument("Migrants Chronicles", "TH K�ln");
        FontSize = 12;
        outputPath = path;
    }

    public void AddPage()
    {
        ++pageIndex;
        drawImages.Add(new List<DrawImageData>());
        sharpPdfPage = sharpPdfDocument.addPage(PageHeight, PageWidth);
    }
    public void DrawText(string text, int x, int y)
    {
        // sharpPdf draws text from the bottom.
        int correctedY = 842 - y - FontSize;
        sharpPdfPage.addText(text, x, correctedY, sharpPDF.Enumerators.predefinedFont.csHelvetica, FontSize);
    }

    public void DrawPNG(string relativePath, int x, int y, int width, int height)
    {
        DrawImageData data = new DrawImageData { relativePath = relativePath, x = x, y = y, width = width, height = height };
        drawImages[pageIndex].Add(data);
    }

    public void DrawPNG(byte[] pngBytes, int x, int y, int width, int height)
    {
        DrawImageData data = new DrawImageData { pngBytes = pngBytes, x = x, y = y, width = width, height = height };
        drawImages[pageIndex].Add(data);
    }

    public void Close()
    {
        sharpPdfDocument.createPDF(outputPath);

        PdfDocument importedDocument = PdfReader.Open(outputPath, PdfDocumentOpenMode.Import);
        PdfDocument outputDocument = new PdfDocument();
        outputDocument.Info.Title = "Migrants Chronicles";

        for(int i = 0; i < pageIndex + 1; i++)
        {
            PdfPage importedPage = importedDocument.Pages[i];
            PdfPage outputPage = outputDocument.AddPage(importedPage);
            XGraphics gfx = XGraphics.FromPdfPage(outputPage);

            List<DrawImageData> images = drawImages[i];
            foreach(DrawImageData data in images)
            {
                XImage img;
                if(data.pngBytes != null)
                {
                    img = XImage.FromStream(new MemoryStream(data.pngBytes));
                }
                else
                {
                    img = XImage.FromFile(Path.Combine(Application.streamingAssetsPath, data.relativePath));
                }
                gfx.DrawImage(img, data.x, data.y, data.width, data.height);
            }
        }

        importedDocument.Close();
        outputDocument.Save(outputPath);
    }

    public void SetFont(string fontFile)
    {
        // not supported
    }

    public void LoadFont(string fontFile)
    {
        // not supported
    }
}

class WinGeoJSONPlatform : IGeoJSONPlatform
{
    private string filePath;

    public WinGeoJSONPlatform(string path)
    {
        filePath = path;
    }

    public void WriteGeoJSON(string json)
    {
        File.WriteAllText(filePath, json);
    }
}
#elif UNITY_ANDROID || UNITY_IOS
class MobilePDFPlatform : IPDFPlatform
{
    public int FontSize { get; set; }
    public int PageWidth { get { return 595; } }
    public int PageHeight { get { return 842; } }

    private string outputPath;
    private TextSettings textSettings;
    private NativePDFNamespace.NativePDF pdf;
    private Dictionary<string, Typeface> fonts = new Dictionary<string, Typeface>();

    public MobilePDFPlatform(string outputPath)
    {
        this.outputPath = outputPath;
        FontSize = 12;
        textSettings = new TextSettings(FontColor.Black, FontSize);
        pdf = new NativePDFNamespace.NativePDF();
        pdf.CreateDocument(PageWidth, PageHeight);
    }

    public void AddPage()
    {
        pdf.AddPage();
    }

    public void Close()
    {
        byte[] data = pdf.GetDocumentData();
        File.WriteAllBytes(outputPath, data);
        NativeFilePicker.Permission permission = NativeFilePicker.ExportFile(outputPath, (success) => Debug.Log("PDF Exported: " + success));
        Debug.Log("PDF Export Permission: " + permission);
    }

    public void DrawPNG(string relativePath, int x, int y, int width, int height)
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
#if UNITY_ANDROID
        UnityWebRequest loadingRequest = UnityWebRequest.Get(fullPath);
        loadingRequest.SendWebRequest();
        while (!loadingRequest.isDone)
        {
            // Wait
        }

        if(loadingRequest.result == UnityWebRequest.Result.Success)
        {
            DrawPNG(loadingRequest.downloadHandler.data, x, y, width, height);
        }
        else
        {
            Debug.Log("Failed to load image " + fullPath);
        }
#elif UNITY_IOS
        byte[] data = File.ReadAllBytes(fullPath);
        if(data != null)
        {
            DrawPNG(data, x, y, width, height);
        }
        else
        {
            Debug.Log("Failed to load image " + fullPath);
        }
#endif
    }

    public void DrawPNG(byte[] pngBytes, int x, int y, int width, int height)
    {
        pdf.DrawImage(pngBytes, pngBytes.Length, x, y, width, height);
    }

    public void DrawText(string text, int x, int y)
    {
        textSettings.FontSize = FontSize;
        pdf.DrawText(text, x, y, textSettings);
    }

    public void LoadFont(string fontFile)
    {
        if(fonts.ContainsKey(fontFile))
        {
            return;
        }

        Typeface typeface = null;

#if UNITY_ANDROID
        string destPath = Path.Combine(Application.persistentDataPath, fontFile);
        if (!File.Exists(destPath))
        {
            string srcPath = Path.Combine(Application.streamingAssetsPath, fontFile);
            UnityWebRequest loadingRequest = UnityWebRequest.Get(srcPath);
            loadingRequest.SendWebRequest();
            while (!loadingRequest.isDone)
            {
                // Wait
            }

            if (loadingRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Failed to load font: " + fontFile);
                return;
            }

            File.WriteAllBytes(destPath, loadingRequest.downloadHandler.data);
        }
            
        typeface = pdf.LoadTypeface(destPath);
#elif UNITY_IOS
        typeface = pdf.LoadTypeface(fontFile);
#endif

        fonts.Add(fontFile, typeface);
    }

    public void SetFont(string fontFile)
    {
        if(!fonts.ContainsKey(fontFile))
        {
            LoadFont(fontFile);
        }

        if(fonts.TryGetValue(fontFile, out Typeface typeface))
        {
            Debug.Log(typeface != null ? "NOT" : "NULL");
            Debug.Log(typeface?.Name);
            pdf.SetTypeface(typeface);
        }
    }
}

class MobileGeoJSONPlatform : IGeoJSONPlatform
{
    private string filePath;

    public MobileGeoJSONPlatform(string path)
    {
        filePath = path;
    }

    public void WriteGeoJSON(string json)
    {
        File.WriteAllText(filePath, json);
        NativeFilePicker.Permission permission = NativeFilePicker.ExportFile(filePath, (success) => Debug.Log("GeoJSON Exported: " + success));
    }
}
#else
#error PDF Platform not supported
#endif

public class PDFBuilder
{
    private string OutputPath
    {
        get
        {
            return Application.persistentDataPath;
        }
    }

    private IPDFPlatform CreatePlatform(string outputPath)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        return new WinPDFPlatform(outputPath);
#elif UNITY_ANDROID || UNITY_IOS
        return new MobilePDFPlatform(outputPath);
#endif
    }

    private IGeoJSONPlatform CreateGeoJSONPlatform(string outputPath)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        return new WinGeoJSONPlatform(outputPath);
#elif UNITY_ANDROID || UNITY_IOS
        return new MobileGeoJSONPlatform(outputPath);
#endif
    }

    private void DrawPageNumber(IPDFPlatform pdf, int pageNumber)
    {
        int oldFontSize = pdf.FontSize;
        pdf.FontSize = 12;
        pdf.DrawText(pageNumber.ToString(), 295, 811);
        pdf.FontSize = oldFontSize;
    }

    private void DrawTitlePage(IPDFPlatform pdf, ref int pageNumber, string username, float playtime)
    {
        pdf.AddPage();
        pdf.DrawPNG("PDF/PDF_Background_1.png", 0, 0, pdf.PageWidth, pdf.PageHeight);
        pdf.FontSize = 15;
        // Date
        pdf.DrawText(DateTime.Now.ToString("d MMMM yyyy"), 305, 343);
        // Player name
        pdf.DrawText(username, 305, 367);
        //pdf.SetFont("AlegreyaSans-Black.ttf");
        // Overall Playtime
        int totalMinutes = Mathf.CeilToInt(playtime / 60);
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;
        pdf.DrawText(string.Format("{0:00}h {1:00}m", hours, minutes), 305, 392);
        // Character choice
        string characterFrame = "";
        switch(NewGameManager.Instance.PlayerCharacterManager.SelectedCharacter)
        {
            case CharacterType.Elis: characterFrame = "PDF/PortraitElis.png"; break;
            case CharacterType.Punnels: characterFrame = "PDF/PortraitPunnels.png"; break;
            case CharacterType.Michel: characterFrame = "PDF/PortraitMichel.png"; break;
        }
        pdf.DrawPNG(characterFrame, 190, 498, 214, 250);
        // Page Number
        DrawPageNumber(pdf, ++pageNumber);
    }

    private void DrawJourneyPage(IPDFPlatform pdf, Texture2D mapScreenshot, ref int pageNumber)
    {
        pdf.AddPage();
        pdf.FontSize = 12;
        pdf.DrawPNG("PDF/PDF_Background_2_1.png", 0, 0, pdf.PageWidth, pdf.PageHeight);
        // Screenshot
        RectInt screenshotRect = new RectInt(61, 102, 472, 295);
        if(mapScreenshot != null)
        {
            pdf.DrawPNG(mapScreenshot.EncodeToPNG(), screenshotRect.x, screenshotRect.y, screenshotRect.width, screenshotRect.height);
        }
        else
        {
            pdf.DrawPNG("PDF/Diary-Book_Route.png", screenshotRect.x, screenshotRect.y, screenshotRect.width, screenshotRect.height);
        }
        DrawPageNumber(pdf, ++pageNumber);
    }

    private void DrawJourneys(IPDFPlatform pdf, List<Journey> journeys, ref int pageNumber)
    {
        int journeyIndex = 0;
        int entryIndex = 0;
        int i = 0;
        while(journeyIndex < journeys.Count)
        {
            Journey journey = journeys[journeyIndex];
            if(entryIndex >= journey.diaryEntries.Count)
            {
                entryIndex = 0;
                ++journeyIndex;
                continue;
            }

            DiaryEntryData diaryEntry = journey.diaryEntries[entryIndex];

            bool top = i % 2 == 0;
            if(top)
            {
                pdf.AddPage();
                string background = (entryIndex == journey.diaryEntries.Count - 1 && journeyIndex == journeys.Count - 1) ?
                    (i == 0 ? "PDF/PDF_Background_3.png" : "PDF/PDF_Background_3.3.png") :
                    (i == 0 ? "PDF/PDF_Background_3.1.png" : "PDF/PDF_Background_3.2.png");
                pdf.DrawPNG(background, 0, 0, pdf.PageWidth, pdf.PageHeight);
                DrawPageNumber(pdf, ++pageNumber);
            }

            // City Name
            pdf.FontSize = 15;
            string cityName = $"{i + 1}. {NewGameManager.Instance.LocationManager.GetLocalizedName(journey.destination)}";
            pdf.DrawText(cityName, 94, top ? 49 : 457);

            // Screenshot
            Texture2D screenshot = LevelInstance.Instance.TakeDiaryScreenshot(diaryEntry);
            if(screenshot != null)
            {
                RectInt screenshotRect = new RectInt(94, top ? 77 : 486, 408, 255);
                pdf.DrawPNG(screenshot.EncodeToPNG(), screenshotRect.x, screenshotRect.y, screenshotRect.width, screenshotRect.height);
            }

            // Transport icon
            if(i > 0 && entryIndex == 0)
            {
                string iconPath = "PDF/Methods/";
                switch (journey.method)
                {
                    case TransportationMethod.Walking: iconPath += "PDF_Transportation Icon_6.png"; break;
                    case TransportationMethod.Train: iconPath += "PDF_Transportation Icon_7.png"; break;
                    case TransportationMethod.Ship: iconPath += "PDF_Transportation Icon_2.png"; break;
                    case TransportationMethod.Carriage: iconPath += "PDF_Transportation Icon_4.png"; break;
                    case TransportationMethod.Cart: iconPath += "PDF_Transportation Icon_3.png"; break;
                    case TransportationMethod.Tram: iconPath += "PDF_Transportation Icon_5.png"; break;
                }

                pdf.DrawPNG(iconPath, 275, top ? 13 : 407, 28, 28);
            }

            // Status
            pdf.FontSize = 10;
            int row0Y = top ? 347 : 751;
            int row1Y = top ? 360 : 764;
            int row2Y = top ? 372 : 776;
            // Ingame time frame.
            int hours = (int) (diaryEntry.info.playtime / 3600);
            int minutes = (int)((diaryEntry.info.playtime % 3600) / 60);
            int seconds = (int)(diaryEntry.info.playtime % 60);
            pdf.DrawText(string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds), 187, row0Y);
            // Money
            pdf.DrawText($"{journey.money} LUF", 187, row1Y);
            // Names
            for(int characterIndex = 0; characterIndex < diaryEntry.info.healthStates.Count; ++characterIndex)
            {
                DiaryHealthState healthState = diaryEntry.info.healthStates[characterIndex];
                string fullName = LocalizationManager.Instance.GetLocalizedString(healthState.character.fullName);
                int y = characterIndex == 0 ? row0Y : (characterIndex == 1 ? row1Y : row2Y);
                pdf.DrawText(fullName, 297, y);
                pdf.DrawText(healthState.healthState.ToString(), 392, y);
            }

            ++i;
            ++entryIndex;
        }
    }

    public void Generate(List<Journey> journeys, string username, float playtime)
    {
        // Responsible for generating the pdf based on the state.
        // Maybe outsource this to a thread, since it will take some time?
        int pageNumber = 0;
        string filePath = GenerateFilePath();
        UnityEngine.Debug.Log($"Generating pdf document at {filePath}");

        IPDFPlatform pdf = CreatePlatform(filePath);

        //pdf.LoadFont("AlegreyaSans-Black.ttf");
        //pdf.LoadFont("AlegreyaSans-Regular.ttf");
        //pdf.SetFont("AlegreyaSans-Regular.ttf");

        // Generate the map screenshot
        Texture2D mapScreenshot = LevelInstance.Instance ? LevelInstance.Instance.TakeMapScreenshot() : null;

        // TITLE PAGE
        DrawTitlePage(pdf, ref pageNumber, username, playtime);

        // JOURNEY
        DrawJourneyPage(pdf, mapScreenshot, ref pageNumber);

        // JOURNEYS
        DrawJourneys(pdf, journeys, ref pageNumber);

        // OUTRO
        pdf.AddPage();
        pdf.DrawPNG("PDF/PDF_Background_4.png", 0, 0, pdf.PageWidth, pdf.PageHeight);

        pdf.Close();
    }

    private string GenerateFilePath()
    {
#if DEBUG
        return Path.Combine(OutputPath, "MigrantsChronicles.pdf");
#else
        DateTime time = DateTime.Now;
        return Path.Combine(OutputPath, $"MigrantsChronicles-{time.Year}-{time.Month}-{time.Day}-{time.Hour}-{time.Minute}.pdf");
#endif
    }

    public void GenerateGeoJSON(List<Journey> journeys)
    {
        List<double[]> coordinatesList = NewGameManager.Instance.GeoJSONManager.GenerateRoute(journeys);

        var feature = new
        {
            type = "Feature",
            geometry = new
            {
                type = "LineString",
                coordinates = coordinatesList
            },
            properties = new { }
        };

        var featureCollection = new
        {
            type = "FeatureCollection",
            features = new[] { feature }
        };

        string json = JsonConvert.SerializeObject(featureCollection, Formatting.Indented);

        string outputPath = GenerateGeoJSONFilePath();
        IGeoJSONPlatform platform = CreateGeoJSONPlatform(outputPath);
        platform.WriteGeoJSON(json);
    }

    private string GenerateGeoJSONFilePath()
    {
#if DEBUG
        return Path.Combine(OutputPath, "MigrantsChronicles.geojson");
#else
        DateTime time = DateTime.Now;
        return Path.Combine(OutputPath, $"MigrantsChronicles-{time.Year}-{time.Month}-{time.Day}-{time.Hour}-{time.Minute}.geojson");
#endif
    }
}

