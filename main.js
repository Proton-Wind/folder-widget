const { app, BrowserWindow, session } = require('electron');
const path = require('path');
const fs = require('fs');

// 設定ファイルの保存場所
const configPath = () => path.join(app.getPath('userData'), 'window-state.json');

let lastValidBounds = null;

// 位置・サイズの読み込み
function loadWindowState() {
  try {
    const data = fs.readFileSync(configPath(), 'utf8');
    const state = JSON.parse(data);
    return {
      width: (typeof state.width === 'number' && state.width >= 200) ? state.width : 380,
      height: (typeof state.height === 'number' && state.height >= 200) ? state.height : 600,
      x: typeof state.x === 'number' ? state.x : undefined,
      y: typeof state.y === 'number' ? state.y : undefined
    };
  } catch (e) {
    return { width: 380, height: 600, x: undefined, y: undefined };
  }
}

// 正常値の更新・保存関数
function updateValidBounds(win) {
  if (!win || win.isDestroyed()) return;
  if (win.isMinimized() || win.isMaximized()) return;

  try {
    const bounds = win.getNormalBounds ? win.getNormalBounds() : win.getBounds();

    if (bounds.width >= 200 && bounds.height >= 200) {
      lastValidBounds = {
        x: bounds.x,
        y: bounds.y,
        width: bounds.width,
        height: bounds.height
      };
      fs.writeFileSync(configPath(), JSON.stringify(lastValidBounds));
    }
  } catch (e) {
    console.error('保存エラー:', e);
  }
}

app.whenReady().then(async () => {
  const state = loadWindowState();
  lastValidBounds = { ...state };

  const winOptions = {
    width: state.width,
    height: state.height,
    transparent: true,
    frame: false,
    show: false, // ★重要: 最初は非表示で作成（ディスプレイ跨ぎ時のDPI誤変換を遮断）
    webPreferences: {
      nodeIntegration: false
    }
  };

  if (typeof state.x === 'number' && typeof state.y === 'number') {
    winOptions.x = state.x;
    winOptions.y = state.y;
  }

  const win = new BrowserWindow(winOptions);

  // ★重要: モニター2へ配置された直後、125%→100%のDPI移動で縮んだサイズを380pxに強制固定して表示
  win.once('ready-to-show', () => {
    if (typeof state.x === 'number' && typeof state.y === 'number') {
      win.setBounds({
        x: state.x,
        y: state.y,
        width: state.width,
        height: state.height
      });
    }
    win.show(); // 完璧なサイズ・位置になってから画面に表示
  });

  // マウス操作でドラッグ・リサイズを完了した時のみ保存
  win.on('resized', () => updateValidBounds(win));
  win.on('moved', () => updateValidBounds(win));

  // アプリ終了時はメモリ上の正常値だけを書き出す
  win.on('close', () => {
    if (lastValidBounds) {
      try {
        fs.writeFileSync(configPath(), JSON.stringify(lastValidBounds));
      } catch (e) {}
    }
  });

  // キャッシュクリア
  try {
    if (session.defaultSession) {
      await session.defaultSession.clearCache();
    }
  } catch (e) {}

  // ★ご自身の GitHub Pages の URL に書き換えてください
  const targetUrl = 'https://meiden-widget.nd.meiden.ed.jp/h-folder-widget/';

  win.loadURL(targetUrl, {
    extraHeaders: 'pragma: no-cache\nCache-Control: no-cache\n'
  }).catch(err => console.error(err));
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit();
});
