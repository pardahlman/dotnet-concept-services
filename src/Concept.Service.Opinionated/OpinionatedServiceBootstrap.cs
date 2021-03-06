﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Serilog;

namespace Concept.Service.Opinionated
{
  public abstract class OpinionatedServiceBootstrap<TService> : ServiceBootstrap<TService>, IDisposable where TService : Service
  {
    protected IContainer AutofacContainer;

    public override void ConfigureLogger()
    {
      var loggerConfig = new LoggerConfiguration();
      ConfigureSerilog(loggerConfig);
      Log.Logger = loggerConfig.CreateLogger();
    }

    public override void RegisterDependencies()
    {
      var builder = new ContainerBuilder();
      RegisterDependencies(builder);
      AutofacContainer = builder.Build();
    }

    protected override Task<TService> CreateServiceAsync(CancellationToken ct = default (CancellationToken))
    {
      return Task.FromResult(AutofacContainer.Resolve<TService>());
    }

    protected abstract void RegisterDependencies(ContainerBuilder builder);

    protected virtual void ConfigureSerilog(LoggerConfiguration config)
    {
      var meta = CreateMetadata();
      config
        .WriteTo.LiterateConsole()
        .Enrich.WithEnvironmentUserName()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.FromLogContext()
        .Enrich.WithProperty(LoggerProperty.Application, meta.Name)
        .Enrich.WithProperty(LoggerProperty.Version, meta.Version);
    }

    public virtual void Dispose()
    {
      AutofacContainer.Dispose();
    }
  }
}
