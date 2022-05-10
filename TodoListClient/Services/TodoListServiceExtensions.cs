// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;

namespace TodoListClient.Services;

public static class TodoListServiceExtensions
{
    public static void AddTodoListService(this IServiceCollection services)
    {
        // https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        services.AddHttpClient<ITodoListService, TodoListService>();
    }
}
