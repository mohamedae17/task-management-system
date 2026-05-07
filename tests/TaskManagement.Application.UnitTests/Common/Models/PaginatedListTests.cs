using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Models;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Application.UnitTests.Common.Models;

public class PaginatedListTests
{
    [Fact]
    public async Task CreateAsync_should_paginate_with_count()
    {
        await using var ctx = NewContext();
        for (var i = 0; i < 25; i++)
        {
            ctx.Labels.Add(new Label { Name = $"L{i:00}", ColorHex = "#FFFFFF" });
        }
        await ctx.SaveChangesAsync();

        var page1 = await PaginatedList<Label>.CreateAsync(ctx.Labels.OrderBy(l => l.Name), 1, 10);
        var page3 = await PaginatedList<Label>.CreateAsync(ctx.Labels.OrderBy(l => l.Name), 3, 10);

        page1.TotalCount.Should().Be(25);
        page1.Items.Should().HaveCount(10);
        page1.HasNextPage.Should().BeTrue();
        page1.HasPreviousPage.Should().BeFalse();

        page3.Items.Should().HaveCount(5);
        page3.HasNextPage.Should().BeFalse();
        page3.HasPreviousPage.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(1, 0, 1, 20)]   // pageSize 0 -> default 20
    [InlineData(1, 500, 1, 200)] // pageSize > max -> clamped 200
    public async Task CreateAsync_should_clamp_pagination_arguments(
        int pageNumber, int pageSize, int expectedPage, int expectedSize)
    {
        await using var ctx = NewContext();
        var page = await PaginatedList<Label>.CreateAsync(ctx.Labels, pageNumber, pageSize);

        page.PageNumber.Should().Be(expectedPage);
        page.PageSize.Should().Be(expectedSize);
    }

    private static ApplicationDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"paginated-{Guid.NewGuid()}")
            .Options;
        return new ApplicationDbContext(options, Array.Empty<Microsoft.EntityFrameworkCore.Diagnostics.ISaveChangesInterceptor>());
    }
}
