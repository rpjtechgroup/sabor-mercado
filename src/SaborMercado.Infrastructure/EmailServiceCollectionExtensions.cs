using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaborMercado.Infrastructure.Email;

namespace SaborMercado.Infrastructure;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddSaborMercadoEmail(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        return services;
    }
}
