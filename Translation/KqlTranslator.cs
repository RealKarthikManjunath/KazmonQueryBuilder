using Anthropic;
using Anthropic.Models.Messages;
using KazmonQueryBuilder.Display;

namespace KazmonQueryBuilder.Translation;

public class KqlTranslator(AnthropicClient client)
{
    private static readonly string SystemPrompt = $"""
        You are an expert in Kusto Query Language (KQL), used in Azure Data Explorer, Azure Monitor, Microsoft Sentinel, and Log Analytics.

        Your job is to translate natural language queries into precise, correct KQL.

        Guidelines:
        - Output ONLY the KQL query — no explanations, no markdown fences, no extra text.
        - Use proper KQL operators: where, summarize, project, extend, join, union, render, bin(), ago(), now(), count(), avg(), sum(), max(), min(), dcount(), tostring(), toint(), datetime(), etc.
        - Prefer readable, well-formatted multi-line KQL.
        - If the intent is ambiguous, make a reasonable assumption and produce a valid query.
        - Always pick the most relevant table from the list below based on the user's intent.
        - Keep result sets reasonable — add a top 100 or limit unless the user asks for all results.

        {TableCatalog.SystemPromptSection}
        """;

    public async Task<string> TranslateAsync(string naturalLanguage)
    {
        var parameters = new MessageCreateParams
        {
            Model = Model.ClaudeOpus4_6,
            MaxTokens = 2048,
            Thinking = new ThinkingConfigAdaptive(),
            System = SystemPrompt,
            Messages = [new() { Role = Role.User, Content = naturalLanguage }]
        };

        var kqlBuilder = new System.Text.StringBuilder();

        await foreach (var streamEvent in client.Messages.CreateStreaming(parameters))
        {
            if (streamEvent.TryPickContentBlockDelta(out var delta) &&
                delta.Delta.TryPickText(out var text))
            {
                kqlBuilder.Append(text.Text);
            }
        }

        return kqlBuilder.ToString().Trim();
    }
}
