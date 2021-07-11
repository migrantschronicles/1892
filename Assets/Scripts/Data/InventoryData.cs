using System.Collections.Generic;
using System.Collections.ObjectModel;

public class InventoryModel
{
    public string Name { get; set; }
    public int Price { get; set; }
    public int Volume { get; set; }
}

public static class InventoryData
{
    public const string BagKey = "bag";
    public const string BibleKey = "bible";
    public const string BirthCertificateKey = "birth_certificate";
    public const string BlanketKey = "blanket";
    public const string EaringsKey = "earings";
    public const string FoodKey = "food";
    public const string LocoKey = "loco";
    public const string MoneyKey = "money";
    public const string NecklaceKey = "necklace";
    public const string PictureKey = "picture";
    public const string SewingKey = "sewing";
    public const string TeddyKey = "teddy";

    public static IReadOnlyDictionary<int, InventoryModel> InventoryById = new ReadOnlyDictionary<int, InventoryModel>(
    new Dictionary<int, InventoryModel>()
    {
        { 
            1, 
            new InventoryModel
            {
                Name = BagKey,
                Price = 5,
                Volume = 1
            } 
        },
        {
            2,
            new InventoryModel
            {
                Name = BibleKey,
                Price = 5,
                Volume = 1
            }
        },
        {
            3,
            new InventoryModel
            {
                Name = BirthCertificateKey,
                Price = 5,
                Volume = 1
            }
        },
        {
            4,
            new InventoryModel
            {
                Name = BlanketKey,
                Price = 5,
                Volume = 1
            }
        },
        {
            5,
            new InventoryModel
            {
                Name = EaringsKey,
                Price = 5,
                Volume = 1
            }
        },
        //{
        //    6,
        //    new InventoryModel
        //    {
        //        Name = FoodKey,
        //        Price = 5,
        //        Volume = 1
        //    }
        //},
        {
            7,
            new InventoryModel
            {
                Name = LocoKey,
                Price = 5,
                Volume = 1
            }
        },
        //{
        //    8,
        //    new InventoryModel
        //    {
        //        Name = MoneyKey,
        //        Price = 5,
        //        Volume = 1
        //    }
        //},
        {
            9,
            new InventoryModel
            {
                Name = NecklaceKey,
                Price = 5,
                Volume = 1
            }
        },
        {
            10,
            new InventoryModel
            {
                Name = PictureKey,
                Price = 5,
                Volume = 1
            }
        },
        {
            11,
            new InventoryModel
            {
                Name = SewingKey,
                Price = 5,
                Volume = 1
            }
        },
        {
            12,
            new InventoryModel
            {
                Name = TeddyKey,
                Price = 5,
                Volume = 2
            }
        }
    });
}
