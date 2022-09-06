using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using sharpPDF;
using System;

public class PDFBuilder
{
    private string Path
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
        pdfDocument doc = new pdfDocument("TestDoc", "Me");
        pdfPage page = doc.addPage();
        page.addText("Hello World!", 200, 340, sharpPDF.Enumerators.predefinedFont.csHelvetica, 20);
        string filePath = GenerateFilePath();
        Debug.Log($"Generating pdf document at {filePath}");
        doc.createPDF(filePath);
    }

    private string GenerateFilePath()
    {
        DateTime time = DateTime.Now;
        return $"{Path}/MigrantsChronicles-{time.Year}-{time.Month}-{time.Day}-{time.Hour}-{time.Minute}.pdf";
    }
}
