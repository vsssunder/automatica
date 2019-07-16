﻿using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Automatica.Core.Base.Common;
using Automatica.Core.EF.Models;
using Automatica.Core.Runtime.Abstraction.Plugins;
using Automatica.Core.Runtime.Abstraction.Plugins.Drivers;
using Automatica.Core.Runtime.Abstraction.Plugins.Logics;
using Microsoft.Extensions.Logging;

namespace Automatica.Core.Runtime.Core.Plugins
{
    internal class PluginHandler : IPluginHandler
    {
        private readonly ILogger<PluginHandler> _logger;
        private readonly AutomaticaContext _dbContext;
        private readonly IDriverLoader _driverLoader;
        private readonly ILogicLoader _logicLoader;
        private readonly object _lock = new object();

        public PluginHandler(ILogger<PluginHandler> logger, AutomaticaContext dbContext, IDriverLoader driverLoader, ILogicLoader logicLoader)
        {
            _logger = logger;
            _dbContext = dbContext;
            _driverLoader = driverLoader;
            _logicLoader = logicLoader;
        }

        public Task CheckAndInstallPluginUpdates()
        {
            var updateDirectory = Path.Combine(Path.GetTempPath(), ServerInfo.PluginUpdateDirectoryName);
            if (!Directory.Exists(updateDirectory))
            {
                Directory.CreateDirectory(updateDirectory);
                return Task.CompletedTask;
            }

            var files = Directory.GetFiles(updateDirectory);

            foreach (var f in files)
            {
                try
                {
                    var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));
                    var manifest = Common.Update.Plugin.GetPluginManifest(_logger, f, tmp);

                    Directory.Delete(tmp, true);
                    if (manifest == null)
                    {
                        _logger.LogWarning($"Could no update plugin with file from {f}");

                        File.Delete(f);
                        continue;
                    }

                    var assemblyDir = new FileInfo(GetEntryAssemblyPath()).DirectoryName;
                    var pluginType = manifest.Automatica.Type == "driver" ? ServerInfo.DriversDirectory : ServerInfo.LogicsDirectory;
                    var pluginDir = Path.Combine(assemblyDir, pluginType);
                    var componentDir = Path.Combine(pluginDir, manifest.Automatica.ComponentName);

                    if (Directory.Exists(componentDir))
                    {
                        Directory.Delete(componentDir, true);
                    }
                    Common.Update.Plugin.InstallPlugin(f, pluginDir);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not install plugin");
                }
                File.Delete(f);
            }
            return Task.CompletedTask;
        }

        public async Task LoadPlugin(Plugin plugin)
        {

            if (plugin.PluginType == PluginType.Driver)
            {
                var factories = PluginLoader.LoadSingle(_logger, plugin, _dbContext);

                foreach (var factory in factories)
                {
                    await _driverLoader.Load(factory);
                }
            }
            else if (plugin.PluginType == PluginType.Logic)
            {
                var factories = RuleLoader.LoadSingle(_logger, plugin, _dbContext);

                foreach (var factory in factories)
                {
                    await _logicLoader.Load(factory);
                }
            }

        }

        private string GetEntryAssemblyPath()
        {
            return Assembly.GetEntryAssembly()?.Location;
        }
    }
}