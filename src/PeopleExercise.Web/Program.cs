using MudBlazor.Services;
using PeopleExercise.Web.Components;
using PeopleExercise.Web.Configuration;
using PeopleExercise.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.Configure<StorageOptions>(
    builder.Configuration.GetSection(StorageOptions.SectionName));

builder.Services.AddScoped<IUserNotificationStore, UserNotificationStore>();
builder.Services.AddScoped<IPersonStorageService, FilePersonStorageService>();
builder.Services.AddScoped<IPersonSchemaService, ReflectionPersonSchemaService>();
builder.Services.AddScoped<IComuniStorageService, FileComuniStorageService>();
builder.Services.AddScoped<ITodoStorageService, FileTodoStorageService>();
builder.Services.AddScoped<IGuideHintService, FileGuideHintService>();



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var comuniStorageService = scope.ServiceProvider.GetRequiredService<IComuniStorageService>();
    try
    {
        await comuniStorageService.GetNomiComuniAsync();
    }
    catch (Exception exception)
    {
        app.Logger.LogWarning(exception, "Errore nel preload dei comuni all'avvio.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
