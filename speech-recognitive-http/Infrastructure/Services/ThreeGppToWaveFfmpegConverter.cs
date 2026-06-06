using api.Application.Services;
using System.Diagnostics;
using System.Text;

namespace api.Infrastructure.Services;

public class ThreeGppToWaveFfmpegConverter() : IConverter
{
    readonly string ffmpegPath = Path.Combine("dist", "ffmpeg", "ffmpeg.exe");

    public async Task Convert(Stream inputStream, Stream outputStream)
    {
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        using var process = new Process();
        process.EnableRaisingEvents = true;
        process.StartInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = $@"-i pipe:0 -acodec pcm_s16le -ar 8000 -ac 1 -f wav pipe:1", // from stream to stream
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var errorCommandLogs = new StringBuilder();
        process.ErrorDataReceived += (sender, e) =>
        {
            errorCommandLogs.AppendLine(e.Data);
        };

        //Запускаем процесс
        process.Start();
        process.BeginErrorReadLine();

        var writeStreamTask = Task.Run(async () =>
        {
            using var stdin = process.StandardInput.BaseStream;
            await inputStream.CopyToAsync(stdin);
            // Обязательно закрываем stdin, чтобы FFmpeg понял, что входные данные закончились
            stdin.Close();
        }, tokenSource.Token);


        var readerStreamTask = Task.Run(async () =>
        {
            using var stdout = process.StandardOutput.BaseStream;
            await stdout.CopyToAsync(outputStream);
        }, tokenSource.Token);

        await Task.WhenAll(writeStreamTask, readerStreamTask).WaitAsync(tokenSource.Token);
        //Дожидаемся завершение процесса и закрываем его
        await process.WaitForExitAsync();

        Console.WriteLine(errorCommandLogs.ToString());

        outputStream.Position = 0;

        if (process.ExitCode != 0)
        {
            throw new Exception($"FFmpeg завершился с ошибкой (код {process.ExitCode})");
        }
        process.Close();
    }
}
