using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Media;

namespace OrFins
{
    enum States
    {
        stand, alert, walk, prone, jump, onRope, die,
        stab1, stab2, stab3, swing1, swing2, swing3,
        mobSkill,
        launched, hit,
        back, middle, front,
        bottom, top, hover,
        hud, hpBar, mpBar, xpBar, levels,
        marco, chiang,
        equipment,
        money,
        notClicked, buttonHover, clicked
    };

    enum Folders
    {
        // Player
        player, player_bald,
        // Clothing
        Coif, Jouster, Crusader, Walker, Sparta,
        Hwarang, Scorpio, Corporal, Jangoon, Platine, EagleEye,
        Corp, Jango, Martial, Trixter, Plat, Scorp,
        Moss, Trigger,
        Sword, Dragon,
        // Monsters
        teddy, worm, monkey, Drake,
        // BG
        greenBackGround, cityBackGround, forestBackGround, lavaBackGround,
        // Skills
        Ball, Banana, Fireball,
        rope, portal,
        hud,
        npc, regularButton, SoundOn, SoundOff,
        shop,
        pickup
    };

    static class SpritesDictionary
    {
        // A dictionary to access ImageProcessor pages using a folder and a state.
        public static Dictionary<Folders, Dictionary<States, ImageProcessor>> dictionary;

        public static void LoadSprites(ContentManager cm, GraphicsDevice graphicsDevice)
        {
            dictionary = new Dictionary<Folders, Dictionary<States, ImageProcessor>>();
            string path;

            // For each folder, add a dictionary of states to main dictionary
            foreach (Folders folder in Enum.GetValues(typeof(Folders)))
            {
                Dictionary<States, ImageProcessor> states_dictionary = new Dictionary<States, ImageProcessor>();

                // For each state, create an ImageProcessor instance and add to the folder's dictionary
                foreach (States state in Enum.GetValues(typeof(States)))
                {
                    path = folder.ToString() + "/" + state.ToString();

                    if (File.Exists("Content/" + path + ".xnb"))
                        states_dictionary.Add(state, new ImageProcessor(cm, path, graphicsDevice));
                }

                dictionary.Add(folder, states_dictionary);
            }
        }
    }
}
