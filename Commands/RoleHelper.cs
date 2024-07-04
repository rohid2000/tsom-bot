using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tsom_bot.config;

namespace tsom_bot.Commands
{
    public static class RoleHelper
    {
        public static string noRoleMessage = "You don't have the right role to use this command";
        public async static Task<bool> hasRole(Role role, DiscordMember member)
        {
            foreach(DiscordRole dRole in member.Roles) 
            {
                if(role == Role.SithLord) 
                {
                    return dRole.Id == await GetIdByRole(Role.SithLord);
                }

                if(role == Role.Mandalorian)
                {
                    if(
                        dRole.Id == await GetIdByRole(Role.SithLord) ||
                        dRole.Id == await GetIdByRole(Role.Mandalorian)
                        )
                    {
                        return true;
                    }
                }

                if (role == Role.Apprentice)
                {
                    if (
                        dRole.Id == await GetIdByRole(Role.SithLord) ||
                        dRole.Id == await GetIdByRole(Role.Mandalorian) ||
                        dRole.Id == await GetIdByRole(Role.Apprentice)
                        )
                    {
                        return true;
                    }
                }

                if (role == Role.Acolyte)
                {
                    if (
                        dRole.Id == await GetIdByRole(Role.SithLord) ||
                        dRole.Id == await GetIdByRole(Role.Mandalorian) ||
                        dRole.Id == await GetIdByRole(Role.Apprentice) ||
                        dRole.Id == await GetIdByRole(Role.Acolyte)
                        )
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async static Task<ulong> GetIdByRole(Role role)
        {
            var configReader = new ConfigReader();
            await configReader.readConfig();
            switch (role) 
            {
                case Role.Acolyte:
                    return configReader.roleids.acolyte;
                case Role.Apprentice:
                    return configReader.roleids.apprentice;
                case Role.Mandalorian:
                    return configReader.roleids.mandalorian;
                case Role.SithLord:
                    return configReader.roleids.sithlord;
            }

            return 1;
        }
    }

    public enum Role
    {
        Acolyte,
        Apprentice,
        Mandalorian,
        SithLord,
    }
}
