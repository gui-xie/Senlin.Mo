using FluentAssertions;

namespace Senlin.Mo.Domain.Test;

public static class ResultTest
{
    [Fact]
    public static void SuccessTest()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public static void FailTest()
    {
        var result = Result.Fail("error");

        result.IsSuccess.Should().BeFalse();
        result.GetErrorMessage().Should().Be("error");
    }

    [Fact]
    public static void FailWithDelegate()
    {
        var result = Result.Fail(() => "ERROR");
        
        result.IsSuccess.Should().BeFalse();
        result.GetErrorMessage().Should().Be("ERROR");
    }

    [Fact]
    public static async Task SuccessTaskTest()
    {
        var resultTask = Result.SuccessTask();

        var result = await resultTask;

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public static void SuccessTTest()
    {
        var data = new
        {
            Name = "Senlin"
        };
        
        var result = Result.Success(data);
        
        result.IsSuccess.Should().BeTrue();
        result.GetData()?.Name.Should().Be("Senlin");
    }
    
    [Fact]
    public static async Task SuccessTaskTTest()
    {
        var data = new
        {
            Name = "Senlin"
        };
        
        var resultTask = Result.SuccessTask(data);

        var result = await resultTask;

        result.IsSuccess.Should().BeTrue();
        result.GetData()?.Name.Should().Be("Senlin");
    }
    
    [Fact]
    public static void FailTTest()
    {
        var result = Result.Fail<object>("error");

        result.IsSuccess.Should().BeFalse();
        result.GetErrorMessage().Should().Be("error");
    }
    
    [Fact]
    public static void FailTWithDelegate()
    {
        var result = Result.Fail<object>(() => "ERROR");
        
        result.IsSuccess.Should().BeFalse();
        result.GetErrorMessage().Should().Be("ERROR");
    }
}