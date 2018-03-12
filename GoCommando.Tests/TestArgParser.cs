using System;
using System.Collections.Generic;
using System.Linq;
using GoCommando.Internals;
using Xunit;

namespace GoCommando.Tests
{
    public class TestArgParser
    {
        [Fact]
        public void CanReturnSimpleCommand()
        {
            var arguments = Parse(new[] { "run" });

            Assert.Equal("run", arguments.Command);
        }

        [Fact]
        public void CommandIsNullWhenNoCommandIsGiven()
        {
            var arguments = Parse(new[] { "-file", @"""C:\temp\file.json""" });

            Assert.Null(arguments.Command);
        }

        // [Fact, Ignore]
        //[Test, Ignore("arguments.Command should just be null")]
        public void DoesNotAcceptSwitchAsCommand()
        {
            var ex = Assert.Throws<GoCommandoException>(() =>
            {
                Parse(new[] { "-file", @"""C:\temp\file.json""" });
            });

            Console.WriteLine(ex);
        }

        [Fact]
        public void CanParseOrdinaryArguments()
        {
            var args = @"run
-path
c:\Program Files
-dir
c:\Windows\Microsoft.NET\Framework
-flag
-moreflag".Split(new[] { "\r\n", "\r", "\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var arguments = Parse(args);

            Console.WriteLine(arguments);

            Assert.Equal("run", arguments.Command);
            Assert.Equal(4, arguments.Switches.Count());
            Assert.Equal(@"c:\Program Files", arguments.Get<string>("path"));
            Assert.Equal(@"c:\Windows\Microsoft.NET\Framework", arguments.Get<string>("dir"));

            Assert.True(arguments.Get<bool>("flag"));
            Assert.True(arguments.Get<bool>("moreflag"));
            Assert.False(arguments.Get<bool>("flag_not_specified_should_default_to_false"));
        }

        [Theory]
        [InlineData(@"-path:""c:\temp""")]
        [InlineData(@"-path=""c:\temp""")]
        [InlineData(@"-path""c:\temp""")]
        public void SupportsVariousSingleTokenAliases(string alias)
        {
            var arguments = Parse(new[] { alias });

            Assert.Equal(1, arguments.Switches.Count());
            Assert.Equal("path", arguments.Switches.Single().Key);
            Assert.Equal(@"c:\temp", arguments.Switches.Single().Value);
        }

        [Theory, InlineData(@"-n23")]
        public void SupportsShortFormWithNumber(string alias)
        {
            var arguments = Parse(new[] { alias });

            Assert.Equal(1, arguments.Switches.Count());
            Assert.Equal("n", arguments.Switches.Single().Key);
            Assert.Equal(@"23", arguments.Switches.Single().Value);
        }

        static Arguments Parse(IEnumerable<string> args)
        {
            return Go.Parse(args, new Settings());
        }
    }
}
