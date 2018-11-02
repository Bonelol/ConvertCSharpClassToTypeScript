using System;
using System.ComponentModel.Design;
using System.Globalization;
using EnvDTE;
using EnvDTE80;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class FolderCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4130;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("84228d9e-1ac3-4e28-a0b1-225f58db027f");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private FolderCommand(Package package)
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
        public static FolderCommand Instance
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
            Instance = new FolderCommand(package);
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

            if (item == null)
                return;

            var projectItem = (ProjectItem)item.Object;
            var folderPath = projectItem.Properties.Item("FullPath").Value.ToString();

            var output = new OutputDialog((DTE2)ServiceProvider.GetService(typeof(DTE)), folderPath)
            {
                HasMinimizeButton = false,
                HasMaximizeButton = false
            };
            output.ShowModal();
        }

        private UIHierarchyItem GetSelectedSolutionExplorerItem()
        {
            var dte2 = (DTE2)ServiceProvider.GetService(typeof(DTE));

            if (!(dte2.ToolWindows.SolutionExplorer.SelectedItems is UIHierarchyItem[] items) || items.Length != 1)
                return null;

            return items[0];
        }
    }
}
