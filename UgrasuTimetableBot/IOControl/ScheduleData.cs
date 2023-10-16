namespace UgrasuTimetableBot.IOControl
{
    public interface Entity
    {
        int Oid { get; set; }
        string Name { get; set; }
    }

    public class Group : Entity
    {
        public required int Oid { get; set; }
        public required string Name { get; set; }
        public required string Faculty { get; set; }
    }

    public class Tutor : Entity
    {
        public required int Oid { get; set; }
        public required string Name { get; set; }
    }
}
