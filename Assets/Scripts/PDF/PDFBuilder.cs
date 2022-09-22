using UnityEngine;
using System.IO;
using System;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using PdfSharp.Pdf;
using sharpPDF;
using PdfSharp.Pdf.IO;
using System.Collections.Generic;
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
        sharpPdfPage = sharpPdfDocument.addPage(842, 595);
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
}
#elif UNITY_ANDROID || UNITY_IOS
class MobilePDFPlatform : IPDFPlatform
{
    public int FontSize { get; set; }

    private string outputPath;
    private TextSettings textSettings;
    private NativePDFNamespace.NativePDF pdf;

    public MobilePDFPlatform(string outputPath)
    {
        this.outputPath = outputPath;
        FontSize = 12;
        textSettings = new TextSettings(FontColor.Black, FontSize);
        pdf = new NativePDFNamespace.NativePDF();
        pdf.CreateDocument(595, 842);
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
}
#else
#   error PDF Platform not supported
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

    public void Generate()
    {
        // Responsible for generating the pdf based on the state.
        // Maybe outsource this to a thread, since it will take some time?
        string filePath = GenerateFilePath();
        Debug.Log($"Generating pdf document at {filePath}");

        IPDFPlatform pdf = CreatePlatform(filePath);
        pdf.FontSize = 12;
        pdf.AddPage();
        pdf.DrawText("Hello world", 0, 0);
        pdf.DrawPNG("Screenshot.png", 100, 100, 200, 200);
        pdf.AddPage();
        pdf.FontSize = 28;
        pdf.DrawText("Page 3", 0, 0);
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
