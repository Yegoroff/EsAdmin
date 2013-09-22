using System;
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using Microsoft.Win32;
using PlainElastic.Net;
using PlainElastic.Net.Utils;

namespace EsAdmin
{
    public partial class MainWindow : Window
    {
        private const string lastTextFileName = "lasttext.txt";
        private const string lastStateFileName = "laststate.txt";
        
        private string currentFileName;
        private FoldingManager textfoldingManager;
        private FoldingManager outputfoldingManager;
        private AbstractFoldingStrategy foldingStrategy;

 

        public MainWindow()
        {
            InitializeComponent();

            SetupHotkeysAndCommands();

            SetupFolding();

            SetupHighlighting();

            ShowSpacesAndEol = false;

            SetupStateTracking();

            RestoreLastState();
        }


        public bool ShowSpacesAndEol
        {
            get { return textEditor.Options.ShowSpaces | textEditor.Options.ShowEndOfLine | textEditor.Options.ShowTabs; }
            set
            {
                textEditor.Options.ShowSpaces = textEditor.Options.ShowEndOfLine = textEditor.Options.ShowTabs = value;
                output.Options.ShowSpaces = output.Options.ShowEndOfLine = output.Options.ShowTabs = value;
            }
        }




        private void SetupStateTracking()
        {
            textEditor.TextChanged += StoreTextChanges;
            Login.TextChanged += UpdateLastStateHandler;
            Host.TextChanged += UpdateLastStateHandler;
            Port.TextChanged += UpdateLastStateHandler;
        }

 

        private void StoreTextChanges(object sender, EventArgs e)
        {
            File.WriteAllText(lastTextFileName, textEditor.Text);
        }

