namespace FileManagerServerPlugin.Extensions
{
    static class FileTypeExtensions
    {
        public static string ExtToType(this string ext)
        {
            switch (ext.ToLower())
            {
                case "dir":
                    return "File Folder";
                case "hdd":
                    return "Hard Disk Drive";
                case "cd":
                    return "CD Drive";
                case "usb":
                    return "USB Drive";
                case "net":
                    return "Network Drive";

                case ".app":
                    return "Application";
                case ".7z":
                case ".asp":
                case ".aspx":
                case ".avi":
                    break;
                case ".bat":
                    return "Windows Batch File";
                case ".bin":
                    break;
                case ".bmp":
                    return "Bitmap Image";
                case ".bz2":
                case ".c":
                    break;
                case ".cat":
                    return "Security Catelog";
                case ".cdr":
                case ".chm":
                case ".class":
                case ".cmd":
                    return "Windows Command Script";
                case ".com":
                    return "MS-DOS Application";
                case ".conf":
                case ".cpp":
                case ".crt":
                    return "Security Certificate";
                case ".cs":
                case ".css":
                case ".csv":
                    break;
                case ".db":
                    return "Database File";
                case ".deb":
                case ".divx":
                    break;
                case ".dll":
                    return "Application Extension";
                case ".doc":
                case ".docx":
                case ".dot":
                    break;
                case ".exe":
                    return "Application";
                case ".f4v":
                case ".flv":
                    break;
                case ".gif":
                    return "GIF Image";
                case ".gz":
                case ".hlp":
                case ".htm":
                case ".html":
                case ".img":
                    return "Disk Image File";
                case ".inf":
                    return "Setup Information";
                case ".ini":
                    return "Configuration Settings";
                case ".iso":
                    return "Disk Image File";
                case ".jar":
                case ".java":
                    break;
                case ".jpg":
                case ".jpeg":
                    return "JPEG Image";
                case ".js":
                case ".jsp":
                    break;
                case ".lnk":
                    return "Shortcut";
                case ".lua":
                case ".m":
                case ".mm":
                case ".m4a":
                case ".m4v":
                case ".mov":
                case ".mp3":
                case ".mp4v":
                case ".mpeg":
                case ".msi":
                    break;
                case ".pdf":
                    return "Adobe Acrobat Document";
                case ".perl":
                case ".pfx":
                    return "Personal Information Exchange";
                case ".php":
                case ".pl":
                    break;
                case ".png":
                    return "PNG Image";
                case ".ppt":
                    return "Powerpoint Presentation";
                case ".ps":
                case ".psd":
                case ".py":
                case ".ram":
                    break;
                case ".rar":
                    return "WinRAR Archive";
                case ".reg":
                case ".rtf":
                case ".ruby":
                case ".sig":
                case ".sql":
                case ".svg":
                case ".swf":
                    break;
                case ".sys":
                    return "System File";
                case ".tar":
                case ".tgz":
                case ".tif":
                    break;
                case ".ttf":
                    return "TrueType Font File";
                case ".txt":
                    return "Text Document";
                case ".url":
                    return "Internet Shortcut";
                case ".vb":
                case ".vbs":
                case ".vbscript":
                    return "VBScript Script File";
                case ".vcf":
                case ".vdo":
                case ".vsd":
                case ".wav":
                case ".wma":
                case ".wmv":
                    break;
                case ".xls":
                    return "Excel Spreadsheet";
                case ".xml":
                case ".xpi":
                case ".xul":
                case ".xvid":
                    break;
                case ".zip":
                    return "Compressed Archive";
            }

            if (ext.StartsWith("."))
                return ext.ToUpper().Remove(ext.IndexOf('.'), 1) + (string.IsNullOrEmpty(ext) ? "" : " ") + "File";
            else
                return ext.ToUpper() + (string.IsNullOrEmpty(ext) ? "" : " ") + "File";
        }

        public static string TypeToImageKey(this string type)
        {
            switch (type.ToLower())
            {
                case "dir":
                    return "folder";
                case "hdd":
                case "usb":
                    return "drive";
                case "cd":
                    return "drive_cd";
                case "net":
                    return "drive_network";
                case "web":
                    return "drive_web";

                case ".app":
                    return "Application";
                case ".7z":
                case ".asp":
                case ".aspx":
                case ".avi":
                case ".bat":
                case ".bin":
                case ".bmp":
                case ".bz2":
                case ".c":
                case ".cat":
                case ".cdr":
                case ".chm":
                case ".class":
                case ".cmd":
                case ".com":
                case ".conf":
                case ".cpp":
                case ".crt":
                case ".cs":
                case ".css":
                case ".csv":
                case ".db":
                case ".deb":
                case ".dll":
                case ".doc":
                case ".docx":
                case ".dot":
                case ".exe":
                case ".f4v":
                case ".flv":
                case ".gif":
                case ".gz":
                case ".hlp":
                case ".htm":
                case ".html":
                case ".inf":
                case ".ini":
                case ".iso":
                case ".jar":
                case ".java":
                case ".jpg":
                case ".jpeg":
                case ".js":
                case ".jsp":
                case ".lnk":
                case ".lua":
                case ".m":
                case ".mm":
                case ".m4a":
                case ".m4v":
                case ".mov":
                case ".mp3":
                case ".mp4v":
                case ".mpeg":
                case ".msi":
                case ".pdf":
                case ".perl":
                case ".pfx":
                case ".php":
                case ".pl":
                case ".png":
                case ".ppt":
                case ".ps":
                case ".psd":
                case ".py":
                case ".ram":
                case ".rar":
                case ".reg":
                case ".rtf":
                case ".ruby":
                case ".sig":
                case ".sql":
                case ".svg":
                case ".swf":
                case ".sys":
                case ".tar":
                case ".tgz":
                case ".tif":
                case ".ttf":
                case ".txt":
                case ".url":
                case ".vb":
                case ".vbs":
                case ".vbscript":
                case ".vcf":
                case ".vdo":
                case ".vsd":
                case ".wav":
                case ".wma":
                case ".wmv":
                case ".xls":
                case ".xml":
                case ".zip":
                    return type.ToLower().Remove(0, 1);
                default:
                    return "file";
            }
        }
    }
}