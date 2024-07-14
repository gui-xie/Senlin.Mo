using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Domain;
using IResult = Senlin.Mo.Domain.IResult;

namespace SenlinMo.Applications.Pets;

public class AddPetService : ICommandService<AddPetDto>
{
    public Task<IResult> ExecuteAsync(AddPetDto request, CancellationToken cancellationToken)
    {
        return Result.SuccessTask();
    }
}

public class AddPetEvent : IDomainEvent
{
    
}