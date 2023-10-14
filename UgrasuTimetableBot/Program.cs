global using ArveCore.Botf;
global using System;
global using System.Collections.Generic;

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

            builder.Services.TryAddBotf(builder.Configuration.GetConnectionString("botf")!, default); //botf

            var app = builder.Build();

            app.TryUseBotf(dropPendingUpdates: true); //botf

            app.Run();
        }
    }
}