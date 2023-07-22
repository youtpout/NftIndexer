using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NftIndexer.Entities;
using NftIndexer.Interfaces;
using NftIndexer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<IIndexationService, IndexationService>();

builder.Services.AddHostedService<IndexationBackgroundTaskService>();

builder.Services.AddControllersWithViews();

// AutoMapper
var configuration = new MapperConfiguration(cfg =>
{
  
});
var mapper = configuration.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<NftIndexerContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("NftDatabase"), o => o.CommandTimeout(30)).UseSnakeCaseNamingConvention());


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<NftIndexerContext>();
    context.Database.EnsureCreated();

    // migrate any database changes on startup (includes initial db creation)
    context.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();

