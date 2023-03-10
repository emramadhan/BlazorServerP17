using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using BookApp.Interfaces;
using BookApp.Data;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace BookApp
{
   public class Startup
   {
      public Startup(IConfiguration configuration)
      {
         Configuration = configuration;
      }

      public IConfiguration Configuration { get; }

      // This method gets called by the runtime. Use this method to add services to the container.
      // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
      public void ConfigureServices(IServiceCollection services)
      {
         services.AddRazorPages();
         services.AddServerSideBlazor();

         //BookAuthor service  
         services.AddScoped<IBookAuthorService, BookAuthorService>();
         //Author service  
         services.AddScoped<IAuthorService, AuthorService>();
         //Publisher service  
         services.AddScoped<IPublisherService, PublisherService>();
         //Book service  
         services.AddScoped<IBookService, BookService>();
         //Register dapper in scope  
         services.AddScoped<IDapperService, DapperService>();
         //Google Authentication service         
         services.AddAuthentication().AddGoogle(googleOptions =>
         {
            googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];
            googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
         });
         services.AddAuthentication().AddFacebook(facebookOptions =>
         {
            facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
            facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];

            facebookOptions.Events = new OAuthEvents()
            {
               OnRemoteFailure = loginFailureHandler =>
               {
                  var authProperties = facebookOptions.StateDataFormat.Unprotect(loginFailureHandler.Request.Query["state"]);
                  loginFailureHandler.Response.Redirect("/Identity/Account/Login");
                  loginFailureHandler.HandleResponse();
                  return Task.FromResult(0);
               }
            };
         });
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
      {
         if (env.IsDevelopment())
         {
            app.UseDeveloperExceptionPage();
         }
         else
         {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
         }

         app.UseHttpsRedirection();
         app.UseStaticFiles();

         app.UseRouting();

         app.UseAuthentication();
         app.UseAuthorization();

         app.UseEndpoints(endpoints =>
         {
            endpoints.MapControllers();
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
         });
      }
   }
}