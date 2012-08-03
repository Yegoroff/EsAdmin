using System;
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



        private void OpenFileClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            if ((bool) dlg.ShowDialog())
            {
                currentFileName = dlg.FileName;
                textEditor.Load(currentFileName);                
            }
        }

        private void SaveFileClick(object sender, EventArgs e)
        {
            if (currentFileName == null)
            {
                var dlg = new SaveFileDialog {DefaultExt = ".txt"};
                if (!((bool) dlg.ShowDialog()))
                    return;

                currentFileName = dlg.FileName;

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


        private void SetupHotkeysAndCommands()
        {
            var ib = new InputBinding(UiCommands.Execute, new KeyGesture(Key.F5));
            InputBindings.Add(ib);

            ib = new InputBinding(UiCommands.BeautifyJson, new KeyGesture(Key.B, ModifierKeys.Control));
            InputBindings.Add(ib);

            // Bind handler
            var cb = new CommandBinding(UiCommands.Execute);
            cb.Executed += ExecuteHandler;
            CommandBindings.Add(cb);

            cb = new CommandBinding(UiCommands.BeautifyJson);
            cb.Executed += BeautifyJsonHandler;
            CommandBindings.Add(cb);
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
