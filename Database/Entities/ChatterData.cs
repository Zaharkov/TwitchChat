namespace Database.Entities
{
    public class ChatterData
    {
        public ChatterData(string name, string chatName, string type, long seconds)
        {
            Name = name;
            ChatName = chatName;
            Type = type;
            Seconds = seconds;
        }

        public string Name { get; set; }
        public string ChatName { get; set; }
        public string Type { get; set; }
        public long Seconds { get; set; }
    }
}
