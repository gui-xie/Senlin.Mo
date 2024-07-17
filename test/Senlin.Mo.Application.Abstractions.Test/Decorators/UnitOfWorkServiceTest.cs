using Microsoft.Extensions.Logging;
using Moq;
using Senlin.Mo.Application.Abstractions.Decorators;
using Senlin.Mo.Application.Abstractions.Decorators.UnitOfWork;

namespace Senlin.Mo.Application.Abstractions.Test.Decorators;

public class UnitOfWorkServiceTest
{
    [Fact]
    public async Task UnitOfWorkServiceShouldBeExecuted()
    {
        var updateCommand = new UpdateCommand();
        var updateUserService = new UpdateUserService();
        var unitOfWorkHandler = new Mock<IUnitOfWorkHandler>();
        var eventHandler = new Mock<IEventExecutor>();
        var logger = new Mock<ILogger<UnitOfWorkService<UpdateCommand, bool>>>();
        UnitOfWorkService<UpdateCommand, bool> unitOfWorkService =
            new(updateUserService, unitOfWorkHandler.Object, eventHandler.Object, logger.Object);

        _ = await unitOfWorkService.ExecuteAsync(updateCommand, CancellationToken.None);

        unitOfWorkHandler.Verify(x =>
                x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task UnitOfWorkServiceShouldNotBeExecuted()
    {
        var updateCommand = new UpdateCommand();
        var updateUserService = new UpdateUserService();
        var unitOfWorkHandler = new Mock<IUnitOfWorkHandler>();
        var eventHandler = new Mock<IEventExecutor>();
        var logger = new Mock<ILogger<UnitOfWorkService<UpdateCommand, bool>>>();
        UnitOfWorkService<UpdateCommand, bool> unitOfWorkService =
            new(updateUserService, unitOfWorkHandler.Object, eventHandler.Object, logger.Object)
            {
                AttributeData = new UnitOfWorkAttribute(false)
            };

        _ = await unitOfWorkService.ExecuteAsync(updateCommand, CancellationToken.None);

        unitOfWorkHandler.Verify(x =>
                x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }


    private record UpdateCommand;

    private class UpdateUserService : IService<UpdateCommand, bool>
    {
        public Task<bool> ExecuteAsync(UpdateCommand request, CancellationToken cancellationToken) => Task.FromResult(true);
    }
}