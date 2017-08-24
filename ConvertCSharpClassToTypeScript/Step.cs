using System;
using System.Collections.Generic;
using System.Text;

namespace ConvertCSharpClassToTypeScript
{
    public class Step : IStep
    {
        public string Content { get; set; }

        public event EventHandler<string> Result;

        public Step(string content)
        {
            Content = content;
        }

        public void Show()
        {
            Console.WriteLine(this.Content);
        }

        public void Watch()
        {
            var result = Console.ReadLine();
            this.OnResult(result);
        }

        public void ShowAndWatch()
        {
            this.Show();
            this.Watch();
        }

        protected virtual void OnResult(string e)
        {
            Result?.Invoke(this, e);
        }
    }

    public interface IStep
    {
        void Show();
        void Watch();
    }
}
