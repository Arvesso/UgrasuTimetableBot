using Newtonsoft.Json.Linq;
using UgrasuTimetableBot.Extensions;
using UgrasuTimetableBot.IOControl;

namespace UgrasuTimetableBot.Services
{
    public class EntityUpdateService : BackgroundService
    {
        private const string GroupsEndpoint = "https://www.ugrasu.ru/api/directory/groups";
        private const string TutorsEndpoint = "https://www.ugrasu.ru/api/directory/lecturers";

        private readonly InMemoryStorage _storage;
        private readonly ILogger _logger;
        private readonly TimeSpan _updateTimeout;
        public EntityUpdateService(InMemoryStorage memoryStorage, ILogger<EntityUpdateService> logger)
        {
            _storage = memoryStorage;
            _logger = logger;
            _updateTimeout = TimeSpan.FromMinutes(1);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var groups = await GetGroups(stoppingToken);
                    var tutors = await GetTutors(stoppingToken);
                    var faculties = groups.Select(g => g.Faculty)
                        .Distinct(new FacultyEqualityComparer())
                        .Where(f => !Patterns.IgnoreEntry.Any(e => f.FacultyName.Contains(e, StringComparison.OrdinalIgnoreCase)));

                    _storage.ClearGroups();
                    _storage.ClearTutors();
                    _storage.ClearFaculties();

                    foreach (var entity in groups)
                    {
                        _storage.AddGroup(entity);
                    }
                    foreach (var entity in tutors)
                    {
                        _storage.AddTutor(entity);
                    }
                    foreach (var entity in faculties)
                    {
                        _storage.AddFaculty(entity);
                    }
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex) 
                {
                    _logger.LogWarning("An error has occured while updating entities: {0}", ex.Message);
                }

                await Task.Delay(_updateTimeout, stoppingToken);
            }
        }

        private async Task<IEnumerable<Group>> GetGroups(CancellationToken stoppingToken)
        {
            var data = await GetDataString(GroupsEndpoint, stoppingToken);
            var parsedResult = JArray.Parse(data);

            var groups = new List<Group>();

            foreach (var entity in parsedResult)
            {
                dynamic element = JObject.Parse(entity.ToString());

                var groupName = (string)element.name;
                var groupFaculty = (string)element.faculty;
                var groupFacultyOid = (int)element.facultyOid;
                var groupOid = (int)element.groupOid;

                var group = new Group()
                {
                    Name = groupName,
                    Faculty = new()
                    {
                        FacultyName = groupFaculty,
                        FacultyOid = groupFacultyOid
                    },
                    Oid = groupOid
                };

                groups.Add(group);
            }

            return groups.Distinct();
        }
        private async Task<IEnumerable<Tutor>> GetTutors(CancellationToken stoppingToken)
        {
            var data = await GetDataString(TutorsEndpoint, stoppingToken);
            var parsedResult = JArray.Parse(data);

            var tutors = new List<Tutor>();

            foreach (var entity in parsedResult)
            {
                dynamic element = JObject.Parse(entity.ToString());

                var tutorName = (string)element.shortFIO;
                var tutorOid = (int)element.lecturerOid;

                var tutor = new Tutor()
                {
                    Name = tutorName,
                    Oid = tutorOid
                };

                tutors.Add(tutor);
            }

            return tutors.Distinct();
        }
        private async Task<string> GetDataString(string uri, CancellationToken stoppingToken)
        {
            using var client = new HttpClient();
            return await client.GetStringAsync(uri, stoppingToken);
        }
    }
}
