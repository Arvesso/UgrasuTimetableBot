using UgrasuTimetableBot.IOControl;

namespace UgrasuTimetableBot.Extensions
{
    public class FacultyEqualityComparer : IEqualityComparer<Faculty>
    {
        public bool Equals(Faculty? x, Faculty? y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is null || y is null)
                return false;

            return x.FacultyName == y.FacultyName;
        }

        public int GetHashCode(Faculty faculty)
        {
            return faculty.FacultyName.GetHashCode();
        }
    }
}
