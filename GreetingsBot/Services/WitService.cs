using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.valgut.libs.bots.Wit;
using com.valgut.libs.bots.Wit.Models;
using GreetingsBot.QueryFactory;

namespace GreetingsBot.Services
{
    public class WitService
    {
        public readonly string UserId;

        private readonly IQueryFactory _queryFactory;

        private readonly WitConversation<MessageContext> _client;
        
        private bool didMerge = false;
        private bool didStop = false;

        private delegate MessageContext OperationDelegate(MessageContext context, Dictionary<string, List<com.valgut.libs.bots.Wit.Models.Entity>> entities);
        private readonly Dictionary<string, OperationDelegate> _operations;

        public WitService(IQueryFactory queryFactory, string userId)
        {
            _operations =
            new Dictionary<string, OperationDelegate>
            {
                { "GetGreeting", GetGreeting },
                { "GetHello", GetHello },
                { "GetHowdy", GetHowdy },
                { "GetWelcome", GetWelcome },
                { "GetGoodbye", GetGoodbye }
            };

            _queryFactory = queryFactory;
            UserId = userId;
            
            _client = new WitConversation<MessageContext>("YQ353ZQMXAESU4KBN7M4LZL7YMU7UWXU", UserId, null,
                DoMerge, DoSay, DoAction, DoStop);
            
        }

        private string GetValue(string valueName, Dictionary<string, List<Entity>> entities)
        {
            string result = null;
            List<Entity> subEntities;
            if (entities.TryGetValue(valueName, out subEntities))
            {
                result = subEntities.First().value.ToString();
            }
            return result;
        }

        private MessageContext GetGreeting(MessageContext context, Dictionary<string, List<Entity>> entities)
        {
            string holiday = GetValue("holiday", entities);
            string recipient = GetValue("recipient", entities);
            MessageContext resultContext;

            if (!string.IsNullOrEmpty(holiday))
            {
                resultContext = new MessageContext() {Holiday = holiday, Recipient = recipient};
            }
            else
            {
                resultContext = new MessageContext(context);
            }

            if (!string.IsNullOrEmpty(context?.MissingHoliday))
            {
                resultContext.Recipient = context.Recipient ?? resultContext.Recipient;
            }
            
            if (string.IsNullOrEmpty(resultContext.Holiday))
            {
                resultContext.MissingHoliday = true.ToString();
            }
            else
            {
                resultContext.MissingHoliday = null;

                Greeting greeting;
                if (string.IsNullOrEmpty(resultContext.Recipient))
                {
                    greeting = _queryFactory.GetRandomGreetingEl(resultContext.Holiday);
                }
                else
                {
                    var possibleRecipients = _queryFactory.GetData<Recipient>();
                    if (possibleRecipients.All(x => x.Text != resultContext.Recipient))
                    {
                        resultContext.Recipient = null;
                    }
                    greeting = _queryFactory.GetRandomGreetingEl(resultContext.Holiday, resultContext.Recipient);
                }

                // Dispose context to save memory
                _queryFactory.DisposeContext();


                resultContext.Message = greeting != null ? greeting.Text : "Нет подходящих поздравлений, попробуйте другой запрос.";
            }

            return resultContext;
        }

        private MessageContext GetHello(MessageContext context, Dictionary<string, List<com.valgut.libs.bots.Wit.Models.Entity>> entities)
        {
            var resultContext = new MessageContext {Message = _queryFactory.GetRandomEl<Hello>().Text};
            
            return resultContext;
        }

        private MessageContext GetHowdy(MessageContext context, Dictionary<string, List<com.valgut.libs.bots.Wit.Models.Entity>> entities)
        {
            var resultContext = new MessageContext { Message = _queryFactory.GetRandomEl<Howdy>().Text };

            return resultContext;
        }

        private MessageContext GetWelcome(MessageContext context, Dictionary<string, List<com.valgut.libs.bots.Wit.Models.Entity>> entities)
        {
            var resultContext = new MessageContext { Message = _queryFactory.GetRandomEl<ThanksAnswer>().Text };

            return resultContext;
        }

        private MessageContext GetGoodbye(MessageContext context, Dictionary<string, List<com.valgut.libs.bots.Wit.Models.Entity>> entities)
        {
            var resultContext = new MessageContext {Message = _queryFactory.GetRandomEl<Goodbye>().Text};
            
            return resultContext;
        }

        private MessageContext GetMoreInfo(MessageContext context, Dictionary<string, List<com.valgut.libs.bots.Wit.Models.Entity>> entities)
        {
            var resultContext = new MessageContext { Message = "Не совсем понимаю что вы от меня хотите..." };

            return resultContext;
        }

        public MessageContext PerformOperation(string op, MessageContext context, Dictionary<string, List<com.valgut.libs.bots.Wit.Models.Entity>> entities)
        {
            if (op == null || !_operations.ContainsKey(op))
                return GetMoreInfo(context, entities);
            return _operations[op](context, entities);
        }

        private string _responseMsg;
        public string Analyze(string msg)
        {
            _responseMsg = "";

            Task<bool> t = _client.SendMessageAsync(msg);
            t.Wait();

            return _responseMsg;
        }

        public MessageContext DoMerge(string conversationId, MessageContext context, Dictionary<string, List<com.valgut.libs.bots.Wit.Models.Entity>> entities, double confidence)
        {
            didMerge = true;
            return context;
        }

        public void DoSay(string conversationId, MessageContext context, string msg, double confidence)
        {
            _responseMsg = msg;
        }

        public MessageContext DoAction(string conversationId, MessageContext context, string action, Dictionary<string, List<com.valgut.libs.bots.Wit.Models.Entity>> entities, double confidence)
        {
            return PerformOperation(action, context, entities);
        }

        public MessageContext DoStop(string conversationId, MessageContext context)
        {
            didStop = true;
            return context;
        }

        public class MessageContext
        {
            public MessageContext()
            {

            }

            public MessageContext(MessageContext context)
            {
                if (context != null)
                {
                    Message = context.Message;
                    Holiday = context.Holiday;
                    Recipient = context.Recipient;
                    MissingHoliday = context.MissingHoliday;
                }
            }

            public string Holiday { get; set; }

            public string Recipient { get; set; }

            public string Message { get; set; }

            public string MissingHoliday { get; set; }
        }
    }
}