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
using System.Windows.Forms;

namespace OrFins
{
    class OnlineClient
    {
        private TcpClient client;
        private NetworkStream stream;
        private StreamWriter writer;
        private StreamReader reader;

        private string SERVER_IP;
        private int SERVER_PORT;

        public OnlineClient(string SERVER_IP, int SERVER_PORT)
        {
            this.SERVER_IP = SERVER_IP;
            this.SERVER_PORT = SERVER_PORT;

            client = new TcpClient();
            client.NoDelay = true;
        }

        public void Connect()
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "",
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = "Your name?" };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            System.Windows.Forms.Button confirmation = new System.Windows.Forms.Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            if (prompt.ShowDialog() == DialogResult.OK)
            {
                client.Connect(SERVER_IP, SERVER_PORT);

                stream = client.GetStream();

                writer = new StreamWriter(stream);
                reader = new StreamReader(stream);

                Send(textBox.Text);
            }
        }

        public void Send(params string[] strings)
        {
            foreach (string str in strings)
                writer.WriteLine(str);

            writer.Flush();
        }

        public string GetString()
        {
            return (reader.ReadLine());
        }

        public Vector2 GetVector2()
        {
            return (new Vector2(float.Parse(reader.ReadLine()), float.Parse(reader.ReadLine())));
        }

        public float GetFloat()
        {
            return (float.Parse(reader.ReadLine()));
        }

        public int GetInt()
        {
            return (int.Parse(reader.ReadLine()));
        }

        public bool GetBool()
        {
            return (bool.Parse(reader.ReadLine()));
        }

        public void CloseConnection()
        {
            stream.Close();
            client.Close();
            writer.Dispose();
            reader.Dispose();
        }

        ~OnlineClient()
        {
            this.CloseConnection();
        }
    }
}
