using System.Windows.Input;

namespace EsAdmin
{
    public static class UiCommands
    {
        public static ICommand Execute = new RoutedCommand();

        public static ICommand BeautifyJson = new RoutedCommand();
    }
}
