using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace WilsonPluginModels.Interfaces
{
    public interface IJobScheduler
    {
        /// <summary>
        ///     For cron expression see: <see href="https://en.wikipedia.org/wiki/Cron#CRON_expression">Documentation</see>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="methodCall"></param>
        /// <param name="cron"></param>
        void ReccuringTask(string name, Expression<Action> methodCall, string cron);

        void Task(Expression<Func<Task>> methodCall);
    }
}