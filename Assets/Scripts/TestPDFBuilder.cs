using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPDFBuilder : MonoBehaviour
{
    public Texture2D fontTexture;
    public int fontCountX;
    public int fontCountY;

    // Start is called before the first frame update
    void Start()
    {
        PDFBuilder builder = new PDFBuilder();
        PDFBuilderOptions options = new PDFBuilderOptions { fontTexture = fontTexture, fontCountX = fontCountX, fontCountY = fontCountY };
        builder.Generate(options);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
