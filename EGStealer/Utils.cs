using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EGStealer
{
    internal class Utils
    {
        internal class URLInfo
        {
            public string Name { get; set; }
            public string URL { get; set; }
            public string Path { get; set; }
            public string IconPath { get; set; }
            public bool IsFromDesktop { get; set; }
            public bool StealProcess { get; set; }
        }

        readonly internal static string MyTempPath = Path.GetTempPath() + @"EGStealer\";
        readonly internal static string MyAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\EGStealer\";
        readonly internal static string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        internal static List<URLInfo> Shortcuts = new List<URLInfo>();
        internal static Dictionary<string, string> NoIconGames = new Dictionary<string, string>();

        static Utils()
        {
            if (!Directory.Exists(MyTempPath.Remove(MyTempPath.Length - 1)))
                Directory.CreateDirectory(MyTempPath.Remove(MyTempPath.Length - 1));

            if (!Directory.Exists(MyAppData.Remove(MyAppData.Length - 1)))
                Directory.CreateDirectory(MyAppData.Remove(MyAppData.Length - 1));

            if (!Directory.Exists($"{MyAppData}URLFiles"))
                Directory.CreateDirectory($"{MyAppData}URLFiles");

            NoIconGames.Add("RunFallGuys", "FallGuys_client_game");
        }

        public static string ReadFileFromIncrustedResources(string filename)
        {
            string fileContent;
            string resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith(filename));

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
                fileContent = reader.ReadToEnd();

            return fileContent;
        }

        public static void GetURLFiles(string Path, bool desktop = false)
        {
            var shortcuts = Directory.GetFiles(Path, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".url"));
            foreach (var shortcut in shortcuts)
                GenerateURLInfo(shortcut, desktop);
            shortcuts = null;
        }

        private static void GenerateURLInfo(string path, bool isFromDesktop)
        {
            string url = null;
            string iconPath = null;
            foreach (var line in File.ReadLines(path).Skip(3))
            {
                if (line.Contains("URL="))
                    url = line.Replace("URL=", "");
                else if (line.Contains("IconFile="))
                    iconPath = line.Replace("IconFile=", "");
            }

            if (!url.Contains("com.epicgames.launcher"))
                return;
            if (!File.Exists(iconPath))
                return;

            string name = Path.GetFileNameWithoutExtension(path);

            URLInfo item = Shortcuts.FirstOrDefault(s => s.Name == name);
            if (item != null)
                Shortcuts.Remove(item);

            Shortcuts.Add(new URLInfo()
            {
                Name = name,
                URL = url,
                Path = path,
                IconPath = iconPath,
                IsFromDesktop = isFromDesktop,
                StealProcess = isFromDesktop || File.Exists($"{DesktopPath}\\{name} Stealed.lnk")
            });
        }

        public static async Task<bool> WaitForFile(string filename)
        {
            FileInfo file = new FileInfo(filename);

            while (IsFileLocked(file))
                await Task.Delay(TimeSpan.FromMilliseconds(1000));

            return true;
        }

        private static bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }

    }
}
