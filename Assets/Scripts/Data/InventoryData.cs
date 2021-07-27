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
    LuxembougShop,
    ParisShop,
    Katrin
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
    public const string CholeraMedicineKey = "cholera_medicine";
    public const string ChildBookKey = "child_book";
    public const string HorseOneKey = "horse1";
    public const string HorseTwoKey = "horse2";
    public const string LoliKey = "loli";


    public static IReadOnlyDictionary<int, InventoryModel> InventoryById = new ReadOnlyDictionary<int, InventoryModel>(
    new Dictionary<int, InventoryModel>()
    {
        { 
            1, 
            new InventoryModel
            {
                Name = BagKey,
                Price = 3,
                Volume = 1,
                Location = InventoryLocation.AtHome
            } 
        },
        {
            2,
            new InventoryModel
            {
                Name = BibleKey,
                Price = 1,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            3,
            new InventoryModel
            {
                Name = BirthCertificateKey,
                Price = 0,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            4,
            new InventoryModel
            {
                Name = BlanketKey,
                Price = 2,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            5,
            new InventoryModel
            {
                Name = EaringsKey,
                Price = 4,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            6,
            new InventoryModel
            {
                Name = FoodKey,
                Price = 9,
                Volume = 1,
                Location = InventoryLocation.ParisShop
            }
        },
        {
            7,
            new InventoryModel
            {
                Name = LocoKey,
                Price = 2,
                Volume = 2,
                Location = InventoryLocation.AtHome
            }
        },
        {
            8,
            new InventoryModel
            {
                Name = ChildBookKey,
                Price = 2,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            9,
            new InventoryModel
            {
                Name = NecklaceKey,
                Price = 6,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            10,
            new InventoryModel
            {
                Name = PictureKey,
                Price = 1,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            11,
            new InventoryModel
            {
                Name = SewingKey,
                Price = 1,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            12,
            new InventoryModel
            {
                Name = TeddyKey,
                Price = 2,
                Volume = 2,
                Location = InventoryLocation.AtHome
            }
        },
        {
            13,
            new InventoryModel
            {
                Name = SuitcaseKey,
                Price = 3,
                Volume = 1,
                Location = InventoryLocation.LuxembougShop
            }
        },
        {
            14,
            new InventoryModel
            {
                Name = MomCoatKey,
                Price = 3,
                Volume = 2,
                Location = InventoryLocation.LuxembougShop
            }
        },
        {
            15,
            new InventoryModel
            {
                Name = GirlCoatKey,
                Price = 2,
                Volume = 1,
                Location = InventoryLocation.LuxembougShop
            }
        },
        {
            16,
            new InventoryModel
            {
                Name = BoyCoatKey,
                Price = 2,
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
                Price = 2,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            19,
            new InventoryModel
            {
                Name = HorseTwoKey,
                Price = 2,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            20,
            new InventoryModel
            {
                Name = LoliKey,
                Price = 1,
                Volume = 1,
                Location = InventoryLocation.AtHome
            }
        },
        {
            21,
            new InventoryModel
            {
                Name = CholeraMedicineKey,
                Price = 5,
                Volume = 1,
                Location = InventoryLocation.Katrin
            }
        },
         {
            22,
            new InventoryModel
            {
                Name = CholeraMedicineKey,
                Price = 5,
                Volume = 1,
                Location = InventoryLocation.ParisShop
            }
        }
    });


    public static IReadOnlyDictionary<int, string> DescriptionById = new ReadOnlyDictionary<int, string>(
    new Dictionary<int, string>()
    {
        {
            1,
            $"Mrs Hutain gave it to you to bring it to her nephew Neckel in Luxembourg City. ({InventoryById[1].Price}\u20A3)"
        },
        {
            2,
            $"Your word is a lamp for my feet and a light on my path. ({InventoryById[2].Price}\u20A3)"
        },
        {
            3,
            $"Birth Certificates: 1 x Mattis Beffort, 1 x Mreis Beffort, 1 x Elis Beffort. ({InventoryById[3].Price}\u20A3)"
        },
        {
            4,
            $"Fluffy and thick blanket for cold nights. ({InventoryById[4].Price}\u20A3)"
        },
        {
            5,
            $"Earrings: a gift from J. B. Beffort to his daugther. ({InventoryById[5].Price}\u20A3)"
        },
        {
            6,
            $"Food. ({InventoryById[6].Price}\u20A3)"
        },
        {
            7,
            $"Mattis' toy locomotive. He loves locomotives, although he has never seen one from the inside. ({InventoryById[7].Price}\u20A3)"
        },
        {
            8,
            $"Children's book: a gift from J. B. Beffort to Mattis. ({InventoryById[8].Price}\u20A3)"
        },
        {
            9,
            $"Necklace: a gift from J. B. Beffort to Elis. ({InventoryById[9].Price}\u20A3)"
        },
        {
            10,
            $"A picture of the family Beffort from Feb. 1892. ({InventoryById[10].Price}\u20A3)"
        },
        {
            11,
            $"Used to repair clothing. ({InventoryById[11].Price}\u20A3)"
        },
        {
            12,
            $"Mreis’ Teddy Bear. She used to play with it more often when she was younger, but it still seems quite important to her. ({InventoryById[12].Price}\u20A3)"
        },
        {
            13,
            $"Unlocks new suitcase. ({InventoryById[13].Price}\u20A3)"
        },
        {
            14,
            $"A warm coat for a grown woman. ({InventoryById[14].Price}\u20A3)"
        },
        {
            15,
            $"A warm coat for a girl. ({InventoryById[15].Price}\u20A3)"
        },
        {
            16,
            $"A warm coat  for a boy. ({InventoryById[16].Price}\u20A3)"
        },
        {
            17,
            $"This medicine might help against the cold. ({InventoryById[17].Price}\u20A3)"
        },
        {
            18,
            $"Horse toy: Mattis' toy. ({InventoryById[18].Price}\u20A3)"
        },
        {
            19,
            $"Horse toy: Mreis' toy. ({InventoryById[19].Price}\u20A3)"
        },
        {
            20,
            $"Lollipop: a delicious, sweet delight. ({InventoryById[20].Price}\u20A3)"
        },
        {
            21,
            $"This medicine might help against cholera.  ({InventoryById[21].Price}\u20A3)"
        },
        {
            22,
            $"This medicine might help against cholera.  ({InventoryById[22].Price}\u20A3)"
        }
    });
}
