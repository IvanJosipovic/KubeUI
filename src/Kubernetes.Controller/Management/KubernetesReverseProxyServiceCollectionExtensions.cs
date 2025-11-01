// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Configuration;
using Yarp.Kubernetes.Controller;
using Yarp.Kubernetes.Controller.Client;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions for <see cref="IServiceCollection"/>
/// used to register the Kubernetes-based ReverseProxy's components.
/// </summary>
public static class KubernetesReverseProxyServiceCollectionExtensions
{
    /// <summary>
    /// Registers the resource informer.
    /// </summary>
    /// <typeparam name="TResource">The type of the t related resource.</typeparam>
    /// <typeparam name="TService">The implementation type of the resource informer.</typeparam>
    /// <param name="services">The services.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection RegisterResourceInformer<TResource, TService>(this IServiceCollection services)
        where TResource : class, IKubernetesObject<V1ObjectMeta>, new()
        where TService : IResourceInformer<TResource>
    {
        return services.RegisterResourceInformer<TResource, TService>(null);
    }

    /// <summary>
    /// Registers the resource informer with a field selector.
    /// </summary>
    /// <typeparam name="TResource">The type of the t related resource.</typeparam>
    /// <typeparam name="TService">The implementation type of the resource informer.</typeparam>
    /// <param name="services">The services.</param>
    /// <param name="fieldSelector">A field selector to constrain the resources the informer retrieves.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection RegisterResourceInformer<TResource, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection services, string fieldSelector)
        where TResource : class, IKubernetesObject<V1ObjectMeta>, new()
        where TService : IResourceInformer<TResource>
    {
        services.AddSingleton(new ResourceSelector<TResource>(fieldSelector));
        services.AddSingleton(typeof(IResourceInformer<TResource>), typeof(TService));

        return services.RegisterHostedService<IResourceInformer<TResource>>();
    }
}
