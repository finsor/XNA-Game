using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace OrFins
{
    static class ClothingDictionary
    {
        public static Dictionary<ClothingType, List<ClothingData>> dictionary;

        public static void Initialize(SpriteBatch spriteBatch, params string[] clothing_data_files)
        {
            StreamReader reader;

            // Creating the dictionary itself
            dictionary = new Dictionary<ClothingType, List<ClothingData>>();
            foreach (ClothingType clothingType in Enum.GetValues(typeof(ClothingType)))
            {
                dictionary.Add(clothingType, new List<ClothingData>());
            }

            // Adding clothing data to the dictionary
            Folders folder;
            ClothingType type;
            int minLevel;
            int defence;
            int strength;
            int buying_price;
            int selling_price;
            AES_Encryption AES = new AES_Encryption();

            foreach (string file_path in clothing_data_files)
            {
                using (reader = new StreamReader(file_path))
                {
                    while (AES.Read(reader) != null)
                    {
                        folder = (Folders)Enum.Parse(typeof(Folders), AES.Read(reader));
                        type = (ClothingType)Enum.Parse(typeof(ClothingType), AES.Read(reader));
                        minLevel = int.Parse(AES.Read(reader));
                        defence = int.Parse(AES.Read(reader));
                        strength = int.Parse(AES.Read(reader));
                        buying_price = int.Parse(AES.Read(reader));
                        selling_price = buying_price / 2;

                        dictionary[type].Add(new ClothingData(spriteBatch, folder, type, minLevel, defence, strength, buying_price, selling_price));
                    }
                }
            }
        }

        public static Clothing GetClothing(ClothingType type, Folders folder)
        {
            ClothingData data = dictionary[type].Find(item => item.GetFolder() == folder);

            if(data != null)
                return (data.GetClothing());

            return (null);
        }
    }
}
