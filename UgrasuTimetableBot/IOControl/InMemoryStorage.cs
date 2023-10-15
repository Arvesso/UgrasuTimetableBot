using System.Collections.Generic;
using UgrasuTimetableBot.Schedule;

namespace UgrasuTimetableBot.IOControl
{
    public class InMemoryStorage
    {
        public InMemoryStorage()
        {
            
        }

        private List<Group> Groups { get; init; } = new();
        private List<Tutor> Tutors { get; init; } = new();
        private List<string> Faculties { get; init; } = new();

        public void AddGroup(Group group) => Groups.Add(group);
        public void AddTutor(Tutor tutor) => Tutors.Add(tutor);
        public void AddFaculty(string faculty) => Faculties.Add(faculty);
    }
}
