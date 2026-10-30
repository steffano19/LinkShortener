using System.Net;
using System.Net.Http.Json;
using LinkShortener.Api.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LinkShortener.Tests;

public class LinksApiTests : IClassFixture<TestAppFactory>
{
    private readonly TestAppFactory _factory;

    public LinksApiTests(TestAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_ValidUrl_ReturnsCreatedWithCode()
    {
        var client = _factory.CreateClient();
        var request = new CreateLinkRequest { OriginalUrl = "https://github.com" };

        var response = await client.PostAsJsonAsync("/api/links", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<LinkResponse>();
        Assert.NotNull(body);
        Assert.Equal(6, body.Code.Length);
        Assert.Equal("https://github.com", body.OriginalUrl);
    }

    [Fact]
    public async Task Create_InvalidUrl_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var request = new CreateLinkRequest { OriginalUrl = "привет" };

        var response = await client.PostAsJsonAsync("/api/links", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReservedCode_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var request = new CreateLinkRequest
        {
            OriginalUrl = "https://github.com",
            CustomCode = "api"
        };

        var response = await client.PostAsJsonAsync("/api/links", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateCustomCode_ReturnsConflict()
    {
        var client = _factory.CreateClient();
        var request = new CreateLinkRequest
        {
            OriginalUrl = "https://github.com",
            CustomCode = "dup-test"
        };

        var first = await client.PostAsJsonAsync("/api/links", request);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/links", request);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Redirect_ExistingCode_RedirectsAndCountsClick()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var request = new CreateLinkRequest
        {
            OriginalUrl = "https://github.com",
            CustomCode = "redirect-test"
        };
        await client.PostAsJsonAsync("/api/links", request);

        var response = await client.GetAsync("/redirect-test");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("github.com", response.Headers.Location!.ToString());

        var stats = await client.GetFromJsonAsync<StatsResponse>("/api/links/redirect-test/stats");
        Assert.NotNull(stats);
        Assert.Equal(1, stats.ClicksCount);
    }

    [Fact]
    public async Task Redirect_UnknownCode_ReturnsNotFound()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/no-such-code");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingLink_RemovesIt()
    {
        var client = _factory.CreateClient();

        var request = new CreateLinkRequest
        {
            OriginalUrl = "https://github.com",
            CustomCode = "delete-test"
        };
        await client.PostAsJsonAsync("/api/links", request);

        var deleteResponse = await client.DeleteAsync("/api/links/delete-test");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync("/delete-test");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}