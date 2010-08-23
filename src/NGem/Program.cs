using System;
using System.Collections.Generic;
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
                    !string.IsNullOrEmpty(args[0]) &&
                    args[0].Equals(
                        "install", 
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrEmpty(args[1]))
                {
                    var gemName = args[1];
                    ResolveGem(gemName);
                }
                else if (args != null &&
                    args.Length == 4 &&
                    !string.IsNullOrEmpty(args[0]) &&
                    args[0].Equals(
                        "make", 
                    StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrEmpty(args[1]) &&
                    !string.IsNullOrEmpty(args[2]) &&
                    !string.IsNullOrEmpty(args[3]))
                {
                    CreateGem(args);
                }
                else
                {
                    PrintUsage();
                    return;
                }
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }

        #region CreateGem()
        private static void CreateGem(IList<string> args)
        {
            if (NGemLib.CreateGem(args[1], args[2], args[3]))
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
        #endregion

        #region ReportError()
        private static void ReportError(Exception ex)
        {
            Console.WriteLine("Ups something went wrong!");
            Console.WriteLine(
                "Write \"details\" and hit [Enter] to " +
                "see the excetions details");
            var userInput = Console.ReadLine();
            if (userInput != null && userInput.Equals("details"))
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine(string.Empty);
                Console.WriteLine("Hit [ENTER] to quit...");
            }
        } 
        #endregion

        #region ResolveGem()
        private static void ResolveGem(string gemName)
        {
            var resolved = false;
            while (!resolved)
            {
                try
                {
                    NGemLib.ResolvePackage(
                        gemName,
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
                        Console.CursorVisible = false;
                        var password = Console.ReadLine();
                        Console.ForegroundColor = color;
                        Console.CursorVisible = true;
                        Properties.Settings.Default.UserName =
                            userName;
                        Properties.Settings.Default.Password =
                            password;
                        Properties.Settings.Default.Save();

                        continue;
                    }
                    throw;
                }
            }
        } 
        #endregion

        #region PrintUsage()
        static void PrintUsage()
        {
            Console.WriteLine("gem install {packagename}");
            Console.WriteLine(" or");
            Console.WriteLine("gem make {pathtolib} {library} {manufaturer} {library}");
            Console.WriteLine(string.Empty);
            Console.WriteLine("Hit [ENTER] to quit...");
            Console.ReadLine();
        } 
        #endregion
    }
}