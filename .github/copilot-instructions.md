# GitHub Copilot Instructions for AstroScope

## このドキュメントについて

- GitHub Copilot や各種 AI ツールが本リポジトリのコンテキストを理解しやすくするためのガイドです。
- 新しい機能を実装する際はここで示す技術選定・設計方針・モジュール構成を前提にしてください。
- 不確かな点がある場合は、リポジトリのファイルを探索し、ユーザーに「こういうことですか？」と確認してください。

## 前提条件

- **言語**: 回答は必ず日本語でしてください。
- **変更規模**: 大規模な変更（例: 100 行以上の追加・削除）を行う前には、まず変更計画を提案し、「このような計画で進めようと思います。」と提示してください。
- **コードスタイル**: 既存のコードスタイルと命名規則を維持してください。
- **XML ドキュメント**: public メソッド・プロパティには必ず XML ドキュメントコメント (`/// <summary>`) を追加してください。
- **エラーハンドリング**: 計算エラーや無効な入力値に対する適切な例外処理を含めてください。
- **指示書の更新**: セッション内で設計方針・運用ルールが固まった場合、今後も再利用する内容は `.github/copilot-instructions.md` へ反映する前提で扱ってください。

## アプリの概要

AstroScope は Unity 6 で動作する天文時計・ホロスコープシステムです。主な機能：

- **西洋占星術**: 惑星位置計算、ハウス計算（等ハウス方式）、サインの配置、インターセプト検出
- **東洋占星術**: 太陰太陽暦、九星気学、干支（十干十二支）、二十四節気、土用期間
- **ローカライズ**: 英語・日本語対応の天体名・サイン名・節気名表示
- **Unity 統合**: MonoBehaviour を使用したランタイム計算とログ出力

## 技術スタック

- **Unity**: 6.0 LTS
- **言語**: Unity C# (.NET Standard) 2.1
- **テストフレームワーク**: Unity Test Runner (NUnit)
- **プラットフォーム**: Windows, macOS
- **タイムゾーン**: Windows (Windows タイムゾーン ID) / macOS (IANA タイムゾーン ID)

## ディレクトリ構成

```
Assets/AstroScope/
├── Scripts/
│   ├── Math/
│   │   └── Angle.cs                        # 角度正規化ユーティリティ
│   ├── Time/
│   │   ├── AstroTime.cs                    # 恒星時・黄道傾斜角計算
│   │   └── JulianDate.cs                   # ユリウス日変換
│   ├── Core/
│   │   ├── AstroScopeService.cs            # 統合ファサード（西洋・東洋）
│   │   └── EphemerisCalculator.cs          # エフェメリス計算エンジン
│   ├── Western/
│   │   ├── HouseCalculator.cs              # ハウス計算（等ハウス方式）
│   │   ├── WesternHoroscopeCalculator.cs   # 西洋ホロスコープ計算
│   │   └── ZodiacUtility.cs               # 黄道十二宮ユーティリティ
│   ├── Eastern/
│   │   └── EasternAstrologyCalculator.cs   # 東洋占星術計算（太陰太陽暦・九星・干支・節気）
│   ├── Localization/
│   │   └── AstroLocalization.cs            # 多言語対応（English/Japanese）
│   ├── Data/
│   │   ├── CelestialTypes.cs               # 天体・サイン・ハウスデータ構造
│   │   ├── EasternAstrologyTypes.cs        # 東洋占星術データ構造
│   │   ├── OrbitalElements.cs              # 軌道要素データ構造
│   │   └── OrbitalElementsDatabase.cs      # 軌道要素データベース
│   └── UI/
│       ├── AstroScopeCelestialPreview.cs   # 天体プレビュー UI
│       └── AstroScopeLogView.cs            # ログ表示 UI
├── Tests/
│   └── EditMode/
│       └── AstroScopeCalculatorTests.cs    # Unity Test Runner 用テスト
├── AstroScopeMain.cs      # サンプル MonoBehaviour
└── README.md
```

## アーキテクチャ・設計指針

- **サービス指向**: `AstroScopeService` が西洋・東洋計算の統合ファサードとして機能
- **計算分離**: 各領域（西洋/東洋）は独立した Calculator クラスで実装
- **データ構造**: 計算結果は immutable な構造体 (`Result` サフィックス) で返却
- **ユーティリティ**: 共通機能（角度正規化、時刻変換）は静的クラスで提供
- **ローカライズ**: `AstroLocalization` クラスで言語切り替えを一元管理

## 命名法則テーブル（新規コード記述基準）

新しいコードを書く際は以下のテーブルを参照してください。

| 対象                                | 規則                      | 例                                                 |
| ----------------------------------- | ------------------------- | -------------------------------------------------- |
| クラス名・構造体名・enum 名         | PascalCase                | `EphemerisCalculator`, `CelestialPosition`         |
| public メソッド名                   | PascalCase                | `ComputePlanetPositions`, `CalculateForceAndAngle` |
| private / protected メソッド名      | PascalCase                | `NormalizeAngle`, `ApplyObliquity`                 |
| public プロパティ名                 | PascalCase                | `LongitudeDegrees`, `DistanceAstronomicalUnits`    |
| public フィールド（Inspector 公開） | camelCase（`_` なし）     | `latitudeDegrees`, `timeZoneId`                    |
| private フィールド                  | `_` + camelCase           | `_western`, `_eastern`, `_obliquityRadians`        |
| ローカル変数                        | camelCase                 | `julianDate`, `ascendantDegrees`                   |
| const / static readonly 定数        | PascalCase                | `MaxIterations`, `J2000Epoch`                      |
| パラメータ名                        | camelCase                 | `dateTimeUtc`, `latitudeDegrees`                   |
| 計算クラス                          | `Calculator` サフィックス | `HouseCalculator`, `EphemerisCalculator`           |
| データ構造（複数型まとめ）          | `Types` サフィックス      | `CelestialTypes`, `EasternAstrologyTypes`          |
| 計算結果構造体                      | `Result` サフィックス     | `WesternHoroscopeResult`, `EasternAstrologyResult` |
| テストクラス                        | `Tests` サフィックス      | `AstroScopeCalculatorTests`                        |
| Unity コンポーネント                | 機能名 PascalCase         | `AstroScopeCelestialPreview`, `AstroScopeLogView`  |

