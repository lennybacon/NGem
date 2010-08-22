using System;
using System.IO;
using System.IO.Packaging;
using System.Configuration;
using System.Net;
using System.Web;

namespace devplex
{
    class NGemLib
    {
        private static string s_gemSource;
        private static string s_gemSourceUserName;
        private static string s_gemSourcePassword;

        static NGemLib()
        {
            s_gemSource = 
                ConfigurationManager.AppSettings["nGemSource"];
            if (string.IsNullOrEmpty(s_gemSource))
            {
                throw new InvalidOperationException(
                    "The configuration is missing the appSettings" +
                    " key \"gemSource\".");
            }
            if (!s_gemSource.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                s_gemSource = string.Concat(s_gemSource, "/");
            }
            s_gemSourceUserName = 
                ConfigurationManager.AppSettings["nGemSourceUserName"];
            s_gemSourcePassword = 
                ConfigurationManager.AppSettings["nGemSourcePassword"];
        }

        public static bool CreatePackage(
            string directory, 
            string manufacturer, 
            string library)
        {
            var path = 
                Path.GetFullPath(
                    Path.Combine(directory, manufacturer, library));
            var baseFileName = string.Concat(manufacturer, ".", library);

            var zipFileName =
                Path.Combine(
                    Environment.CurrentDirectory, 
                    string.Concat(baseFileName, ".zip"));

            if (File.Exists(zipFileName))
            {
                File.Delete(zipFileName);
            }

            using (var zipFile =
                Package.Open(
                    zipFileName,
                    FileMode.CreateNew,
                    FileAccess.ReadWrite))
            {
                var dirInfo = new DirectoryInfo(path);

                AddFilesToPackage(
                    dirInfo, 
                    zipFile, 
                    string.Concat(
                        "/", 
                        HttpUtility.UrlEncode(manufacturer), 
                        "/", 
                        HttpUtility.UrlEncode(library), 
                        "/"));
            }

            return true;
        }

        private static void AddFilesToPackage(
            DirectoryInfo dir, 
            Package zipFile, 

            string path)
        {
            var files = dir.GetFiles();
            foreach (var fileName in files)
            {
                if (!fileName.Name.EndsWith(".dll") ||
                    fileName.Name.EndsWith(".xml") ||
                    fileName.Name.EndsWith(".txt"))
                {
                    continue;
                }

                var fullUri = 
                    string.Concat(
                        path, 
                        HttpUtility.UrlEncode(fileName.Name));
                
                var filePackagePart =
                    zipFile.CreatePart(
                        new Uri(
                            fullUri,
                            UriKind.Relative),
                        "application/octet-stream",
                        CompressionOption.Normal);

                if (filePackagePart == null) continue;

                var fileContent = File.ReadAllBytes(fileName.FullName);

                filePackagePart.GetStream().Write(
                    fileContent,
                    0,
                    fileContent.Length);
            }

            var dirs = dir.GetDirectories();

            foreach (var t in dirs)
            {
                AddFilesToPackage(t, zipFile, string.Concat(path, t.Name, "/"));
            }
        }
        

        public static bool DownloadPackage(string gemName)
        {
            if (string.IsNullOrWhiteSpace(s_gemSource))
                throw new ConfigurationErrorsException("Enter a source url.");

            var webClient = new WebClient();
            if (!string.IsNullOrWhiteSpace(s_gemSourceUserName) &&
                !string.IsNullOrWhiteSpace(s_gemSourcePassword))
            {
                webClient.Credentials =
                    new NetworkCredential(s_gemSourceUserName, s_gemSourcePassword);
            }

            if (!s_gemSource.EndsWith("/"))
                s_gemSource += "/";

            string libDir = EnsureLibDirectory();

            string tempGem = 
                Path.Combine(Path.GetTempPath(), string.Concat(gemName, ".zip"));

            webClient.DownloadFile(
                string.Concat(s_gemSource, gemName, ".zip"),
                tempGem);

            ExtractGem(tempGem, libDir);

            File.Delete(tempGem);

            return true;
        }

        private static void ExtractGem(string tempGem, string libDir)
        {
            using (var zipFile =
               Package.Open(
                   tempGem,
                   FileMode.Open,
                   FileAccess.Read))
            {
                foreach (var packagePart in zipFile.GetParts())
                {
                    var packageUriParts = 
                        packagePart.Uri.OriginalString.Split(
                            new[]{"/"}, 
                            StringSplitOptions.RemoveEmptyEntries);

                    var packagePartDir = libDir;
                    for (int i = 0; i < packageUriParts.Length - 1; i++)
                    {
                        packagePartDir =
                            Path.Combine(
                                packagePartDir,
                                HttpUtility.UrlDecode(packageUriParts[i]));

                        if (!Directory.Exists(packagePartDir))
                        {
                            Directory.CreateDirectory(packagePartDir);
                        }
                    }
                    using (var file = 
                        File.Create(
                            Path.Combine(packagePartDir, 
                            HttpUtility.UrlDecode(
                                packageUriParts[packageUriParts.Length - 1]))))
                    {
                        packagePart.GetStream().CopyTo(file);
                    }
                }
            }
        }

        private static string EnsureLibDirectory()
        {
            string gemPath = 
                Path.Combine(Environment.CurrentDirectory, "lib");
            
            if (!Directory.Exists(gemPath))
            {
                Directory.CreateDirectory(gemPath);
            }

            return gemPath;
        }
    }
}
