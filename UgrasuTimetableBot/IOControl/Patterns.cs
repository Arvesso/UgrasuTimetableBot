namespace UgrasuTimetableBot.IOControl
{
    public class Patterns
    {
        public static IEnumerable<DayOfWeek> DaysWithoutSunday { get; } = Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>().Where(d => d != DayOfWeek.Sunday);

        public static List<string> IgnoreEntry { get; } = new()
        {
            "филиал", "служебный"
        };
    }
}
