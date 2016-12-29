using System;
using System.Net;

namespace server
{
#if WINDOWS || XBOX
    class Program
    {
        static void Main(string[] args)
        {
            using (Server server = new Server(IPAddress.Any, 9999))
            {
                Console.Title = "Or Fins - Game Server";
                server.Run();
            }
        }
    }
#endif
}

