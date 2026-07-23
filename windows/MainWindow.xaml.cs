using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;

namespace WidgetApp
{
    public class WindowStateData
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; } = 380;
        public double Height { get; set; } = 600;
    }

    public partial class MainWindow : Window
    {
        private string configPath;
        private bool isLoaded = false;

        // Win32 API: 確実にウィンドウをドラッグ移動させるネイティブ処理
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        public MainWindow()
        {
            InitializeComponent();

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder = Path.Combine(appData, "WidgetApp");
            Directory.CreateDirectory(folder);
            configPath = Path.Combine(folder, "window-state.json");

            LoadWindowState();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await webView.EnsureCoreWebView2Async();

                // 1. 最下部リンクポップアップ（ステータスバー）を非表示
                webView.CoreWebView2.Settings.IsStatusBarEnabled = false;

                // 2. API側はエラー回避のため Transparent を指定
                webView.DefaultBackgroundColor = System.Drawing.Color.Transparent;

                // 3. Web画面からのメッセージ受信処理（ドラッグ移動用）
                webView.WebMessageReceived += WebView_WebMessageReceived;

                // 4. HTML側に目に見えない極小透過 (0.01) とドラッグ監視スクリプトを自動挿入
                await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
                    window.addEventListener('DOMContentLoaded', () => {
                        // 見た目は完全透明だが、マウス反応(HitTest)を100%受け取る極小背景色を設定
                        document.body.style.backgroundColor = 'rgba(0, 0, 0, 0.01)';

                        const dragTargets = document.querySelectorAll('.widget-title, body');
                        dragTargets.forEach(el => {
                            el.addEventListener('mousedown', (e) => {
                                // ボタン、リンク、入力欄、折りたたみメニュー以外のクリック時にC#へ移動メッセージ送信
                                if (!e.target.closest('a, button, input, summary, details, .folder-link')) {
                                    window.chrome.webview.postMessage('start_drag');
                                }
                            });
                        });
                    });
                ");

                // ★ご自身の GitHub Pages URL に書き換えてください
                string targetUrl = "https://meiden-widget.nd.meiden.ed.jp/h-folder-widget/";

                if (Uri.TryCreate(targetUrl, UriKind.Absolute, out Uri? uriResult) && uriResult != null)
                {
                    webView.Source = uriResult;
                }
                else
                {
                    MessageBox.Show("URLの形式が正しくありません。MainWindow.xaml.cs 内の URL を確認してください。", "設定エラー");
                }

                isLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"WebView2初期化エラー:\n{ex.Message}", "エラー");
            }
        }

        // Web画面からドラッグ通知を受け取ったら、OSのネイティブ移動処理を発火
        private void WebView_WebMessageReceived(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            if (e.TryGetWebMessageAsString() == "start_drag")
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                ReleaseCapture();
                SendMessage(hwnd, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
            }
        }

        private void LoadWindowState()
        {
            try
            {
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var state = JsonSerializer.Deserialize<WindowStateData>(json);
                    if (state != null)
                    {
                        this.Width = state.Width >= 150 ? state.Width : 380;
                        this.Height = state.Height >= 150 ? state.Height : 600;
                        this.Left = state.Left;
                        this.Top = state.Top;
                        return;
                    }
                }
            }
            catch { }

            this.Width = 380;
            this.Height = 600;
        }

        private void SaveWindowState()
        {
            if (!isLoaded || this.WindowState != WindowState.Normal) return;

            try
            {
                var state = new WindowStateData
                {
                    Left = this.Left,
                    Top = this.Top,
                    Width = this.Width,
                    Height = this.Height
                };
                string json = JsonSerializer.Serialize(state);
                File.WriteAllText(configPath, json);
            }
            catch { }
        }

        private void Window_LocationChanged(object sender, EventArgs e) => SaveWindowState();
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) => SaveWindowState();
        private void Window_Closing(object sender, CancelEventArgs e) => SaveWindowState();
    }
}