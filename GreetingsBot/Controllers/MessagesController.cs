using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using GreetingsBot.QueryFactory;
using Microsoft.Bot.Connector;
using com.valgut.libs.bots.Wit;
using com.valgut.libs.bots.Wit.Models;
using GreetingsBot.Services;

namespace GreetingsBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private readonly Entities _data;
        private readonly IQueryFactory _queryFactory;

        private delegate string CommandDelegate();
        private readonly Dictionary<string, CommandDelegate> _commands;
        
        public MessagesController()
        {
            _data = new Entities();
            _queryFactory = new QueryFactory.QueryFactory(_data);

            _commands = new Dictionary<string, CommandDelegate>
            {
                { "/help", GetHelp },
                { "/start", GetHelp },
                { "/about", GetAbout }
            };
        }

        private string GetAbout()
        {
            return @"С помощью этого бота можно подобрать поздравления для ваших близких 🎉.

Пишите ваши пожелания в социальных сетях или просто добавляйтесь 😉:
VK – http://vk.com/mixvlad
FB – https://www.facebook.com/kozlovm";
        }

        private string ToMarkdown(string input)
        {
            return input.Replace("\r", "").Replace("\n\n", "\n# \n").Replace("\n", "\n\n");
        }

        private string GetHelp()
        {
            return @"Бот понимает текстовые команды: приветсвие, прощание, благодарность, как дела и название праздника.
Например можно написать:

**поздравление для мамы с Новым Годом**

В общем случае, пишите просто название праздника, если он есть в базе, бот вас поймет. Например:

**нг**

Также, можно прощаться и приветсвовать бота:

**Привет бот**

Чем больше сообщений вы присылаете, тем лучше он вас понимает 😉

**Список праздников в базе бота:**
* Новый год
* Рождество
* День рождения
* Свадьба
* День влюбленных
* 23 февраля
* 8 марта
* 1 апреля
* Пасха
* 1 мая
* 9 мая
* День ВДВ
* День учителя

Устанавливайте приложение 'Мои поздравления' для Windows: http://koz.tv/portfolio/uwp-app-my-greetings/";

        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                var responseMsg = new StringBuilder();
                int length = (activity.Text ?? string.Empty).Length;
                if (length > 0)
                {
                    if (_commands.ContainsKey(activity.Text))
                    {
                        responseMsg.Append(_commands[activity.Text]());
                    }
                    else
                    {
                        var wit = new WitService(_queryFactory, activity.From.Id);

                        var message = wit.Analyze(activity.Text);
                        responseMsg.Append(message);
                    }
                    
                }
                // calculate something for us to return

                // return our reply to the user 
                Activity reply = activity.CreateReply(ToMarkdown($"{responseMsg}"));
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}