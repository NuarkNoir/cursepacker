namespace cursepacker
{
    internal class CFAddonModel
    {
        public string Name { get => name; }
        public string FileName { get => fileName; }
        public string DownloadUrl { get => downloadUrl; }

        private string name;
        private string fileName;
        private string downloadUrl;

        public CFAddonModel(string name, string fileName, string downloadUrl)
        {
            this.name = name;
            this.fileName = fileName;
            this.downloadUrl = downloadUrl;
        }
    }
}