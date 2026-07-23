
# electronでの開発
- 必要な環境
  - Node.js環境
- 必要なファイル
  - package.json
  - main.js
  - widget-start.vbs
- ライブラリーのインストールとコマンド起動
  - 上記の３つのファイルを１つのフォルダに入れる
  - コマンドプロンプトやターミナルを使って，ファイルを入れたフォルダで以下を実行
```
npm install electron --save-dev
npm start
```
- 通常起動・終了の仕方
  - widget-start.vbsをダブルクリックで起動
  - タスクバーを右クリックして「×ウィンドウを閉じる」を選択する