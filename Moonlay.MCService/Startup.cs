using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Http;
using GraphQL.Server;
using GraphQL.Server.Ui.Playground;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Moonlay.MCService.Customers;
using Moonlay.MCService.Customers.GraphQL;
using Moonlay.MCService.Db;
using Swashbuckle.AspNetCore.Swagger;

namespace Moonlay.MCService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddDbContext<MyDbContext>(options => options.UseSqlServer(Configuration.GetSection("ConnectionStrings:Connection").Value));
            services.AddDbContext<MyDbTrailContext>(options => options.UseSqlServer(Configuration.GetSection("ConnectionStrings:ConnectionTrail").Value));
            services.AddTransient<IDbContext, MyDbContext>();
            services.AddTransient<IDbTrailContext, MyDbTrailContext>();

            services.AddTransient<ISignInService, SignInService>();
            services.AddTransient<Customers.ICustomerRepository, Customers.Repository>();
            services.AddTransient<Customers.ICustomerService, Customers.Service>();

            services.AddHttpContextAccessor();

            AddRestFullServices(services);

            // AddGraphQL(services);

            services.AddHostedService<KafkaConsumersHosted>();
            // services.AddScoped<IScopedProcessingService, ScopedProcessingService>();
        }

        private void AddRestFullServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "API",
                    Description = "Moonlay Web API BoilerPlate",
                    Contact = new OpenApiContact()
                    {
                        Name = "Afandy Lamusu",
                        Email = "afandy.lamusu@moonlay.com",
                        // Url = new Uri("www.dotnetdetail.net", UriKind.RelativeOrAbsolute)
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Moonlay",
                        // Url = new Uri("www.dotnetdetail.net", UriKind.RelativeOrAbsolute)
                    },
                });

            });
        }

        private void AddGraphQL(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<IDependencyResolver>(s => new FuncDependencyResolver(s.GetRequiredService));

            // services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
            // services.AddSingleton<IDocumentWriter, DocumentWriter>();

            services.AddSingleton<CustomerType>();
            // services.AddSingleton<StarWarsMutation>();
            // services.AddSingleton<HumanType>();
            // services.AddSingleton<HumanInputType>();
            // services.AddSingleton<DroidType>();
            // services.AddSingleton<CharacterInterface>();
            // services.AddSingleton<EpisodeEnum>();

            services.AddSingleton<DQuery>();
            services.AddSingleton<DMutation>();
            services.AddSingleton<ISchema, DSchema>();

            services.AddGraphQL(_ =>
            {
                _.EnableMetrics = true;
                _.ExposeExceptions = true;
            })
            .AddUserContextBuilder(httpContext => new GraphQLUserContext { User = httpContext.User });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            // add http for Schema at default url /graphql
            // app.UseGraphQL<ISchema>();

            // if (env.IsDevelopment())
            // use graphql-playground at default url /ui/playground
            // app.UseGraphQLPlayground(new GraphQLPlaygroundOptions());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Test API V1");
            });
        }
    }
}