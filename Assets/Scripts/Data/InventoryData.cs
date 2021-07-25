using System.Collections.Generic;
using System.Collections.ObjectModel;

public class InventoryModel
{
    public string Name { get; set; }
    public int Price { get; set; }
    public int Volume { get; set; }
    public InventoryLocation Location { get; set; }
}

public enum InventoryLocation
{
    AtHome,
    OnHand,
    LuxembougShop
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
    public const string SuitcaseKey = "suitcase";
    public const string MomCoatKey = "mom_coat";
    public const string GirlCoatKey = "girl_coat";
    public const string BoyCoatKey = "boy_coat";
    public const string MedicineKey = "medicine";
    public const string ChildBookKey = "child_book";
    public const string HorseOneKey = "horse1";
    public const string HorseTwoKey = "horse2";
    public const string LoliKey = "loli";

    public static IReadOnlyDictionary<int, string> DescriptionById = new ReadOnlyDictionary<int, string>(
    new Dictionary<int, string>()
    {
        {
            1,
            ""
        },
        {
            2,
            ""
        },
        {
            3,
            ""
        },
        {
            4,
            ""
        },
        {
            5,
            ""
        },
        //{
        //    6,
        //    ""
        //},
        {
            7,
            ""
        },
        {
            8,
            ""
        },
        {
            9,
            ""
        },
        {
            10,
            ""
        },
        {
            11,
            ""
        },
        {
            12,
            ""
        },
        {
            13,
            ""
        },
        {
            14,
            ""
        },
        {
            15,
            ""
        },
        {
            16,
            ""
        },
        {
            17,
            ""
        },
        {
            18,
            ""
        },
        {
            19,
            ""
        },
        {
            20,
            ""
        }
    });


    public static IReadOnlyDictionary<int, InventoryModel> InventoryById = new ReadOnlyDictionary<int, InventoryModel>(
    new Dictionary<int, InventoryModel>()
    {
        { 
            1, 
            new InventoryModel
            {
                Name = BagKey,
                Price = 5,
                Volume = 1,
                Location = InventoryLocation.AtHome
            } 
        },
        {
            2,
            new InventoryModel
            {
                Name = BibleKey,
                Price = 5,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            3,
            new InventoryModel
            {
                Name = BirthCertificateKey,
                Price = 5,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            4,
            new InventoryModel
            {
                Name = BlanketKey,
                Price = 5,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            5,
            new InventoryModel
            {
                Name = EaringsKey,
                Price = 5,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        //{
        //    6,
        //    new InventoryModel
        //    {
        //        Name = FoodKey,
        //        Price = 5,
        //        Volume = 1,
        //        Location = InventoryLocation.AtHome
        //    }
        //},
        {
            7,
            new InventoryModel
            {
                Name = LocoKey,
                Price = 5,
                Volume = 2,
                Location = InventoryLocation.AtHome
            }
        },
        {
            8,
            new InventoryModel
            {
                Name = ChildBookKey,
                Price = 5,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            9,
            new InventoryModel
            {
                Name = NecklaceKey,
                Price = 5,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            10,
            new InventoryModel
            {
                Name = PictureKey,
                Price = 5,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            11,
            new InventoryModel
            {
                Name = SewingKey,
                Price = 5,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            12,
            new InventoryModel
            {
                Name = TeddyKey,
                Price = 5,
                Volume = 2,
                Location = InventoryLocation.AtHome
            }
        },
        {
            13,
            new InventoryModel
            {
                Name = SuitcaseKey,
                Price = 5,
                Volume = 1,
                Location = InventoryLocation.LuxembougShop
            }
        },
        {
            14,
            new InventoryModel
            {
                Name = MomCoatKey,
                Price = 5,
                Volume = 2,
                Location = InventoryLocation.LuxembougShop
            }
        },
        {
            15,
            new InventoryModel
            {
                Name = GirlCoatKey,
                Price = 5,
                Volume = 1,
                Location = InventoryLocation.LuxembougShop
            }
        },
        {
            16,
            new InventoryModel
            {
                Name = BoyCoatKey,
                Price = 5,
                Volume = 1,
                Location = InventoryLocation.LuxembougShop
            }
        },
        {
            17,
            new InventoryModel
            {
                Name = MedicineKey,
                Price = 5,
                Volume = 1,
                Location = InventoryLocation.LuxembougShop
            }
        },
        {
            18,
            new InventoryModel
            {
                Name = HorseOneKey,
                Price = 5,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            19,
            new InventoryModel
            {
                Name = HorseTwoKey,
                Price = 5,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            20,
            new InventoryModel
            {
                Name = LoliKey,
                Price = 5,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        }
    });
}
