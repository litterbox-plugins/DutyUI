using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Threading.Tasks;
using Command = OpenMod.Core.Commands.Command;

namespace DutyUI.Commands
{
    [Command("checkduty")]
    [CommandDescription("Check if a player are on duty.")]
    [CommandSyntax("<player name>")]
    [CommandActor(typeof(UnturnedUser))]
    public class CommandCheckDuty : Command
    {
        public CommandCheckDuty(IServiceProvider serviceProvider, IStringLocalizer stringLocalizer, DutyUI dutyUI) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_DutyUI = dutyUI;
        }

        protected override async Task OnExecuteAsync()
        {
            UnturnedUser actor = (UnturnedUser)Context.Actor;
            string playername = await Context.Parameters.GetAsync<string>(0);
            var player = PlayerTool.getSteamPlayer(playername);
            if (player == null)
            {
                await actor.PrintMessageAsync(m_StringLocalizer["plugin_translations:player_notfound"]);
            }
            else
            {
                if (m_DutyUI.DutysOn.Contains(player.playerID.steamID))
                {
                    await actor.PrintMessageAsync(m_StringLocalizer["plugin_translations:player_onduty", new { player = playername }]);
                }
                else
                {
                    await actor.PrintMessageAsync(m_StringLocalizer["plugin_translations:player_offduty", new { player = playername }]);
                }
            }
        }

        private readonly IStringLocalizer m_StringLocalizer;
        private readonly DutyUI m_DutyUI;
    }
}
