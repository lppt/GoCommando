using System;
using System.Collections.Generic;
using System.Linq;
using GoCommando.Internals;
using Xunit;

namespace GoCommando.Tests
{
    public class TestCommand
    {
        [InlineData("-switch:value2")]
        [InlineData("-switch=value2")]
        [InlineData(@"-switch""value2""")]
        public void CanCorrectlyHandleDifferentAlternativeSwitchFormatsFoundInOneSingleTokenOnly(string switchText)
        {
            var settings = new Settings();
            var invoker = new CommandInvoker("bimse", settings, new Bimse());
            var arguments = Go.Parse(new[] { switchText }, settings);

            invoker.Invoke(arguments.Switches, EnvironmentSettings.Empty);

            var bimseInstance = (Bimse)invoker.CommandInstance;

            Assert.Equal("value2", bimseInstance.Switch);
        }
        [Theory]
        [InlineData("-s:value2")]
        [InlineData("-s=value2")]
        [InlineData(@"-s""value2""")]
        public void CanCorrectlyHandleDifferentAlternativeSwitchFormatsFoundInOneSingleTokenOnly_Shortname(string switchText)
        {
            var settings = new Settings();
            var invoker = new CommandInvoker("bimse", settings, new Bimse());
            var arguments = Go.Parse(new[] { switchText }, settings);

            invoker.Invoke(arguments.Switches, EnvironmentSettings.Empty);

            var bimseInstance = (Bimse)invoker.CommandInstance;

            Assert.Equal("value2", bimseInstance.Switch);
        }

        [Command("bimse")]
        class Bimse : ICommand
        {
            [Parameter("switch", shortName: "s")]
            public string Switch { get; set; }

            public void Run()
            {
            }
        }

        [Fact]
        public void CanUseSuppliedCommandFactory()
        {
            var commandFactory = new CustomFactory();

            var commandInvoker = new CommandInvoker("null", typeof(CreatedByFactory), new Settings(), commandFactory: commandFactory);

            commandInvoker.Invoke(Enumerable.Empty<Switch>(), new EnvironmentSettings());

            Assert.IsType<CreatedByFactory>(commandInvoker.CommandInstance);

            var createdByFactory = (CreatedByFactory)commandInvoker.CommandInstance;
            Assert.Equal("ctor!!", createdByFactory.CtorInjectedValue);

            Assert.True(commandFactory.WasProperlyReleased, "The created command instance was NOT properly released after use!");
        }

        class CustomFactory : ICommandFactory
        {
            CreatedByFactory _instance;

            public bool WasProperlyReleased { get; set; }

            public ICommand Create(Type type)
            {
                if (type == typeof(CreatedByFactory))
                {
                    _instance = new CreatedByFactory("ctor!!");
                    return _instance;
                }

                throw new ArgumentException($"Unknown command type: {type}");
            }

            public void Release(ICommand command)
            {
                if (_instance == command)
                {
                    WasProperlyReleased = true;
                }
            }
        }

        class CreatedByFactory : ICommand
        {
            public string CtorInjectedValue { get; }

            public CreatedByFactory(string ctorInjectedValue)
            {
                CtorInjectedValue = ctorInjectedValue;
            }

            public void Run()
            {
            }
        }

        [Fact]
        public void CanGetParameterFromAppSettingsAndConnectionStrings()
        {
            var invoker = new CommandInvoker("null", typeof(CanUseAppSetting), new Settings());

            var appSettings = new Dictionary<string, string>
            {
                {"my-setting", "my-value"}
            };

            var connectionStrings = new Dictionary<string, string>
            {
                {"my-conn", "my-value"}
            };

            var environmentVariables = new Dictionary<string, string>
            {
                {"my-env", "my-value"}
            };

            invoker.Invoke(Enumerable.Empty<Switch>(), new EnvironmentSettings(appSettings, connectionStrings, environmentVariables));

            var instance = (CanUseAppSetting)invoker.CommandInstance;

            Assert.Equal("my-value", instance.AppSetting);
            Assert.Equal("my-value", instance.ConnectionString);
            Assert.Equal("my-value", instance.EnvironmentVariable);
        }

        class CanUseAppSetting : ICommand
        {
            [Parameter("my-setting", allowAppSetting: true)]
            public string AppSetting { get; set; }

            [Parameter("my-conn", allowConnectionString: true)]
            public string ConnectionString { get; set; }

            [Parameter("my-env", allowEnvironmentVariable: true)]
            public string EnvironmentVariable { get; set; }

            public void Run()
            {

            }
        }
    }
}