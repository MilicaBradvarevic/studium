using Application;
using Application.Courses;
using Domain;
using Infrastructure;
using Infrastructure.Demo;

var builder = WebApplication.CreateBuilder(args);

var dataFolder = Path.Combine(builder.Environment.ContentRootPath, "App_Data");

// Composition root: spoljašnji sloj bira implementaciju ugovora.
builder.Services.AddSingleton<ICourseReader>(_ => DemoData.CreateCourseReader());
builder.Services.AddScoped<ListCoursesHandler>();
builder.Services.AddSingleton<ITimskiZadaciIFazeRepozitorijum>(_ =>
    new JsonTimskiZadaciIFazeRepozitorijum(dataFolder));

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Javno dostupan demonstracioni endpoint, bez autentifikacije:
// ne predstavlja funkcionalnost ST-003 i ne koristiti ga za stvarne podatke.
app.MapGet("/demo/courses", async (
    ListCoursesHandler handler,
    CancellationToken cancellationToken) =>
{
    var courses = await handler.HandleAsync(cancellationToken);
    return Results.Ok(courses);
});

app.MapPost("/api/st023-st024/timovi/demo", async (
    ITimskiZadaciIFazeRepozitorijum repo,
    CancellationToken ct) =>
{
    var timId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    var projekatId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    var clan = new ClanTimaSnapshot(
        Guid.Parse("33333333-3333-3333-3333-333333333333"),
        "Milica Bradvarevic");

    var tim = new Tim(timId, projekatId, "Tim Alfa", [clan]);
    await repo.SacuvajTimAsync(tim, ct);
    var ucitani = await repo.VratiTimPoIdAsync(timId, ct);

    return Results.Ok(new
    {
        savedToFile = Path.Combine(dataFolder, "st023_st024.json"),
        tim = ucitani
    });
});

app.MapGet("/api/st023-st024/timovi/{id:guid}", async (
    Guid id,
    ITimskiZadaciIFazeRepozitorijum repo,
    CancellationToken ct) =>
{
    var tim = await repo.VratiTimPoIdAsync(id, ct);
    return tim is null ? Results.NotFound() : Results.Ok(tim);
});

app.Run();
