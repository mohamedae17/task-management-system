using TaskManagement.Application.Common.Models;

namespace TaskManagement.Application.UnitTests.Common.Models;

public class ResultTests
{
    [Fact]
    public void Success_should_have_no_errors()
    {
        var result = Result.Success();

        result.Succeeded.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.ErrorCode.Should().BeNull();
    }

    [Fact]
    public void Failure_should_carry_error_messages()
    {
        var result = Result.Failure("first", "second");

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().BeEquivalentTo(new[] { "first", "second" });
    }

    [Fact]
    public void Failure_with_code_should_carry_code_and_messages()
    {
        var result = Result.FailureWithCode("E_CONFLICT", "duplicate key");

        result.Succeeded.Should().BeFalse();
        result.ErrorCode.Should().Be("E_CONFLICT");
        result.Errors.Should().ContainSingle().Which.Should().Be("duplicate key");
    }

    [Fact]
    public void Generic_Success_should_carry_value()
    {
        var result = Result<int>.Success(42);

        result.Succeeded.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Generic_Failure_should_have_default_value()
    {
        var result = Result<string>.Failure("nope");

        result.Succeeded.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().ContainSingle().Which.Should().Be("nope");
    }
}
