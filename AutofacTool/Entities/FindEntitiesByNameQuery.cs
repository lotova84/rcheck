using System;
using System.Collections.Generic;
using System.Text;

namespace AutofacTool.Entities
{
    class FindEntitiesByNameQuery : IQuery<FindEnititesCriterion, IEnumerable<Entity>>
    {
        private readonly ISomeService _someService;

        public FindEntitiesByNameQuery(ISomeService someService)
        {
            _someService = someService;
        }

        public IEnumerable<Entity> Ask(FindEnititesCriterion criterion)
        {
            return new[] { new Entity("Johny"), new Entity("Lili") };
        }
    }

    internal interface IQuery<in TCriterion, out TResult>
    {
        TResult Ask(TCriterion criterion);
    }

    class FindEnititesCriterion
    {

    }

    class Entity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public Entity(string name)
        {
            Name = name;
        }
    }
}
