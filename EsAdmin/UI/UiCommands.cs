using System.Windows.Input;

namespace EsAdmin
{
    public static class UiCommands
    {
        public static ICommand Execute = new RoutedCommand();

        public static ICommand BeautifyJson = new RoutedCommand();

        public static ICommand SaveFile = new RoutedCommand();

        public static ICommand OpenFile = new RoutedCommand();

        public static ICommand NewFile = new RoutedCommand();

        public static ICommand FoldAll = new RoutedCommand();
    }
}
