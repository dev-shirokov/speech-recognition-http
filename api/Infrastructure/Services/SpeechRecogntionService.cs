using api.Application.Services;
using api.Infrastructure.Configurations.Options;
using MassTransit.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OllamaSharp;

namespace api.Infrastructure.Services;

public class SpeechRecognitionService : ISpeechRecognitionService
{
    private readonly IChatClient _chatClient;
    private readonly List<ChatMessage> _chatHistory;
    private readonly ILogger<SpeechRecognitionService> _logger;

    public SpeechRecognitionService(IOptions<LlmOptions> options, ILogger<SpeechRecognitionService> logger)
    {
        _logger = logger;

        // Инициализация клиента
        _chatClient = new OllamaApiClient(new Uri(options.Value.Endpoint), options.Value.ModelName);

        _chatHistory = new List<ChatMessage>();

        var systemPrompt = @"Ты — ассистент для создания задач. 
        Проанализируй голосовой запрос пользователя и извлеки из него параметры для создания задачи.
        
        Задача может быть нескольких типов: 
        1. Напоминание о каком-то событии, активности, мероприятии - reminder;
        2. Долгосрочная цель, которой нужно достигнуть пользователю - goal;
        3. Задача, которую нужно выполнить - task.
        
        Задача может иметь один из следующих приоритетов:
        - high - высокий;
        - medium - средний (обычный);
        - low - низкий;
        - unknown - исключительный случай, когда пользовательский запрос не релевантен для ассистента.

        Название задачи должно всегда начинаться с глагола, например, сходить к врачу, передать отчет руководителю, почистить обувь и так далее.        
                
        Формат ответа должен быть из следующих полей:
        {
            ""type"":""тип задачи, строковое значение, может быть только одним из типов задач - remind или goal или issue, поле всегда обязательно"",
            ""title"": ""название, строковое значение, всегда обязательно"",
            ""due_date"": ""срок выполнения, строковое значение, необязательно, если в запросе уточняется время или дата или период, то значение выставляем в формате yyyy-MM-ddTHH:mm:ss"",
            ""priority"": ""приоритет (срочность) задачи, строковое значение, поле обязательно, если в запросе указана срочность (приоритет) запроса, то значение может быть: high или medium или low, по-умолчанию всегда выставляем medium""
        }

        Если пользовательский запрос семантически не относится к постановке задачи, цели, напоминании о событии, активности или мероприятии, то необходимо возращать ответ следующего вида:
        {
            ""type"":""unknown"",
            ""errorMessage"":""Не удалось распознать пользовательский запрос"",
        }

        Всегда отвечай строго в формате JSON, соблюдая все правила написания, без пояснений, комментариев или лишнего текста.
        Поля заполнять на том языке, на котором разговаривает пользователь.
        ";


        _chatHistory.Add(new ChatMessage(ChatRole.System, systemPrompt));
    }

    public async Task<string> ProcessAsync(string speechText, CancellationToken token)
    {
        try
        {
            speechText += $"{Environment.NewLine}Дата сегодня: {DateTime.Now}";

            // Добавляем запрос пользователя в историю
            _chatHistory.Add(new ChatMessage(ChatRole.User, speechText));

            // Отправляем запрос и получаем ответ
            var response = await _chatClient.GetResponseAsync(_chatHistory, cancellationToken: token);

            var result = string.Join(". ", response.Messages.Select(x => x.Text));

            _logger.LogDebug("Запрос: '{0}'\nОтвет: {1}", speechText, result);

            // Добавляем ответ ассистента в историю
            _chatHistory.Add(new ChatMessage(ChatRole.Assistant, result));

            return result;
        }
        catch (Exception ex)
        {
            return $"{{\"error\": \"Ошибка при обработке: {ex.Message}\"}}";
        }
    }
}
