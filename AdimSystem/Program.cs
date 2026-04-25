using AdimSystem;
using AdimSystem.Components;
using AdimSystem.Configuration;
using AdimSystem.Data;
using AdimSystem.Interfaces;
using AdimSystem.Services;
using AdimSystem.Services.Api;
using AdimSystem.Services.Auth;
using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ChandorApiOptions>(builder.Configuration.GetSection(ChandorApiOptions.SectionName));

void ConfigureChandorBase(HttpClient client)
{
    var o = builder.Configuration.GetSection(ChandorApiOptions.SectionName).Get<ChandorApiOptions>() ?? new ChandorApiOptions();
    client.BaseAddress = new Uri(o.BaseUrl.TrimEnd('/') + "/");
}

builder.Services.AddHttpClient("ChandorApi", ConfigureChandorBase);
builder.Services.AddHttpClient("ChandorApi.Auth", ConfigureChandorBase);

builder.Services.AddScoped<IAuthState, AuthState>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ChandorApiHttp>();
builder.Services.AddScoped<CalendarDataAdaptor>();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAgeGroupService, AgeGroupService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IChurchProgramService, ChurchProgramService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IDepartmentTeamService, DepartmentTeamService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IExpensesService, ExpensesService>();
builder.Services.AddScoped<IExpensesTypeService, ExpensesTypeService>();
builder.Services.AddScoped<IIncomeService, IncomeService>();
builder.Services.AddScoped<IIncomeTypeService, IncomeTypeService>();
builder.Services.AddScoped<IMemberActivityService, MemberActivityService>();
builder.Services.AddScoped<IMemberRoleService, MemberRoleService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMemberTypeService, MemberTypeService>();
builder.Services.AddScoped<IMinistryService, MinistryService>();
builder.Services.AddScoped<IMinistiesScheduleService, MinistiesScheduleService>();
builder.Services.AddScoped<IOutreachesService, OutreachesService>();
builder.Services.AddScoped<IProgramTypeService, ProgramTypeService>();
builder.Services.AddScoped<ITelephoneService, TelephoneService>();
builder.Services.AddScoped<IUserService, UserService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<SidebarLayoutOptions>();
builder.Services.AddSyncfusionBlazor();

var app = builder.Build();
//Register Syncfusion license https://help.syncfusion.com/common/essential-studio/licensing/how-to-generate
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JHaF5cWWdCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdlWXtednVVRWddWUN3V0dWYEo=");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
