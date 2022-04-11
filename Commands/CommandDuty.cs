using Cysharp.Threading.Tasks;
using DutyUI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Permissions;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Command = OpenMod.Core.Commands.Command;

namespace DutyUI.Commands
{
    [Command("duty")]
    [CommandDescription("Enter or leave from duty.")]
    [CommandActor(typeof(UnturnedUser))]
    public class CommandDuty : Command
    {
        public CommandDuty(IServiceProvider serviceProvider, 
            IConfiguration configuration, 
            IStringLocalizer stringLocalizer,
            IPermissionChecker permissionChecker, 
            IPermissionRoleStore permissionRoleStore, 
            IUserDataStore userDataStore,
            DutyUI dutyUI) : base(serviceProvider)
        {
            m_Configuration = configuration;
            m_StringLocalizer = stringLocalizer;
            m_PermissionChecker = permissionChecker;
            m_PermissionRoleStore = permissionRoleStore;
            m_DutyUI = dutyUI;
            m_UserDataStore = userDataStore;
        }

        protected override async Task OnExecuteAsync()
        {
            UnturnedUser actor = (UnturnedUser)Context.Actor;
            List<DutyGroup> dutygroups = m_Configuration.GetSection("DutyGroups").Get<List<DutyGroup>>();
            if (dutygroups.Count < 1)
            {
                await actor.PrintMessageAsync("No groups set up.");
            }
            else
            {
                foreach (DutyGroup dgroup in dutygroups)
                {
                    if (await m_PermissionChecker.CheckPermissionAsync(actor, dgroup.Permission) == PermissionGrantResult.Grant)
                    {
                        var userData = await m_UserDataStore.GetUserDataAsync(actor.Id, actor.Type);
                        if (!userData.Roles.Contains(dgroup.GroupID))
                        {
                            await m_PermissionRoleStore.AddRoleToActorAsync(actor, dgroup.GroupID);
                            m_DutyUI.DutysOn.Add(actor.SteamId);
                            await UniTask.SwitchToMainThread();
                            EffectManager.sendUIEffect(31200, 200, actor.SteamId, true);
                            ChatManager.serverSendMessage(m_StringLocalizer["plugin_translations:duty_on", new { player = actor.DisplayName }], UnityEngine.Color.green, null, null, EChatMode.SAY, null, true);
                        }
                        else if (userData.Roles.Contains(dgroup.GroupID))
                        {
                            await m_PermissionRoleStore.RemoveRoleFromActorAsync(actor, dgroup.GroupID);
                            m_DutyUI.DutysOn.Remove(actor.SteamId);
                            await UniTask.SwitchToMainThread();
                            EffectManager.askEffectClearByID(31200, actor.SteamId);
                            ChatManager.serverSendMessage(m_StringLocalizer["plugin_translations:duty_off", new { player = actor.DisplayName }], UnityEngine.Color.green, null, null, EChatMode.SAY, null, true);
                        }
                        else
                        {
                            await actor.PrintMessageAsync(m_StringLocalizer["plugin_translations:group_error", new { group = dgroup.GroupID }]);
                        }
                        return;
                    }
                }

                await actor.PrintMessageAsync(m_StringLocalizer["plugin_translations:duty_nopermission"]);
            }
        }

        private readonly IUserDataStore m_UserDataStore;
        private readonly IConfiguration m_Configuration;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IPermissionRoleStore m_PermissionRoleStore;
        private readonly DutyUI m_DutyUI;
    }
}
