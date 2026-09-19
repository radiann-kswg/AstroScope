# AstroScope Astrological Engine

This module provides reusable services for Western horoscope (zodiac + houses) and Eastern 九星気学 (Nine Star Ki) calculations inside Unity 6.

## 機能概要

- **エフェメリス計算**: 太陽・月・水金地火木土天海冥の黄経・黄緯・距離を Meeus ベースの軌道要素から導出し、サイン内度数も同時に計算。
- **ハウス計算**: 等ハウス方式でアセンダントと 12 室、MC を算出し、サインの重複・欠落によるインターセプトも自動検出。
- **太陰太陽暦**: 新月アルゴリズムで朔日を推定し、月齢、閏月、節月（主気）の有無を判定。
- **二十四節気+土用**: 24 節気の通過時刻と、立春・立夏・立秋・立冬／翌立春を基準にした春夏秋冬土用期間を算出。
- **九星気学 & 干支**: 年・月・日盤 (九星) および 年・月・日干支 (十干十二支) を取得。
- **ローカライズ対応**: `AstroLocalization` で英語/日本語の UI 文字列を集約。天体名・サイン名・節気名・干支名などを即座に切替可能。

## 主なスクリプト

| ファイル                                        | 役割                                             |
| ----------------------------------------------- | ------------------------------------------------ |
| `Scripts/Math/Angle.cs`                         | 角度計算ユーティリティ。                         |
| `Scripts/Time/JulianDate.cs`                    | 日時とユリウス日 JD/Century 変換。               |
| `Scripts/Core/EphemerisCalculator.cs`           | 惑星・太陽・月の位置計算。                       |
| `Scripts/Western/HouseCalculator.cs`            | 等ハウス方式のハウス計算とインターセプト検出。   |
| `Scripts/Western/WesternHoroscopeCalculator.cs` | 西洋ホロスコープ統合 (サイン割当 & 欠落サイン)。 |
| `Scripts/Eastern/EasternAstrologyCalculator.cs` | 太陰太陽暦・九星・干支・節気・土用の算出。       |
| `Scripts/Localization/AstroLocalization.cs`     | 英語/日本語ローカライズ用リソース。              |
| `Scripts/Core/AstroScopeService.cs`             | 西洋/東洋結果をまとめて提供。                    |
| `AstroScpoeMain.cs`                             | サンプル Monobehaviour。                         |
| `Tests/Editor/AstroScopeCalculatorTests.cs`   | NUnit ベースの検証テスト。                       |

## 利用方法

1. シーンに `AstroScpoeMain` を配置し、緯度・経度・タイムゾーンを設定。
2. 実行またはインスペクターのコンテキストメニュー「Compute Horoscopes Now」で結果を `Console` に出力。
3. サービスを直接利用する場合:

```csharp
var service = new AstroScopeService();
var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo");
DateTime localTime = new DateTime(2024, 2, 4, 12, 0, 0);
var (western, eastern) = service.ComputeFull(localTime, 35.6895, 139.6917, timeZone);

// ローカライズ例
AstroLanguage language = AstroLanguage.Japanese;
foreach (var placement in western.Placements)
{
	string planet = AstroLocalization.GetPlanetName(placement.Body, language);
	string sign = AstroLocalization.GetZodiacName(placement.Sign, language);
	Console.WriteLine($"{planet}: {sign} {placement.DegreesInSign:F2}°");
}

string yearGanzhi = AstroLocalization.GetSexagenaryName(eastern.Sexagenary.Year, language);
Console.WriteLine($"干支(年): {yearGanzhi}");
```

## API リファレンス (概要)

- `AstroScopeService`
  - `ComputeWestern(DateTime utc, double lat, double lon)` : `WesternHoroscopeResult`
  - `ComputeEastern(DateTime local, double lon, TimeZoneInfo tz)` : `EasternAstrologyResult`
  - `ComputeFull(DateTime local, double lat, double lon, TimeZoneInfo tz)` : 両者まとめて取得。
- `WesternHoroscopeResult`
  - `Bodies`: `CelestialPosition` のリスト (黄経/緯度/距離/サイン内度数)。
  - `Placements`: UI 向け {惑星, サイン, サイン内度数}。
  - `Houses`: `HouseCalculationResult` (アセンダント・MC・ハウスカスプ・インターセプト情報)。
  - `MissingCuspSigns`: ハウスカスプに現れないサイン一覧。
- `EasternAstrologyResult`
  - `LunisolarDate`, `LunarAgeDays`, `SetsubunLocal/Utc`。
  - `Chart`: 九星気学 (年/月/日盤)。
  - `Sexagenary`: 干支 (年/月/日)。
  - `SolarTerms`: その年の 24 節気 (`SolarTermEntry`)。
  - `DoyouPeriods`: 春夏秋冬の土用期間。
