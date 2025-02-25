using AgricHub.API.Extension;
using AgricHub.Contracts;
using AgricHub.DAL;
using AgricHub.DAL.Context;
using AgricHub.Presentation.Filters;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();

builder.Services.Configure<FormOptions>(o =>
{
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartBodyLengthLimit = int.MaxValue;
    o.MemoryBufferThreshold = int.MaxValue;
});

builder.Services.ConfigureIdentity();
builder.Services.ConfigureEmail(builder.Configuration);

builder.Services.ConfigureJWT(builder.Configuration);

builder.Services.ConfigureSqlContext(builder.Configuration);

builder.Services.AddScoped<ValidationFilterAttribute>();

builder.Services.AddControllers().AddApplicationPart(typeof(AgricHub.Presentation.AssemblyReference ).Assembly);

builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AgricHub", Version = "v1" });


    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description =
            "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                            Array.Empty<string>()
                    },
                });
});


builder.Services.AddScoped<IUnitOfWork, UnitOfWork<AgricHubDbContext>>();
builder.Services.ConfigureServices();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(Assembly.Load("AgricHub.BLL"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var app = builder.Build();
app.ConfigureExceptionHandler();

app.UseCors("CorsPolicy");

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Resources")),
    RequestPath = new PathString("/Resources")
});



app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});
app.UseAuthentication();
/*app.UseHttpsRedirection();*/

app.UseAuthorization();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AgricHub v1");
});

using (var scope = app.Services.CreateScope())
{
    var servicesProvider = scope.ServiceProvider;
    var roleManager = servicesProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await DataSeeder.SeedRoles(roleManager);

    /*var userManager = servicesProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await DataSeeder.SeedUsers(userManager);*/
}

app.Run();
