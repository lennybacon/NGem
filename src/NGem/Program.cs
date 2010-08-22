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
                NGemLib.DownloadPackage(args[1]);
            }
            else if (args != null && 
                args.Length == 4 && 
                args[0].Equals("make", StringComparison.OrdinalIgnoreCase))
            {
                NGemLib.CreatePackage(args[1], args[2], args[3]);
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