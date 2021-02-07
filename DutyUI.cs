using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Cysharp.Threading.Tasks;
using OpenMod.Unturned.Plugins;
using OpenMod.API.Plugins;
using DutyUI.Models;
using System.Collections.Generic;
using OpenMod.API.Permissions;
using Steamworks;

[assembly: PluginMetadata("SS.DutyUI", DisplayName = "DutyUI")]
namespace DutyUI
{
    public class DutyUI : OpenModUnturnedPlugin
    {
        public DutyUI(
            IConfiguration configuration, 
            ILogger<DutyUI> logger, 
            IPermissionRegistry permissionRegistry,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_Configuration = configuration;
            m_Logger = logger;
            m_PermissionRegistry = permissionRegistry;
        }

        protected override async UniTask OnLoadAsync()
        {
            // await UniTask.SwitchToMainThread();
            if (!m_Configuration.GetSection("configuration:enabled").Get<bool>())
            {
                m_Logger.LogInformation(" Plugin disabled! Please enable it in the config");
                await this.UnloadAsync();
                return; 
            }

            foreach (DutyGroup dgroup in m_Configuration.GetSection("DutyGroups").Get<List<DutyGroup>>())
            {
                m_PermissionRegistry.RegisterPermission(this, dgroup.Permission);
            }

            m_Logger.LogInformation("Plugin loaded correctly!");
        }

        protected override async UniTask OnUnloadAsync()
        {
            // await UniTask.SwitchToMainThread();
            m_Logger.LogInformation("Plugin unloaded correctly!");
        }

        private readonly IConfiguration m_Configuration;
        private readonly ILogger<DutyUI> m_Logger;
        private readonly IPermissionRegistry m_PermissionRegistry;

        internal List<CSteamID> DutysOn = new List<CSteamID>();
    }
}
