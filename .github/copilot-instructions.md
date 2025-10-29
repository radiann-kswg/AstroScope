# GitHub Copilot Instructions for AstroScope

## 前提条件

- **言語**: 回答は必ず日本語でしてください。
- **変更規模**: 大規模な変更（例: 100 行以上の追加・削除）を行う前には、まず変更計画を提案してください。
- **コードスタイル**: 既存のコードスタイルと命名規則を維持してください。
- **XML ドキュメント**: public メソッド・プロパティには必ず XML ドキュメントコメント (`/// <summary>`) を追加してください。
- **エラーハンドリング**: 計算エラーや無効な入力値に対する適切な例外処理を含めてください。

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
│   ├── Math/              # 数学ユーティリティ（角度、ユリウス日）
│   ├── Time/              # 時刻関連計算（恒星時、黄道傾斜角）
│   ├── Core/              # 核となる計算エンジン（エフェメリス、サービス）
│   ├── Western/           # 西洋占星術計算（ハウス、ホロスコープ、黄道十二宮）
│   ├── Eastern/           # 東洋占星術計算（太陰太陽暦、九星、干支、節気）
│   ├── Localization/      # 多言語対応リソース
│   └── Data/              # データ構造定義（天体、ハウス、東洋占星術）
├── Tests/
│   └── EditMode/          # Unity Test Runner 用テスト
├── AstroScpoeMain.cs      # サンプル MonoBehaviour
└── README.md
```

## アーキテクチャ・設計指針

- **サービス指向**: `AstroScopeService` が西洋・東洋計算の統合ファサードとして機能
- **計算分離**: 各領域（西洋/東洋）は独立した Calculator クラスで実装
- **データ構造**: 計算結果は immutable な構造体 (`Result` サフィックス) で返却
- **ユーティリティ**: 共通機能（角度正規化、時刻変換）は静的クラスで提供
- **ローカライズ**: `AstroLocalization` クラスで言語切り替えを一元管理

## コーディング規則

### 命名規則

- **クラス**: PascalCase (`EphemerisCalculator`)
- **メソッド**: PascalCase (`ComputePlanetPositions`)
- **プロパティ**: PascalCase (`LongitudeDegrees`)
- **フィールド**: camelCase (`obliquityRadians`)
- **定数**: PascalCase (`MaxIterations`)

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

- **フレームレート**: Update() での重い計算は避け、必要時のみ実行
- **メモリ**: 毎フレームの new インスタンス生成を避ける
- **エディタ**: `[SerializeField]` でインスペクター公開、`[System.Serializable]` でデータ永続化
- **ログ**: `Debug.Log` ではなく `AstroLogger` 経由でローカライズ対応

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
