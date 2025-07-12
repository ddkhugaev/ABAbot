using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

public class UpdateHandler
{
	private readonly IServiceProvider _services;
	private readonly Dictionary<long, UserSession> _sessions;
	private readonly ITelegramBotClient _bot;
	private readonly Dictionary<long, int> _lastMessageIds; 

	public UpdateHandler(IServiceProvider services)
	{
		_services = services;
		_sessions = services.GetRequiredService<Dictionary<long, UserSession>>();
		_bot = services.GetRequiredService<ITelegramBotClient>();
		_lastMessageIds = new Dictionary<long, int>();
	}

	public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
	{
		if (update.Message != null)
			await HandleMessage(update.Message);

		if (update.CallbackQuery != null)
			await HandleCallback(update.CallbackQuery);

		if (update.EditedMessage != null)
			await HandleEditedMessage(update.EditedMessage);
	}

	private async Task HandleMessage(Message message)
	{
		var chatId = message.Chat.Id;
		var text = message.Text ?? "";

		if (!_sessions.ContainsKey(chatId))
			_sessions[chatId] = new UserSession();

		var session = _sessions[chatId];

		if (text == "/start")
		{
			session.Step = Step.AskName;
			var sentMessage = await _bot.SendMessage(chatId, "👋 Введите ваше имя и фамилию:");
			UpdateLastMessageId(chatId, sentMessage.MessageId);
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
				await ShowMainMenu(chatId, session.FullName, isEdit);
				break;

			case Step.Question1:
				session.Love = text;
				session.Step = Step.Question2;
				await SendOrEditMessage(chatId, "💪 В чём вы хороши?", isEdit);
				break;

			case Step.Question2:
				session.GoodAt = text;
				session.Step = Step.Question3;
				await SendOrEditMessage(chatId, "💰 За что вам могут платить?", isEdit);
				break;

			case Step.Question3:
				session.PaidFor = text;
				session.Step = Step.Question4;
				await SendOrEditMessage(chatId, "🌍 Что нужно миру?", isEdit);
				break;

			case Step.Question4:
				session.WorldNeeds = text;
				session.Step = Step.MainMenu;

				var ideas = GenerateIdeas(session);
				var resultText = $"🚀 Вот ваши гипотезы, {session.FullName}:\n\n{string.Join("\n\n", ideas)}";

				if (isEdit)
				{
					await _bot.EditMessageText(chatId, _lastMessageIds[chatId], resultText);
				}
				else
				{
					var sentMessage = await _bot.SendMessage(chatId, resultText);
					UpdateLastMessageId(chatId, sentMessage.MessageId);
				}

				await ShowMainMenu(chatId, session.FullName, isEdit);
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
			await _bot.EditMessageText(chatId, query.Message.MessageId, "❤️ Что вы любите?");
			UpdateLastMessageId(chatId, query.Message.MessageId);
		}
	}

	private async Task ShowMainMenu(long chatId, string name, bool isEdit = false)
	{
		var menu = new InlineKeyboardMarkup(
			InlineKeyboardButton.WithCallbackData("🧭 Пройти Икигай", "start_ikigai"));

		var messageText = $"👋 Привет, {name}!\nВыберите действие:";

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

	private async Task SendOrEditMessage(long chatId, string text, bool isEdit)
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

	private List<string> GenerateIdeas(UserSession u) => new()
	{
		$"📌 Объедините «{u.Love}» и «{u.WorldNeeds}» — это может быть призвание.",
		$"💼 Вы хороши в «{u.GoodAt}» и зарабатываете на «{u.PaidFor}». Найдите точку пересечения.",
		$"🌍 «{u.Love}» и «{u.GoodAt}» — путь к страсти.",
		$"💰 То, за что платят, и что нужно миру — это может быть ваша миссия.",
		$"🧭 Всё вместе — возможно, ваш Икигай!"
	};

	public Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
	{
		Console.WriteLine($"❌ Ошибка: {exception.Message}");
		return Task.CompletedTask;
	}
}