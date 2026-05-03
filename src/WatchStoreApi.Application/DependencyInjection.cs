using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WatchStoreApi.Application.Interfaces;
using WatchStoreApi.Application.Services;
using WatchStoreApi.Application.Validators.Auth;

namespace WatchStoreApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IAdminService, AdminService>();

        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        return services;
    }
}
