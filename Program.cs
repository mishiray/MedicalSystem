using MedicalSystem.Configurations;
using MedicalSystem.Data;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ConnectionString");
builder.Services.ConfigureDatabase(connectionString);

// Add services to the container.
builder.Services.AddScopedServices();


builder.Services.ConfigureAuthentication(builder.Configuration);

builder.Services.ConfigureAuthorization();
builder.Services.ConfigureAutoMapper();
builder.Services.ConfigureSwagger();

builder.Services.ConfigureAppSetting(builder.Configuration);

builder.Services.ConfigureDefaultIdentity();


builder.Services.AddApiVersioningExtension();


builder.Services.AddApiVersionedExplorerExtension();

builder.Services.AddScopedServices();

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();



app.UseCors(option => option.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());


app.UseHttpsRedirection(); 
await SeedDatabaseAsync();
app.UseStaticFiles();
app.UseRouting();
app.UseHsts();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionMiddleware>();

IApiVersionDescriptionProvider provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
    }
});

app.MapControllers();

app.Run();

async Task SeedDatabaseAsync() 
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<Seeder>();
        await dbInitializer.SeedIt();
    }
}
