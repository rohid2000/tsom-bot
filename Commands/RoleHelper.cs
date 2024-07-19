using DSharpPlus.Entities;
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

            foreach (DiscordRole dRole in member.Roles)
            {
                if (role == Role.JediMaster)
                {
                    return dRole.Id == await GetIdByRole(Role.JediMaster);
                }

                if (role == Role.JediKnight)
                {
                    if (
                        dRole.Id == await GetIdByRole(Role.JediMaster) ||
                        dRole.Id == await GetIdByRole(Role.JediKnight)
                        )
                    {
                        return true;
                    }
                }

                if (role == Role.Padawan)
                {
                    if (
                        dRole.Id == await GetIdByRole(Role.JediMaster) ||
                        dRole.Id == await GetIdByRole(Role.JediKnight) ||
                        dRole.Id == await GetIdByRole(Role.Padawan)
                        )
                    {
                        return true;
                    }
                }

                if (role == Role.Youngling)
                {
                    if (
                        dRole.Id == await GetIdByRole(Role.JediMaster) ||
                        dRole.Id == await GetIdByRole(Role.JediKnight) ||
                        dRole.Id == await GetIdByRole(Role.Padawan) ||
                        dRole.Id == await GetIdByRole(Role.Youngling)
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
                    return configReader.roleIds.acolyte;
                case Role.Apprentice:
                    return configReader.roleIds.apprentice;
                case Role.Mandalorian:
                    return configReader.roleIds.mandalorian;
                case Role.SithLord:
                    return configReader.roleIds.sithlord;
            }

            switch (role)
            {
                case Role.Youngling:
                    return configReader.roleIds.youngling;
                case Role.Padawan:
                    return configReader.roleIds.padawan;    
                case Role.JediKnight:
                    return configReader.roleIds.jediKnight;
                case Role.JediMaster:
                    return configReader.roleIds.jediMaster;
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
        Youngling,
        Padawan,
        JediKnight,
        JediMaster
    }
}
