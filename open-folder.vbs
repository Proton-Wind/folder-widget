Set objArgs = WScript.Arguments
If objArgs.Count > 0 Then
    Dim rawUrl, psCmd
    rawUrl = objArgs(0)
    
    ' シングルクォートのエスケープ処理
    rawUrl = Replace(rawUrl, "'", "''")
    
    ' PowerShellで完全かつ正確にURLデコードしてフォルダを開くコマンド
    psCmd = "powershell.exe -NoProfile -NonInteractive -WindowStyle Hidden -Command """ & _
            "$raw = '" & rawUrl & "';" & _
            "$path = [System.Uri]::UnescapeDataString($raw) -replace '^open-folder:', '';" & _
            "if ($path -match '^[/\\]+') { $path = '\\' + ($path -replace '^[/\\]+', '') };" & _
            "Start-Process 'explorer.exe' -ArgumentList ('""' + $path + '""')"""
    
    ' 第2引数の「0」により、PowerShell起動時の黒い画面をOSレベルで完全に非表示化
    Set objShell = CreateObject("WScript.Shell")
    objShell.Run psCmd, 0, False
End If