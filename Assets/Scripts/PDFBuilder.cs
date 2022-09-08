using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.IO;

[System.Serializable]
public class PerCharacterKerning
{
    /// Character
    public string First = "";
    /// Kerning, ex: 0.201
    public float Second;

    public PerCharacterKerning(string character, float kerning)
    {
        this.First = character;
        this.Second = kerning;
    }

    public PerCharacterKerning(char character, float kerning)
    {
        this.First = "" + character;
        this.Second = kerning;
    }

    public char GetChar()
    {
        return First[0];
    }

    public float GetKerningValue()
    {
        return Second;
    }
}

class TextToTextureRenderer
{
    private const int ASCII_START_OFFSET = 32;
    private Texture2D fontTexture;
    private int fontCountX;
    private int fontCountY;
    private float[] kerningValues;
    private bool supportSpecialCharacters;
    private Texture2D outputTexture;

    public TextToTextureRenderer(Texture2D fontTexture, int fontCountX, int fontCountY, PerCharacterKerning[] perCharacterKernings, bool supportSpecialCharacters,
        int textureWidth, int textureHeight)
    {
        this.fontTexture = fontTexture;
        this.fontCountX = fontCountX;
        this.fontCountY = fontCountY;
        this.kerningValues = perCharacterKernings != null ? GetCharacterKerningValuesFromPerCharacterKernings(perCharacterKernings) : null;
        this.supportSpecialCharacters = supportSpecialCharacters;
        outputTexture = CreateTexture(Color.clear, textureWidth, textureHeight);
    }

    public void DrawText(string text, int positionX, int positionY, float characterSize, float lineSpacing)
    {
        int fontGridCellWidth = (int)(fontTexture.width / fontCountX);
        int fontGridCellHeight = (int)(fontTexture.height / fontCountY);
        int fontItemWidth = (int)(fontGridCellWidth * characterSize);
        int fontItemHeight = (int)(fontGridCellHeight * characterSize);
        Vector2 charTexturePos;
        Color[] charPixels;
        float textPosX = positionX;
        float textPosY = positionY;
        float charKerning;
        bool nextCharacterSpecial;
        char letter;

        for(int n = 0; n < text.Length; ++n)
        {
            letter = text[n];
            nextCharacterSpecial = false;
            if(letter == '\\' && supportSpecialCharacters)
            {
                nextCharacterSpecial = true;
                if(n + 1 < text.Length)
                {
                    ++n;
                    letter = text[n];
                    if(letter == 'n' || letter == 'r')
                    {
                        textPosY -= fontItemHeight * lineSpacing;
                        textPosX = positionX;
                    }
                    else if(letter == 't')
                    {
                        textPosX += fontItemWidth * GetKerningValue(' ') * 4;
                    }
                    else if(letter == '\\')
                    {
                        nextCharacterSpecial = false;
                    }
                }
            }

            //if(!nextCharacterSpecial && font.HasCharacter(letter))
            if(!nextCharacterSpecial)
            {
                charTexturePos = GetCharacterGridPosition(letter);
                charTexturePos.x *= fontGridCellWidth;
                charTexturePos.y *= fontGridCellHeight;
                charPixels = fontTexture.GetPixels((int)charTexturePos.x, fontTexture.height - (int)charTexturePos.y - fontGridCellHeight, 
                    fontGridCellWidth, fontGridCellHeight);
                charPixels = ChangeDimensions(charPixels, fontGridCellWidth, fontGridCellHeight, fontItemWidth, fontItemHeight);

                AddPixelsToTextureIfClear(charPixels, (int)textPosX, (int)textPosY, fontItemWidth, fontItemHeight);
                charKerning = GetKerningValue(letter);
                textPosX += (fontItemWidth * charKerning);
            }
            else if(!nextCharacterSpecial)
            {
                Debug.Log("Letter not found: " + letter);
            }
        }
    }

    private void AddPixelsToTextureIfClear(Color[] newPixels, int positionX, int positionY, int width, int height)
    {
        int pixelCount = 0;
        Color[] curPixels;

        if(positionX + width < outputTexture.width && positionY + height < outputTexture.height)
        {
            curPixels = outputTexture.GetPixels(positionX, positionY, width, height);
            for(int y = 0; y < height; ++y)
            {
                for(int x = 0; x < width; ++x)
                {
                    pixelCount = x + (y * width);
                    if (curPixels[pixelCount] != Color.clear)
                    {
                        newPixels[pixelCount] = curPixels[pixelCount];
                    }
                }
            }

            outputTexture.SetPixels(positionX, positionY, width, height, newPixels);
        }
        else
        {
            Debug.Log("Letter falls outside bounds of texture: " + (positionX + width) + "/" + (positionY + height));
        }
    }

