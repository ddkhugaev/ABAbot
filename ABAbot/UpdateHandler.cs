using ABAbot.Db;
using ABAbot.Db.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using YandexGpt;

public class UpdateHandler
{
    private readonly IServiceProvider _services;
    private readonly Dictionary<long, UserSession> _sessions;
    private readonly ITelegramBotClient _bot;
    private readonly Dictionary<long, int> _lastMessageIds;
    private readonly IUserGptRequestLogsDbRepository _userGptRequestLogsDbRepository;
    readonly IUsersRepository usersRepository;
    readonly IIkigaiesRepository ikigaiesRepository;

    public UpdateHandler(IServiceProvider services)
    {
        _services = services;
        _sessions = services.GetRequiredService<Dictionary<long, UserSession>>();
        _bot = services.GetRequiredService<ITelegramBotClient>();
        _lastMessageIds = new Dictionary<long, int>();

        usersRepository = services.GetRequiredService<IUsersRepository>();
        ikigaiesRepository = services.GetRequiredService<IIkigaiesRepository>();
        _userGptRequestLogsDbRepository = services.GetRequiredService<IUserGptRequestLogsDbRepository>();
    }

    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Message != null)
            await HandleMessage(update.Message, update.Message.From.Id);

        if (update.CallbackQuery != null)
            await HandleCallback(update.CallbackQuery);

        if (update.EditedMessage != null)
            await HandleEditedMessage(update.EditedMessage);
    }

    private async Task HandleMessage(Message message, long userId)
    {
        var chatId = message.Chat.Id;
        var text = message.Text ?? "";

        if (!_sessions.ContainsKey(chatId))
            _sessions[chatId] = new UserSession() { UserId = message.From.Id };

        var session = _sessions[chatId];

        if (text == "/start")
        {
            var user = await usersRepository.TryGetByIdAsync(userId);
            if (user == null)
            {
                session.Step = Step.AskName;
                var sentMessage = await _bot.SendMessage(chatId, "👋 Введите ваше имя и фамилию:");
                UpdateLastMessageId(chatId, sentMessage.MessageId);
            }
            else
            {
                session.Step = Step.MainMenu;
                await ShowMainMenu(chatId, user.Name, session);
            }

            return;
        }
        await ProcessSessionStep(chatId, text, session);
    }

    private async Task HandleEditedMessage(Message editedMessage)
    {
        var chatId = editedMessage.Chat.Id;
        var text = editedMessage.Text ?? "";

        if (!_sessions.ContainsKey(chatId))
            return;

        var session = _sessions[chatId];
        await ProcessSessionStep(chatId, text, session, true);
    }

    private async Task ProcessSessionStep(long chatId, string text, UserSession session, bool isEdit = false)
    {
        switch (session.Step)
        {
            case Step.AskName:
                session.FullName = text;
                session.Step = Step.MainMenu;
                await usersRepository.AddAsync(new ABAbot.Db.Models.User { Id = session.UserId, Name = session.FullName });
                await ShowMainMenu(chatId, session.FullName, session, isEdit);
                break;

            case Step.Question1:
                session.Love = text;
                session.Step = Step.Question2;
                await SendOrEditMessageAsync(chatId, "💪 То, в чем вы хороши – ваши навыки и компетенции, то, в чем у вас есть талант", isEdit);
                break;

            case Step.Question2:
                session.GoodAt = text;
                session.Step = Step.Question3;
                await SendOrEditMessageAsync(chatId, "💰 То, за что вам могут платить – деятельность, которая приносит доход", isEdit);
                break;

            case Step.Question3:
                session.PaidFor = text;
                session.Step = Step.Question4;
                await SendOrEditMessageAsync(chatId, "🌍 То, что нужно миру – деятельность, которая приносит пользу обществу или решает важные проблемы", isEdit);
                break;

            case Step.Question4:
                session.WorldNeeds = text;
                session.Step = Step.MainMenu;

                var sentMessage = await _bot.SendMessage(chatId, "🔮Ищем вдохновение...🔎");
                UpdateLastMessageId(chatId, sentMessage.MessageId);

                var gptBusinessIdeas = await GenerateIdeas(session);

                var existingUser = await usersRepository.TryGetByIdAsync(chatId);


                // добавление икигай в бд
                await ikigaiesRepository.AddAsync(new Ikigai { UserId = chatId,
                    GptAns = gptBusinessIdeas,
                    Date = DateTime.Now,
                    WhatYouLove = session.Love,
                    WhatYouAreGoodAt = session.GoodAt,
                    WhatYouCanBePaidFor = session.PaidFor,
                    WhatTheWorldNeeds = session.WorldNeeds });

                var resultText = $"🚀{existingUser.Name}, {gptBusinessIdeas}";

                if (isEdit)
                {
                    await _bot.EditMessageText(chatId, _lastMessageIds[chatId], resultText);
                }
                else
                {
                    sentMessage = await _bot.SendMessage(chatId, resultText);
                    UpdateLastMessageId(chatId, sentMessage.MessageId);
                }

                await ShowMainMenu(chatId, existingUser.Name, session, isEdit);
                break;

            case Step.MarketIntelligenceQuestion:
                session.Step = Step.MainMenu;

                var awaitMessage = await _bot.SendMessage(chatId, "🔮Анализируем рынок...🔎");
                UpdateLastMessageId(chatId, awaitMessage.MessageId);
                
                var gptRecommendations = await GetGptRecommendations(text);

                var existingUser1 = await usersRepository.TryGetByIdAsync(chatId);

                //добавление в бд
                await _userGptRequestLogsDbRepository.AddAsync(new UserGptRequestLog
                {
                    UserId = chatId,
                    Date = DateTime.Now,
                    GptAnswer = gptRecommendations,
                    UserRequest = text
                });

                var answerText = $"🚀{existingUser1.Name}, {gptRecommendations}";

                await SendOrEditMessageAsync(chatId, answerText, isEdit);
                await ShowMainMenu(chatId, existingUser1.Name, session, isEdit);
                break;
        }
    }

    private async Task HandleCallback(CallbackQuery query)
    {
        var chatId = query.Message.Chat.Id;

        if (!_sessions.ContainsKey(chatId))
            _sessions[chatId] = new UserSession();

        var session = _sessions[chatId];

        if (query.Data == "start_ikigai")
        {
            session.Step = Step.Question1;
            await _bot.EditMessageText(chatId, query.Message.MessageId, "❤️ То, что вы любите – ваши страсти, увлечения, то, что приносит радость");
            UpdateLastMessageId(chatId, query.Message.MessageId);
        }

        if(query.Data == "market_intelligence")
        {
            session.Step = Step.MarketIntelligenceQuestion;
            await _bot.EditMessageText(chatId, query.Message.MessageId, "🔥 Помогу исследовать рынок! По какой нише вы бы хотели получить подсказку?\nНапример: 'Конные прогулки в горах'");
            UpdateLastMessageId(chatId, query.Message.MessageId);
        }
    }

    private async Task ShowMainMenu(long chatId, string name, UserSession session, bool isEdit = false)
    {
        var menu = new InlineKeyboardMarkup(
            InlineKeyboardButton.WithCallbackData("🧭 Пройти Икигаи", "start_ikigai"),
            InlineKeyboardButton.WithCallbackData("🔍️ Разведка рынка", "market_intelligence"));

        var userok = await usersRepository.TryGetByIdAsync(chatId);
        var messageText = $"👋 Привет, {userok.Name}!\nВыберите действие:";

        if (isEdit && _lastMessageIds.ContainsKey(chatId))
        {
            await _bot.EditMessageText(chatId, _lastMessageIds[chatId], messageText, replyMarkup: menu);
        }
        else
        {
            var sentMessage = await _bot.SendMessage(chatId, messageText, replyMarkup: menu);
            UpdateLastMessageId(chatId, sentMessage.MessageId);
        }
    }

    private async Task SendOrEditMessageAsync(long chatId, string text, bool isEdit)
    {
        if (isEdit && _lastMessageIds.ContainsKey(chatId))
        {
            await _bot.EditMessageText(chatId, _lastMessageIds[chatId], text);
        }
        else
        {
            var sentMessage = await _bot.SendMessage(chatId, text);
            UpdateLastMessageId(chatId, sentMessage.MessageId);
        }
    }

    private void UpdateLastMessageId(long chatId, int messageId)
    {
        _lastMessageIds[chatId] = messageId;
    }

    private async Task<string> GenerateIdeas(UserSession u)
    {
        var gptYandex = new YandexGptClient();
        var userRequestIkigai = $"Студент прошел опрос по модели Икигаи. Пользователю было задано четыре вопроса и пользователь дал четыре ответа." +
            $"\nНа вопрос: ‘То,что вы любите (What You Love) – ваши страсти, увлечения, то, что приносит радость.’ пользователь ответил: ‘{u.Love}’." +
            $"\nНа вопрос: ‘То, в чем вы хороши (What You Are Good At) – ваши навыки и компетенции, то, в чем у вас есть талант.’ пользователь ответил: ‘{u.GoodAt}’." +
            $"\nНа вопрос: ‘То, за что вам могут платить (What You Can Be Paid For) – деятельность, которая приносит доход.’ пользователь ответил: ‘{u.PaidFor}’." +
            $"\nНа вопрос: ‘То, что нужно миру (What The World Needs) – деятельность, которая приносит пользу обществу или решает важные проблемы’ пользователь ответил: ‘{u.WorldNeeds}’." +
            $"\nПроанализируй ответы на поставленные вопросы и напиши рекомендации согласно модели Икигаи. Не используй разметку Markdown!";

        return await gptYandex.GetGptResponseAsync(userRequestIkigai);
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
    {
        Console.WriteLine($"Ошибка: {exception.Message}");
        return Task.CompletedTask;
    }

    private async Task<string> GetGptRecommendations(string userPrompt)
    {
        var gptYandex = new YandexGptClient();
        var userRequest = $"Разведка рынка. Тема бизнес проекта - {userPrompt}. Дай подсказки по исследованию ниши и ссылки на рынки без гипотез. Например, где можно посмотреть данные по отчетности корпораций. Не используй разметку Markdown!";
        return await gptYandex.GetGptResponseAsync(userRequest);
    }
}