using System.Collections.Generic;

namespace ConvertCSharpClassToTypeScript
{
    public interface ICommandsList
    {
        IDictionary<string, string> Commands { get; set; }

        void Show();

        void Listen();
    }
}