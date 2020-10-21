namespace FileManagerClientPlugin.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    static class FileExtensions
    {
        public static bool IsSystemFile(this string path)
        {
            return (File.GetAttributes(path) & FileAttributes.System) == FileAttributes.System;
        }

        public static bool IsHiddenFile(this string path)
        {
            return (File.GetAttributes(path) & FileAttributes.Hidden) == FileAttributes.Hidden;
        }

        public static string PersonalFolderNameToFullPath(this string personalFolder)
        {
            switch (personalFolder)
            {
                case "Desktop":
                    return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                case "Documents":
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                case "Downloads":
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                case "Favorites":
                    return Environment.GetFolderPath(Environment.SpecialFolder.Favorites);
                case "Music":
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                case "Pictures":
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                case "Videos":
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }
            return personalFolder;
        }

        public static bool IsPersonalFolder(this string name)
        {
            var personalFolders = new List<string>
            {
                "desktop",
                "documents",
                "downloads",
                "favorites",
                "music",
                "pictures",
                "videos"
            };
            return personalFolders.Contains(name.ToLower());
        }
    }
}
