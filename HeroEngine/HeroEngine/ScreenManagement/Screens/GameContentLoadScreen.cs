﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Threading;
using System.IO;
using HeroEngine.CoreGame;
using HeroEngine.LevelEditing;
using HeroEngineShared;

namespace HeroEngine.ScreenManagement.Screens
{
    class GameContentLoadScreen : ScreenComponent
    {
        Texture2D tex_loading;
        SpriteFont font;
        static ContentManager content;
        static ContentManager loaded_content;
        Thread l_thread = new Thread(LoadingThread);
        static string LoadingFileName = "Currently Loading : Zilch!";
        bool loading = false;
        static bool loaded = false;
        static ResourceCache<Texture2D> textures = new ResourceCache<Texture2D>();
        static ResourceCache<SpriteFont> fonts = new ResourceCache<SpriteFont>();
        static ResourceCache<SoundEffect> sounds = new ResourceCache<SoundEffect>();
        static ResourceCache<Song> music = new ResourceCache<Song>();
        static bool content_done = false;
        static bool content_written = false;
        public GameContentLoadScreen(Game game, ScreenManager manager)
            : base(game, manager)
        {

        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void LoadContent()
        {
            //Screen Content
            if (content == null)
                content = new ContentManager(screenManager.Game.Services, "Content");
            font = content.Load<SpriteFont>("gui/font/menu_item");
                tex_loading = content.Load<Texture2D>("gui/loadscreen");
            base.LoadContent();
        }

        public override void UnloadContent()
        {
            content.Unload();
            base.UnloadContent();
        }

        public override void Update(GameTime gt)
        {
            if (loading == false)
            {
                loaded_content = new ContentManager(Game.Services, "Content");
                l_thread.Start();
                Thread.Sleep(50);
                loading = true;
            }
            if (loaded == true)
            {
                l_thread.Abort();
            }
            if (content_done)
            {
                GameResources.SetManager(loaded_content);
                GameResources.fonts = fonts;
                GameResources.music = music;
                GameResources.sounds = sounds;
                GameResources.textures = textures;
                content_written = true;
            }
            if (l_thread.ThreadState == ThreadState.Stopped)
            {
                GameScreen g_screen = new GameScreen(Game,smanager);
                smanager.AddScreen(g_screen,this);
                smanager.RemoveScreen(this);
            }
           base.Update(gt);
        }

        public override void Draw(GameTime gameTime)
        {
            smanager.GraphicsDevice.Clear(Color.White);
            SpriteBatch sb = smanager.SpriteBatch;
            sb.Begin();
            sb.Draw(tex_loading,smanager.GraphicsDevice.Viewport.Bounds, Color.White);
            sb.DrawString(font,LoadingFileName,new Vector2(100,400),Color.Black);
            sb.End();
             base.Draw(gameTime);
        }

        public static void LoadingThread()
        {
            DateTime start = DateTime.Now;
            //Music
            LoadingFileName = "Loading Music";
            OpenDataFile<Song>(Constants.HeroEngine_Folder_Data + Constants.HeroEngine_Data_Music);

            //Fonts
            LoadingFileName = "Loading Fonts";
            OpenDataFile<SpriteFont>(Constants.HeroEngine_Folder_Data + Constants.HeroEngine_Data_Fonts);

            //Textures
            LoadingFileName = "Loading Textures";
            OpenDataFile<Texture2D>(Constants.HeroEngine_Folder_Data + Constants.HeroEngine_Data_Texture);

            //Sounds
            LoadingFileName = "Loading Sounds";
            OpenDataFile<Texture2D>(Constants.HeroEngine_Folder_Data + Constants.HeroEngine_Data_Sounds);

            //Tiles
            LoadingFileName = "Loading Tiles";
            //OpenDataFile<Texture2D>(Constants.HeroEngine_Folder_Data + Constants.HeroEngine_Data_Tiles);

            //Custom
            LoadingFileName = "Loading User Content (if any)";
            //OpenDataFile<Texture2D>(Constants.HeroEngine_Folder_Data + Constants.HeroEngine_Data_Custom);

            content_done = true;
            while (!content_written)
            {
                Thread.Sleep(1);
            }
            //Load up the Tiles
            LoadingFileName = "Loading Tiles";
            TileDB.LoadTileData();

            System.Diagnostics.Debug.WriteLine("Res load time: " + (DateTime.Now.TimeOfDay.TotalMilliseconds - start.TimeOfDay.TotalMilliseconds));
        }

        private static bool OpenDataFile<T>(string path)
        {
            string[] lines; // Lines of res file
            //path = Directory.GetCurrentDirectory() + path;
            try
            {
                if (!path.Contains(Constants.HeroEngine_Data_Extension)) // Has the user specified the res, i don't mind :)
                    lines = File.ReadAllLines(Directory.GetCurrentDirectory() + path + Constants.HeroEngine_Data_Extension); //Add one on
                else
                    lines = File.ReadAllLines(path); //Leave it
            }
            catch (FileNotFoundException e)
            {
                    System.Diagnostics.Debug.WriteLine("WARNING: Couldn't find Res file " + path + ". Could mean lack of resources for game. Exception Data: " + e);
                    return false;
            }
            string type = "";
            
            foreach (var item in lines) //Lets take a look at the lines in the file then.
            {
                if (item == "") { continue; } //Blank Line
                if (item.Substring(0, 2) == "//") { continue; }  //Leave out our comments
                //Whats the type of the item?
                if (item.Substring(0, 4) == "TYPE") //If the file specifes a type, change the current type. Tip: You can do this to have multiple types in one file :P
                {
                    type = item.Substring(5,item.Length - 5); // Get the item type
                    
                    continue; // We are done here!
                }

                string name = item.Substring(0, item.IndexOf(":")); // We made it this far, what name is the item called. If the item conflicts, a exception will be thrown.

                

                string itempath = item.Substring(item.IndexOf(":") + 1, item.Length - (item.IndexOf(":") + 1)); //Work out the path
               
                switch (type.ToLower()) //Its a crap method, but im outa ideas. This is pretty self explanitory.
                {
                    case "texture":
                         textures.AddResource(loaded_content.Load<Texture2D>(itempath), name);
                        break;
                    case "sound":
                        sounds.AddResource(loaded_content.Load<SoundEffect>(itempath), name);
                        break;
                    case "music":
                        music.AddResource(loaded_content.Load<Song>(itempath), name);
                        break;
                    case "font":
                        fonts.AddResource(loaded_content.Load<SpriteFont>(itempath), name);
                        break;
                    default:
                        {
                            //Trying to load a content type that isnt supported (YET!)
                            break;
                        }
                }
            }

            System.Diagnostics.Debug.Print("Loaded a Resource Cache");
            return true; //Return our cache. We are done. Phew.
        }
    }
}
