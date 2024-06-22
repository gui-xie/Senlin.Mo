using Moq;
using Senlin.Mo.Application.Abstractions.Decorators;

namespace Senlin.Mo.Application.Abstractions.Test.Decorators;

public class UnitOfWorkServiceAttributeTest
{
    [Fact]
    public async Task UnitOfWorkServiceShouldBeExecuted()
    {
        var updateCommand = new UpdateCommand();
        var updateUserService = new UpdateUserService();
        var unitOfWorkHandler = new Mock<IUnitOfWorkHandler>();
        UnitOfWorkServiceAttribute<UpdateCommand, bool> unitOfWorkServiceAttribute =
            new(updateUserService, unitOfWorkHandler.Object);

        _ = await unitOfWorkServiceAttribute.ExecuteAsync(updateCommand, CancellationToken.None);

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
        UnitOfWorkServiceAttribute<UpdateCommand, bool> unitOfWorkServiceAttribute =
            new(updateUserService, unitOfWorkHandler.Object)
            {
                IsEnable = false
            };

        _ = await unitOfWorkServiceAttribute.ExecuteAsync(updateCommand, CancellationToken.None);

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