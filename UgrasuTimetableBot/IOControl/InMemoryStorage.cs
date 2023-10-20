namespace UgrasuTimetableBot.IOControl
{
    public class InMemoryStorage
    {
        private List<Group> Groups { get; init; } = new();
        private List<Tutor> Tutors { get; init; } = new();
        private List<Faculty> Faculties { get; init; } = new();

        public List<Group> GetGroups => Groups;
        public List<Tutor> GetTutors => Tutors;
        public List<Faculty> GetFaculties => Faculties;

        public void AddGroup(Group group) => Groups.Add(group);
        public void AddTutor(Tutor tutor) => Tutors.Add(tutor);
        public void AddFaculty(Faculty faculty) => Faculties.Add(faculty);
        public void ClearGroups() => Groups.Clear();
        public void ClearTutors() => Tutors.Clear();
        public void ClearFaculties() => Faculties.Clear();

        public IEnumerable<Entity> FindBestMatches(IEnumerable<Entity> list, string input)
        {
            const double similarityValue = 0.419; // Higher - more precisely search

            IEnumerable<Entity> result;

            if (string.IsNullOrEmpty(input))
                return Enumerable.Empty<Entity>();

            input = input.Trim();

            result = list.Where(x => x.Name.Equals(input, StringComparison.OrdinalIgnoreCase));

            if (!result.Any())
                result = list.Where(x => x.Name.Contains(input, StringComparison.OrdinalIgnoreCase));
            if (!result.Any())
                result = list.Where(x => x.Name.CalculateSimilarity(input) > similarityValue);

            return result;
        }
    }
}
