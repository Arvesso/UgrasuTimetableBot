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

            try
            {
                if (_storage.GetGroups.Any(g => g.Oid == oid))
                {
                    var result = await client.GetStringAsync(GetGroupLessonsUri(from, to, oid));
                    var parsedResult = JArray.Parse(result);

                    foreach (var element in parsedResult)
                    {
                        dynamic lecture = JObject.Parse(element.ToString());

                        var day = (DayOfWeek)lecture.dayOfWeek;
                        var position = (int)lecture.lessonNumberStart;
                        var subject = (string)lecture.discipline;
                        var group = (string)lecture.group;
                        var tutor = (string)lecture.lecturer;
                        var classroom = (string)lecture.auditorium;
                        var start = (string)lecture.beginLesson;
                        var end = (string)lecture.endLesson;

                        if (!schedule.Lectures.ContainsKey(day))
                        {
                            schedule.Lectures.Add(day, new());
                        }

                        schedule.Lectures[day].Add(new Lecture()
                        {
                            Subject = subject,
                            Group = group,
                            Tutor = tutor,
                            Classroom = classroom,
                            StartTime = start,
                            EndTime = end,
                            Position = position
                        });
                    }

                    return schedule;
                }
                else if (_storage.GetTutors.Any(t => t.Oid == oid))
                {
                    var result = await client.GetStringAsync(GetTutorLessonsUri(from, to, oid));
                    return schedule;
                }
            }
            catch (Exception ex) 
            {
                _logger.LogWarning("An error has occured while sending request to schedule api: {0}", ex.Message);
            }

            return default;
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
