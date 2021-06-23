using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WPM;

public class GlobeDesigner : IGlobeDesigner
{
    private const string SelectionTextureText = "Selection";
    private const string TextureFolderPath = "Globe/";

    private const string Germany = "Germany";
    private const string France = "France";
    private const string Luxembourg = "Luxembourg";
    private const string Netherlands = "Netherlands";
    private const string Belgium = "Belgium";

    private const string Common = "Common";

    private WorldMapGlobe map;

    private Dictionary<string, Texture2D> textureByCountry = new Dictionary<string, Texture2D>();
    private Dictionary<string, Texture2D> selectionTextureByCountry = new Dictionary<string, Texture2D>();

    private bool isTexturesLoaded = false;

    public GlobeDesigner(WorldMapGlobe map)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));

        this.map = map;
    }

    public void AssignTextures()
    {
        LoadTextures();

        foreach (var country in map.countries)
        {
            map.decorator.SetCountryDecorator(0, country.name, new CountryDecorator
            {
                labelVisible = false,
                isColorized = true,
                texture = textureByCountry.ContainsKey(country.name) ? textureByCountry[country.name] : textureByCountry[Common]
            });
        }
    }

    public void UpdateSelectionTexture(string counrtyName, bool isSelected = false)
    {
        if(isSelected)
        {
            map.decorator.SetCountryDecorator(0, counrtyName, new CountryDecorator
            {
                labelVisible = false,
                isColorized = true,
                texture = selectionTextureByCountry.ContainsKey(counrtyName) ? selectionTextureByCountry[counrtyName] : selectionTextureByCountry[Common]
            });
        }
        else
        {
            map.decorator.SetCountryDecorator(0, counrtyName, new CountryDecorator
            {
                labelVisible = false,
                isColorized = true,
                texture = textureByCountry.ContainsKey(counrtyName) ? textureByCountry[counrtyName] : textureByCountry[Common]
            });
        }
    }

    private void LoadTextures()
    {
        if (isTexturesLoaded)
        {
            return;
        }

        try
        {
            textureByCountry.Add(Germany, Resources.Load(TextureFolderPath + Germany) as Texture2D);
            textureByCountry.Add(France, Resources.Load(TextureFolderPath + France) as Texture2D);
            textureByCountry.Add(Luxembourg, Resources.Load(TextureFolderPath + Luxembourg) as Texture2D);
            textureByCountry.Add(Netherlands, Resources.Load(TextureFolderPath + Netherlands) as Texture2D);
            textureByCountry.Add(Belgium, Resources.Load(TextureFolderPath + Belgium) as Texture2D);
            textureByCountry.Add(Common, Resources.Load(TextureFolderPath + Common) as Texture2D);

            selectionTextureByCountry.Add(Germany, Resources.Load(TextureFolderPath + Germany + SelectionTextureText) as Texture2D);
            selectionTextureByCountry.Add(France, Resources.Load(TextureFolderPath + France + SelectionTextureText) as Texture2D);
            selectionTextureByCountry.Add(Luxembourg, Resources.Load(TextureFolderPath + Luxembourg + SelectionTextureText) as Texture2D);
            selectionTextureByCountry.Add(Netherlands, Resources.Load(TextureFolderPath + Netherlands + SelectionTextureText) as Texture2D);
            selectionTextureByCountry.Add(Belgium, Resources.Load(TextureFolderPath + Belgium + SelectionTextureText) as Texture2D);
            selectionTextureByCountry.Add(Common, Resources.Load(TextureFolderPath + Common + SelectionTextureText) as Texture2D);
        }
        catch
        {
            throw new Exception("Failed to load textures.");
        }

        isTexturesLoaded = true;
    }
}
