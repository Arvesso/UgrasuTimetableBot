namespace UgrasuTimetableBot.IOControl
{
    public class InMemoryStorage
    {
        private List<Group> Groups { get; init; } = new();
        private List<Tutor> Tutors { get; init; } = new();
        private List<string> Faculties { get; init; } = new();

        public List<Group> GetGroups => Groups;
        public List<Tutor> GetTutors => Tutors;
        public List<string> GetFaculties => Faculties;

        public void AddGroup(Group group) => Groups.Add(group);
        public void AddTutor(Tutor tutor) => Tutors.Add(tutor);
        public void AddFaculty(string faculty) => Faculties.Add(faculty);
        public void ClearGroups() => Groups.Clear();
        public void ClearTutors() => Tutors.Clear();
        public void ClearFaculties() => Faculties.Clear();
    }
}
