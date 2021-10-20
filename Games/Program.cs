using System;
//using System.Reflection;

namespace Games
{
    class Program
         
    {
        static void Main(string[] args)
        {
            Menu menu = new();
            menu.DisplayMenu();
           
            Console.ReadLine();
        }
    }

}