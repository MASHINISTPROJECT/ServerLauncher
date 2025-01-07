using System;
using System.Diagnostics;
using System.IO;
using Serilog;
using Serilog.Events;

namespace ServerLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            // Настройка Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(outputTemplate: "[{Timestamp:dd.MM.yy HH:mm}] [{Level:u4}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("Приложение запущено.");
            Thread.Sleep(1200); // wait 20 seconds

            // Запрос пути к файлу сервера у пользователя
            string serverFilePath = GetServerFilePathFromUser();

            if (string.IsNullOrEmpty(serverFilePath))
            {
                Log.Error("Путь к файлу сервера не был предоставлен. Приложение завершает работу.");
                Thread.Sleep(1200); // wait 20 seconds
                Log.CloseAndFlush();
                return;
            }

            try
            {
                // Запуск сервера
                StartServer(serverFilePath);
            }
            catch (FileNotFoundException ex)
            {
                Log.Error(ex, "Файл сервера не найден: {FilePath}", serverFilePath);
                Thread.Sleep(1200); // wait 20 seconds
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при запуске сервера.");
                Thread.Sleep(1200); // wait 20 seconds
            }


            Log.Information("Приложение завершает работу.");
            Thread.Sleep(1200); // wait 20 seconds
            Log.CloseAndFlush();
        }
        static string GetServerFilePathFromUser()
        {
            Console.Write("Введите путь к файлу сервера: ");
            return Console.ReadLine();
        }

        static void StartServer(string serverFilePath)
        {

            if (!File.Exists(serverFilePath))
            {
                throw new FileNotFoundException("Файл сервера не найден.", serverFilePath);
            }

            Log.Information("Запуск сервера из файла: {FilePath}", serverFilePath);
            Thread.Sleep(1200); // wait 20 seconds

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = serverFilePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true, // Не создавать окно
            };

            using (Process process = Process.Start(startInfo))
            {
                if (process == null)
                {
                    Log.Error("Не удалось запустить процесс.");
                    Thread.Sleep(1200); // wait 20 seconds
                    return;
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Log.Information("[SERVER_OUT]: {Data}", e.Data);
                        Thread.Sleep(1200); // wait 20 seconds
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Log.Error("[SERVER_ERR]: {Data}", e.Data);
                        Thread.Sleep(1200); // wait 20 seconds
                    }
                };

                process.WaitForExit();
                Log.Information("Сервер завершил работу. Код выхода: {ExitCode}", process.ExitCode);
                Thread.Sleep(3600); // wait 1 min


            }
        }
    }
}