        private bool LoadFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                textEditor.Load(fileName);
                return true;
            }
            return false;
        }

        private void SetCurrentFileName(string fileName)
        {
            currentFileName = fileName;
            Title = "EsAdmin - " + fileName;
            UpdateLastState();
        }


        private void UpdateLastStateHandler(object sender, TextChangedEventArgs e)
        {
            UpdateLastState();
        }

        private void UpdateLastState()
        {
            var login = Login.Text;
            var host = Host.Text;
            var port = Port.Text;

            string lastState = "openedFileName|" + currentFileName + "\r\n" +
                               "login|" + login +"\r\n" +
                               "host|" + host + "\r\n" +
                               "port|" + port + "\r\n";

            File.WriteAllText(lastStateFileName,  lastState);
        }

        private void RestoreLastState()
        {
            if (File.Exists(lastStateFileName))
            {
                string[] lastState = File.ReadAllLines(lastStateFileName);
                var settings = lastState.Select(line => line.Split('|')).ToDictionary(s => s[0], s => s[1]);

                string openedFileName, login, host, port;
                settings.TryGetValue("openedFileName",out openedFileName);
                settings.TryGetValue("login", out login);
                settings.TryGetValue("host", out host);
                settings.TryGetValue("port", out port);

                Host.Text = host.IsNullOrEmpty() ? "localhost" : host; 
                Port.Text = port.IsNullOrEmpty() ? "9200" : port;
                Login.Text = login;

                if (LoadFile(openedFileName))
                {   
                    SetCurrentFileName(openedFileName);
                    return;
                }
            }

            if (!File.Exists(lastTextFileName))
                return;

            var lastText = File.ReadAllText(lastTextFileName);
            textEditor.Text = lastText;
        }



        private void SetupHotkeysAndCommands()
        {
            BindKeysAndHandler(UiCommands.Execute, new KeyGesture(Key.F5), ExecuteHandler);

            BindKeysAndHandler(UiCommands.BeautifyJson, new KeyGesture(Key.B, ModifierKeys.Control), BeautifyJsonHandler);

            BindKeysAndHandler(UiCommands.SaveFile, new KeyGesture(Key.S, ModifierKeys.Control), SaveFileHandler);

            BindKeysAndHandler(UiCommands.OpenFile, new KeyGesture(Key.O, ModifierKeys.Control), OpenFileHandler);

            BindKeysAndHandler(UiCommands.NewFile, new KeyGesture(Key.N, ModifierKeys.Control), NewFileHandler);

            BindKeysAndHandler(UiCommands.FoldAll, new KeyGesture(Key.OemMinus, ModifierKeys.Control), FoldAllHandler);

            BindKeysAndHandler(ApplicationCommands.Find, new KeyGesture(Key.F, ModifierKeys.Control), SearchHandler);

            BindKeysAndHandler(ApplicationCommands.SelectAll, new KeyGesture(Key.A, ModifierKeys.Control), null);
        }

        private void BindKeysAndHandler(ICommand command, KeyGesture keyGesture, ExecutedRoutedEventHandler handler)
        {
            var ib = new InputBinding(command, keyGesture);
            InputBindings.Add(ib);

            if (handler == null)
                return;

            var cb = new CommandBinding(command);
            cb.Executed += handler;
            CommandBindings.Add(cb);
        }


        private void SearchHandler(object sender, ExecutedRoutedEventArgs e)
        {
            var sp = new SearchPanel();
            sp.Attach(textEditor.TextArea);
            sp.Dispatcher.BeginInvoke(DispatcherPriority.Input, (Action)sp.Reactivate);
        }

        private void FoldAllHandler(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (FoldingSection fm in textfoldingManager.AllFoldings)
            {
                fm.IsFolded = true;
            }
        }

        private void NewFileHandler(object sender, RoutedEventArgs e)
        {
            SetCurrentFileName("");
            textEditor.Clear();
        }

        private void OpenFileHandler(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            if (dlg.ShowDialog() == true)
            {
                if (LoadFile(dlg.FileName))
                {
                    SetCurrentFileName(dlg.FileName);
                }
            }
        }

        private void SaveFileHandler(object sender, EventArgs e)
        {
            if (currentFileName.IsNullOrEmpty())
            {
                var dlg = new SaveFileDialog { DefaultExt = ".txt" };
                if (!((bool)dlg.ShowDialog()))
                    return;

                SetCurrentFileName(dlg.FileName);
            }
            textEditor.Save(currentFileName);
        }

        private void BeautifyJsonHandler(object sender, EventArgs e)
        {
            try
            {
                var textToBeautify = textEditor.SelectedText;
                if (textToBeautify.IsNullOrEmpty())
                    textToBeautify = textEditor.Text;

                string beautifiedText = JsonBeautifier.Beautify(textToBeautify);

                if (!textEditor.SelectedText.IsNullOrEmpty())
                {
                    textEditor.SelectedText = beautifiedText;
                }
                else
                {
                    textEditor.Text = beautifiedText;
                }

            }
            catch (Exception ex)
            {
                output.Text = "Exception : " + ex;
            }
        }

        private void ExecuteHandler(object sender, EventArgs e)
        {
            try
            {
                var textToExecute = textEditor.SelectedText;
                if (string.IsNullOrEmpty(textToExecute))
                    textToExecute = textEditor.Text;

                var login = Login.Text;
                var password = Password.Text;
                var host = Host.Text;
                int port = int.Parse(Port.Text);

                var connector = new EsConnector(host, port, login, password);
                string result = connector.Execute(textToExecute);
                output.Text = result.BeautifyJson();
            }
            catch (Exception ex)
            {
                output.Text = "Exception : " + ex;
            }
        }

        private void ClearOutputClick(object sender, RoutedEventArgs e)
        {
            output.Clear();
        }


        private void SetupHighlighting()
        {
            using (var highlightingData = new StringReader(Properties.Resources.ESHighlighting))
            {
                using (var reader = new XmlTextReader(highlightingData))
                {
                    IHighlightingDefinition highlightingDefinition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    textEditor.SyntaxHighlighting = highlightingDefinition;
                    output.SyntaxHighlighting = highlightingDefinition;
                }
            }
        }



        #region Folding

        private void SetupFolding()
        {
            textEditor.TextArea.IndentationStrategy =
                new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(textEditor.Options);
            foldingStrategy = new BraceFoldingStrategy();

            textfoldingManager = FoldingManager.Install(textEditor.TextArea);
            outputfoldingManager = FoldingManager.Install(output.TextArea);

            var foldingUpdateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            foldingUpdateTimer.Tick += FoldingUpdateTimerTick;
            foldingUpdateTimer.Start();
        }
 

        void FoldingUpdateTimerTick(object sender, EventArgs e)
        {
            if (foldingStrategy != null)
            {
                foldingStrategy.UpdateFoldings(textfoldingManager, textEditor.Document);
                foldingStrategy.UpdateFoldings(outputfoldingManager, output.Document);
            }
        }
        #endregion


    }
}
