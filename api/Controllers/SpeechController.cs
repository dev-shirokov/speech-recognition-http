using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using speech.api.Controllers;

namespace api.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class SpeechController : Controller
    {
        private readonly ServiceEndpointsOptions _serviceEndpointsOptions;

        public SpeechController(IOptions<ServiceEndpointsOptions> serviceEndpointsOptions)
        {
            _serviceEndpointsOptions = serviceEndpointsOptions.Value;
        }

        [HttpPost("")]
        public async Task<IActionResult> Post(IFormFile file)
        {
            string result = null!;

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                var fileBytes = ms.ToArray();

                var ws = new ClientWebSocket();
                await ws.ConnectAsync(new Uri(_serviceEndpointsOptions.VoskKaldiRu), CancellationToken.None);

                await ProcessData(ws, fileBytes, fileBytes.Length);
                result = await ProcessFinalData(ws);

                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", CancellationToken.None);
            }

            return Ok(result);
        }

        async Task<string> ReceiveResult(ClientWebSocket ws)
        {
            var result = new byte[4096];
            Task<WebSocketReceiveResult> receiveTask = ws.ReceiveAsync(new ArraySegment<byte>(result), CancellationToken.None);
            await receiveTask;
            var receivedString = Encoding.UTF8.GetString(result, 0, receiveTask.Result.Count);
            Console.WriteLine("Result {0}", receivedString);

            return receivedString;
        }

        async Task ProcessData(ClientWebSocket ws, byte[] data, int count)
        {
            await ws.SendAsync(new ArraySegment<byte>(data, 0, count), WebSocketMessageType.Binary, true, CancellationToken.None);
            await ReceiveResult(ws);
        }

        async Task<string> ProcessFinalData(ClientWebSocket ws)
        {
            var eof = Encoding.UTF8.GetBytes("{\"eof\" : 1}");
            await ws.SendAsync(new ArraySegment<byte>(eof), WebSocketMessageType.Text, true, CancellationToken.None);
            return await ReceiveResult(ws);
        }
    }
}
