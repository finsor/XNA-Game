using System;

namespace OrFins
{
#if WINDOWS || XBOX
    static class Program
    {
        static void Main(string[] args)
        {
            using (GameMain game = new GameMain())
            {
                game.Run();
            }
        }
    }
#endif
}
