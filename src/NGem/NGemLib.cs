using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Configuration;
using System.Net;
using System.Web;
using System.Xml;
using devplex.Properties;

namespace devplex
{
    /// <summary>
    /// A gem library for .NET assemblies.
    /// </summary>
    public class NGemLib
    {
        private static string s_gemSource;

        private static readonly List<string> s_ResolvedGems = new List<string>();

        #region Ctor()
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
        #endregion

        #region CreateGem()
        /// <summary>
        /// Creates the package.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="manufacturer">The manufacturer.</param>
        /// <param name="library">The library.</param>
        /// <returns></returns>
        public static bool CreateGem(
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
        #endregion

        #region AddFilesToPackage()
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
                        StringComparison.OrdinalIgnoreCase) &&
                    !fileName.Name.EndsWith(
                        ".exp",
                        StringComparison.OrdinalIgnoreCase) &&
                    !fileName.Name.EndsWith(
                        ".lib",
                        StringComparison.OrdinalIgnoreCase) &&
                    !fileName.Name.EndsWith(
                        ".bin",
                        StringComparison.OrdinalIgnoreCase) &&
                    !fileName.Name.EndsWith(
                        ".def",
                        StringComparison.OrdinalIgnoreCase) &&
                    !fileName.Name.EndsWith(
                        ".targets",
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (fileName.Name.IndexOf(".") > -1)
                    {
                        continue;
                    }
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
        #endregion

        #region ResolvePackage()
        /// <summary>
        /// Resolves the package.
        /// </summary>
        /// <param name="gemName">Name of the gem.</param>
        /// <param name="message">The message.</param>
        public static void ResolvePackage(
            string gemName,
            Action<string> message)
        {
            var tempGem =
                Path.Combine(
                    Path.GetTempPath(),
                    Path.GetTempFileName());

            var libDir = EnsureLibDirectory();

            DownloadPackage(gemName, tempGem);

            s_ResolvedGems.Add(gemName);

            ExtractGem(
                tempGem,
                libDir,
                refGem => ResolvePackage(refGem, message));


            File.Delete(tempGem);
            message(gemName);
        } 
        #endregion

        #region DownloadPackage()
        private static void DownloadPackage(string gemName, string localFile)
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
        #endregion

        #region ExtractGem()
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
                            new[] { "/" },
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
                                        if (!s_ResolvedGems.Contains(
                                            reader.Value))
                                        {
                                            resolveFunction(reader.Value);
                                        }
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
        #endregion

        #region EnsureLibDirectory()
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
        #endregion
    }
}
