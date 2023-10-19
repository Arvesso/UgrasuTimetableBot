using ArveCore;
using ArveCore.Culture;
using ArveCore.SpkSchedule;
using Telegram.Bot.Types.Enums;
using UgrasuTimetableBot.IOControl;

namespace UgrasuTimetableBot.BotControllers
{
    public class MainController : BotController
    {
        private readonly InMemoryStorage _storage;
        private readonly ScheduleApi _scheduleApi;
        private readonly ILogger _logger;
        public MainController(InMemoryStorage memoryStorage, ScheduleApi scheduleApi, ILogger<MainController> logger)
        {
            _logger = logger;
            _storage = memoryStorage;
            _scheduleApi = scheduleApi;
        }

        #region States

        [State]
        private bool IsInitialized = false;

        [State]
        private string SelectedEntity = string.Empty;

        [State]
        private long SelectedEntityOid = -1;

        [State]
        private long SelectedFacultyOid = -1;

        #endregion

        #region SystemHandle

        [On(Handle.BeforeAll)]
        public async Task OnBeforeAll()
        {
            if (IsInitialized)
            {
                if (Context.Update.Type != UpdateType.CallbackQuery || LastSendedMessage is null)
                    return;

                if (Context.GetCallbackQuery().Message?.Date < LastSendedMessage.Date)
                {
                    await AnswerCallback("Устаревшая панель управления");
                    Context.StopHandling();
                }
            }
        }

        [On(Handle.Unknown)]
        public async Task OnUnknownAction()
        {
            if (Context.Update.Type == UpdateType.CallbackQuery)
            {
                await AnswerCallback("Неизвестный запрос");
            }

            MainPanel();
        }

        [On(Handle.Exception)]
        public async Task OnException(Exception ex)
        {
            if (Context.Update.Type == UpdateType.CallbackQuery)
            {
                await AnswerCallback("Ошибка");           
            }

            MainPanel();
            await Send();

            _logger.LogWarning($"Bot task exception: {ex.Message}");
        }

        #endregion

        [Action("/start", "Start the bot")]
        public async Task Start()
        {
            Reply();
            PushL("Приветствую! Я помогу узнать расписание, воспользуйся панелью выбора");
            await Send();
            MainPanel();
        }

        [Action("/panel", "Main panel")]
        public void MainPanel()
        {
            PushL($"<b>Расписание занятий</b>");
            PushL("");
            PushL($"Сегодня {CTimezone.Current.ToString("dddd, dd MMMM", CultureParameters.DefaultRu)}");
            PushL("");
            PushL(">");
            PushL();

            RowButton("Поиск по факультету", Q(SelectFaculty));
            RowButton("Поиск по преподавателю", Q(SelectTutor));
            Button("Поиск по кабинету", Q(SelectClassroom));
            RowButton("Информация", Q(Information));

            if (!IsInitialized)
                IsInitialized = true;
        }

        [Action]
        public void SelectFaculty()
        {
            PushL($"<b>Выбери факультет</b>");
            PushL("");

            int splitValue = 0;
            int facultyCounter = 1;

            foreach (var faculty in _storage.GetFaculties)
            {
                if (splitValue == 4)
                    splitValue = 0;

                PushL($"<b>{facultyCounter}.</b> {faculty.FacultyName.ToLower().ToUpperFirstChar()}");

                if (splitValue == 0)
                {
                    RowButton($"{facultyCounter}", Q(SelectGroup, faculty.FacultyOid));
                }
                else
                {
                    Button($"{facultyCounter}", Q(SelectGroup, faculty.FacultyOid));
                }

                splitValue++;
                facultyCounter++;
            }

            PushL("");
            PushL(">");
            PushL();

            RowButton("Назад", Q(MainPanel));
        }

        [Action]
        public void SelectGroup(long facultyOid)
        {
            var faculty = _storage.GetFaculties.First(f => f.FacultyOid == facultyOid);

            SelectedFacultyOid = faculty.FacultyOid;

            PushL("<b>Выбери группу</b>");
            PushL("");
            PushL($"Выбран факультет: {faculty.FacultyName}");
            PushL("");
            PushL(">");
            PushL();

            int splitValue = 0;

            foreach (var group in _storage.GetGroups)
            {
                if (group.Faculty.FacultyOid != facultyOid)
                    continue;

                if (splitValue == 6)
                    splitValue = 0;

                if (splitValue == 0)
                {
                    RowButton(group.Name, Q(SelectDayOfWeekGroup, group.Oid));
                }
                else
                {
                    Button(group.Name, Q(SelectDayOfWeekGroup, group.Oid));
                }

                splitValue++;
            }

            RowButton("Назад", Q(SelectFaculty));
        }

        [Action]
        public void SelectDayOfWeekGroup(int groupOid)
        {
            var group = _storage.GetGroups.First(g => g.Oid == groupOid);

            SelectedEntity = group.Name;
            SelectedEntityOid = group.Oid;

            PushL("<b>Выбери день недели для отображения расписания группы</b>");
            PushL("");
            PushL($"Выбрана группа: {group.Name}");
            PushL("");
            PushL(">");
            PushL();

            RowButton("Пн", Q(ViewScheduleForDay, DayOfWeek.Monday));
            Button("Вт", Q(ViewScheduleForDay, DayOfWeek.Tuesday));
            Button("Ср", Q(ViewScheduleForDay, DayOfWeek.Wednesday));
            Button("Чт", Q(ViewScheduleForDay, DayOfWeek.Thursday));
            Button("Пт", Q(ViewScheduleForDay, DayOfWeek.Friday));
            Button("Сб", Q(ViewScheduleForDay, DayOfWeek.Saturday));
            RowButton("Сегодня", Q(ViewScheduleForDay, CTimezone.Current.DayOfWeek));
            Button("Вся неделя", Q(ViewSchedule));
            RowButton("Назад", Q(SelectGroup, SelectedFacultyOid));
        }

        [Action]
        public void SelectTutor()
        {

        }

        [Action]
        public void SelectClassroom()
        {

        }

        [Action]
        public async Task ViewSchedule()
        {
            PushL("<b>Расписание занятий</b>");
            PushL("");
            PushL($"Расписание для <b>{SelectedEntity}</b> на всю неделю");
            PushL("");

            var schedule = await _scheduleApi.GetScheduleObjectAsync(CTimezone.StartOfWeek, CTimezone.EndOfWeek, SelectedEntityOid);

            foreach (var day in ScheduleStatic.DaysWithoutSunday) // Move inside
            {
                PushL($"<b>{day}</b>");

                foreach (var lecture in schedule.Lectures[day])
                {
                    if (lecture.Subject == "Пары нет")
                        continue;

                    var printLecture = $"• {lecture.Position} пара ({lecture.StartTime}-{lecture.EndTime}): {lecture.Subject} {lecture.Group} {lecture.Tutor} {lecture.Classroom}";
                    PushL(printLecture.Replace(SelectedEntity, string.Empty).Replace("  ", " "));
                }

                PushL("");
            }

            PushL(">");
            PushL();

            RowButton("Вернуться", Q(MainPanel));
        }

        [Action]
        public void ViewScheduleForDay(DayOfWeek day)
        {

        }

        [Action]
        public void Information()
        {

        }
    }
}
