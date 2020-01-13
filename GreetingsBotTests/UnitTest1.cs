using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.valgut.libs.bots.Wit;
using com.valgut.libs.bots.Wit.Models;
using Xunit;

namespace GreetingsBotTests
{
    public class GreetingsBotTest
    {
        private bool didMerge = false;
        private bool didStop = false;

        [Fact]
        public void TestConnection()
        {
            var client = new WitConversation<DemoContext>("YQ353ZQMXAESU4KBN7M4LZL7YMU7UWXU", "test", null,
                DoMerge, DoSay, DoAction, DoStop);
            Task<bool> t = client.SendMessageAsync("привет");
            t.Wait();

            Assert.Equal(t.Result && didMerge && didStop, true);
        }
        
        public DemoContext DoMerge(string conversationId, DemoContext context, Dictionary<string, List<Entity>> entities, double confidence)
        {
            didMerge = true;
            return context;
        }

        public void DoSay(string conversationId, DemoContext context, string msg, double confidence)
        {
            Console.WriteLine(msg);
        }

        public DemoContext DoAction(string conversationId, DemoContext context, string action, double confidence)
        {
            return context;
        }

        public DemoContext DoStop(string conversationId, DemoContext context)
        {
            didStop = true;
            return context;
        }

        public class DemoContext
        {
            public string someField { get; set; }
        }
    }
}
