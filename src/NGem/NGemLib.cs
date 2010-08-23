using System;
using System.IO;
using System.IO.Packaging;
using System.Configuration;
using System.Net;
using System.Web;
using System.Xml;
using devplex.Properties;

namespace devplex
{
    class NGemLib
    {
        private static string s_gemSource;

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
                if (!fileName.Name.EndsWith(
                        ".dll", 
                        StringComparison.OrdinalIgnoreCase) &&
                    !fileName.Name.EndsWith(
                        ".xml", 
                        StringComparison.OrdinalIgnoreCase) &&
                    !fileName.Name.EndsWith(
                        ".txt", 
                        StringComparison.OrdinalIgnoreCase))
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

        public static void ResolvePackage(
            string gemName, 
            Action<string> message)
        {
            var tempGem =
                Path.Combine(
                    Path.GetTempPath(), 
                    string.Concat(gemName, ".zip"));

            var libDir = EnsureLibDirectory();

            DownloadPackage(gemName, tempGem);

            ExtractGem(
                tempGem, 
                libDir, 
                refGem => ResolvePackage(refGem, message));


            File.Delete(tempGem);
            message(gemName);
        }

        public static void DownloadPackage(string gemName, string localFile)
        {
            if (string.IsNullOrWhiteSpace(s_gemSource))
                throw new ConfigurationErrorsException("Enter a source url.");

            var webClient = new WebClient();
            if (!string.IsNullOrWhiteSpace(Settings.Default.UserName) &&
                !string.IsNullOrWhiteSpace(Settings.Default.Password))
            {
                webClient.Credentials =
                    new NetworkCredential(
                        Settings.Default.UserName, 
                        Settings.Default.Password);
            }

            if (!s_gemSource.EndsWith("/"))
            {
                s_gemSource += "/";
            }

            webClient.DownloadFile(
                string.Concat(s_gemSource, gemName, ".zip"),
                localFile);

        }

        private static void ExtractGem(
            string tempGem, 
            string libDir, 
            Action<string> resolveFunction)
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
                    for (var i = 0; i < packageUriParts.Length - 1; i++)
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
                    var fileName = 
                        HttpUtility.UrlDecode(
                            packageUriParts[packageUriParts.Length - 1]);

                    if (fileName.Equals(
                        "References.xml", 
                        StringComparison.OrdinalIgnoreCase))
                    {
                        using (var stream = packagePart.GetStream())
                        using (var reader = XmlReader.Create(stream))
                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element &&
                                reader.LocalName.Equals(
                                    "add"))
                            {
                                if (reader.MoveToAttribute("name"))
                                {
                                    resolveFunction(reader.Value);
                                }
                            }
                        }
                        continue;
                    }

                    using (var file = 
                        File.Create(Path.Combine(packagePartDir, fileName)))
                    {
                        using (var stream = packagePart.GetStream())
                        {
                            stream.CopyTo(file);
                        }
                    }
                }
            }
        }

        private static string EnsureLibDirectory()
        {
            var gemPath = 
                Path.Combine(Environment.CurrentDirectory, "lib");
            
            if (!Directory.Exists(gemPath))
            {
                Directory.CreateDirectory(gemPath);
            }

            return gemPath;
        }
    }
}
