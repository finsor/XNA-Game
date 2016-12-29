using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Timers;

namespace server
{
    delegate void PublicMapFunction();

    public class Server : Microsoft.Xna.Framework.Game
    {
        TcpListener listener;
        List<OnlinePlayer> clients;
        List<OnlinePlayer> clients_that_are_waiting_for_sync;

        Dictionary<string, OnlineMap> world;

        PublicMapFunction Update_Maps;
        PublicMapFunction ClearPlayerLists;

        #region Construction
        public Server(IPAddress SERVER_IP, int SERVER_PORT)
        {
            Console.Write("Creating server ... ");
            new GraphicsDeviceManager(this); // Initialize GDM
            Content.RootDirectory = "Content";

            listener = new TcpListener(SERVER_IP, SERVER_PORT);
            Console.WriteLine("Done. ");
        }

        protected override void Initialize()
        {
            Console.Write("Initializing Server ... ");

            clients = new List<OnlinePlayer>();
            clients_that_are_waiting_for_sync = new List<OnlinePlayer>();

            listener.Start();

            Console.WriteLine("Done. ");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            #region Initialize Dictionary
            Console.Write("Initializing sprites dictionary ... ");
            SpritesDictionary.LoadSprites(Content);
            Console.WriteLine("Done.");
            #endregion

            #region Create World
            Console.Write("Creating world ... ");

            world = new Dictionary<string, OnlineMap>();

            List<OnlineMap> maps_list = new List<OnlineMap>();

            maps_list.Add(OnlineMap.CreateCity(Content));
            maps_list.Add(OnlineMap.CreateTrainingGrounds(Content));
            maps_list.Add(OnlineMap.CreateForest(Content));
            maps_list.Add(OnlineMap.CreateLava(Content));

            foreach (OnlineMap map in maps_list)
            {
                world.Add(map.name, map);

                Update_Maps += map.Update;
                ClearPlayerLists += map.ClearPlayers;
            }
            Console.WriteLine("Done.");
            #endregion

            new System.Threading.Thread(Accept_Clients).Start();

            Console.WriteLine("Server Running");

            while (true)
            {
                Update();

                Thread.Sleep(1000 / 60);
            }
        }
        #endregion

        #region Accept and remove
        // Thread used to accept new clients
        private void Accept_Clients()
        {
            TcpClient client;
            while (true)
            {
                client = listener.AcceptTcpClient();
                clients_that_are_waiting_for_sync.Add(new OnlinePlayer(client));

                Console.WriteLine("CLIENT CONNECTED , {0} ONLINE", clients.Count + clients_that_are_waiting_for_sync.Count);
            }
        }
        #endregion

        #region Update functions
        // Functions used to update the whole server
        private void Update()
        {
            SyncNewPlayers();

            RecreatePlayerLists();

            Update_Maps();
        }

        // Function synchronizes players that are waiting for their first update
        private void SyncNewPlayers()
        {
            OnlinePlayer newClient;

            for (int i = 0; i < clients_that_are_waiting_for_sync.Count; i++)
            {
                try
                {
                    newClient = clients_that_are_waiting_for_sync[i];


                    newClient.Sync(world, world["City Center"]);

                    clients.Add(newClient);
                    clients_that_are_waiting_for_sync.RemoveAt(i);

                }
                catch { }
            }
        }

        // Recreates player lists in all maps.
        // Used to get connect new players and update players' belonging to maps.
        private void RecreatePlayerLists()
        {
            ClearPlayerLists();

            OnlinePlayer player;

            // Receive info from players and build profiles
            for (int i = 0; i < clients.Count; i++)
            {
                player = clients[i];

                if (player.isConnected)
                {
                    player.map.AddPlayer(player);
                }
                else
                {
                    // Close the connection
                    clients[i].CloseConnection();
                    clients.RemoveAt(i);
                    Console.WriteLine("CLIENT DISCONNECTED , {0} ONLINE", clients.Count + clients_that_are_waiting_for_sync.Count);
                }
            }
        }
        #endregion
    }
}
