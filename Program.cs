var builder = WebApplication.CreateBuilder(args);

// Lägg till tjänster till DI-kontainern
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<WeatherService>(); // Registrera WeatherService

var app = builder.Build();

// Konfigurera HTTP-pipelinen
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();