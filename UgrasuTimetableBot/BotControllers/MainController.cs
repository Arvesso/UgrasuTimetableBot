using ArveCore.Culture;
using ArveCore;
using Microsoft.VisualBasic;
using Telegram.Bot.Types.Enums;

namespace UgrasuTimetableBot.BotControllers
{
    public class MainController : BotController
    {
        private readonly ILogger _logger;
        public MainController(ILogger<MainController> logger)
        {
            _logger = logger;
        }

        #region States

        [State]
        private bool IsInitialized = false;

        [State]
        private string SelectedEntity = string.Empty;

        [State]
        private string SelectedType = string.Empty;

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
        }

        [On(Handle.Exception)]
        public async Task OnException(Exception ex)
        {
            if (Context.Update.Type == UpdateType.CallbackQuery)
            {
                await AnswerCallback("Ошибка");
            }

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
            PushL($"<b>{_memStorage.GetWeekrange}</b>");
            PushL("");
            PushL($"Сегодня {CTimezone.Current.ToString("dddd, dd MMMM", CultureParameters.DefaultRu)}");
            PushL("");
            PushL(">");
            PushL();

            RowButton("1 курс", Q(SelectGroup, 1));
            Button("2 курс", Q(SelectGroup, 2));
            Button("3 курс", Q(SelectGroup, 3));
            Button("4 курс", Q(SelectGroup, 4));
            RowButton("Поиск по фамилии", Q(SelectTeacher));
            Button("Поиск по кабинету", Q(SelectClassroom));
            RowButton("Информация", Q(Information));

            if (!IsInitialized)
                IsInitialized = true;
        }
    }
}
