using AutoMapper;
using dsstats.api;
using dsstats.api.AuthContext;
using dsstats.api.Services;
using dsstats.db8;
using dsstats.db8.AutoMapper;
using dsstats.db8services;
using dsstats.db8services.DsData;
using dsstats.db8services.Import;
using dsstats.ratings;
using dsstats.shared;
using dsstats.shared.Interfaces;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using pax.dsstats.web.Server.Hubs;
using pax.dsstats.web.Server.Services.Arcade;
using System.Threading.RateLimiting;

var MyAllowSpecificOrigins = "dsstatsOrigin";

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("/data/localserverconfig.json", optional: true, reloadOnChange: false);

builder.Services.AddLogging(l => l.AddSimpleConsole(o => o.TimestampFormat = "yyyy-MM-dd HH:mm:ss: "));

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("https://dsstats.pax77.org",
                                             "https://dsstats-dev.pax77.org",
                                             "https://localhost:7257",
                                             "https://localhost:7227")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
});

builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(12);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));

// Add services to the container.
var serverVersion = new MySqlServerVersion(new System.Version(5, 7, 44));
var connectionString = builder.Configuration["ServerConfig:DsstatsConnectionString"];
var importConnectionString = builder.Configuration["ServerConfig:ImportConnectionString"];
var authConnectionString = builder.Configuration["ServerConfig:DsAuthConnectionString"];

builder.Services.AddOptions<DbImportOptions>()
    .Configure(x => x.ImportConnectionString = importConnectionString ?? "");

builder.Services.AddOptions<EMailOptions>()
    .Configure(x =>
    {
        x.Email = builder.Configuration["ServerConfig:EMail:email"] ?? "";
        x.Smtp = builder.Configuration["ServerConfig:EMail:smtp"] ?? "";
        x.Port = int.Parse(builder.Configuration["ServerConfig:EMail:port"] ?? "");
        x.Password = builder.Configuration["ServerConfig:EMail:auth"] ?? "";
    });

builder.Services.AddDbContext<ReplayContext>(options =>
{
    options.UseMySql(connectionString, serverVersion, p =>
    {
        p.CommandTimeout(30);
        p.EnableRetryOnFailure();
        p.MigrationsAssembly("MysqlMigrations");
        p.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
    })
    //.EnableDetailedErrors()
    //.EnableSensitiveDataLogging()
    ;
});

builder.Services.AddDbContext<DsAuthContext>(options =>
{
    options.UseMySql(authConnectionString, serverVersion, p =>
    {
        p.CommandTimeout(30);
        p.EnableRetryOnFailure();
        p.MigrationsAssembly("dsstats.api");
        p.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
    });
});

builder.Services.AddAuthentication(BearerTokenDefaults.AuthenticationScheme)
    .AddBearerToken(options =>
    {
        options.Validate();
    });
builder.Services.AddAuthorizationBuilder();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(DsPolicy.TourneyManager,
        policy => policy.RequireRole(["Admin", "Tourney"]));
    options.AddPolicy(DsPolicy.Admin,
        policy => policy.RequireRole("Admin"));
});

//builder.Services.AddIdentityCore<DsUser>(options =>
//    {
//        options.User.RequireUniqueEmail = true;
//        options.SignIn.RequireConfirmedEmail = true;
//    })
//    .AddRoles<IdentityRole>()
//    // .AddClaimsPrincipalFactory<DsClaimsFactory>()
//    .AddEntityFrameworkStores<DsAuthContext>()
//    .AddApiEndpoints();

builder.Services
    .AddIdentityApiEndpoints<DsUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = true;
        // options.Stores.ProtectPersonalData = true; 
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DsAuthContext>();

builder.Services.AddMemoryCache();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});
builder.Services.AddHttpClient("sc2arcardeClient")
.ConfigureHttpClient(options =>
{
    options.BaseAddress = new Uri("https://api.sc2arcade.com");
    options.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddSingleton<IRatingService, RatingService>();
builder.Services.AddSingleton<IRatingsSaveService, RatingsSaveService>();
builder.Services.AddSingleton<ImportService>();
builder.Services.AddSingleton<UploadService>();
builder.Services.AddSingleton<PickBanService>();
builder.Services.AddSingleton<AuthenticationFilterAttribute>();
builder.Services.AddSingleton<AuthenticationFilterAttributeV6>();
builder.Services.AddSingleton<IRemoteToggleService, RemoteToggleService>();
builder.Services.AddSingleton<DsUnitRepository>();

builder.Services.AddScoped<UserRepository>();

builder.Services.AddScoped<CrawlerService>();
builder.Services.AddScoped<IWinrateService, WinrateService>();
builder.Services.AddScoped<ITimelineService, TimelineService>();
builder.Services.AddScoped<ISynergyService, SynergyService>();
builder.Services.AddScoped<IDurationService, DurationService>();
builder.Services.AddScoped<IDamageService, DamageService>();
builder.Services.AddScoped<ICountService, CountService>();
builder.Services.AddScoped<ITeamcompService, TeamcompService>();
builder.Services.AddScoped<IReplaysService, ReplaysService>();
//builder.Services.AddScoped<IDsstatsService, DsstatsService>();
//builder.Services.AddScoped<IArcadeService, ArcadeService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IBuildService, BuildService>();
builder.Services.AddScoped<ICmdrInfoService, CmdrInfoService>();
builder.Services.AddScoped<IReplayRepository, ReplayRepository>();
builder.Services.AddScoped<ITourneysService, TourneysService>();
builder.Services.AddScoped<IUnitmapService, UnitmapService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IDsDataService, DsDataService>();

//builder.Services.AddScoped<EMailService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

if (builder.Environment.IsProduction())
{
    builder.Services.AddHostedService<TimedHostedService>();
}

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using var scope = app.Services.CreateScope();

var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
mapper.ConfigurationProvider.AssertConfigurationIsValid();

//var context = scope.ServiceProvider.GetRequiredService<ReplayContext>();
//context.Database.Migrate();

var authContext = scope.ServiceProvider.GetRequiredService<DsAuthContext>();
authContext.Database.Migrate();

var uploadSerivce = scope.ServiceProvider.GetRequiredService<UploadService>();
uploadSerivce.ImportInit();

app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    var userRepository = scope.ServiceProvider.GetRequiredService<UserRepository>();
    userRepository.Seed().Wait();
}

// app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PickBanHub>("/hubs/pickban");
app.MapGroup("/account")
    .MapIdentityApi<DsUser>()
    .RequireRateLimiting("fixed");

// app.MapIdentityApi<DsUser>();

app.Run();


