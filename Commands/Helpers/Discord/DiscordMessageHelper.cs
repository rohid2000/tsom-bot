using DocumentFormat.OpenXml.Office2019.Excel.ThreadedComments;
using DocumentFormat.OpenXml.Wordprocessing;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Text.RegularExpressions;
using System.Xml.Linq;
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

        public async static Task BuildMessageWithExecuteWithParams(InteractionContext ctx, i18nBasicMessages messageContainer, Func<Task<Dictionary<string, string>>> execute)
        {
            try
            {
                DiscordWebhookBuilder loadingMessage = new DiscordWebhookBuilder().WithContent(messageContainer.loading);

                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
                await ctx.EditResponseAsync(loadingMessage);

                Dictionary<string, string> parameters = await execute.Invoke();
                var completeMessageString = i18n.i18n.TransformParams(messageContainer.complete, parameters);
                DiscordWebhookBuilder completeMessage = new DiscordWebhookBuilder().WithContent(completeMessageString);

                await ctx.EditResponseAsync(completeMessage);
            }
            catch (Exception ex)
            {
                DiscordWebhookBuilder failMessage = new DiscordWebhookBuilder().WithContent(messageContainer.fail);

                await ctx.EditResponseAsync(failMessage);
                Console.WriteLine(messageContainer.fail + "\n ERROR: " + ex.Message);
            }
        }
        public async static Task<KeyValuePair<string, List<IMention>>> FormatMessage(string message)
        {
            string formattedMessage;
            formattedMessage = message.Replace("|||", ",");
            formattedMessage = formattedMessage.Replace("%%%", "'");

            string pattern = @"@@(.*?)@@"; // Regex pattern to match text surrounded by @@
            MatchCollection matches = Regex.Matches(formattedMessage, pattern);
            Dictionary<string, string> replacements = new Dictionary<string, string>();

            List<IMention> mentions = new List<IMention>();

            foreach (Match match in matches) 
            {
                string name = match.Groups[1].Value;
                if (!replacements.ContainsKey(name))
                {
                    string[] splitName = name.Split(":");
                    string type = splitName[0];

                    switch (type)
                    {
                        case "user":
                            await FormatUserMention(name, replacements, mentions);
                            break;
                        case "role":
                            await FormatRoleMention(name, replacements, mentions);
                            break;
                        case "channel":
                            await FormatChannel(name, replacements);
                            break;
                        case "emoji":
                            await FormatEmojie(name, replacements);
                            break;
                    }
                }
            }

            // Replace matches in the input string
            string formattedString = Regex.Replace(formattedMessage, pattern, match =>
            {
                string name = match.Groups[1].Value;
                return replacements[name];
            });

            if (formattedMessage.Contains("@everyone"))
            {
                mentions.Add(new EveryoneMention());
            }

            return new KeyValuePair<string, List<IMention>>(formattedString, mentions);
        }

        private async static Task FormatUserMention(string value, Dictionary<string, string> replacements, List<IMention> mentions)
        {
            string name = value.Split(":")[1];
            DiscordMember? discordMember = await DiscordUserHelper.GetDiscordMemberByDiscordName(name);
            if (discordMember != null)
            {
                replacements[value] = discordMember.Mention;
                mentions.Add(new UserMention(discordMember));
            }
            else
            {
                replacements[value] = name;
            }
        }

        private async static Task FormatRoleMention(string value, Dictionary<string, string> replacements, List<IMention> mentions)
        {
            string name = value.Split(":")[1];
            DiscordRole? discordRole = await DiscordRoleHelper.GetRoleByName(name);
            if (discordRole != null)
            {
                replacements[value] = discordRole.Mention;
                mentions.Add(new RoleMention(discordRole));
            }
            else
            {
                replacements[value] = name;
            }
        }

        private async static Task FormatChannel(string value, Dictionary<string, string> replacements)
        {
            string name = value.Split(":")[1];
            DiscordChannel? discordChannel = await DiscordChannelHelper.GetChannelByName(name);
            if (discordChannel != null)
            {
                replacements[value] = discordChannel.Mention;
            }
            else
            {
                replacements[value] = name;
            }
        }

        private async static Task FormatEmojie(string value, Dictionary<string, string> replacements)
        {
            string name = value.Split(":")[1];
            string emoji = await EmojieHelper.GetEmojieByName(name);
            if (emoji != "")
            {
                replacements[value] = emoji;
            }
            else
            {
                replacements[value] = name;
            }
        }

        public static string FormatForDatabase(string value)
        {
            string formattedMessage = value.Replace(",", "|||");
            formattedMessage = formattedMessage.Replace("'", "%%%");
            return formattedMessage;
        }
    }
}
