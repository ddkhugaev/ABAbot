using ABAbot.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

Host.CreateDefaultBuilder(args)
	.ConfigureServices((_, services) =>
	{
		services.AddSingleton<ITelegramBotClient>(_ =>
			new TelegramBotClient("7621258848:AAFKhDb77WhEdbpCoi7pNjYtm2_S6ePY-Qc"));
		services.AddSingleton<Dictionary<long, UserSession>>();
		services.AddHostedService<TelegramBotService>();

		// Поключение бд
		services.AddDbContext<DatabaseContext>(options =>
        {
            options.UseSqlite("Data Source=ababot.db");
        });
        services.AddTransient<IUsersRepository, UsersDbRepository>();
        services.AddTransient<IIkigaiesRepository, IkigaiesDbRepository>();
    })
	.Build()
	.Run();