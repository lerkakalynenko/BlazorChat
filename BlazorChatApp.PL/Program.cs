using System.Net.Http.Headers;
using AutoMapper;
using BlazorChatApp.BLL.Contracts.DTOs;
using BlazorChatApp.BLL.Infrastructure.Interfaces;
using BlazorChatApp.BLL.Infrastructure.Services;
using BlazorChatApp.BLL.Mappings;
using BlazorChatApp.DAL.Data.Interfaces;
using BlazorChatApp.DAL.Data.Repositories;
using BlazorChatApp.DAL.Domain.EF;
using BlazorChatApp.PL.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Blazored.LocalStorage;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration
    .GetConnectionString("ConnectionString") ??
                       throw new InvalidOperationException
                           ("Connection string 'ConnectionString' not found.");
builder.Services.AddDbContext<BlazorChatAppContext>(options =>
    options.UseSqlServer(connectionString)); ;

builder.Services.AddRazorPages();
builder.Services.AddSignalRCore();
builder.Services.AddServerSideBlazor();
builder.Services.AddTransient<AccountController>();
builder.Services.AddTransient<MessageController>();
//builder.Services.AddTransient<AuthController>();
builder.Services.AddTransient<IAuthorizationService, AuthorizationService>();
builder.Services.AddTransient<IMessageRepository, MessageRepository>();
builder.Services.AddTransient<IChatRepository, ChatRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<UserManager<IdentityUser>>();

builder.Services.AddSingleton<LoginDto>();
builder.Services.AddSingleton<RegisterDto>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<BlazorChatAppContext>()
    .AddDefaultTokenProviders();

// http client base url and default request header
builder.Services.AddHttpClient("Authorization", async client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Url:Route"]);
   // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
        //await Activator.CreateInstance<ILocalStorageService>().GetItemAsStringAsync("token"));
} );

builder.Services.AddSingleton<HttpClient>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})

    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
        };
    });

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
});

var mapper = mapperConfig.CreateMapper();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{

    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseAuthentication();

app.UseCookiePolicy();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapControllers();

app.Run();
