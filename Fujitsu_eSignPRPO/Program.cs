using eSignPRPO.Data;
using eSignPRPO.interfaces;
using eSignPRPO.Models.Mail;
using eSignPRPO.Services.Account;
using eSignPRPO.Services.Customer;
using eSignPRPO.Services.Mail;
using eSignPRPO.Services.Profiles;
using eSignPRPO.Services.PRPO;
using eSignPRPO.Services.Workflow;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Web;

namespace eSignPRPO
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("ConDB");
            builder.Services.AddDbContext<ESignPrpoContext>(option => option.UseSqlServer(connectionString));

            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IPRPOService, PRPOService>();
            builder.Services.AddScoped<IWorkflowService, WorkflowService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IProfilesService, ProfilesService>();
            builder.Services.AddTransient<IMailService, MailService>();

            var path = Directory.GetCurrentDirectory();

            builder.Logging.ClearProviders();
            builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            builder.Host.UseNLog();


            builder.Services.AddAuthentication("eSignPRPO").AddCookie("eSignPRPO", option =>
            {
                option.Cookie.Name = "eSignPRPO";
                option.ExpireTimeSpan = TimeSpan.FromHours(16);
                option.LoginPath = "/Account/Login";
                option.AccessDeniedPath = "/Account/AccessDenied";
                option.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                option.SlidingExpiration = true;
            });

            builder.Services.AddAuthorization(option =>
            {
                option.AddPolicy("0",
                    policy => policy.RequireClaim("0"));
                option.AddPolicy("1",
                    policy => policy.RequireClaim("1"));
            });
            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=PRPO}/{action=Worklist}/{id?}");

            app.Run();
        }


    }
}