    private Color[] ChangeDimensions(Color[] originalColors, int originalWidth, int originalHeight, int newWidth, int newHeight)
    {
        Color[] newColors;
        Texture2D originalTexture;
        int pixelCount;
        float u;
        float v;

        if(originalWidth == newWidth && originalHeight == newHeight)
        {
            newColors = originalColors;
        }
        else
        {
            newColors = new Color[newWidth * newHeight];
            originalTexture = new Texture2D(originalWidth, originalHeight);
            originalTexture.SetPixels(originalColors);
            for (int y = 0; y < newHeight; ++y)
            {
                for (int x = 0; x < newWidth; ++x)
                {
                    pixelCount = x + (y * newWidth);
                    u = (float)x / newWidth;
                    v = (float)y / newHeight;
                    newColors[pixelCount] = originalTexture.GetPixelBilinear(u, v);
                }
            }
        }

        return newColors;
    }

    public Texture2D Apply()
    {
        return outputTexture; 
    }

    private Texture2D CreateTexture(Color color, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        int pixelCount = texture.width * texture.height;
        Color[] colors = new Color[pixelCount];
        for(int x = 0; x < pixelCount; ++x)
        {
            colors[x] = color;
        }

        texture.SetPixels(colors);
        return texture;
    }

    private Vector2 GetCharacterGridPosition(char c)
    {
        int codeOffset = c - ASCII_START_OFFSET;
        return new Vector2(codeOffset % fontCountX, (int)codeOffset / fontCountX);
    }

    private float[] GetCharacterKerningValuesFromPerCharacterKernings(PerCharacterKerning[] perCharacterKernings)
    {
        float[] perCharKerning = new float[128 - ASCII_START_OFFSET];
        int charCode;
        foreach(PerCharacterKerning kerning in perCharacterKernings)
        {
            if(kerning.First != "")
            {
                charCode = (int)kerning.GetChar();
                if(charCode >= 0 && charCode - ASCII_START_OFFSET < perCharKerning.Length)
                {
                    perCharKerning[charCode - ASCII_START_OFFSET] = kerning.GetKerningValue();
                }
            }
        }
        return perCharKerning;
    }

    private float GetKerningValue(char c)
    {
        if(kerningValues != null)
        {
            return kerningValues[((int) c) - ASCII_START_OFFSET];
        }

        return 0.201f;
    }
}

public class PDFBuilderOptions
{
    public Texture2D fontTexture;
    public int fontCountX;
    public int fontCountY;
    public PerCharacterKerning[] perCharacterKernings;
}

public class PDFBuilder
{
    private string OutputPath
    {
        get
        {
            return Application.persistentDataPath;
        }
    }

    public void Generate(PDFBuilderOptions options)
    {
        // Responsible for generating the pdf based on the state.
        // Maybe outsource this to a thread, since it will take some time?
        string filePath = GenerateFilePath();
        Debug.Log($"Generating pdf document at {filePath}");
        PdfDocument doc = new PdfDocument(filePath);
        doc.Info.Title = "Created with PDFSharp";
        PdfPage page = doc.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);

        // Render all images.
        XImage img = XImage.FromFile(Path.Combine(Application.streamingAssetsPath, "Screenshot.png"));
        gfx.DrawImage(img, 0, 0, 250, 140);

        // Render all text into a texture.
        TextToTextureRenderer textRenderer = new TextToTextureRenderer(options.fontTexture, options.fontCountX, options.fontCountY, options.perCharacterKernings,
            true, 500, 800);
        textRenderer.DrawText("Hello", 200, 300, 1.0f, 2.0f);
        Texture2D textTexture = textRenderer.Apply();
        XImage textImage = XImage.FromStream(new MemoryStream(textTexture.EncodeToPNG()));
        gfx.DrawImage(textImage, 0, 0, page.Width, page.Height);

        doc.Close();
    }

    private string GenerateFilePath()
    {
        DateTime time = DateTime.Now;
        return $"{OutputPath}/MigrantsChronicles-{time.Year}-{time.Month}-{time.Day}-{time.Hour}-{time.Minute}.pdf";
    }
}
