using ConfigurationManager;

namespace MyConsole
{
    public class AppSettings : AppSettingsBase
    {
        public string Folder { get; set; } = string.Empty;
        public int Port { get; set; }

        public override string SectionName => "AppSettings";

        public override string ToString()
        {
            return $"Folder: {Folder}, Port: {Port}";
        }
    }
}
