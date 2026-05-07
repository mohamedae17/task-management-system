using System.Net;
using System.Net.Http.Json;

namespace TaskManagement.Api.IntegrationTests;

public class HealthAndAuthTests : IClassFixture<TaskManagementWebApplicationFactory>
{
    private readonly TaskManagementWebApplicationFactory _factory;

    public HealthAndAuthTests(TaskManagementWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_endpoint_responds_200()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("healthy");
    }

    [Fact]
    public async Task Root_endpoint_describes_the_api()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("TaskManagement.Api");
        body.Should().Contain("/hubs/notifications");
    }

    [Fact]
    public async Task Protected_endpoint_returns_401_without_token()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/users/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Validation_failures_return_problem_details()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = "not-an-email", password = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Email").And.Contain("Password");
    }
}
