if (WScript.Arguments.length > 0) {
    var rawUrl = WScript.Arguments(0);
    
    // 1. 先頭の "open-folder:" を削除
    var path = rawUrl.replace(/^open-folder:/i, '');
    
    // 2. JavaScript標準機能で超高速＆正確にURLデコード
    try {
        path = decodeURIComponent(path);
    } catch (e) {
        // 例外発生時はそのまま使用
    }
    
    // 3. スラッシュをバックスラッシュに置換して "\\server\share" 形式にする
    path = path.replace(/\//g, '\\');
    path = path.replace(/^\\+/, '');
    path = '\\\\' + path;
    
    // 4. エクスプローラーで起動（クリックした瞬間に開く）
    var shell = WScript.CreateObject("WScript.Shell");
    shell.Run('explorer.exe "' + path + '"', 1, false);
}