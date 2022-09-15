using System.IO;
using UnityEngine;
using static NativePDFNamespace.NativePDF;

public class Example : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    Texture2D testTexturePng;
#pragma warning restore 0649

    void Start()
    {
        // create new NativePDF object instance
        NativePDFNamespace.NativePDF nPDF = new NativePDFNamespace.NativePDF();

        // create a new pdf document (A4 is 595x842 pt [pt=standard pdf units])
        // 1 point = 1/72 inch, it means that a page width of 595 points is actually 595 / 72 = 8.27 inches, which is the standard width for DIN A4 page size.
        nPDF.CreateDocument(595, 842);
        nPDF.AddPage();

        // load CustomFontBold.ttf font from device path (using Android)
        // using iOS it will try to find the passed font name in XCode, ignoring the rest of the path (detailed instructions are provided in "readme.md")
        //Typeface typeFace = nPDF.LoadTypeface(Application.persistentDataPath + "/CustomFontBold.ttf");
        
        // create custom text settings and write to pdf document
        TextSettings textSettings = new TextSettings(FontColor.Red, 12);
        textSettings.Underline = true;
        nPDF.DrawText("Hello world", 0f, 0f, textSettings);

        // draw a PNG
        byte[] pngData = testTexturePng.EncodeToPNG();
        nPDF.DrawImage(pngData, pngData.Length, 100f, 100f, 200, 200);

        nPDF.AddPage();
        // edit text settings to draw a new text
        textSettings.FontSize = 28;
        textSettings.FontColor = FontColor.Yellow;
        textSettings.Underline = false;
        // set the font loaded previously
        //nPDF.SetTypeface(typeFace);
        nPDF.DrawText("Page 3", 0f, 0f, textSettings);

        // we are done editing the pdf, so we get its data
        byte[] pdfData = nPDF.GetDocumentData();

        // here you can write the data to disk, upload it, send it attached to an email
        File.WriteAllBytes(Application.persistentDataPath + "/ExamplePDF.pdf", pdfData);
    }
}
