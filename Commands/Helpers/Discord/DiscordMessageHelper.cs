using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Text.RegularExpressions;
using tsom_bot.config;
using tsom_bot.i18n;

namespace tsom_bot.Commands.Helpers.Discord
{
    public static class DiscordMessageHelper
    {
        public async static Task BuildMessageWithExecute(InteractionContext ctx, i18nBasicMessages messageContainer, Func<Task> execute)
        {
            try
            {
                DiscordWebhookBuilder completeMessage = new DiscordWebhookBuilder().WithContent(messageContainer.complete);
                DiscordWebhookBuilder loadingMessage = new DiscordWebhookBuilder().WithContent(messageContainer.loading);

                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
                await ctx.EditResponseAsync(loadingMessage);

                await execute.Invoke();

                await ctx.EditResponseAsync(completeMessage);
            } 
            catch (Exception ex) 
            {
                DiscordWebhookBuilder failMessage = new DiscordWebhookBuilder().WithContent(messageContainer.fail);

                await ctx.EditResponseAsync(failMessage);
                Console.WriteLine(messageContainer.fail + "\n ERROR: " + ex.Message);
            }
        }

        public async static Task BuildMessageWithExecute(InteractionContext ctx, i18nBasicMessages messageContainer, Action execute)
        {
            try
            {
                DiscordWebhookBuilder completeMessage = new DiscordWebhookBuilder().WithContent(messageContainer.complete);
                DiscordWebhookBuilder loadingMessage = new DiscordWebhookBuilder().WithContent(messageContainer.loading);

                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
                await ctx.EditResponseAsync(loadingMessage);

                execute.Invoke();

                await ctx.EditResponseAsync(completeMessage);
            }
            catch (Exception ex)
            {
                DiscordWebhookBuilder failMessage = new DiscordWebhookBuilder().WithContent(messageContainer.fail);

                await ctx.EditResponseAsync(failMessage);
                Console.WriteLine(messageContainer.fail + "\n ERROR: " + ex.Message);
            }
        }
        public async static Task BuildCheckMessageWithExecute(InteractionContext ctx, i18nStructureCheckComplete messageContainer, Func<Task<bool>> execute)
        {
            try
            {
                DiscordWebhookBuilder loadingMessage = new DiscordWebhookBuilder().WithContent(messageContainer.loading);

                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
                await ctx.EditResponseAsync(loadingMessage);

                bool result = await execute.Invoke();

                DiscordWebhookBuilder completeMessage = new DiscordWebhookBuilder()
                    .WithContent(messageContainer.complete.getMessage(result));

                await ctx.EditResponseAsync(completeMessage);
            }
            catch (Exception ex)
            {
                DiscordWebhookBuilder failMessage = new DiscordWebhookBuilder().WithContent("Execute wrong!! ABORTED.");

                await ctx.EditResponseAsync(failMessage);
                Console.WriteLine(ex.Message);
            }
        }

        public async static Task BuildPromotionMessageWithExecute(InteractionContext ctx, i18nStructurePromotionCommandSync messageContainer, Func<i18nStructurePromotionCommandSyncComplete, Task> execute)
        {
            try
            {
                DiscordWebhookBuilder loadingMessage = new DiscordWebhookBuilder().WithContent(messageContainer.loading);
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);

                await execute.Invoke(messageContainer.complete);
            }
            catch (Exception ex)
            {
                DiscordWebhookBuilder failMessage = new DiscordWebhookBuilder().WithContent(messageContainer.fail);

                await ctx.EditResponseAsync(failMessage);
                Console.WriteLine(ex.Message);
            }
        }

        public async static Task<KeyValuePair<string, IEnumerable<IMention>>> FormatMessage(string message)
        {
            string formattedMessage;
            formattedMessage = message.Replace("||@@", ",");

            string pattern = @"@@(.*?)@@"; // Regex pattern to match text surrounded by @@
            MatchCollection matches = Regex.Matches(formattedMessage, pattern);
            Dictionary<string, string> replacements = new Dictionary<string, string>();

            IEnumerable<IMention> mentions = [];

            // Process each match asynchronously
            foreach (Match match in matches)
            {
                string name = match.Groups[1].Value;
                if (!replacements.ContainsKey(name))
                {
                    ConfigReader reader = new ConfigReader();
                    await reader.readConfig();
                    if (name == "tsom")
                    {
                        DiscordRole tsomRole = ClientManager.client.Guilds[reader.server_id].Roles[reader.clanrole_ids.sith];
                        replacements[name] = tsomRole.Mention;
                        mentions.Append(new RoleMention(tsomRole));
                    }
                    else if(name == "tjom")
                    {
                        DiscordRole tjomRole = ClientManager.client.Guilds[reader.server_id].Roles[reader.clanrole_ids.jedi];
                        replacements[name] = tjomRole.Mention;
                        mentions.Append(new RoleMention(tjomRole));
                    }
                    else
                    {
                        DiscordMember? discordMember = await DiscordUserHelper.GetDiscordMemberByDiscordName(name);
                        if (discordMember != null)
                        {
                            replacements[name] = discordMember.Mention;
                            mentions.Append(new UserMention(discordMember));
                        }
                        else
                        {
                            replacements[name] = name;
                        }
                    }
                }
            }

            // Replace matches in the input string
            formattedMessage = Regex.Replace(formattedMessage, pattern, match =>
            {
                string name = match.Groups[1].Value;
                return replacements[name];
            });

            if(formattedMessage.Contains("@everyone"))
            {
                mentions.Append(new EveryoneMention());
            }

            return new KeyValuePair<string, IEnumerable<IMention>>(formattedMessage, mentions);
        }
    }
}
