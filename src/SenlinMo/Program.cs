using Senlin.Mo.Application.Abstractions;
using SenlinMo.Applications.Pets;
using IResult = Senlin.Mo.Domain.IResult;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IService<AddPetDto, IResult>, AddPetService>();
builder.Services.AddTransient<IAddPetService, IAddPetService.AddPetServiceImpl>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapPost("pet", (
        AddPetDto addPetDto,
        IAddPetService service,
        CancellationToken cancellationToken)
        => service.ExecuteAsync(addPetDto, cancellationToken))
    .WithName("AddPet")
    .WithOpenApi();

app.Run();

