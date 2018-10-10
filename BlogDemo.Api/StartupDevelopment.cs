using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogDemo.Infrastructure.DataBase;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using BlogDemo.Core.Interfaces;
using BlogDemo.Infrastructure.Repositories;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using BlogDemo.Api.Extensions;
using AutoMapper;
using FluentValidation;
using BlogDemo.Infrastructure.Resources;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using BlogDemo.Infrastructure.Services;
using Newtonsoft.Json.Serialization;
using FluentValidation.AspNetCore;

namespace BlogDemo.Api
{
    public class StartupDevelopment
    {
        public IConfiguration Configuration { get; }
        public StartupDevelopment(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.ReturnHttpNotAcceptable = true;
                //options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                var intputFormatter = options.InputFormatters.OfType<JsonInputFormatter>().FirstOrDefault();
                if (intputFormatter != null)
                {
                    intputFormatter.SupportedMediaTypes.Add("application/vnd.cgzl.post.create+json");
                    intputFormatter.SupportedMediaTypes.Add("application/vnd.cgzl.post.update+json");
                }

                var outputFormatter = options.OutputFormatters.OfType<JsonOutputFormatter>().FirstOrDefault();
                if (outputFormatter != null)
                {
                    outputFormatter.SupportedMediaTypes.Add("application/vnd.cgzl.hateoas+json");
                }
            })
            .AddJsonOptions(options=>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            })
            .AddFluentValidation();
            services.AddDbContext<MyContext>(options =>
            {
                // var connectionString = Configuration["ConnectionStrings:DefaultConnection"];
                //var connectionString = Configuration.GetConnectionString("DefaultConnection");
                //options.UseSqlite(connectionString);
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });
            //services.AddHttpsRedirection(options =>
            //{
            //    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
            //    options.HttpsPort = 5001;
            //});
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(factory =>
            {
                var actionContext = factory.GetService<IActionContextAccessor>().ActionContext;
                return  new UrlHelper(actionContext);
            });
            services.AddAutoMapper();
            services.AddTransient<IValidator<PostAddResource>, PostAddOrUpdateResourceValidator<PostAddResource>>();
            services.AddTransient<IValidator<PostUpdateResource>, PostAddOrUpdateResourceValidator<PostUpdateResource>>();
            var propertyMappingContainer = new PropertyMappingContainer();
            propertyMappingContainer.Register<PostPropertyMapping>();
            services.AddSingleton<IPropertyMappingContainer>(propertyMappingContainer);

            services.AddTransient<ITypeHelperService, TypeHelperService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseMyExceptionHandler(loggerFactory);
            
            //app.UseDeveloperExceptionPage();
            //app.UseHttpsRedirection();
            if (env.IsProduction())
            {

            }
            if (env.IsStaging())
            {

            }
            if (env.IsEnvironment("Xxx"))
            {

            }
            app.UseMvc();
            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Hello World!");
            //});
        }
    }
}
