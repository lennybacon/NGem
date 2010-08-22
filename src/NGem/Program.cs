using System;
using devplex.Extentions;

namespace devplex
{
    class Program
    {
        static void Main(string[] args)
        {
            args.PrintHeader();

            if (args != null && 
                args.Length == 2 && 
                args[0].Equals("install", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    if (NGemLib.DownloadPackage(args[1]))
                    {
                        Console.WriteLine("Sucessfully added references.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ups something went wrong!");
                    Console.WriteLine(
                        "Write \"details\" and hit [Enter] to " +
                        "see the excetions details");
                    var userInput = Console.ReadLine();
                    if (userInput != null && userInput.Equals("details"))
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            else if (args != null && 
                args.Length == 4 && 
                args[0].Equals("make", StringComparison.OrdinalIgnoreCase))
            {
                try
                { 
                    if (NGemLib.CreatePackage(args[1], args[2], args[3]))
                    {
                        Console.WriteLine("Sucessfully created package.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ups something went wrong!");
                    Console.WriteLine(
                        "Write \"details\" and hit [Enter] to " +
                        " see the excetions details");
                    var userInput = Console.ReadLine();
                    if (userInput != null && userInput.Equals("details"))
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            else
            {
                PrintUsage();
                return;
            }

            Console.ReadLine();
        }
        
        static void PrintUsage()
        {
            Console.WriteLine("gem install {packagename}");
            Console.WriteLine(" or");
            Console.WriteLine("gem make {pathtolib} {manufacturer} {library}");
        }
    }
}