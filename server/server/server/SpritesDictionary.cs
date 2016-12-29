using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace server
{
    static class SpritesDictionary
    {
        public static Dictionary<Folders, Dictionary<States, ImageProcessor>> dictionary;

        public static void LoadSprites(ContentManager cm)
        {
            dictionary = new Dictionary<Folders, Dictionary<States, ImageProcessor>>();
            string path;

            // For each folder, add a dictionary of state to main dictionary
            foreach (Folders folder in Enum.GetValues(typeof(Folders)))
            {
                Dictionary<States, ImageProcessor> states_dictionary = new Dictionary<States, ImageProcessor>();

                foreach (States state in Enum.GetValues(typeof(States)))
                {
                    path = folder.ToString() + "/" + state.ToString();

                    if (File.Exists("Content/" + path + ".xnb"))
                        states_dictionary.Add(state, new ImageProcessor(cm, path));
                }

                dictionary.Add(folder, states_dictionary);
            }
        }
    }
}
