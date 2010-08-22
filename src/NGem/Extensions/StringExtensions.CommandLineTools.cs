using System;
using System.Reflection;

namespace devplex.Extensions
{
    public static partial class StringExtensions
    {
        internal static void PrintHeader(this string[] instance)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();
            var productAttribute = 
                assembly.GetCustomAttributes(
                    typeof(AssemblyProductAttribute), true)
                as AssemblyProductAttribute[];

            var applicationName = assemblyName.Name;
            
            if (productAttribute != null && productAttribute.Length > 0)
            {
                applicationName = productAttribute[0].Product;
            }


            Console.WriteLine(
                string.Concat(
                    applicationName,
                    " v",
                    assemblyName.Version.Major,
                    ".",
                    assemblyName.Version.MajorRevision,
                    " (Build ",
                    assemblyName.Version.Revision,
                    ")"));

            Console.WriteLine();
            var copyrightAttributes = 
                assembly.GetCustomAttributes(
                    typeof (AssemblyCopyrightAttribute), true)
                as AssemblyCopyrightAttribute[];

            if (copyrightAttributes != null && copyrightAttributes.Length > 0)
            {

                Console.WriteLine(
                    string.Concat(
                        copyrightAttributes[0].Copyright,
                        copyrightAttributes[0].Copyright.EndsWith(".") 
                            ? string.Empty 
                            : ".",
                        " All rights reserved."));
            }

            var trademarkAttributes =
                assembly.GetCustomAttributes(
                    typeof(AssemblyTrademarkAttribute), true)
                as AssemblyTrademarkAttribute[];

            if (trademarkAttributes != null && trademarkAttributes.Length > 0)
            {
                Console.WriteLine(trademarkAttributes[0].Trademark);
            }
            Console.WriteLine();
        }
    }
}


