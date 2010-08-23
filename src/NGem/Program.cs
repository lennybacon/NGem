using System;
using System.Configuration;
using devplex.Extensions;

namespace devplex
{
    class Program
    {
        static void Main(string[] args)
        {
            args.PrintHeader();

            try
            {
                if (args != null &&
                    args.Length == 2 &&
                    args[0].Equals("install", StringComparison.OrdinalIgnoreCase))
                {
                    var resolved = false;

                    while (!resolved)
                    {
                        try
                        {
                            NGemLib.ResolvePackage(
                               args[1],
                               gem => Console.WriteLine(
                                   string.Concat(
                                       "Sucessfully added \"", gem, "\".")));

                            resolved = true;
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.IndexOf("401") > -1)
                            {
                                Console.Write("User name:");
                                var userName = Console.ReadLine();
                                Console.Write("Password:");
                                var color = Console.ForegroundColor;
                                Console.ForegroundColor = 
                                    Console.BackgroundColor;
                                var password = Console.ReadLine();
                                Console.ForegroundColor = color;
                                ConfigurationManager.AppSettings[
                                    "nGemSourceUserName"] =
                                        NGemLib.s_gemSourceUserName =
                                            userName;
                                
                                ConfigurationManager.AppSettings[
                                    "nGemSourcePassword"] =
                                        NGemLib.s_gemSourcePassword =
                                            password;

                                //Save Configuration?
                                continue;
                            }
                            throw;
                        }
                    }


                    


                }
                else if (args != null &&
                    args.Length == 3 &&
                    args[0].Equals("make", StringComparison.OrdinalIgnoreCase))
                {

                    if (NGemLib.CreatePackage(args[1], args[2]))
                    {
                        Console.WriteLine(
                            string.Concat(
                                "Sucessfully created package \"",
                                args[2],
                                ".",
                                args[3],
                                "\"."));
                    }

                }
                else
                {
                    PrintUsage();
                    return;
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
                Console.WriteLine("Hit [ENTER] to quit...");
                Console.ReadLine();
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("gem install {packagename}");
            Console.WriteLine(" or");
            Console.WriteLine("gem make {pathtolib} {library}");
            Console.WriteLine("");
            Console.WriteLine("Hit [ENTER] to quit...");
            Console.ReadLine();
        }
    }
}