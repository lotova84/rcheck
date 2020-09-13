using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Autofac;
using Autofac.Builder;
using Module = Autofac.Module;
using AutofacTool.Entities;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;


namespace AutofacTool
{


    /// <summary>
    /// 1) Время жизни объектов:
    ///   -> InstancePerDependency - на каждый запрос новый объект (по умолчанию).
    ///   -> SingleInstance - на каждый запрос один объект.
    ///   -> InstancePerLifetimeScope - на каждый запрос один объект, в пределах области LifetimeScope. В другой области - другой объект. 
    ///   -> InstancePerMatchingLifetimeScope - тоже что InstancePerLifetimeScope, но позволяет промаркировать область.
    ///      (Например InstancePerMatchingLifetimeScope("myScope")). Экземпляр можно будет создать только в пределах этой
    ///      маркированной области(или Exception). Все сложенные(nested) области тоже будут использовать разделяемый экземпляр.
    ///   -> InstancePerRequest - один экземпляр на каждый Http запрос (в web приложениях).
    ///      Важно!!! В ASP.NET Core - лучше использовать InstancePerLifetimeScope. Там не работает InstancePerRequest.
    ///      Autofac НЕ управляет жизненным цикл контроллеров, но зависимости разрешает. Жизненный цикл параметров конструктора
    ///      контроллера обрабатываются временем жизни запроса.
    ///   -> InstancePerOwned - время жизни экземпляра равно времени жизни владельца этого экземпляра (.InstancePerOwned<Owner>()).
    /// 2) Временем жизни управляет сам IoC. Если нужно предоставить внешним сервисам это управление то можно указать ExternallyOwned:
    ///   -> builder.RegisterType<SomeService>().As<ISomeService>().ExternallyOwned();
    /// </summary>
    class AutofacContainerConfig 
    {
        //public static IServiceProvider Configure()
        public static IComponentContext Configure()
        {
            ContainerBuilder builder = new ContainerBuilder(); // создаем builder

            // Варианты регистрации модулей:
            // 1-й вариант. Зарегистрировать только один модуль:
            builder.RegisterModule<SomeModule>(); // зарегистрировать только один модуль.
            builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly()); // зарегистрировать все модули в текущей сборке.


            IContainer container = builder.Build(); // создаем контейнер

            // UnitOfWork. Создаем область из которой будем разрешать зависимости(создавать экземпляры).
            // Когда область освобождается(Dispose) - освобождаются(удаляются) и порожденные ею экземпляры объектов. 
            // -> using (var scope = container.BeginLifetimeScope()) { scope.Resolve<ISomeService>(); }
            // В этом примере экземпляр ISomeService удалиться сразу же после использования, потому что удалиться 
            // область scope породившая его.

            // Не следует напрямую использовать IContainer. Рекомендуется использовать IComponentContext для резолва.
            return container.BeginLifetimeScope();





            //// !!! Альтернативный способ через Microsoft.Extensions.DependencyInjection. Это типа готовой обертки над IoC. !!!
            //// Такой подход используется в ASP.NET Core. 

            //IServiceCollection serviceCollection = new ServiceCollection();

            //var containerBuilder = new ContainerBuilder();

            //containerBuilder.Populate(serviceCollection);
            //containerBuilder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());

            //var container = containerBuilder.Build();
            //IServiceProvider serviceProvider = new AutofacServiceProvider(container);

            //return serviceProvider;
        }
    }

    public class SomeModule : Module
    {
        // Можно указать какой-нибудь параметр и использовать его для различных условий привязок в методе Load:
        // Например: if (Flag) builder.RegisterType<Type1>() else builder.RegisterType<Type2>();
        // Можно также например проверить окружение if (Environment.OSVersion.Platform == PlatformID.Unix) ...
        // builder.RegisterModule(new SomeModule() { Flag = true });
        public bool Flag { get; set; } 

        protected override void Load(ContainerBuilder builder)
        {
            // ======== Обычные варианты регистраций ========
            // 1-й вариант: 
            builder.RegisterType<SomeService>().As<ISomeService>();
            builder.RegisterType<MainClass>().AsSelf(); // сам на себя. Чтобы автоматом разрешить все зависимости через конструктор.

            //// 2-й вариант. Зарегистрировать имеющийся экземпляр:
            //var someService = new SomeService();
            //builder.RegisterInstance(someService).As<ISomeService>();

            
            // ======== Конструкторы ========
            //// 1-й вариант.
            //// Если у класса несколько конструкторов то можно указать какой выбрать.
            //// Выбираем конструктор у которого 2-а вот таких параметра:
            //builder.RegisterType<MainClass>().UsingConstructor(typeof(string), typeof(ISomeService));

            //// 2-й вариант.
            //// Здесь мы получаем контекст(это сам IContainer) и с помощью него вручную разрешаем зависимость,
            //// т.е. создаем сам класс и вручную передаем параметры:
            //// (В доках написано что builder.Register(c => new Component()) работает гораздо быстрее чем builder.RegisterType<Component>()
            //// для объектов, экземпляры которых создаются много раз)
            //builder.Register(ctx => new MainClass(ctx.Resolve<ISomeService>()));

            //// 3-й вариант (указать параметры конструктора вручную):
            //builder.RegisterType<MainClass>().AsSelf().WithParameter("someParameter", "paramValue");
            //builder.RegisterType<MainClass>().AsSelf().WithParameter(new TypedParameter(typeof(string), "paramValue"));

            // 4-й вариант (вручную через условие):
            // Здесь мы проверяем каждый параметр на соответствие условию и если условие истинно выполняем необходимые действия.
            builder.RegisterType<MainClass>().WithParameter(
                (pi, ctx) => pi.ParameterType == typeof(ISomeService), // условие(фильтр) для каждого параметра конструктора.
                (pi, ctx) => ctx.Resolve<ISomeService>()); // если условие выполнено - передать в параметр следующее. 


            // ======== Соглашения (Conventions) ========
            //// 1-й вариант (конвенции):
            //builder.RegisterAssemblyTypes(ThisAssembly) // все типы в этой сборке
            //    .Where(type => type.Namespace == "AutofacTool.Entities") // фильтрация по пространству имен
            ////  .Where(type => type.Namespace.Contains("Entities")) // или
            //    .AsImplementedInterfaces(); // ассоциировать тип со всеми его интерфейсами

            // 2-й вариант (конвенции):
            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(type => type.Name.EndsWith("Query")) // взять все типы имена которых оканчиваются на
                .AsClosedTypesOf(typeof(IQuery<,>)); // для generic типов. или
                //.As(type => type.GetInterfaces().FirstOrDefault(x => x.Name.StartsWith("IQuery"))) // взять все интерфейсы типа и найти нужный

            
        }
    }
}
