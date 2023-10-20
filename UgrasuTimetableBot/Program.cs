global using ArveCore.Botf;
global using ArveCore.Extensions;
global using System;
global using System.Collections.Generic;
using UgrasuTimetableBot.IOControl;
using UgrasuTimetableBot.Services;

namespace UgrasuTimetableBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (!File.Exists("appsettings.json"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Error ] File appsettings.json is not exists");
                Console.ResetColor();
                return;
            }

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped<ScheduleApi>();
            builder.Services.AddSingleton<InMemoryStorage>();
            builder.Services.AddHostedService<EntityUpdateService>();

            builder.Services.TryAddBotf(builder.Configuration.GetConnectionString("botf")!, default); //botf

            builder.WebHost.UseUrls("http://localhost:5001"); // Any ports (In default uses to auto setting webhooks to botf endpoint)

            var app = builder.Build();

            app.TryUseBotf(dropPendingUpdates: true); //botf

            app.Run();
        }
    }
}