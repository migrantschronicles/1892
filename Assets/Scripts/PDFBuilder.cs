using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.IO;
using TMPro;
using static UnityEditor.Progress;
using UnityEngine.TextCore;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;

/**
 * Creates a texture and draws text into that texture.
 */
class TextToTextureRenderer
{
    private TMP_FontAsset font;
    private Texture2D fontTexture;
    private bool supportSpecialCharacters;
    private Texture2D outputTexture;
    private int spacingX = 2;

    /**
     * @param font The TextMeshPro-font
     * @param supportSpecialCharacters True if special characters are recognized and handled, if escaped (e.g. "Hello\\nWorld").
     *          Normal special characters are always handled (e.g. "Hello\nWorld").
     * @param textureWidth The width of the texture to create.
     * @param textureHeight The height of the texture to create.
     */
    public TextToTextureRenderer(TMP_FontAsset font, bool supportSpecialCharacters, int textureWidth, int textureHeight)
    {
        this.font = font;
        this.fontTexture = CopyTexture(font.atlasTexture);
        this.supportSpecialCharacters = supportSpecialCharacters;
        outputTexture = CreateTexture(Color.clear, textureWidth, textureHeight);
    }

    private Texture2D CopyTexture(Texture2D original)
    {
        return CopyTexture(original, original.width, original.height);
    }

    private Texture2D CopyTexture(Texture2D original, int newWidth, int newHeight)
    {
        RenderTexture rtex = RenderTexture.GetTemporary(
            newWidth,
            newHeight,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rtex;
        Graphics.Blit(original, rtex);
        Texture2D readable = new Texture2D(newWidth, newHeight);
        readable.ReadPixels(new Rect(0, 0, rtex.width, rtex.height), 0, 0);
        readable.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rtex);
        return readable;
    }

    /**
     * Draws text onto the texture.
     * @param text The text
     * @param positionX The x position to draw at.
     * @param positionY The y position to draw at.
     * @param fontSize The font size
     * @param lineSpacing A number equivalent to the multiple of the height of the letter 'A' to offset lines vertically (if there are newlines).
     */
    public void DrawText(string text, int positionX, int positionY, int fontSize, float lineSpacing)
    {
        // Get a reference character to compute scaling. Characters have to be scaled to the correct font size.
        TMP_Character referenceCharacter = font.characterLookupTable['A'];
        float scaleFactor = (float) fontSize / referenceCharacter.glyph.glyphRect.height;

        int textPosX = positionX;
        int textPosY = positionY;
        for(int i = 0; i < text.Length; ++i)
        {
            char letter = text[i];
            bool nextCharacterSpecial = false;
            // Handle special characters.
            if(letter == '\\' && supportSpecialCharacters)
            {
                nextCharacterSpecial = true;
                if (i + 1 < text.Length)
                {
                    ++i;
                    letter = text[i];
                    if (letter == 'n' || letter == 'r')
                    {
                        TMP_Character c = font.characterLookupTable['A'];
                        textPosY -= (int) (c.glyph.glyphRect.height * scaleFactor * lineSpacing);
                        textPosX = positionX;
                    }
                    else if (letter == 't')
                    {
                        TMP_Character c = font.characterLookupTable[' '];
                        textPosX += c.glyph.glyphRect.width * 4;
                    }
                    else if (letter == '\\')
                    {
                        nextCharacterSpecial = false;
                    }
                }
            }
            else if (letter == '\n')
            {
                TMP_Character c = font.characterLookupTable['A'];
                textPosY -= (int)(c.glyph.glyphRect.height * scaleFactor * lineSpacing);
                textPosX = positionX;
                nextCharacterSpecial = true;
            }
            else if (letter == '\t')
            {
                TMP_Character c = font.characterLookupTable['A'];
                textPosX += (int) (c.glyph.glyphRect.width * scaleFactor * 4);
                nextCharacterSpecial = true;
            }

            if (!nextCharacterSpecial)
            {
                if(font.HasCharacter(letter))
                {
                    // Draw the character.
                    TMP_Character character = font.characterLookupTable[letter];
                    int characterWidth = (int)(character.glyph.glyphRect.width * scaleFactor);
                    int characterHeight = (int)(character.glyph.glyphRect.height * scaleFactor);
                    DrawCharacter(character, textPosX, textPosY, characterWidth, characterHeight);
                    textPosX += characterWidth + spacingX;
                }
                else
                {
                    Debug.Log($"Letter not found {letter}");
                }
            }
        }
    }

    /**
     * Draws a character onto the texture.
     */
    private void DrawCharacter(TMP_Character character, int x, int y, int width, int height)
    {
        GlyphRect glyphRect = character.glyph.glyphRect;
        Color[] colors = fontTexture.GetPixels(glyphRect.x, glyphRect.y, glyphRect.width, glyphRect.height);
        colors = ChangeDimension(colors, glyphRect.width, glyphRect.height, width, height);
        TryDrawPixels(colors, x, y, width, height);
    }

    /**
     * Draws colors onto the texture, but only to the texels that are not filled yet.
     */
    private void TryDrawPixels(Color[] newPixels, int x, int y, int width, int height)
    {
        if(x + width < outputTexture.width && y + height < outputTexture.height)
        {
            Color[] curPixels = outputTexture.GetPixels(x, y, width, height);
            for(int curY = 0; curY < height; ++curY)
            {
                for(int curX = 0; curX < width; ++curX)
                {
                    int index = curX + (curY * width);
                    if (curPixels[index] != Color.clear)
                    {
                        newPixels[index] = curPixels[index];
                    }
                }
            }

            outputTexture.SetPixels(x, y, width, height, newPixels);
        }
        else
        {
            Debug.Log("Letter falls outside bounds of texture: " + (x + width) + "/" + (y + height));
        }
    }

    /**
     * Resize an array of colors to a new width / height.
     */
    private Color[] ChangeDimension(Color[] originalColors, int originalWidth, int originalHeight, int newWidth, int newHeight)
    {
        Color[] newColors;
        Texture2D originalTexture;

        if(originalWidth == newWidth && originalHeight == newHeight)
        {
            newColors = originalColors;
        }
        else
        {
            originalTexture = new Texture2D(originalWidth, originalHeight, TextureFormat.ARGB32, 0, true);
            originalTexture.SetPixels(originalColors);
            // Needs to be applied, since ScaleTexture (Graphics.Blit) works on GPU
            originalTexture.Apply();
            Texture2D scaledTexture = CopyTexture(originalTexture, newWidth, newHeight);
            newColors = scaledTexture.GetPixels();
        }

        return newColors;
    }

    /**
     * Apply all text writing and retrieve the final texture.
     */
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
}

public class PDFBuilderOptions
{
    public TMP_FontAsset font;
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
        TextToTextureRenderer textRenderer = new TextToTextureRenderer(options.font, true, 500, 800);
        textRenderer.DrawText("Hel\tlo\nWorld", 200, 300, 40, 2.0f);
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
