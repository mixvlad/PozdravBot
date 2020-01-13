using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GreetingsBot.QueryFactory
{
    public class QueryFactory : IQueryFactory
    {
        protected readonly Entities DataContext;
        protected readonly Random Rand;

        public QueryFactory(Entities dataContext)
        {
            DataContext = dataContext;
            Rand = new Random();
        }

        public List<T> GetData<T>()
            where T : class
        {
            return DataContext.Set<T>().ToList();
        }

        public T GetRandomEl<T>()
            where T : class
        {
            var elems = DataContext.Set<T>().ToList();
            return elems.Count > 0 ? elems.ElementAt(Rand.Next(elems.Count)) : null;
        }

        public Greeting GetRandomGreetingEl(string holiday, string recipient)
        {
            Expression<Func<Greeting, bool>> queryExpression;
            
            var elems = DataContext.Set<Greeting>().AsQueryable();

            if (!string.IsNullOrEmpty(holiday))
            {
                queryExpression = x => x.Holiday.Text == holiday;
                elems = elems.Where(queryExpression);
            }

            //if (!string.IsNullOrEmpty(recipient))
            //{
            //    queryExpression = x => x.GreetingRecipients.Any(y => y.Recipient.Text == recipient || y.Recipient.Text == "anyone");
            //    elems = elems.Where(queryExpression);
            //}

            var filteredElems = elems.ToList();
            
            return filteredElems.Count > 0 ? filteredElems.ElementAt(Rand.Next(filteredElems.Count)) : null;
        }

        public void DisposeContext()
        {
            DataContext.Dispose();
        }
    }
}