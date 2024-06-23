using Microsoft.Extensions.Logging;
using Moq;
using Senlin.Mo.Application.Abstractions.Decorators;
using Senlin.Mo.Application.Abstractions.Decorators.Log;
using static Moq.It;

namespace Senlin.Mo.Application.Abstractions.Test.Decorators;

public class LogServiceTest
{
    [Fact]
    public async Task LogServiceShouldBeExecuted()
    {
        var updateCommand = new UpdateCommand();
        var updateUserService = new UpdateUserService();
        var logger = new Mock<ILogger<LogService<UpdateCommand, bool>>>();
        LogService<UpdateCommand, bool> logService =
            new(updateUserService, logger.Object, () => "1");

        _ = await logService.ExecuteAsync(updateCommand, CancellationToken.None);

        logger.Verify(x =>
                x.Log(LogLevel.Information,
                    IsAny<EventId>(),
                    IsAny<object>(), 
                    IsAny<Exception>(),
                    ((Func<object, Exception, string>)IsAny<object>())!),
            Times.Once);
    }
    
    [Fact]
    public async Task LogServiceShouldNotBeExecuted()
    {
        var updateCommand = new UpdateCommand();
        var updateUserService = new UpdateUserService();
        var logger = new Mock<ILogger<LogService<UpdateCommand, bool>>>();
        LogService<UpdateCommand, bool> logService =
            new(updateUserService, logger.Object, () => "1")
            {
                AttributeData = new LogAttribute(false)
            };

        _ = await logService.ExecuteAsync(updateCommand, CancellationToken.None);

        logger.Verify(x =>
                x.Log(LogLevel.Information,
                    IsAny<EventId>(),
                    IsAny<object>(), 
                    IsAny<Exception>(),
                    ((Func<object, Exception, string>)IsAny<object>())!),
            Times.Never);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public record UpdateCommand;

    private class UpdateUserService : IService<UpdateCommand, bool>
    {
        public Task<bool> ExecuteAsync(UpdateCommand request, CancellationToken cancellationToken) => Task.FromResult(true);
    }
}