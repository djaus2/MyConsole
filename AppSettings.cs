namespace MyConsole
{
    public class AppSettings
    {
        public string Folder { get; set; } = string.Empty;
        public int Port { get; set; }

        public override string ToString()
        {
            return $"Folder: {Folder}, Port: {Port}";
        }
    }
}
