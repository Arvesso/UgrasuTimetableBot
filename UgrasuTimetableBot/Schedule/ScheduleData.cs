using System.Collections.Concurrent;

namespace UgrasuTimetableBot.Schedule
{
    public interface Entity
    {
        int Oid { get; set; }
        string Name { get; set; }
    }

    public enum GroupType
    {
        HigherOilSchool,
        HigherPsychologicalPedagogicalSchool,
        HigherSchoolOfHumanities,
        HigherSchoolOfLaw,
        HigherSchoolOfPhysicalCultureSports,
        HigherSchoolOfDigitalEconomics,
        HigherEnvironmentalSchool,
        EngineeringSchoolOfDigitalTechnologies,
        MultidisciplinaryCollegeOfUgrasu,
        PolytechnicSchool,
        ElectivedisciplinesInPhysicalCultureSports
    }

    public class Group : Entity
    {
        public required int Oid {  get; set; }
        public required string Name { get; set; }
        public required string Faculty { get; set; }
    }

    public class Tutor : Entity
    {
        public required int Oid { get; set; }
        public required string Name { get; set; }
    }

    public class ScheduleData
    {
        public static Dictionary<GroupType, string> GroupTypeName { get; } = new()
        {
            { GroupType.HigherOilSchool, ""},
            { GroupType.HigherPsychologicalPedagogicalSchool, ""},
            { GroupType.HigherSchoolOfHumanities, ""},
            { GroupType.HigherSchoolOfLaw, ""},
            { GroupType.HigherSchoolOfPhysicalCultureSports, ""},
            { GroupType.HigherSchoolOfDigitalEconomics, ""},
            { GroupType.HigherEnvironmentalSchool, ""},
            { GroupType.EngineeringSchoolOfDigitalTechnologies, ""},
            { GroupType.MultidisciplinaryCollegeOfUgrasu, ""},
            { GroupType.PolytechnicSchool, ""},
            { GroupType.ElectivedisciplinesInPhysicalCultureSports, ""}
        };

        public static IEnumerable<Group> Groups { get; }
        public static IEnumerable<Tutor> Tutors { get; }
        public static IEnumerable<string> Faculties { get; }
    }
}
