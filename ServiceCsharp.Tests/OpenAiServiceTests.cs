using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using service_csharp.Data;
using service_csharp.Models;
using service_csharp.Services;
using Xunit;

namespace service_csharp.Tests;

public class OpenAiServiceTests
{
    private static ParadoxContext BuildContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ParadoxContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ParadoxContext(options);
    }

    private static HttpClient BuildHttpClient(string responseJson, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        return new HttpClient(handler.Object);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldReturnAssistantText_WhenOpenAiRespondsText()
    {
        // Arrange
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", "dummy-key");
        var context = BuildContext(nameof(SendMessageAsync_ShouldReturnAssistantText));
        var userId = Guid.NewGuid();
        var conversation = new AiConversation { Id = Guid.NewGuid(), CreatedBy = userId };
        context.AiConversations.Add(conversation);
        await context.SaveChangesAsync();

        var openAiReply = """
        {
          "choices": [{
            "message": { "content": "Hola ágil" }
          }]
        }
        """;
        var httpClient = BuildHttpClient(openAiReply);
        var service = new OpenAiService(httpClient, context, Mock.Of<IConfiguration>(), new BoardTools(context));

        // Act
        var result = await service.SendMessageAsync(conversation.Id, "Hola", userId);

        // Assert
        Assert.Equal("Hola ágil", result);
        var messages = await context.AiMessages.ToListAsync();
        Assert.Equal(2, messages.Count); // user + assistant
    }

    [Fact]
    public async Task SendMessageAsync_ShouldThrow_WhenConversationNotOwned()
    {
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", "dummy-key");
        var context = BuildContext(nameof(SendMessageAsync_ShouldThrow_WhenConversationNotOwned));
        var conversation = new AiConversation { Id = Guid.NewGuid(), CreatedBy = Guid.NewGuid() };
        context.AiConversations.Add(conversation);
        await context.SaveChangesAsync();

        var httpClient = BuildHttpClient("""
        { "choices": [{ "message": { "content": "ok" } }] }
        """);
        var service = new OpenAiService(httpClient, context, Mock.Of<IConfiguration>(), Mock.Of<BoardTools>());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.SendMessageAsync(conversation.Id, "hola", Guid.NewGuid()));
    }
}
