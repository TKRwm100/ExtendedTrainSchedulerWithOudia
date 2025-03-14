# BveEx.Plugins.ExtendedTrainSchedulerWithOudia

[OudiaSecond](http://oudiasecond.seesaa.net/)のファイルをもとに[改造他列車走行スケジュール拡張プラグイン](https://github.com/TKRwm100/BveEx.Plugins.ExtendedTrainScheduler)のステートメントを生成するプラグインです.

## 利用方法

マップ構文, 設定ファイルを適切に記述してください.
尚, 動作には [BveEx](https://bveex.okaoka-depot.com/)のver2.0.50314.1以降, [改造他列車走行スケジュール拡張プラグイン](https://github.com/TKRwm100/BveEx.Plugins.ExtendedTrainScheduler/releases) が別途必要です.[![BveEx](https://www.okaoka-depot.com/contents/bve/banner_AtsEX.svg)](https://bveex.okaoka-depot.com/)
又, 動作にはリソースファイルが必要です. 再配布等の際は忘れず同梱してください.

設定によってはプラグイン以下 output/Toukaitetudou/ExtendedTrainSchedulerWithOudia/(ファイル名)\_(行番号)\_(列番号).map 内に構文が生成されます.

微修正の場合生成されたファイルを編集しマップ構文を削除の上ex_includeにて当該ファイルの読み込みをしてください.

尚, 構文解説については[__こちら__](Reference/Reference.pdf)をご参照ください.

## サンプルシナリオについて

サンプルシナリオでは内部でBveExサンプルシナリオを参照しています.
BveExサンプルシナリオと同階層に配置する等, 参照依存を適切に解決してください.

## よくあるであろうお問い合わせ

### 読み込んだ際 "ハンドルされていない例外" のウインドウが出る

ゾーン識別子の削除ができていない可能性があります.

エクスプローラにてファイルを右クリック->プロパティ->許可するにチェックを入れて適用又はOKを押下してください.

### 読み込んだ際 "この BveEX プラグインはバージョン  以上の BveEX でのみ動作します。" のウインドウが出る

BveExのバージョンが最低バージョンより低いです.
少なくともウインドウ内に書かれているバージョンにしてください.

### 読み込んだ際 "oud2ファイルのバージョンが適切ではありません." のエラーが出る

対応しているoud2のバージョンではありません. 対応しているバージョンへバージョンアップしてください.
対応バージョンは以下の通りです.

|  プラグインバージョン  |  Oudiaバージョン  |  補足  |
|  --  |  --  |  --  |
|  0.1.0.0  |  OudiaSecond.1.10  |    |
|  0.1.0.0  |  OudiaSecond.1.11  |  理論値  |
|  0.1.0.0  |  OudiaSecond.1.12  |  理論値  |
|  0.1.0.0  |  OudiaSecond.1.13  |  理論値  |
|  0.1.0.0  |  OudiaSecond.1.14  |  理論値  |
|  0.1.0.0  |  OudiaSecond.1.15  |    |

### 読み込んだ際 "運用番号が定義されていません." のエラーが出る

参照するoud2ファイルで運用番号が設定されていません. 適切に設定してください.

### 途中駅の停車が全く反映されない

未改造の他列車走行スケジュール拡張プラグインを参照している可能性があります.
改造済みの[改造他列車走行スケジュール拡張プラグイン](https://github.com/TKRwm100/BveEx.Plugins.ExtendedTrainScheduler/releases)を参照してください.

### 他列車の挙動が明らかに異常である

他列車が重複している可能性があります.
キーが ExtendedTrainSchedulerWithOudia_(列車番号) の他列車が存在しないか確認してください.

それでも解決しない場合[改造他列車走行スケジュール拡張プラグイン](https://github.com/TKRwm100/BveEx.Plugins.ExtendedTrainScheduler)の問題の可能性があります.

### ～ができません

殆どの場合仕様です.

気が向いたら作るかもですが, それが待てないならば是非ご自身で実装してください.

## ライセンス

[PolyForm Noncommercial License 1.0.0](LICENSE.md)

## 使用ライブラリ等

### [BveEx.CoreExtensions](https://github.com/automatic9045/BveEX)(PolyForm NonCommercial 1.0.0)

Copyright (c) 2022 automatic9045

### [BveEx.PluginHost](https://github.com/automatic9045/BveEX) (PolyForm NonCommercial 1.0.0)

Copyright (c) 2022 automatic9045

### [Harmony](https://github.com/pardeike/Harmony) (MIT)

Copyright (c) 2017 Andreas Pardeike
