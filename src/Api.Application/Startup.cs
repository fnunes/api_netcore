using System.Collections.Generic;
using System;
using Api.CrossCutting.DependencyInjection;
using Api.Domain.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Api.CrossCutting.Mappings;
using AutoMapper;

namespace application
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
            var config = new AutoMapper.MapperConfiguration(cfg =>
           {
               cfg.AddProfile(new DtoToModelProfile());
               cfg.AddProfile(new EntityToDtoProfile());
               cfg.AddProfile(new ModelToEntityProfile());
           });
            IMapper mapper = config.CreateMapper();
            services.AddSingleton(mapper);

            ConfigureService.ConfigureDependenciesService(services);
            ConfigureRepository.ConfigureDependenciesRepositories(services);

            var tokenConfigurations = new TokenConfigurations();
            var signingConfigurations = new SigningConfigurations();
            services.AddSingleton(signingConfigurations);
            new ConfigureFromConfigurationOptions<TokenConfigurations>(
                Configuration.GetSection("TokenConfigurations"))
                    .Configure(tokenConfigurations);
            services.AddSingleton(tokenConfigurations);

            services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearerOptions =>
            {
                var paramsValidation = bearerOptions.TokenValidationParameters;
                paramsValidation.IssuerSigningKey = signingConfigurations.Key;
                paramsValidation.ValidAudience = tokenConfigurations.Audience;
                paramsValidation.ValidIssuer = tokenConfigurations.Issuer;
                paramsValidation.ValidateIssuerSigningKey = true;
                paramsValidation.ValidateLifetime = true;
                paramsValidation.ClockSkew = TimeSpan.Zero;
            });

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().Build());
            });

            services.AddControllers();
            services.AddSwaggerGen(swagger =>
           {
               swagger.SwaggerDoc("v1", new OpenApiInfo
               {
                   Version = "v1",
                   Title = "Aprendendo APIs com .NetCore",
                   Description = ".Net com DDD - Aprendendo",

                   TermsOfService = new Uri("https://fabianofnunes.com.br"),
                   Contact = new OpenApiContact
                   {
                       Name = "Fabiano F. Nunes",
                       Email = "contato@fabianofnunes.com.br",
                       Url = new Uri("https://fabianofnunes.com.br")
                   },
                   License = new OpenApiLicense
                   {
                       Name = "Termo para Licença de Uso",
                       Url = new Uri("https://fabianofnunes.com.br")
                   }
               });

               swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
               {
                   Description = "Insira um token JWT válido para a API",
                   Name = "Authorization",
                   In = ParameterLocation.Header,
                   Type = SecuritySchemeType.ApiKey
               });

               swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
               {
                    {
                    new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                                {
                                    Id = "Bearer",
                                    Type = ReferenceType.SecurityScheme
                                }
                            }, new List<string>()
                        }
                   }
               );
           });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(swagger =>
            {
                swagger.SwaggerEndpoint("/swagger/v1/swagger.json", "Aprendendo APIs com .Net");
                swagger.RoutePrefix = string.Empty;
            });
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
