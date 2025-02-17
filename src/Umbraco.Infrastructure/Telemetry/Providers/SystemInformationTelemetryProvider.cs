﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers
{
    internal class SystemInformationTelemetryProvider : IDetailedTelemetryProvider, IUserDataService
    {
        private readonly IUmbracoVersion _version;
        private readonly ILocalizationService _localizationService;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly Lazy<IUmbracoDatabase> _database;
        private readonly GlobalSettings _globalSettings;
        private readonly HostingSettings _hostingSettings;
        private readonly ModelsBuilderSettings _modelsBuilderSettings;

        public SystemInformationTelemetryProvider(
            IUmbracoVersion version,
            ILocalizationService localizationService,
            IOptions<ModelsBuilderSettings> modelsBuilderSettings,
            IOptions<HostingSettings> hostingSettings,
            IOptions<GlobalSettings> globalSettings,
            IHostEnvironment hostEnvironment,
            Lazy<IUmbracoDatabase> database)
        {
            _version = version;
            _localizationService = localizationService;
            _hostEnvironment = hostEnvironment;
            _database = database;
            _globalSettings = globalSettings.Value;
            _hostingSettings = hostingSettings.Value;
            _modelsBuilderSettings = modelsBuilderSettings.Value;
        }

        private string CurrentWebServer => IsRunningInProcessIIS() ? "IIS" : "Kestrel";

        private string ServerFramework => RuntimeInformation.FrameworkDescription;

        private string ModelsBuilderMode => _modelsBuilderSettings.ModelsMode.ToString();

        private string CurrentCulture => Thread.CurrentThread.CurrentCulture.ToString();

        private bool IsDebug => _hostingSettings.Debug;

        private bool UmbracoPathCustomized => _globalSettings.UmbracoPath != Constants.System.DefaultUmbracoPath;

        private string AspEnvironment => _hostEnvironment.EnvironmentName;

        private string ServerOs => RuntimeInformation.OSDescription;

        private string DatabaseProvider => _database.Value.DatabaseType.GetProviderName();

        public IEnumerable<UsageInformation> GetInformation() =>
            new UsageInformation[]
            {
                new(Constants.Telemetry.ServerOs, ServerOs),
                new(Constants.Telemetry.ServerFramework, ServerFramework),
                new(Constants.Telemetry.OsLanguage, CurrentCulture),
                new(Constants.Telemetry.WebServer, CurrentWebServer),
                new(Constants.Telemetry.ModelsBuilderMode, ModelsBuilderMode),
                new(Constants.Telemetry.CustomUmbracoPath, UmbracoPathCustomized),
                new(Constants.Telemetry.AspEnvironment, AspEnvironment),
                new(Constants.Telemetry.IsDebug, IsDebug),
                new(Constants.Telemetry.DatabaseProvider, DatabaseProvider),
            };

        public IEnumerable<UserData> GetUserData() =>
            new UserData[]
            {
                new("Server OS", ServerOs),
                new("Server Framework", ServerFramework),
                new("Default Language", _localizationService.GetDefaultLanguageIsoCode()),
                new("Umbraco Version", _version.SemanticVersion.ToSemanticStringWithoutBuild()),
                new("Current Culture", CurrentCulture),
                new("Current UI Culture", Thread.CurrentThread.CurrentUICulture.ToString()),
                new("Current Webserver", CurrentWebServer),
                new("Models Builder Mode", ModelsBuilderMode),
                new("Debug Mode", IsDebug.ToString()),
                new("Database Provider", DatabaseProvider),
            };

        private bool IsRunningInProcessIIS()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return false;
            }

            string processName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);
            return (processName.Contains("w3wp") || processName.Contains("iisexpress"));
        }
    }
}
