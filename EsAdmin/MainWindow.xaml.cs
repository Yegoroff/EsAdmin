using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit.Folding;
using Microsoft.Win32;
using PlainElastic.Net;
using PlainElastic.Net.Utils;

namespace EsAdmin
{
    public partial class MainWindow : Window
    {
        private const string lastTextFileName = "lasttext.txt";
        private const string lastOpenedFileName = "lastopened.txt";
        
        private string currentFileName;
        private FoldingManager textfoldingManager;
        private FoldingManager outputfoldingManager;
        private AbstractFoldingStrategy foldingStrategy;

 

        public MainWindow()
        {
            InitializeComponent();

            SetupHotkeysAndCommands();

            SetupFolding();

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
            File.WriteAllText(lastOpenedFileName, fileName);
        }

        private void RestoreLastState()
        {
            if (File.Exists(lastOpenedFileName))
            {
                var lastOpenedFile = File.ReadAllText(lastOpenedFileName);
                if (LoadFile(lastOpenedFile))
                    return;
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
