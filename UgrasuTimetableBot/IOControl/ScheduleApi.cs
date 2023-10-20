using ArveCore.Culture;
using Newtonsoft.Json.Linq;

namespace UgrasuTimetableBot.IOControl
{
    public class ScheduleApi
    {
        private const string BaseGroupApiUri = "https://www.ugrasu.ru/api/directory/lessons?fromdate=[1]&todate=[2]&groupOid=[3]";
        private const string BaseTutorApiUri = "https://www.ugrasu.ru/api/directory/lessons?fromdate=[1]&todate=[2]&lecturerOid=[3]";

        private readonly InMemoryStorage _storage;
        private readonly ILogger _logger;
        public ScheduleApi(InMemoryStorage memoryStorage, ILogger<ScheduleApi> logger)
        {
            _storage = memoryStorage;
            _logger = logger;
        }

        public async Task<ScheduleObject?> GetScheduleObjectAsync(DateTime from, DateTime to, long oid)
        {
            using var client = new HttpClient();

            var schedule = new ScheduleObject()
            {
                Lectures = new()
            };

            foreach (var day in Patterns.DaysWithoutSunday)
            {
                schedule.Lectures.Add(day, new());
            }

            try
            {
                if (_storage.GetGroups.Any(g => g.Oid == oid))
                {
                    var result = await client.GetStringAsync(GetGroupLessonsUri(from, to, oid));
                    var parsedResult = JArray.Parse(result);

                    ParseJArray(parsedResult, schedule);

                    return schedule;
                }
                else if (_storage.GetTutors.Any(t => t.Oid == oid))
                {
                    var result = await client.GetStringAsync(GetTutorLessonsUri(from, to, oid));
                    var parsedResult = JArray.Parse(result);

                    ParseJArray(parsedResult, schedule);

                    return schedule;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("An error has occured while sending request to schedule api: {0}", ex.Message);
            }

            return default;

            static void ParseJArray(JArray jarray, ScheduleObject schedule)
            {
                foreach (var element in jarray)
                {
                    dynamic lecture = JObject.Parse(element.ToString());

                    var day = (DayOfWeek)lecture.dayOfWeek;
                    var position = (int)lecture.lessonNumberStart;
                    var date = (string)lecture.date;
                    var subject = (string)lecture.discipline;
                    var group = (string)lecture.group;
                    var tutor = (string)lecture.lecturer;
                    var classroom = (string)lecture.auditorium;
                    var start = (string)lecture.beginLesson;
                    var end = (string)lecture.endLesson;

                    schedule.Lectures[day].Add(new Lecture()
                    {
                        Date = DateTime.ParseExact(date, "yyyy.MM.dd", null).ToString("dddd, dd MMMM", CultureParameters.DefaultRu),
                        Subject = subject,
                        Group = group,
                        Tutor = tutor,
                        Classroom = classroom,
                        StartTime = start,
                        EndTime = end,
                        Position = position
                    });
                }
            }
        }

        private static string GetGroupLessonsUri(DateTime from, DateTime to, long oid)
        {
            return BaseGroupApiUri.Replace("[1]", from.ToString("yyyy-MM-dd")).Replace("[2]", to.ToString("yyyy-MM-dd")).Replace("[3]", oid.ToString());
        }
        private static string GetTutorLessonsUri(DateTime from, DateTime to, long oid)
        {
            return BaseTutorApiUri.Replace("[1]", from.ToString("yyyy-MM-dd")).Replace("[2]", to.ToString("yyyy-MM-dd")).Replace("[3]", oid.ToString());
        }
    }
}
