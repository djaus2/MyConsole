using Sportronics.ConfigurationManager;

namespace MyConsole
{
    public class AppSettings : AppSettingsBase
    {
        public string Folder { get; set; } = @"C:\temp\BaseSettings";
        public int Port { get; set; } = 1000;

        public override string SectionName => "AppSettings";

        public override string ToString()
        {
            return $"Folder: {Folder}, Port: {Port}";
        }
    }
}
