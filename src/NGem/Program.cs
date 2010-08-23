using System;
using devplex.Extensions;

namespace devplex
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain appDomain = AppDomain.CurrentDomain;
            appDomain.UnhandledException += HandleException;

            args.PrintHeader();

            if (args != null &&
                args.Length == 2 &&
                args[0].Equals("install", StringComparison.OrdinalIgnoreCase))
            {
                if (NGemLib.DownloadPackage(args[1]))
                {
                    Console.WriteLine("Sucessfully added references.");
                }
            }

            else if (args != null &&
                args.Length == 4 &&
                args[0].Equals("make", StringComparison.OrdinalIgnoreCase))
            {
                if (NGemLib.CreatePackage(args[1], args[2], args[3]))
                {
                    Console.WriteLine("Sucessfully created package.");
                }
            }

            else
            {
                PrintUsage();
                return;
            }

            Console.ReadLine();
        }

        static void HandleException(
            object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = (Exception)e.ExceptionObject;

            Console.WriteLine("Ups something went wrong!");
            Console.WriteLine(
                "Write \"details\" and hit [Enter] to " +
                " see the excetions details");
            var userInput = Console.ReadLine();
            if (userInput != null && userInput.Equals("details"))
            {
                Console.WriteLine(exception.ToString());
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("ngem install {packagename}");
            Console.WriteLine(" or");
            Console.WriteLine("ngem make {pathtolib} {manufacturer} {library}");
        }
    }
}