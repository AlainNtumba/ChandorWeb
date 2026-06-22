using ChandorAdmin;
using ChandorAdmin.Configuration;
using ChandorAdmin.Services.Auth;
using ChandorAdmin.Data;
using ChandorAdmin.Interfaces.Api;
using ChandorAdmin.Interfaces.Auth;
using ChandorAdmin.Interfaces.ChurchAdmin;
using ChandorAdmin.Services.ChurchAdmin;
using ChandorAdmin.Services.Api;
using Syncfusion.Blazor.Popups;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Syncfusion.Blazor;

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JHaF5cWWdCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdlWXtednVVRWddWUN3V0dWYEo=");

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddSyncfusionBlazor();
builder.Services.AddScoped<SfDialogService>();
builder.Services.Configure<ChandorApiOptions>(builder.Configuration.GetSection(ChandorApiOptions.SectionName));
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.SectionName));
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<InactivityMonitor>();

void ConfigureChandorBase(HttpClient client)
{
    var o = builder.Configuration.GetSection(ChandorApiOptions.SectionName).Get<ChandorApiOptions>() ?? new ChandorApiOptions();
    client.BaseAddress = new Uri(o.BaseUrl.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}

builder.Services.AddHttpClient("ChandorApi", ConfigureChandorBase);
builder.Services.AddHttpClient("ChandorApi.Auth", ConfigureChandorBase);

builder.Services.AddScoped<IAuthState, AuthState>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ChandorApiHttp>();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAgeGroupService, AgeGroupService>();
builder.Services.AddScoped<IAppUserService, AppUserService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IChurchProgramService, ChurchProgramService>();
builder.Services.AddScoped<CalendarDataAdaptor>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IDepartmentTeamService, DepartmentTeamService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IExpensesService, ExpensesService>();
builder.Services.AddScoped<IFinanceService, FinanceService>();
builder.Services.AddScoped<IExpensesTypeService, ExpensesTypeService>();
builder.Services.AddScoped<IIncomeService, IncomeService>();
builder.Services.AddScoped<IIncomeTypeService, IncomeTypeService>();
builder.Services.AddScoped<IMemberActivityService, MemberActivityService>();
builder.Services.AddScoped<IMemberRoleService, MemberRoleService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMemberTypeService, MemberTypeService>();
builder.Services.AddScoped<IMinistryService, MinistryService>();
builder.Services.AddScoped<IMinistiesScheduleService, MinistiesScheduleService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IOutreachesService, OutreachesService>();
builder.Services.AddScoped<IProgramTypeService, ProgramTypeService>();
builder.Services.AddScoped<ITelephoneService, TelephoneService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IChurchAdminDashboardService, ChurchAdminDashboardMockService>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
