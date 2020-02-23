using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;

namespace cursepacker
{
    internal class CFClient
    {
        public static string ApiBaseEndpoint { get => "https://addons-ecs.forgesvc.net/api/v2/addon/"; }

        public static CFAddonModel GetAddonData(int addonId, int fileId, string fix_prefix)
        {
            var ep = $"{ApiBaseEndpoint}{addonId}";
            var raw_data = "";
            using (var wc = GetWebClient())
            {
                raw_data = wc.DownloadString(new Uri(ep));
            }
            dynamic dyn_data = JObject.Parse(raw_data);
            string modName = dyn_data.name;
            var downloadUrl = "";
            var fileName = "";
            try
            {
                dynamic fobj = (from file in new List<dynamic>(dyn_data.latestFiles)
                                where file.id == fileId
                                select file).First();
                downloadUrl = fobj.downloadUrl;
                fileName = fobj.fileName;
                Console.Write("[       found       ] ");
            }
            catch (InvalidOperationException)
            {
                if (fix_prefix.Length == 0)
                {
                    Console.Write("[     not found     ] ");
                    throw new Exception($"Try search at `{dyn_data.websiteUrl}`");
                }

                foreach (var file in dyn_data.latestFiles)
                {
                    bool flag = false;
                    foreach (var version in file.gameVersion)
                    {
                        if ((version.ToString() as string).StartsWith(fix_prefix, true, CultureInfo.InvariantCulture))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        downloadUrl = file.downloadUrl;
                        fileName = file.fileName;
                        Console.Write("[ found  compatible ] ");
                        break;
                    }
                }
                if (downloadUrl.Length == 0 || fileName.Length == 0)
                {
                    Console.Write("[     not found     ] ");
                    throw new Exception($"Try search at `{dyn_data.websiteUrl}`");
                }
            }
            return new CFAddonModel(modName, fileName, downloadUrl);
        }

        public static void DownloadFile(string base_fpath, CFAddonModel mod)
        {
            Console.Write($"Downloading {mod.Name}... ");
            var fpath = Path.Combine(base_fpath, mod.FileName);
            try
            {
                using var wc = GetWebClient();
                wc.DownloadFile(mod.DownloadUrl, fpath);
                Console.WriteLine("done");
            }
            catch (Exception e)
            {
                Console.WriteLine($"error occurred: \n\tUrl: {mod.DownloadUrl} \n\tMessage: {e.Message}");
            }
        }

        private static WebClient GetWebClient()
        {
            var wc = new WebClient();
            wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) " +
                       "twitch-desktop-electron-platform/1.0.0 Chrome/66.0.3359.181 Twitch/3.0.16 Safari/537.36 " +
                       "desklight/8.42.2");
            wc.Headers.Add("Authority", "addons-ecs.forgesvc.net");
            wc.Headers.Add(HttpRequestHeader.Referer, "https://www.twitch.tv");
            return wc;
        }
    }
}