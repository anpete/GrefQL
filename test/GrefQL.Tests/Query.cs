using System;
using GraphQL;
using GraphQL.Http;
using GraphQL.Types;
using GrefQL.Tests.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace GrefQL.Tests
{
    public class Query
    {
        [Fact]
        public void HelloWorld()
        {
            const string query = @"
                query HeroNameQuery {
                  hero {
                    name
                  }
                }
            ";

//            const string expected = @"{
//              hero: {
//                name: 'R2-D2'
//              }
//            }";

            using (var data = CreateContext())
            {
                var schema = new StarWarsSchema(data);
                var documentExecutor = new DocumentExecuter();

                var result = documentExecutor.ExecuteAsync(schema, null, query, null).Result;

                var documentWriter = new DocumentWriter();

                var jsonResult = documentWriter.Write(result);

                WriteLine(jsonResult);
            }
        }

        public class StarWarsSchema : Schema
        {
            public StarWarsSchema(DbContext data)
            {
                Query = new StarWarsQuery(data);
            }
        }

        public class StarWarsQuery : ObjectGraphType
        {
            public StarWarsQuery(DbContext data)
            {
                Name = "Query";

                Field<CharacterInterface>(
                    "hero",
                    resolve: context => { return data.Set<Character>().SingleAsync(h => h.Id == "3"); });

                Field<HumanType>(
                    "human",
                    arguments: new QueryArguments(
                        new[]
                        {
                            new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "id of the human" }
                        }),
                    resolve: context => { throw new NotImplementedException(); });

                Field<DroidType>(
                    "droid",
                    arguments: new QueryArguments(
                        new[]
                        {
                            new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "id of the droid" }
                        }),
                    resolve: context => { throw new NotImplementedException(); });
            }
        }

        public class CharacterInterface : InterfaceGraphType
        {
            public CharacterInterface()
            {
                Name = "Character";
                Description = "A character in the Star Wars Trilogy.";

                Field<NonNullGraphType<StringGraphType>>("id", "The id of the character.");
                Field<StringGraphType>("name", "The name of the character.");
            }
        }

        public class HumanType : ObjectGraphType
        {
            public HumanType()
            {
                Name = "Human";
                Description = "A humanoid creature in the Star Wars universe.";

                Interface<CharacterInterface>();

                Field<NonNullGraphType<StringGraphType>>("id", "The id of the human.");
                Field<StringGraphType>("name", "The name of the human.");
                Field<StringGraphType>("homePlanet", "The home planet of the human.");

                IsTypeOf = value => value is Human;
            }
        }

        public class DroidType : ObjectGraphType
        {
            public DroidType()
            {
                Name = "Droid";
                Description = "A mechanical creature in the Star Wars universe.";

                Interface<CharacterInterface>();

                Field<NonNullGraphType<StringGraphType>>("id", "The id of the droid.");
                Field<StringGraphType>("name", "The name of the droid.");
                Field<StringGraphType>("primaryFunction", "The primary function of the droid.");

                IsTypeOf = value => value is Droid;
            }
        }

        private StarWarsContext CreateContext()
        {
            var serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .GetInfrastructure()
                    .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            loggerFactory.AddProvider(new TestOutputHelperLoggerProvider(_testOutputHelper));

            return new StarWarsContext(serviceProvider);
        }

        private readonly ITestOutputHelper _testOutputHelper;

        public Query(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private void WriteLine(object s) => _testOutputHelper.WriteLine(s.ToString());
    }
}
