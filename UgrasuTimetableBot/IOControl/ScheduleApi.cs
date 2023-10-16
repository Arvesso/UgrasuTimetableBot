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

        public async Task<JArray?> GetScheduleObjectAsync(DateTime from, DateTime to, int oid)
        {
            using var client = new HttpClient();

            try
            {
                if (_storage.GetGroups.Any(g => g.Oid == oid))
                {
                    var result = await client.GetStringAsync(GetGroupLessonsUri(from, to, oid));
                    return JArray.Parse(result);
                }
                else if (_storage.GetTutors.Any(t => t.Oid == oid))
                {
                    var result = await client.GetStringAsync(GetTutorLessonsUri(from, to, oid));
                    return JArray.Parse(result);
                }
            }
            catch (Exception ex) 
            {
                _logger.LogWarning("An error has occured while sending request to schedule api: {0}", ex.Message);
            }

            return default;
        }

        private string GetGroupLessonsUri(DateTime from, DateTime to, int oid)
        {
            return BaseGroupApiUri.Replace("[1]", from.ToShortDateString()).Replace("[2]", to.ToShortDateString()).Replace("[3]", oid.ToString());
        }
        private string GetTutorLessonsUri(DateTime from, DateTime to, int oid)
        {
            return BaseTutorApiUri.Replace("[1]", from.ToShortDateString()).Replace("[2]", to.ToShortDateString()).Replace("[3]", oid.ToString());
        }
    }
}
