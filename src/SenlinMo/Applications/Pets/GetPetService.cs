using Senlin.Mo.Application.Abstractions;

namespace SenlinMo.Applications.Pets;

public class GetPetService : IService<GetPetDto, PetDto>
{
    public Task<PetDto> ExecuteAsync(GetPetDto request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new PetDto(request.Name));
    }
}