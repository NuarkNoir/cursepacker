using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace cursepacker
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Write("Drag-and-drop `manifest.json` from zip archive to this window >>> ");
            string fpath = Console.ReadLine();
            if (fpath.Length == 0 || !new FileInfo(fpath).Exists)
            {
                Console.WriteLine("This is not a file path!");
                return;
            }
            Console.Write("Reading...");
            string fjson = File.ReadAllText(fpath);
            dynamic fdata = JObject.Parse(fjson);
            Console.Clear();
            if (fdata.manifestType != "minecraftModpack")
            {
                Console.WriteLine("This json scheme not supported!");
                return;
            }

            Console.WriteLine($"Loaded data for `{fdata.name}` v. {fdata.version} by `{fdata.author}`");
            Console.Write("Please, enter version prefix, with which we try to find mods with wrong/old fileID >>> ");
            string pref = Console.ReadLine();

            string base_fpath = Path.Combine(Environment.CurrentDirectory, fdata.name.ToString());

            Console.WriteLine("Loading info for mods...");
            int mc = fdata.files.Count;
            var modList = new List<CFAddonModel>();
            foreach (var file in fdata.files)
            {
                Console.Write($"{modList.Count.ToString().PadLeft(5)}. ");
                try
                {
                    var modData = CFClient.GetAddonData((int)file.projectID, (int)file.fileID, pref);
                    modList.Add(modData);
                    Console.WriteLine($" {modData.Name}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}");
                }
                mc -= 1;
            }

            Console.WriteLine("Downloading mods...");
            Directory.CreateDirectory(base_fpath);
            for (var i = 0; i < modList.Count; i++)
            {
                CFClient.DownloadFile(base_fpath, modList[i]);
            }
        }
    }
}