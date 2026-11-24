using Application.Common.Abstractions;
using Application.Common.Decorators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);



        /*
         Scrutor
        مكتبة صغيرة تضيف ميزتين لنظام 
        الـ Dependency Injection المدمج في .NET.
        1. Scanning (التسجيل التلقائي) :
             بدلاً من تسجيل كل خدمة يدوياً، يمكننا مسح الأسمبليز للعثور على الخدمات تلقائياً.
         */

        services.Scan(scan => scan.FromAssemblyOf<IApplicationDbContext>()
            .AddClasses(classes => classes
                .AssignableToAny(typeof(ICommandHandler<,>), typeof(IQueryHandler<,>))
                .Where(type => !type.IsGenericTypeDefinition), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)), publicOnly: false)
            .As(type => type.GetInterfaces().Where(service =>
                service.IsGenericType && service.GetGenericTypeDefinition() == typeof(IValidator<>)))
            .WithScopedLifetime());

        services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator.Command<,>));
        services.Decorate(typeof(IQueryHandler<,>), typeof(ValidationDecorator.Query<,>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.Command<,>));
        services.Decorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.Query<,>));

        return services;


        /*
        2. Decorate (التغليف)

        هذه الميزة تسمح لك بأن تغلّف كل هاندلر بطبقه اضافيه لوغ او تحقق قبل او بعده ان يصل التنفيذ له

          لـ MediatR 
        عادةً هو الذي يوفّر 
        الـ pipeline for validation & logging.
              

        LoggingDecorator
                    └─ ValidationDecorator
                                └─ CreateCourseCommandHandler
         */
    }
}
