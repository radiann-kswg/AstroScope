# AstroScope Astrological Engine

This module provides reusable services for Western horoscope (zodiac + houses) and Eastern 九星気学 (Nine Star Ki) calculations inside Unity 6.

## 機能概要

- **エフェメリス計算**: 太陽・月・水金地火木土天海冥の黄経・黄緯・距離を低軌道要素から導出。
- **ハウス計算**: 等ハウス方式でアセンダントと 12 室、MC を算出。
- **太陰太陽暦**: 新月アルゴリズムで朔日を推定し、月齢と閏月判定を生成。
- **節分検出**: 太陽黄経 315° の通過時刻を二分探索で特定し、ローカルタイムへ変換。
- **九星気学**: 年・月・日盤を 9 周期の数値で返却。

## 主なスクリプト

| ファイル                                        | 役割                               |
| ----------------------------------------------- | ---------------------------------- |
| `Scripts/Math/Angle.cs`                         | 角度計算ユーティリティ。           |
| `Scripts/Time/JulianDate.cs`                    | 日時とユリウス日 JD/Century 変換。 |
| `Scripts/Core/EphemerisCalculator.cs`           | 惑星・太陽・月の位置計算。         |
| `Scripts/Western/HouseCalculator.cs`            | 等ハウス方式のハウス計算。         |
| `Scripts/Western/WesternHoroscopeCalculator.cs` | 西洋ホロスコープ統合。             |
| `Scripts/Eastern/EasternAstrologyCalculator.cs` | 太陰太陽暦と九星気学の算出。       |
| `Scripts/Core/AstroScopeService.cs`             | 西洋/東洋結果をまとめて提供。      |
| `AstroScpoeMain.cs`                             | サンプル Monobehaviour。           |
| `Tests/EditMode/AstroScopeCalculatorTests.cs`   | NUnit ベースの検証テスト。         |

## 利用方法

1. シーンに `AstroScpoeMain` を配置し、緯度・経度・タイムゾーンを設定。
2. 実行またはインスペクターのコンテキストメニュー「Compute Horoscopes Now」で結果を `Console` に出力。
3. サービスを直接利用する場合:

```csharp
var service = new AstroScopeService();
var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo");
DateTime localTime = new DateTime(2024, 2, 4, 12, 0, 0);
var (western, eastern) = service.ComputeFull(localTime, 35.6895, 139.6917, timeZone);
```

## テスト(Mac 環境)

Unity Test Runner の Edit Mode で `AstroScope/Tests/EditMode/AstroScopeCalculatorTests` を実行してください。コマンドライン実行例 (Unity エディターインストール済みの場合):

```bash
/Applications/Unity/Hub/Editor/6000.0.0f1/Unity.app/Contents/MacOS/Unity \
  -projectPath "${PWD}" \
  -runTests -testPlatform editmode \
  -testResults ./TestResults.xml
```

> **Note:** バージョン番号は使用中の Unity6 β/正式版に合わせて置き換えてください。

## 既知の制限

- 惑星位置は Meeus 第 2 版の簡易係数を使用しており、数分角程度の誤差が想定されます。
- 九星気学の月盤判定は簡略化された主気法に基づいているため、公式暦と差異が発生する可能性があります。必要に応じて補正テーブルの導入をご検討ください。
