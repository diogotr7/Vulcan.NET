using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcan.NET;
using System.Drawing;
using System.Diagnostics;

namespace TestApp
{
    static class Program
    {
        static void Main()
        {
            if (VulcanKeyboard.Initialize())
            {
                Console.WriteLine("Found Vulcan!");
                VulcanKeyboard.SetColor(Color.Red);
                VulcanKeyboard.SetKeyColor(Key.W, Color.Blue);
                VulcanKeyboard.SetKeyColor(Key.A, Color.Blue);
                VulcanKeyboard.SetKeyColor(Key.S, Color.Blue);
                VulcanKeyboard.SetKeyColor(Key.D, Color.Blue);

                var watch = new Stopwatch();
                watch.Start();
                bool success = VulcanKeyboard.Update();
                Console.WriteLine("Set colors: " + success + ", took :" + watch.ElapsedMilliseconds + "ms");
                watch.Stop();
                Console.ReadLine();
                Console.WriteLine("Disconnecting...");
                VulcanKeyboard.Disconnect();
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Did not find vulcan!");
                Console.ReadLine();
            }
        }
    }
}
