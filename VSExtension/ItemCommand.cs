using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;


namespace VSExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ItemCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4129;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("84228d9e-1ac3-4e28-a0b1-225f58db027f");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private ItemCommand(Package package)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));

            if (this.ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ItemCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider => this.package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new ItemCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var item = GetSelectedSolutionExplorerItem();
            var projectItem = (ProjectItem) item.Object;
            var filePath = projectItem.Properties.Item("FullPath").Value.ToString();
            var classDefinitions = ConvertHelper.ParseFile(filePath).ToDictionary(c => c.Name, c => c);

            foreach (var definition in classDefinitions)
            {
                var content = CreateTsFile(classDefinitions, definition.Value);

                var dlg = new SaveFileDialog
                {
                    Filter = "Typescript file (.ts)|*.ts",
                    DefaultExt = "ts",
                    AddExtension = true
                };

                var result = dlg.ShowDialog();

                // Process save file dialog box results
                if (result == DialogResult.OK)
                {
                    // Save document
                    File.WriteAllText(dlg.FileName, content);
                }
            }
        }

        private UIHierarchyItem GetSelectedSolutionExplorerItem()
        {
            var dte2 = (DTE2)ServiceProvider.GetService(typeof(DTE));

            if (!(dte2.ToolWindows.SolutionExplorer.SelectedItems is UIHierarchyItem[] items) || items.Length != 1)
                return null;

            return items[0];
        }

        private string CreateTsFile(IDictionary<string, ClassDefinition> classes, ClassDefinition c)
        {
            var builder = new StringBuilder();
            var imports = c.References.Where(classes.ContainsKey);

            foreach (var import in imports)
            {
                builder.AppendLine($@"import {{ {import} }} from './{import}'");
            }

            builder.AppendLine().AppendLine($"export class {c.Name} {{");

            foreach (var property in c.Properties)
            {
                builder.AppendLine($"    {property.Name}: {property.Type.TypeScriptName};");
            }

            builder.AppendLine("}");

            return builder.ToString();
        }
    }
}
