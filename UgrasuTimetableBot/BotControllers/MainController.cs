using ArveCore;
using ArveCore.Culture;
using Telegram.Bot.Types.Enums;
using UgrasuTimetableBot.IOControl;

namespace UgrasuTimetableBot.BotControllers
{
    #region Records

    public record WaitEnterTutor();

    #endregion

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
        private string SelectedFaculty = string.Empty;

        [State]
        private long SelectedEntityOid = -1;

        [State]
        private long SelectedFacultyOid = -1;

        private bool IsFacultySelected => !string.IsNullOrEmpty(SelectedFaculty);

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

            await MainPanel();
            await Send();
        }

        [On(Handle.Exception)]
        public async Task OnException(Exception ex)
        {
            if (Context.Update.Type == UpdateType.CallbackQuery)
            {
                await AnswerCallback("Ошибка");
            }

            await MainPanel();
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
            await MainPanel();
        }

        [Action("/panel", "Main panel")]
        public async Task MainPanel()
        {
            await ClearState();

            PushL($"<b>Расписание занятий</b>");
            PushL("");
            PushL($"Сегодня {CTimezone.Current.ToString("dddd, dd MMMM", CultureParameters.DefaultRu)}");

            if (IsFacultySelected)
            {
                PushL("");
                PushL($"Ваш факультет:  {SelectedFaculty}");
            }

            PushL("");
            PushL(">");
            PushL();

            RowButton("Выбрать факультет", Q(SelectFaculty));

            if (IsFacultySelected)
            {
                Button("Поиск по группе", Q(SelectGroup, SelectedFacultyOid, 0));
            }
            else
            {
                Button("Поиск по группе", Q(SelectFaculty));
            }

            RowButton("Поиск по преподавателю", Q(SelectTutor));
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

                PushL($"<b>{facultyCounter}.</b> {faculty.FacultyName}");

                if (splitValue == 0)
                {
                    RowButton($"{facultyCounter}", Q(ReverseSelectFaculty, faculty.FacultyOid));
                }
                else
                {
                    Button($"{facultyCounter}", Q(ReverseSelectFaculty, faculty.FacultyOid));
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
        public async Task ReverseSelectFaculty(long facultyOid)
        {
            var faculty = _storage.GetFaculties.First(f => f.FacultyOid == facultyOid);

            SelectedFaculty = faculty.FacultyName;
            SelectedFacultyOid = faculty.FacultyOid;

            await MainPanel();
        }

        [Action]
        public void SelectGroup(long facultyOid, int page)
        {
            var faculty = _storage.GetFaculties.First(f => f.FacultyOid == facultyOid);

            SelectedFaculty = faculty.FacultyName;
            SelectedFacultyOid = faculty.FacultyOid;

            PushL("<b>Выбери группу</b>");
            PushL("");
            PushL($"Выбран факультет: {faculty.FacultyName}");
            PushL("");
            PushL(">");
            PushL();

            var groups = _storage.GetGroups.Where(g => g.Faculty.FacultyOid == facultyOid);

            var pager = new PagingService();
            var query = groups.Select((c, i) => new { entity = c, index = i }).AsQueryable();
            var pageModel = pager.Paging(query, new PageFilter { Count = 12, Page = page });

            Pager(pageModel, i => (i.entity.Name, Q(SelectDayOfWeekGroup, i.entity.Oid)), Q(SelectGroup, SelectedFacultyOid, "{0}"), 4);

            if (IsFacultySelected)
            {
                RowButton("Назад", Q(MainPanel));
            }
            else
            {
                RowButton("Назад", Q(SelectFaculty));
            }
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
            RowButton("Назад", Q(SelectGroup, SelectedFacultyOid, 0));
        }

        [Action]
        public async Task SelectTutor()
        {
            PushL($"<b>Введи фамилию преподавателя</b>");
            PushL("");
            PushL("Примеры корректного ввода");
            PushL("");
            PushL("• <i>брейн</i>");
            PushL("• <i>брейнерт</i>");
            PushL("• <i>брейнерт в.и</i>");
            PushL("• <i>Брейнерт В.И</i>");
            PushL("");
            PushL(">");
            PushL();

            RowButton("Отмена", Q(MainPanel));

            await State(new WaitEnterTutor());
        }

        [State]
        public async ValueTask EnterTutor(WaitEnterTutor _)
        {
            var tutors = _storage.FindBestMatches(_storage.GetTutors, Context.GetSafeTextPayload() ?? string.Empty);

            if (!tutors.Any())
            {
                PushL($"<b>Преподаватель не обнаружен, попробуй еще раз</b>");
                PushL("");
                PushL(">");
                PushL();

                RowButton("Отмена", Q(MainPanel));

                await State(new WaitEnterTutor());

                return;
            }

            if (tutors.Count() > 1)
            {
                PushL($"<b>Обнаружено несколько преподаваталей, выбери нужного</b>");
                PushL("");
                PushL(">");
                PushL();

                if (tutors.Count() > 12)
                    tutors = tutors.Take(12);

                int split = 0;
                int splitLimit = 4;

                foreach (var entity in tutors)
                {
                    if (split == splitLimit)
                        split = 0;

                    if (split == 0)
                    {
                        RowButton(entity.Name, Q(SelectDayOfWeekTutor, entity.Oid));
                    }
                    else
                    {
                        Button(entity.Name, Q(SelectDayOfWeekTutor, entity.Oid));
                    }

                    split++;
                }

                RowButton("Отмена", Q(MainPanel));

                return;
            }

            SelectDayOfWeekTutor(tutors.ElementAt(0).Oid);
        }

        [Action]
        public void SelectDayOfWeekTutor(int tutorOid)
        {
            var tutor = _storage.GetTutors.First(t => t.Oid == tutorOid);

            SelectedEntity = tutor.Name;
            SelectedEntityOid = tutor.Oid;

            PushL("<b>Выбери день недели для отображения расписания преподавателя</b>");
            PushL("");
            PushL($"Выбран преподаватель: {tutor.Name}");
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
            RowButton("Назад", Q(SelectTutor));
        }

        [Action]
        public async Task ViewSchedule()
        {
            PushL("<b>Расписание занятий</b>");
            PushL("");
            PushL($"Расписание для <b>{SelectedEntity}</b> на текущую неделю");
            PushL("");

            var schedule = await _scheduleApi.GetScheduleObjectAsync(CTimezone.StartOfWeek, CTimezone.EndOfWeek, SelectedEntityOid);

            var isScheduleDetected = false;

            if (schedule is null)
            {
                PushL("<b>Ошибка при получении данных, попробуйте снова</b>");
            }
            else
            {
                foreach (var day in Patterns.DaysWithoutSunday)
                {
                    if (!schedule.Lectures[day].Any())
                        continue;

                    isScheduleDetected = true;

                    var date = schedule.Lectures[day][0].Date.ToUpperFirstChar();

                    PushL($"<b>{date}</b>");

                    foreach (var lecture in schedule.Lectures[day])
                    {
                        var printLecture = $"• {lecture.Position} пара ({lecture.StartTime}-{lecture.EndTime}): {lecture.Subject} {lecture.Group} {lecture.Tutor} {lecture.Classroom}";

                        PushL(printLecture.Replace(SelectedEntity, string.Empty).Replace("  ", " "));
                    }

                    PushL("");
                }
            }

            if (!isScheduleDetected)
            {
                PushL("<b>Нет пар на неделе</b>");
                PushL("");
            }

            PushL(">");
            PushL();

            RowButton("Вернуться", Q(MainPanel));
        }

        [Action]
        public async Task ViewScheduleForDay(DayOfWeek day)
        {
            PushL("<b>Расписание занятий</b>");
            PushL("");
            PushL($"Расписание для <b>{SelectedEntity}</b>");
            PushL("");

            var schedule = await _scheduleApi.GetScheduleObjectAsync(CTimezone.StartOfWeek, CTimezone.EndOfWeek, SelectedEntityOid);

            if (schedule is null)
            {
                PushL("<b>Ошибка при получении данных, попробуйте снова</b>");
            }
            else if (!schedule.Lectures.ContainsKey(day) || !schedule.Lectures[day].Any())
            {
                PushL("<b>В выбранный вами день пар нет</b>");
                PushL("");
            }
            else
            {
                var date = schedule.Lectures[day][0].Date.ToUpperFirstChar();

                PushL($"<b>{date}</b>");

                foreach (var lecture in schedule.Lectures[day])
                {
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
        public void Information()
        {
            PushL("<b>Информация</b>");
            PushL("");
            PushL("• Данные расписания актуальны, пока <a href='https://www.ugrasu.ru/timetable/'>основной сервис</a> активен");
            PushL("");
            PushL("• Данный бот разработан <a href='https://t.me/Arvesso'>Arvesso</a>, специально для <b>Студенческого Диджитал Многоборья Югры 2023</b>");
            PushL("");
            PushL(">");
            PushL();

            RowButton("Вернуться", Q(MainPanel));
        }
    }
}
