using IdGen;
using Senlin.Mo.Domain;

namespace Senlin.Mo;

internal class IdGenerator
{
    private readonly IdGen.IdGenerator _idGenerator;

    public IdGenerator()
    {
        var epoch = new DateTime(2023, 1, 1, 0, 0, 0);
        var structure = new IdStructure(42, 5, 16);
        var options = new IdGeneratorOptions(structure, new DefaultTimeSource(epoch));
        _idGenerator = new IdGen.IdGenerator(0, options);
    }

    public EntityId New() => new(_idGenerator.CreateId());
}