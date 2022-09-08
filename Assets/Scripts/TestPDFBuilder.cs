using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestPDFBuilder : MonoBehaviour
{
    public TMP_FontAsset font;

    // Start is called before the first frame update
    void Start()
    {
        PDFBuilder builder = new PDFBuilder();
        PDFBuilderOptions options = new PDFBuilderOptions { font = font };
        builder.Generate(options);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
