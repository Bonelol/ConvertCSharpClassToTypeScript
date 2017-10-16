using System;
using System.Collections.Generic;
using System.Linq;

namespace ConvertCSharpClassToTypeScript
{
    public class CommandsList : ICommandsList
    {
        public event EventHandler<string> Command;

        public CommandsList(IEnumerable<string> commands)
        {
            var list = commands.ToList();
            this.Commands = list.ToDictionary(i => list.IndexOf(i).ToString(), i => i);
        }

        public CommandsList(IDictionary<string, string> commands)
        {
            this.Commands = commands;
        }

        public CommandsList()
        {
            this.Commands = new Dictionary<string, string>();
        }

        public IDictionary<string, string> Commands { get; set; }

        public void Show()
        {
            for (int i = 0; i < Commands.Count; i++)
            {
                var value = Commands.ElementAt(i);
                Console.WriteLine($"{value.Key}. {value.Value}");
            }
        }

        public void Listen()
        {
            var index = Console.ReadLine();

            while (!this.Commands.ContainsKey(index))
            {
                Console.WriteLine($"Cannot find {index} in commmands, please retry.");
                index = Console.ReadLine();
            }

            OnCommand(this.Commands[index]);
        }

        protected virtual void OnCommand(string e)
        {
            Command?.Invoke(this, e);
        }
    }
}
