using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using VanillaStyleDiplomacy.Helpers;

namespace VanillaStyleDiplomacy.Managers
{
    public class UpdateManager
    {
        private static UpdateManager instance = null;
        private string versionPrefix = ConstantsHelper.Version;

        public static UpdateManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new UpdateManager();
                return instance;
            }
        }

        public bool CanUpdate { get; private set; } = false;

        public void CheckNewestVersion()
        {
            try
            {
                RespondBodyHelper resp = new RespondBodyHelper("https://raw.githubusercontent.com/KaySteinhoff/VanillaStyleDiplomacy/main/VanillaStyleDiplomacy.csproj");
                byte[] buf = resp.FetchBody();
                string xml = Encoding.UTF8.GetString(buf, 0, buf.Length);

                XDocument doc = XDocument.Parse(xml);

                XElement versionNode = doc.Root.Element("PropertyGroup").Element("Version");
                if (new Version(versionNode.Value).CompareTo(new Version(ConstantsHelper.Version)) <= 0)
                    return;

                Instance.CanUpdate = true;
                versionPrefix = versionNode.Value;
            }
            catch (Exception e)
            {
                LoggingManager.Instance.LogException(e);
            }
        }

        private void CopyDirectoryContent(string sourceDirectory, string destinationDirectory, string fileMatchRegex)
        {
            string[] files = Directory.GetFiles(sourceDirectory);
            for (int i = 0; i < files.Length; ++i)
                if (Regex.IsMatch(files[i], fileMatchRegex))
                    File.Copy(files[i], destinationDirectory + Path.GetFileName(files[i]));
        }

        public void Update()
        {
            if (ConstantsHelper.Version == versionPrefix)
                return;
            try
            {
                // Download zip
                RespondBodyHelper resp = new RespondBodyHelper($"https://github.com/KaySteinhoff/VanillaStyleDiplomacy/releases/download/{versionPrefix}/VanillaStyleDiplomacy.zip");
                Stream target = File.Open($"{ConstantsHelper.ModuleFolder}{versionPrefix}.zip", FileMode.Create);
                byte[] data = resp.FetchBody(new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("user-agent", "anything") });
                target.Write(data, 0, data.Length);
                target.Close();

                // Extract data
                ZipFile.ExtractToDirectory($"{ConstantsHelper.ModuleFolder}{versionPrefix}.zip", ConstantsHelper.ModuleFolder + versionPrefix);
                // Replace existing files with downloaded files(keep old config file)
                CopyDirectoryContent($"{ConstantsHelper.ModuleFolder}{versionPrefix}/VanillaStyleDiplomacy/bin/Win64_Shipping_Client", $"{ConstantsHelper.ModuleFolder}/bin/Win64_Shipping_Client/", ".");
                CopyDirectoryContent($"{ConstantsHelper.ModuleFolder}{versionPrefix}/VanillaStyleDiplomacy/Module_Data", $"{ConstantsHelper.ModuleFolder}/Module_Data/", "^(?!.+\\/?config\\.dat).+$");

                // Clean up
                File.Delete($"{ConstantsHelper.ModuleFolder}{versionPrefix}.zip");
                Directory.Delete(ConstantsHelper.ModuleFolder + versionPrefix, true);
            }
            catch (Exception e)
            {
                LoggingManager.Instance.LogException(e);
            }
        }
    }
}