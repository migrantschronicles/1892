using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.IO;

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
        PdfDocument doc = new PdfDocument(filePath);
        doc.Info.Title = "Created with PDFSharp";
        PdfPage page = doc.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);

        XImage img = XImage.FromFile(Path.Combine(Application.streamingAssetsPath, "Screenshot.png"));
        gfx.DrawImage(img, 0, 0, 250, 140);

        doc.Close();
    }

    private string GenerateFilePath()
    {
        DateTime time = DateTime.Now;
        return $"{OutputPath}/MigrantsChronicles-{time.Year}-{time.Month}-{time.Day}-{time.Hour}-{time.Minute}.pdf";
    }
}
