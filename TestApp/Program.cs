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
            var keyboards = VulcanFinder.FindKeyboards();
            if (!keyboards.Any())
            {
                Console.WriteLine("Couldn't find any keyboards, exiting.");
                return;
            }
            foreach (var keyboard in keyboards)
            {
                Console.WriteLine($"Found keyboard of type {keyboard.KeyboardType}");

                keyboard.SetColor(255, 0, 0);
                keyboard.SetKeyColor(Key.W, 0, 0, 255);
                keyboard.SetKeyColor(Key.A, 0, 0, 255);
                keyboard.SetKeyColor(Key.S, 0, 0, 255);
                keyboard.SetKeyColor(Key.D, 0, 0, 255);

                var sw = Stopwatch.StartNew();
                bool success = keyboard.Update();
                sw.Stop();

                Console.WriteLine("Set colors: " + success + ", took :" + sw.ElapsedMilliseconds + "ms");

                Console.ReadLine();
                Console.WriteLine("Disconnecting...");
                keyboard.Dispose();
            }
        }
    }
}
