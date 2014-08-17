using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Common.Extensions;
using FreePIE.GUI.Common.CommandLine.Commands;

namespace FreePIE.GUI.Common.CommandLine
{
    public class Parser : IParser
    {
        private readonly Func<Type, Command> facotory;

        public Parser(Func<Type, Command> facotory)
        {
            this.facotory = facotory;
        }

        public void ParseAndExecute()
        {
            var args = Environment
                .GetCommandLineArgs()
                .Skip(1);

            var commands = typeof (Command).GetTypes().Select(t => facotory(t));

            foreach (var arg in args)
            {
                var isFile = !arg.StartsWith("/");
                var key = isFile ? arg : arg.Substring(1);
                var matchingCommands = commands.Where(c => (isFile && !c.Keys.Any()) || c.Keys.Any(k => k == key));

                matchingCommands.ForEach(c => c.Execute(key));
            }
        }
    }
}
