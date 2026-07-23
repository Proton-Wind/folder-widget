using System.Windows;

namespace WidgetApp
{
    public partial class App : Application
    {
        public App()
        {
            // アプリ内で未処理のエラーが発生した場合にダイアログを表示する
            this.DispatcherUnhandledException += (s, e) =>
            {
                MessageBox.Show($"エラーが発生しました:\n{e.Exception.Message}", "起動エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            };
        }
    }
}