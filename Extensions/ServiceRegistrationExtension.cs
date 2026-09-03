using sjam.Bal.Interfaces;
using sjam.Bal.Services;
using sjam.Dal;
using sjam.Dal.Interfaces;
using sjam.Dal.Repositories;
using sjam.Helpers;
using sjam.RabbitMQ.Common;
using sjam.RabbitMQ.Interfaces;
using sjam.RabbitMQ.Repository;
using System.Data;

namespace sjam.Extensions
{
    public static class ServiceRegistrationExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<DapperContext>();
            services.AddScoped<ApiConfigFilter>();
            services.AddHostedService<ApiConfigRefreshService>();
            services.AddScoped<IAuthClaimService, AuthClaimService>();
            services.AddScoped<IRabbitMqService, RabbitMqService>();
            services.AddTransient<IMQueueProcessingService, MQueueProcessingService>(); 
            services.AddScoped<CaptchaHelper>();
            services.AddScoped<IMasterService, MasterService>();
            services.AddScoped<IPublicService, PublicService>();
            services.AddScoped<IOtpService, OtpService>();

            return services;
        }
        public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IMasterRepo, MasterRepo>();
            services.AddScoped<IPublicRepo, PublicRepo>();
            services.AddScoped<IOtpRepo, OtpRepo>();


            #region RabbitMQ repository
            services.AddTransient<IMessageQueueRepository, MessageQueueRepository>();
            services.AddTransient<IConsumeLogRepository, ConsumeLogRepository>();
            services.AddTransient<IConsumeFailedLogRepository, ConsumeFailedLogRepository>();
            services.AddTransient<IMessageQueueFailedLogsRepository, MessageQueueFailedLogsRepository>();
            services.AddTransient<IPublishedAcknowledgementLogRepository, PublishedAcknowledgementLogRepository>();
            services.AddTransient<IConsumedAcknowledgementLogRepository, ConsumedAcknowledgementLogRepository>();
            #endregion

            return services;
        }
        public static IServiceCollection AddRabbitMQServices(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddRabbitMQ(configuration).AddMessageProcessing(configuration);

            //services.AddTransient<IMessageQueueRepository, MessageQueueRepository>();
            //services.AddTransient<IConsumeLogRepository, ConsumeLogRepository>();
            //services.AddTransient<IConsumeFailedLogRepository, ConsumeFailedLogRepository>();
            //services.AddTransient<IMessageQueueFailedLogsRepository, MessageQueueFailedLogsRepository>();
            //services.AddTransient<IPublishedAcknowledgementLogRepository, PublishedAcknowledgementLogRepository>();
            //services.AddTransient<IConsumedAcknowledgementLogRepository, ConsumedAcknowledgementLogRepository>();

            //services.AddTransient<IDbConnection>(sp => sp.GetRequiredService<DapperContext>().CreateConnection());
            return services;
        }
    }
}
