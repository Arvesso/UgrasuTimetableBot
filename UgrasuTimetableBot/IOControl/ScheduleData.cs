namespace UgrasuTimetableBot.IOControl
{
    public interface Entity
    {
        int Oid { get; set; }
        string Name { get; set; }
    }

    public class Faculty
    {
        public required int FacultyOid { get; set; }
        public required string FacultyName { get; set; }
    }

    public class Group : Entity
    {
        public required int Oid { get; set; }
        public required string Name { get; set; }
        public required Faculty Faculty { get; set; }
    }

    public class Tutor : Entity
    {
        public required int Oid { get; set; }
        public required string Name { get; set; }
    }

    public class ScheduleObject
    {
        public Dictionary<DayOfWeek, List<Lecture>> Lectures { get; set; } = new();
    }

    public class Lecture
    {
        public required int Position { get; set; }
        public required string Date { get; set; }
        public required string Subject { get; set; }
        public required string Group { get; set; }
        public required string Tutor { get; set; }
        public required string Classroom { get; set; }
        public required string StartTime { get; set; }
        public required string EndTime { get; set; }
    }
}