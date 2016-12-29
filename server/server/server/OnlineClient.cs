using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace server
{
    class OnlineClient
    {
        private TcpClient client;
        private NetworkStream stream;
        private StreamWriter writer;
        private StreamReader reader;

        public bool isConnected { get; protected set; }

        public OnlineClient(TcpClient tcp_client)
        {
            this.client = tcp_client;

            isConnected = true;
            client.NoDelay = true;

            stream = client.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
        }

        protected string GetString()
        {
            return (reader.ReadLine());
        }

        protected void Send(params string[] strings)
        {
            foreach (string str in strings)
                writer.WriteLine(str);

            writer.Flush();
        }

        protected Vector2 GetVector2()
        {
            return (new Vector2(float.Parse(reader.ReadLine()), float.Parse(reader.ReadLine())));
        }

        protected float GetFloat()
        {
            return (float.Parse(reader.ReadLine()));
        }

        protected int GetInt()
        {
            return (int.Parse(reader.ReadLine()));
        }

        protected bool GetBool()
        {
            string n = reader.ReadLine();
            return (bool.Parse(n));
        }

        public void CloseConnection()
        {
            stream.Close();
            client.Close();
            writer.Dispose();
            reader.Dispose();
            stream.Dispose();
            client.Client.Dispose();
        }

        protected Rectangle GetRectangle()
        {
            return (new Rectangle(
                GetInt(),
                GetInt(),
                GetInt(),
                GetInt()));
        }

        ~OnlineClient()
        {
            CloseConnection();
        }
    }
}
