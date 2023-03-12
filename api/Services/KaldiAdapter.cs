using api.Controllers;
using api.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

namespace api.Services;
public class KaldiAdapter : IKaldiAdapter
{
    private readonly string _endpoint;

    public KaldiAdapter(IOptions<ServiceEndpointsOptions> serviceEndpointsOptions)
    {
        _endpoint = serviceEndpointsOptions.Value.VoskKaldiRu;
    }

    public async Task<KaldiResult?> Recognize(IFormFile file)
    {
        KaldiResult? result = null;

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var fileBytes = ms.ToArray();

        var ws = new ClientWebSocket();
        try
        {
            await ws.ConnectAsync(new Uri(_endpoint), CancellationToken.None);

            await ProcessData(ws, fileBytes, fileBytes.Length);
            result = await ProcessFinalData(ws);

            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: {0}", ex);
            throw;
        }

        return result;
    }
    async Task<KaldiResult?> ReceiveResult(ClientWebSocket ws)
    {
        KaldiResult? kaldiResult = default;

        var bytes = new byte[4096];
        var receiveTask = ws.ReceiveAsync(new ArraySegment<byte>(bytes), CancellationToken.None);
        await receiveTask;
        var receivedString = Encoding.UTF8.GetString(bytes, 0, receiveTask.Result.Count);

        // todo try-catch
        kaldiResult = JsonConvert.DeserializeObject<KaldiResult>(receivedString);

        return kaldiResult;
    }

    async Task ProcessData(ClientWebSocket ws, byte[] data, int count)
    {
        await ws.SendAsync(new ArraySegment<byte>(data, 0, count), WebSocketMessageType.Binary, true, CancellationToken.None);
        await ReceiveResult(ws);
    }

    async Task<KaldiResult?> ProcessFinalData(ClientWebSocket ws)
    {
        var eof = Encoding.UTF8.GetBytes("{\"eof\" : 1}");
        await ws.SendAsync(new ArraySegment<byte>(eof), WebSocketMessageType.Text, true, CancellationToken.None);
        return await ReceiveResult(ws);
    }
}
