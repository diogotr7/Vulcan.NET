using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcan.NET;
using System.Drawing;

namespace TestApp
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (VulcanKeyboard.Initialize())
            {
                Console.WriteLine("Found Vulcan!");
                VulcanKeyboard.SetColor(Color.Red);
                VulcanKeyboard.SetKeyColor(Key.W, Color.Blue);
                VulcanKeyboard.SetKeyColor(Key.A, Color.Blue);
                VulcanKeyboard.SetKeyColor(Key.S, Color.Blue);
                VulcanKeyboard.SetKeyColor(Key.D, Color.Blue);

                Console.WriteLine("Set colors: " + VulcanKeyboard.Update());
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
