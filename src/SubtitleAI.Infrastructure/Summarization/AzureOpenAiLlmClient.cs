// Положить в: src/SubtitleAI.Infrastructure/Summarization/AzureOpenAiLlmClient.cs
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using SubtitleAI.Application.Summarization;

namespace SubtitleAI.Infrastructure.Summarization;

public class AzureOpenAiOptions
{
    /// <summary>https://&lt;resource&gt;.openai.azure.com/</summary>
    public string Endpoint { get; set; } = "";

    /// <summary>The Azure AI Foundry deployment name, not the model name.</summary>
    public string Deployment { get; set; } = "";

    /// <summary>When empty, uses DefaultAzureCredential: local az login or an Azure managed identity.</summary>
    public string? ApiKey { get; set; }

    public float Temperature { get; set; } = 0.2f;
    public int MaxOutputTokens { get; set; } = 1500;
}

public sealed class AzureOpenAiLlmClient : ILlmClient
{
    private readonly ChatClient _chat;
    private readonly AzureOpenAiOptions _opt;

    public AzureOpenAiLlmClient(IOptions<AzureOpenAiOptions> options)
    {
        _opt = options.Value;

        var endpoint = new Uri(_opt.Endpoint);
        var client = string.IsNullOrWhiteSpace(_opt.ApiKey)
            ? new AzureOpenAIClient(endpoint, new DefaultAzureCredential())
            : new AzureOpenAIClient(endpoint, new AzureKeyCredential(_opt.ApiKey));

        _chat = client.GetChatClient(_opt.Deployment);
    }

    public string ModelName => _opt.Deployment;

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        // Reasoning deployments (o-series / GPT-5) may not support Temperature.
        var options = new ChatCompletionOptions
        {
            Temperature = _opt.Temperature,
            MaxOutputTokenCount = _opt.MaxOutputTokens
        };

        ChatCompletion completion = await _chat.CompleteChatAsync(
            [new SystemChatMessage(systemPrompt), new UserChatMessage(userPrompt)],
            options,
            ct);

        if (completion.FinishReason == ChatFinishReason.ContentFilter)
            throw new InvalidOperationException("Azure OpenAI content filter blocked the response.");

        // Content may be empty due to filtering or refusal; the caller handles an empty response.
        return completion.Content.Count > 0 ? completion.Content[0].Text : "";
    }
}
