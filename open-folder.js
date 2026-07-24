if (WScript.Arguments.length > 0) {
    var rawUrl = WScript.Arguments(0);
    var path = rawUrl.replace(/^open-folder:/i, '');
    try {
        path = decodeURIComponent(path);
    } catch (e) {}
    path = path.replace(/\//g, '\\');
    path = path.replace(/^\\+/, '');
    path = '\\\\' + path;
    var shell = WScript.CreateObject("WScript.Shell");
    shell.Run('explorer.exe "' + path + '"', 1, false);
}