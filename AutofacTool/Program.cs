using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Builder;
using AutofacTool.Entities;

namespace AutofacTool
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = AutofacContainerConfig.Configure();

            // Варианты разрешения:
            // 1 - й вариант(если ISomeService не зарегистрирован - DependencyResolutionException):
            //var obj = (ISomeService)container.GetService(typeof(ISomeService));

            //obj.DoSomething();

            //var query = (IQuery<FindEnititesCriterion, IEnumerable<Entity>>)container.GetService(typeof(IQuery<FindEnititesCriterion, IEnumerable<Entity>>));
            //var result = query.Ask(new FindEnititesCriterion());

            //// Варианты разрешения:
            //// 1-й вариант (если ISomeService не зарегистрирован - DependencyResolutionException):
            //var obj = container.Resolve<ISomeService>();

            //// 2-й вариант (если ISomeService не зарегистрирован - вернется null):
            //obj = container.ResolveOptional<ISomeService>();

            //// 3-й вариант (если нужно взять все реализации интерфейса (в Ninject - GetAll())):
            //var objs = container.Resolve<IEnumerable<ISomeService>>();

            //obj.DoSomething();
            // added comment
            var query = container.ResolveOptional<IQuery<FindEnititesCriterion, IEnumerable<Entity>>>();
            var result = query.Ask(new FindEnititesCriterion());

            Console.ReadLine();
        }
    }
}
