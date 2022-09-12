using UnityEngine;
using System;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.IO;
using TMPro;
using UnityEngine.TextCore;
using sharpPDF;
using PdfSharp.Pdf.IO;

public class PDFBuilder
{
    private string OutputPath
    {
        get
        {
            return Application.persistentDataPath;
        }
    }

    public void Generate()
    {
        // Responsible for generating the pdf based on the state.
        // Maybe outsource this to a thread, since it will take some time?
        string filePath = GenerateFilePath();
        Debug.Log($"Generating pdf document at {filePath}");

        // First render the text into a new pdf document using sharpPDF.
        // sharpPDF is not able to render images, since System.Drawing.dll is not supported in Unity.
        HandleText(filePath);
        // Now open that newly created document and render all images using PDFsharp.
        // PDFsharp is not able to render text, since System.Drawing.dll is not supported in Unity.
        HandleImages(filePath);
    }

    private void HandleText(string filePath)
    {
        pdfDocument doc = new pdfDocument("Created with SharpPDF", "Me");
        pdfPage page = doc.addPage();
        page.addText("Hell oWorld", 200, 200, sharpPDF.Enumerators.predefinedFont.csHelvetica, 20);
        doc.createPDF(filePath);
    }

    private void HandleImages(string filePath)
    {
        PdfDocument doc = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
        PdfDocument outputDoc = new PdfDocument();
        outputDoc.Info.Title = "Created with PDFsharp";
        if(doc.PageCount > 0)
        {
            PdfPage page = doc.Pages[0];
            PdfPage newPage = outputDoc.AddPage(page);
            XGraphics gfx = XGraphics.FromPdfPage(newPage);

            XImage img = XImage.FromFile(Path.Combine(Application.streamingAssetsPath, "Screenshot.png"));
            gfx.DrawImage(img, 0, 0, 250, 140);
        }
        else
        {
            Debug.Log("Page count is 0");
        }

        doc.Close();
        outputDoc.Save(filePath);
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
