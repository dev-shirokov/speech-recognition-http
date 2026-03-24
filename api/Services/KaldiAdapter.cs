using api.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Text;

namespace api.Services;

public class KaldiAdapter : IKaldiAdapter
{
    readonly string _kaldiEndpoint;

    public KaldiAdapter(IOptions<ServiceEndpointsOptions> serviceEndpointsOptions)
    {
        _kaldiEndpoint = serviceEndpointsOptions.Value.VoskKaldiRu;
    }

    public async Task<KaldiResult?> Recognize(byte[] fileBytes, CancellationToken token)
    {
        KaldiResult? result = null;

        var ws = new ClientWebSocket();
        try
        {
            await ws.ConnectAsync(new Uri(_kaldiEndpoint), token);

            await ProcessData(ws, fileBytes, fileBytes.Length, token);

            result = await ProcessFinalData(ws, token);

            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", token);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: {0}", ex);
            throw;
        }
        finally
        {
            ws.Dispose();
        }

        return result;
    }

    public async Task<KaldiResult?> Recognize(Stream inputStream, CancellationToken token)
    {
        KaldiResult? result = null;

        var ws = new ClientWebSocket();
        try
        {
            await ws.ConnectAsync(new Uri(_kaldiEndpoint), token);

            await ProcessData(ws, inputStream, token);

            result = await ProcessFinalData(ws, token);

            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", token);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: {0}", ex);
            throw;
        }
        finally
        {
            ws.Dispose();
        }


        return result;
    }

    async Task ProcessData(ClientWebSocket webSocket, Stream inputStream, CancellationToken token)
    {
        const int BufferSize = 4096;
        var buffer = new byte[BufferSize];
        int bytesRead;

        while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
        {
            var segment = new ArraySegment<byte>(buffer, 0, bytesRead);

            bool isMessageEnd = (inputStream.Position == inputStream.Length);

            await webSocket.SendAsync(
                segment,
                WebSocketMessageType.Binary,
                isMessageEnd,
                token);
        }

        await ReceiveResult(webSocket, token);

    }


    async Task ProcessData(ClientWebSocket webSocket, byte[] data, int count, CancellationToken token)
    {
        await webSocket.SendAsync(new ArraySegment<byte>(data, 0, count), WebSocketMessageType.Binary, true, CancellationToken.None);
        await ReceiveResult(webSocket, token);
    }

    async Task<KaldiResult?> ReceiveResult(ClientWebSocket webSocket, CancellationToken token)
    {
        KaldiResult? kaldiResult = default;

        var bytes = new byte[4096];
        var receiveTask = await webSocket.ReceiveAsync(new ArraySegment<byte>(bytes), token);
        var receivedString = Encoding.UTF8.GetString(bytes, 0, receiveTask.Count);

        // todo try-catch
        kaldiResult = JsonConvert.DeserializeObject<KaldiResult>(receivedString);

        return kaldiResult;
    }

    async Task<KaldiResult?> ProcessFinalData(ClientWebSocket webSocket, CancellationToken token)
    {
        var eof = Encoding.UTF8.GetBytes("{\"eof\" : 1}");
        await webSocket.SendAsync(new ArraySegment<byte>(eof), WebSocketMessageType.Text, true, CancellationToken.None);
        return await ReceiveResult(webSocket, token);
    }
}
