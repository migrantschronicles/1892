using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using static NativePDFNamespace.NativePDF;
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
        sharpPdfDocument = new pdfDocument("Migrants Chronicles", "TH Köln");
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
                XImage img = XImage.FromFile(Path.Combine(Application.streamingAssetsPath, data.relativePath));
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
    }

    public void DrawPNG(string relativePath, int x, int y, int width, int height)
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
        UnityWebRequest loadingRequest = UnityWebRequest.Get(fullPath);
        loadingRequest.SendWebRequest();
        while (!loadingRequest.isDone)
        {
            // Wait
        }

        if(loadingRequest.result == UnityWebRequest.Result.Success)
        {
            pdf.DrawImage(loadingRequest.downloadHandler.data, loadingRequest.downloadHandler.data.Length, x, y, width, height);
        }
        else
        {
            Debug.Log("Failed to load image " + fullPath);
        }
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

    private int pageNumber = 0;

    private IPDFPlatform CreatePlatform(string outputPath)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        return new WinPDFPlatform(outputPath);
#elif UNITY_ANDROID || UNITY_IOS
        return new MobilePDFPlatform(outputPath);
#endif
    }

    private void DrawPageNumber(IPDFPlatform pdf, int pageNumber)
    {
        int oldFontSize = pdf.FontSize;
        pdf.FontSize = 12;
        pdf.DrawText(pageNumber.ToString(), 295, 811);
        pdf.FontSize = oldFontSize;
    }

    private void DrawTitlePage(IPDFPlatform pdf)
    {
        pdf.AddPage();
        pdf.DrawPNG("PDF/PDF_Background_1.png", 0, 0, pdf.PageWidth, pdf.PageHeight);
        pdf.FontSize = 15;
        // Date
        pdf.DrawText("13/10/2022", 305, 343);
        // Player name
        pdf.DrawText("Alina Menten", 305, 367);
        //pdf.SetFont("AlegreyaSans-Black.ttf");
        // Overall Playtime
        pdf.DrawText("02h 28m", 305, 392);
        // Character choice
        pdf.DrawPNG("Screenshot.png", 190, 498, 214, 250);
        // Page Number
        DrawPageNumber(pdf, ++pageNumber);
    }

    private void DrawJourneyPage(IPDFPlatform pdf)
    {
        pdf.AddPage();
        pdf.FontSize = 12;
        pdf.DrawPNG("PDF/PDF_Background_2.png", 0, 0, pdf.PageWidth, pdf.PageHeight);
        // Screenshot
        pdf.DrawPNG("Screenshot.png", 61, 102, 473, 295);
        // Official documents
        pdf.DrawText("10", 408, 534);
        // Personal items
        pdf.DrawText("10", 408, 560);
        // Religious items
        pdf.DrawText("10", 408, 586);
        // Clothing
        pdf.DrawText("10", 408, 613);
        // Food
        pdf.DrawText("10", 408, 639);
        // Medicine
        pdf.DrawText("10", 408, 666);
        // Items of worth
        pdf.DrawText("10", 408, 692);
        // Souvenirs
        pdf.DrawText("10", 408, 718);
        // Heirlooms
        pdf.DrawText("10", 408, 745);
        // Page Number
        DrawPageNumber(pdf, ++pageNumber);
    }

    private void DrawJourneys(IPDFPlatform pdf)
    {
        // Only test data
        Journey[] journeys = new Journey[] {
            new Journey { destination = "Luxembourg", method = TransporationMethod.Train, money = 82 },
            new Journey { destination = "Paris", method = TransporationMethod.Foot, money = 2 },
            new Journey { destination = "Hamburg", method = TransporationMethod.Ship, money = 112 },
            new Journey { destination = "Antwerp", method = TransporationMethod.Carriage, money = 10293 },
            new Journey { destination = "London", method = TransporationMethod.Train, money = 2834 }
        };

        for (int i = 0; i < journeys.Length; ++i)
        {
            bool top = i % 2 == 0;
            if(top)
            {
                pdf.AddPage();
                pdf.DrawPNG(i == 0 ? "PDF/PDF_Background_3.png" : "PDF/PDF_Background_3.2.png", 0, 0, pdf.PageWidth, pdf.PageHeight);
                DrawPageNumber(pdf, ++pageNumber);
            }

            // City Name
            pdf.FontSize = 15;
            string cityName = $"{i + 1}. {journeys[i].destination}";
            pdf.DrawText(cityName, 94, top ? 56 : 460);

            // Screenshot
            pdf.DrawPNG("Screenshot.png", 94, top ? 82 : 486, 407, 255);

            // Transport icon
            if(i > 0)
            {
                string iconPath = "PDF/Methods/";
                switch(journeys[i].method)
                {
                    case TransporationMethod.Foot: iconPath += "PDF_Transportation Icon_6.png"; break;
                    case TransporationMethod.Train: iconPath += "PDF_Transportation Icon_7.png"; break;
                    case TransporationMethod.Ship: iconPath += "PDF_Transportation Icon_2.png"; break;
                    case TransporationMethod.Carriage: iconPath += "PDF_Transportation Icon_4.png"; break;
                }

                pdf.DrawPNG(iconPath, 275, top ? 13 : 407, 28, 28);
            }

            // Status
            pdf.FontSize = 10;
            // Ingame time frame.
            pdf.DrawText("02:03:02", 305, top ? 347 : 751);
            // Money
            pdf.DrawText($"{journeys[i].money} LUF", 305, top ? 361 : 765);
            // Health
            pdf.DrawText("Healthy", 305, top ? 372 : 776);
        }
    }

    public void Generate()
    {
        // Responsible for generating the pdf based on the state.
        // Maybe outsource this to a thread, since it will take some time?
        string filePath = GenerateFilePath();
        Debug.Log($"Generating pdf document at {filePath}");

        IPDFPlatform pdf = CreatePlatform(filePath);
        //pdf.LoadFont("AlegreyaSans-Black.ttf");
        //pdf.LoadFont("AlegreyaSans-Regular.ttf");
        //pdf.SetFont("AlegreyaSans-Regular.ttf");

        // TITLE PAGE
        DrawTitlePage(pdf);

        // JOURNEY
        DrawJourneyPage(pdf);

        // JOURNEYS
        DrawJourneys(pdf);

        // OUTRO
        pdf.AddPage();
        pdf.DrawPNG("PDF/PDF_Background_4.png", 0, 0, pdf.PageWidth, pdf.PageHeight);

        pdf.Close();
    }

    private string GenerateFilePath()
    {
#if DEBUG
        return $"{OutputPath}/MigrantsChronicles.pdf";
#else
        DateTime time = DateTime.Now;
        return $"{OutputPath}/MigrantsChronicles-{time.Year}-{time.Month}-{time.Day}-{time.Hour}-{time.Minute}.pdf";
#endif
    }
}
