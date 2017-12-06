using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.PlatformUI;
using Path = System.IO.Path;

namespace VSExtension
{
    /// <summary>
    /// Interaction logic for OutputDialog.xaml
    /// </summary>
    public partial class OutputDialog : DialogWindow, INotifyPropertyChanged
    {
        private readonly DTE2 _dte2;
        private bool _outputInterface;
        private bool _generateIndexfile;
        private readonly string _folderPath;
        private ObservableCollection<OutputFile> _files;
        private OutputOptions _selectedOption;
        private const string OUTPUT_PANE_NAME = "Convert to TypeScript";

        public ObservableCollection<OutputFile> Files
        {
            get => _files;
            set
            {
                _files = value;
                OnPropertyChanged();       
            }
        }

        public IList<OutputOptions> OutputOptionList = Enum.GetValues(typeof(OutputOptions)).Cast<OutputOptions>().ToList();
        

        public OutputOptions SelectedOption
        {
            get => _selectedOption;
            set
            {
                _selectedOption = value;
                OnPropertyChanged();
            }
        }

        public bool OutputInterface
        {
            get => _outputInterface;
            set
            {
                _outputInterface = value;
                OnPropertyChanged();
            }
        }

        public bool GenerateIndexFile
        {
            get => _generateIndexfile;
            set
            {
                _generateIndexfile = value;
                OnPropertyChanged();
            }
        }

        public OutputDialog(DTE2 dte2, string path)
        {
            _dte2 = dte2;
            _folderPath = path;
            Files = new ObservableCollection<OutputFile>();
            InitializeComponent();
        }

        public override void BeginInit()
        {
            base.BeginInit();
            Files = new ObservableCollection<OutputFile>(Directory.GetFiles(_folderPath, "*.cs")
                .Select(f => new OutputFile() {FileName = Path.GetFileName(f), FilePath = f, Selected = true})
                .OrderBy(f => f.FileName));
        }

        public override void EndInit()
        {
            base.EndInit();
            this.checkBox.IsChecked = true;
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog {Description = "Choose an output folder.", ShowNewFolderButton = true })
            {
                var result = dialog.ShowDialog();

                if (result != System.Windows.Forms.DialogResult.OK)
                    return;

                var folderName = dialog.SelectedPath;
                var outputWindow = _dte2.ToolWindows.OutputWindow;
                var pane = outputWindow.OutputWindowPanes.Cast<OutputWindowPane>().FirstOrDefault(p => p.Name == OUTPUT_PANE_NAME);

                // Add a new pane to the Output window.
                pane = pane ?? outputWindow.OutputWindowPanes.Add(OUTPUT_PANE_NAME);

                var convert = new ConvertHelper();
                var dict = new Dictionary<OutputFile, List<ClassDefinition>>();
                var classDefinitions = new Dictionary<string, ClassDefinition>();

                foreach (var file in Files.Where(f=>f.Selected))
                {
                    try
                    {
                        var results = convert.ParseFile(file.FilePath);
                        dict.Add(file, results);
                        results.ForEach(c => classDefinitions.Add(c.Name, c));
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception);
                        pane.OutputString($"Error: {exception}{Environment.NewLine}");
                        file.Result = "Failed";
                    }
                }

                foreach (var d in dict)
                {
                    try
                    {
                        foreach (var definition in d.Value)
                        {
                            var content = CreateTsFile(classDefinitions, definition);
                            var outputFilePath = $@"{folderName}\{definition.TypeName}.ts";

                            if (!File.Exists(outputFilePath))
                            {
                                File.WriteAllText(outputFilePath, content);
                                d.Key.Result = "Created";
                            }
                            else
                            {
                                switch (SelectedOption)
                                {
                                    case OutputOptions.Skip:
                                        d.Key.Result = "Skipped";
                                        break;
                                    case OutputOptions.Merge:
                                        File.AppendAllText(outputFilePath, content);
                                        d.Key.Result = "Merged";
                                        break;
                                    case OutputOptions.Overwrite:
                                        File.WriteAllText(outputFilePath, content);
                                        d.Key.Result = "Overwritten";
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception);
                        pane.OutputString($"Error: {exception}{Environment.NewLine}");
                        d.Key.Result = "Failed";
                    }

                    pane.OutputString($"File {d.Key.FileName} {d.Key.Result.ToLower()}.{Environment.NewLine}");
                }

                try
                {
                    var content = CreateIndexFile(classDefinitions);
                    var outputFilePath = $@"{folderName}\index.ts";

                    if (!File.Exists(outputFilePath))
                    {
                        File.WriteAllText(outputFilePath, content);
                    }
                    else
                    {
                        File.AppendAllText(outputFilePath, content);
                    }

                    pane.OutputString($"File index.ts created.{Environment.NewLine}");
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                    pane.OutputString($"Error: {exception}{Environment.NewLine}");
                }
            }
        }

        private string CreateTsFile(IDictionary<string, ClassDefinition> classes, ClassDefinition c)
        {
            var builder = new StringBuilder();
            var imports = c.References.Where(classes.ContainsKey);

            foreach (var import in imports)
            {
                builder.AppendLine($@"import {{ {import} }} from './{import}'");
            }

            var keyword = this.OutputInterface ? "interface" : "class";
            builder.AppendLine().AppendLine($"export {keyword} {c.Name} {{");

            foreach (var property in c.Properties)
            {
                builder.AppendLine($"    {property.Name}: {property.Type.TypeScriptName};");
            }

            builder.AppendLine("}");

            return builder.ToString();
        }

        private string CreateIndexFile(IReadOnlyDictionary<string, ClassDefinition> classes)
        {
            var builder = new StringBuilder();

            foreach (var c in classes.Values)
            {
                builder.AppendLine($@"export {{{c.Name}}} from './{c.Name}'");
            }

            return builder.ToString();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var file in this.Files)
            {
                file.Selected = true;
            }
        }

        private void CheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            foreach (var file in this.Files)
            {
                file.Selected = false;
            }
        }
    }

    public enum OutputOptions
    {
        Skip, Merge, Overwrite
    }
}
