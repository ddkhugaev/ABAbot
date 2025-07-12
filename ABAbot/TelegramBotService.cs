
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using static Telegram.Bot.TelegramBotClient;

public class TelegramBotService : BackgroundService
{
	private readonly ITelegramBotClient _botClient;
	private readonly IServiceProvider _services;

	public TelegramBotService(ITelegramBotClient botClient, IServiceProvider services)
	{
		_botClient = botClient;
		_services = services;
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var handler = new UpdateHandler(_services);

		_botClient.StartReceiving(
			updateHandler: handler.HandleUpdateAsync,
			errorHandler: handler.HandlePollingErrorAsync,
			receiverOptions: new ReceiverOptions(),
			cancellationToken: stoppingToken);

		Console.WriteLine("Бот запущен");
		return Task.CompletedTask;
	}
}
