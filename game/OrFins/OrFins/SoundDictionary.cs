using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Timers;

namespace OrFins
{
    enum BGM
    {
        Menu,
        City_Center,
        TrainingGrounds,
        Forest,
        Dungeon
    }

    enum SoundEffects
    {
        hit1, hit2, hit3,
        attack1, attack2, attack3,
        Death,
        HeartBeat, Jump,
        PickUp, Teleport,
        pageFlip,
        ballLaunch, ballHit,
        bananaThrow, bananaHit,
        DrakeAttacks, FireballHits,
        MonkeyDies, WormDies, TeddyDies, DrakeDies
    }

    static class SoundDictionary
    {
        #region Data
        private static Dictionary<BGM, Song> BGMdictionary;
        private static Dictionary<SoundEffects, SoundEffect> SEdictionary;
        private static Song bgm;
        private static Timer bgm_timer;
        public static Button soundButton { get; private set; }
        public static bool SoundAvailable { private get; set; }
        #endregion

        #region Construction
        public static void Initialize(ContentManager contentManager, string BGM_DIR, string SE_DIR, Button soundButton)
        {
            BGMdictionary = new Dictionary<BGM, Song>();
            SEdictionary = new Dictionary<SoundEffects, SoundEffect>();

            foreach (BGM sound in Enum.GetValues(typeof(BGM)))
            {
                BGMdictionary.Add(sound, contentManager.Load<Song>(BGM_DIR + "/" + sound.ToString()));
            }

            foreach (SoundEffects se in Enum.GetValues(typeof(SoundEffects)))
            {
                SEdictionary.Add(se, contentManager.Load<SoundEffect>(SE_DIR + "/" + se.ToString()));
            }

            SoundDictionary.soundButton = soundButton;

            bgm_timer = new Timer(1);
            bgm_timer.Enabled = false;
            bgm_timer.Elapsed += OnTimedEvent;
        }
        #endregion

        #region Public functions
        public static void Play(BGM bgm)
        {
            if (SoundAvailable)
            {
                MediaPlayer.Play(BGMdictionary[bgm]);
            }
        }
        public static void Play(SoundEffects se)
        {
            if (SoundAvailable)
            {
                SEdictionary[se].Play();
            }
        }

        public static void PlayHit()
        {
            if (SoundAvailable)
            {
                SEdictionary[(SoundEffects)new Random().Next((int)SoundEffects.hit1, (int)SoundEffects.hit3)].Play();
            }
        }
        public static void PlayAttack()
        {
            if (SoundAvailable)
            {
                SEdictionary[(SoundEffects)new Random().Next((int)SoundEffects.attack1, (int)SoundEffects.attack3)].Play();
            }
        }

        public static Song Get(BGM bgm)
        {
            return BGMdictionary[bgm];
        }
        public static SoundEffect Get(SoundEffects se)
        {
            return SEdictionary[se];
        }

        public static void SetBackGroundMusic(BGM new_bgm)
        {
            bgm = BGMdictionary[new_bgm];
            bgm_timer.Interval = bgm.Duration.TotalMilliseconds;

            bgm_timer.Enabled = true;
            bgm_timer.Start();

            if (SoundAvailable && MediaPlayer.State != MediaState.Paused)
            {
                MediaPlayer.Play(bgm);
            }
        }

        public static void ToggleSound()
        {
            SoundAvailable = !SoundAvailable;

            if (!SoundAvailable)
            {
                MediaPlayer.Pause();
                soundButton.SetFolder(Folders.SoundOff);
            }
            else
            {
                MediaPlayer.Resume();
                soundButton.SetFolder(Folders.SoundOn);
            }
        }
        #endregion

        #region Private functions
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (SoundAvailable)
            {
                MediaPlayer.Play(bgm);
            }
        }
        #endregion
    }
}
