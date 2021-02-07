using Cysharp.Threading.Tasks;
using DutyUI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Eventing;
using OpenMod.API.Permissions;
using OpenMod.API.Users;
using OpenMod.Core.Users.Events;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DutyUI
{
    public class Events
    {
        public class PlayerDisconnect : IEventListener<IUserDisconnectedEvent>
        {
            public PlayerDisconnect(DutyUI dutyUI,
                IPermissionChecker permissionChecker, 
                IConfiguration configuration, 
                IPermissionRoleStore permissionRoleStore, 
                IStringLocalizer stringLocalizer,
                IUserManager userManager)
            {
                m_DutyUI = dutyUI;
                m_PermissionChecker = permissionChecker;
                m_Configuration = configuration;
                m_PermissionRoleStore = permissionRoleStore;
                m_StringLocalizer = stringLocalizer;
                m_UserManager = userManager;
            }

            public async Task HandleEventAsync(object sender, IUserDisconnectedEvent @event)
            {
                UnturnedUser user = (UnturnedUser)@event.User;

                if (m_DutyUI.DutysOn.Contains(user.SteamId))
                {
                    m_DutyUI.DutysOn.Remove(user.SteamId);
                    List<DutyGroup> dutygroups = m_Configuration.GetSection("DutyGroups").Get<List<DutyGroup>>();
                    if (dutygroups.Count > 1)
                    {
                        foreach (DutyGroup dgroup in dutygroups)
                        {
                            if (await m_PermissionChecker.CheckPermissionAsync(@event.User, dgroup.Permission) == PermissionGrantResult.Grant)
                            {
                                if (await m_PermissionRoleStore.RemoveRoleFromActorAsync(@event.User, dgroup.GroupID))
                                {
                                    await UniTask.SwitchToMainThread();
                                    EffectManager.sendUIEffect(31201, 201, true, m_StringLocalizer["plugin_translations:duty_off", new { player = user.DisplayName }]);
                                    await m_UserManager.BroadcastAsync(m_StringLocalizer["plugin_translations:duty_off", new { player = user.DisplayName }], System.Drawing.Color.FromName(m_Configuration.GetSection("configuration:announcecolor").Get<string>()));
                                }
                            }
                        }
                    }
                }
            }

            private readonly IUserManager m_UserManager;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IPermissionRoleStore m_PermissionRoleStore;
            private readonly IConfiguration m_Configuration;
            private readonly IPermissionChecker m_PermissionChecker;
            private readonly DutyUI m_DutyUI;
        }
    }
}