## コーディング規則

### ファイル構成

- **計算クラス**: `Calculator` サフィックス
- **データ構造**: `Types` サフィックス（複数の型を含む場合）
- **結果構造体**: `Result` サフィックス

### XML ドキュメント必須項目

```csharp
/// <summary>
/// 機能の簡潔な説明
/// </summary>
/// <param name="paramName">パラメータの説明</param>
/// <returns>戻り値の説明</returns>
```

## テスト方針

- **フレームワーク**: Unity Test Runner (Edit Mode)
- **テストファイル**: `*Tests.cs` の命名規則
- **配置**: `Assets/AstroScope/Tests/EditMode/`
- **カバレッジ**: 公開 API の主要パスを網羅
- **テストデータ**: 既知の天体位置（2025 年基準）で検証

### テスト例

```csharp
[Test]
public void ComputePlanetPositions_ValidDate_ReturnsAccuratePositions()
{
    // Arrange, Act, Assert パターンを使用
}
```

## アンチパターン

### 禁止事項

- **ハードコードされた定数**: マジックナンバーの直接使用は禁止（定数で定義）
- **グローバル変数**: static フィールドでの状態管理は避ける
- **例外の無視**: try-catch で例外を握りつぶさない
- **プラットフォーム固有コード**: Windows/macOS 専用の実装を避ける（タイムゾーン以外）

### 非推奨パターン

- **過度なネスト**: メソッド内の if-else は 3 層まで
- **長大なメソッド**: 50 行を超えるメソッドは分割を検討
- **曖昧な変数名**: `temp`, `data`, `result` などの汎用名は避ける

## Unity 固有の考慮事項

### 実行時パフォーマンス

- **フレームレート**: `Update()` での重い計算は避け、必要時のみ実行
- **メモリ**: 毎フレームの `new` インスタンス生成を避ける
- **ログ**: `Debug.Log` ではなく `AstroLogger` 経由でローカライズ対応

### Inspector・シリアライズ

- **フィールド公開**: `[SerializeField]` でインスペクター公開を推奨。`public` フィールドは可能な限り避ける
- **データ永続化**: `[System.Serializable]` でデータ永続化
- **`public` フィールドのリネームリスク**: Inspector に表示される `public` フィールドをリネームすると、Prefab・Scene の Inspector 値が失われる可能性がある。リネーム前に必ず Prefab・Scene 上の参照状況を確認すること

### Unity アセット操作

- **Prefab・Scene の YAML 構造**: `.unity`・`.prefab` ファイルは Unity の YAML テキスト形式。C# のフィールド名が `propertyPath` として記録されるため、識別子リネーム時は YAML 側の更新も必要になる場合がある
- **Editor を閉じてから編集**: YAML を直接テキスト置換する場合は Unity Editor を必ず閉じてから行い、再起動後に自動再インポートを確認する
- **タグ・レイヤー**: タグは `TagManager.asset` で管理。C# の文字列参照と asset 側の定義が常に一致していることを確認する

### 識別子リネーム時のリスク分類

| 識別子の種類                                                  | リスク                 | 対応方針                                                                 |
| ------------------------------------------------------------- | ---------------------- | ------------------------------------------------------------------------ |
| `private` フィールド・ローカル変数・`private` メソッド        | 低（安全）             | C# のみ変更                                                              |
| `public` フィールド（Inspector 参照あり）                     | 中（要注意）           | Prefab/Scene の参照を事前確認してから変更                                |
| `[Serializable]` クラスの `public` フィールド（セーブデータ） | 高（リリース後は禁止） | ゲームリリース前であれば C# + セーブデータ両方変更、リリース後は変更禁止 |
| GameObject 名・Prefab ファイル名                              | Editor 操作が必要      | Unity Editor の Rename 機能を使用                                        |

## 特殊な実装ルール

### 天文計算

- **精度**: Meeus "Astronomical Algorithms" 第 2 版の係数を使用
- **座標系**: J2000.0 基準の黄道座標系
- **時刻系**: UTC ベースでローカル時刻との変換を明示

### ローカライズ

- **言語切り替え**: `AstroLanguage` enum で English/Japanese を指定
- **文字列リソース**: `AstroLocalization` クラスで一元管理
- **フォールバック**: 未定義の場合は英語を返却

### プラットフォーム対応

- **タイムゾーン**: Windows (`Tokyo Standard Time`) vs IANA (`Asia/Tokyo`) の適切な使い分け
- **パス区切り**: `Path.Combine` を使用してクロスプラットフォーム対応

これらの指針に従って、一貫性があり保守しやすいコードの生成をお願いします。