- `AstroLocalization`
  - `GetPlanetName`, `GetZodiacName`, `GetSolarTermName`, `GetSexagenaryName` など、英語/日本語テキストを返すユーティリティ。

## Unity6 (Windows) での動作確認シーン

### シーン構成例

- `Assets/Scenes/AstroScopeDemo.unity` を新規作成し、以下の GameObject を配置します。
  - `Directional Light` / `Main Camera` : Unity の既定オブジェクトをそのまま利用。
  - `AstroScopeSystem` (Empty GameObject)
  - `AstroScpoeMain` コンポーネントを追加。
  - インスペクター設定値の例:
    - `Latitude Degrees`: `35.6895`
    - `Longitude Degrees`: `139.6917`
    - `Time Zone Id`: **Windows では** `Tokyo Standard Time`（IANA 形式 `Asia/Tokyo` は例外で落ちるため要注意）
    - `Language`: `Japanese` もしくは `English`
    - `Compute On Start`: `true`
  - `Canvas` (Screen Space - Overlay)
  - `Panel` (Image) : 任意の背景色を設定し、幅 600px / 高さ 400px 程度の情報パネルを作成。
    - `Button` (TextMeshPro 推奨) : `OnClick` に `AstroScopeSystem` の `AstroScpoeMain.ComputeAndLogHoroscopes` を登録して手動再計算を可能に。
    - `Scroll View` (任意) : Console 出力を UI にも転記したい場合は、以下のサンプルを参考にログ集約用スクリプトを別途配置してください。

```csharp
// Assets/AstroScope/Scripts/UI/AstroScopeLogView.cs (任意で作成)
using System.Text;
using UnityEngine;
using TMPro;

public class AstroScopeLogView : MonoBehaviour
{
  [SerializeField] private TMP_Text target;
  private readonly StringBuilder buffer = new();

  private void OnEnable()
  {
    Application.logMessageReceived += HandleLog;
  }

  private void OnDisable()
  {
    Application.logMessageReceived -= HandleLog;
  }

  private void HandleLog(string condition, string stackTrace, LogType type)
  {
    if (type == LogType.Log && condition.StartsWith("[AstroScope]"))
    {
      buffer.AppendLine(condition);
      if (target != null)
      {
        target.text = buffer.ToString();
      }
    }
  }
}
```

### 動作確認手順

1. 上記シーンを保存して開いた状態で、`Window > General > Console` を表示。
2. Play モードに入ると `Compute On Start` により即時に西洋 / 東洋の結果がログに出力されます。
3. 出力例 (Japanese 設定):

- `Sun (牡羊座 12.34°): λ=...`
- `インターセプト: 双子座 → 第5室`
- `Sexagenary: Year=甲辰, Month=乙卯, Day=丙子`
- `Upcoming Solar Terms` / `土用` 期間など

4. 値や表示言語を変更したい場合は Play 中に `Language` や緯度・経度・タイムゾーンを編集し、`Button` から再計算。

### 補足

- `Time Zone Id` は Windows と macOS で名称が異なります。Windows: `Tokyo Standard Time` / macOS: `Asia/Tokyo`。他地域でも Windows のタイムゾーン ID 一覧を確認し、例外発生時はログに表示されるフォールバックメッセージを参考に修正してください。
- エディットモードテストは `Test Runner (Window > General > Test Runner)` の `Edit Mode` タブから `AstroScope/Tests/Editor` 配下を実行できます。

## テスト(Mac 環境)

Unity Test Runner の Edit Mode で `AstroScope/Tests/Editor/AstroScopeCalculatorTests` を実行してください。コマンドライン実行例 (Unity エディターインストール済みの場合):

```bash
/Applications/Unity/Hub/Editor/6000.0.0f1/Unity.app/Contents/MacOS/Unity \
  -projectPath "${PWD}" \
  -runTests -testPlatform editmode \
  -testResults ./TestResults.xml
```

> **Note:** バージョン番号は使用中の Unity6 β/正式版に合わせて置き換えてください。

## 既知の制限

- 惑星位置は Meeus 第 2 版の簡易係数を使用しており、数分角程度の誤差が想定されます。
- 九星気学の月盤および干支の月判定は簡略化された主気法に基づいているため、公式暦と差異が発生する可能性があります。必要に応じて補正テーブルの導入をご検討ください。
- 土用期間は節気の実測値から一律 18 日を遡る方式で計算しています。細かな流派差異には未対応です。
