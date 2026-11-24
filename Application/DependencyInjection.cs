using Application.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        /*
        MediatR
        يحل هنا محل Scrutor في شيئين:

        1. التسجيل التلقائي:
           RegisterServicesFromAssembly يبحث في مشروع Application
           عن كل IRequestHandler ويسجّله تلقائياً.

        2. الـ pipeline بدل الـ Decorators:
           AddOpenBehavior يغلّف كل handler بطبقة تعمل قبله وبعده.
           الترتيب مهم: أول behavior يُسجَّل هو الطبقة الخارجية.

           LoggingBehavior
                    └─ ValidationBehavior
                                └─ CreateCourseCommandHandler
        */
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Validators are internal sealed, so internal types must be included.
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly, ServiceLifetime.Scoped, includeInternalTypes: true);

        return services;
    }
}
