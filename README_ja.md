# FlashCap

![FlashCap](Images/FlashCap.100.png)

FlashCap - シンプルで依存性のない、カメラキャプチャライブラリ

[![Project Status: WIP – Initial development is in progress, but there has not yet been a stable, usable release suitable for the public.](https://www.repostatus.org/badges/latest/wip.svg)](https://www.repostatus.org/#wip)

## NuGet

| Package  | NuGet                                                                                                                |
|:---------|:---------------------------------------------------------------------------------------------------------------------|
| FlashCap | [![NuGet FlashCap](https://img.shields.io/nuget/v/FlashCap.svg?style=flat)](https://www.nuget.org/packages/FlashCap) |

## CI

| main                                                                                                                                                                 | develop                                                                                                                                                                       |
|:---------------------------------------------------------------------------------------------------------------------------------------------------------------------|:------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [![FlashCap CI build (main)](https://github.com/kekyo/FlashCap/workflows/.NET/badge.svg?branch=main)](https://github.com/kekyo/FlashCap/actions?query=branch%3Amain) | [![FlashCap CI build (develop)](https://github.com/kekyo/FlashCap/workflows/.NET/badge.svg?branch=develop)](https://github.com/kekyo/FlashCap/actions?query=branch%3Adevelop) |

----

[English language is here](https://github.com/kekyo/FlashCap)

## これは何?

.NETでカメラキャプチャ機能を実装する必要がありますか？
.NETでのカメラキャプチャライブラリに困っていますか？

このライブラリは、カメラのキャプチャ機能のみに特化したカメラ画像取り込みライブラリです。
シンプルなAPIで使いやすく、簡素なアーキテクチャで、ネイティブライブラリを含んでいません。
また、公式以外の他のライブラリに依存することもありません。
[NuGetの依存ページを参照して下さい](https://www.nuget.org/packages/FlashCap)

対応する.NETプラットフォームは以下の通りです（ほぼ全てです！）:

* .NET 6, 5 (`net6.0`, `net5.0`)
* .NET Core 3.1, 3.0, 2.2, 2.1, 2.0 (`netcoreapp3.1` and etc)
* .NET Standard 2.1, 2.0, 1.3 (`netstandard2.1` and etc)
* .NET Framework 4.8, 4.6.1, 4.5, 4.0, 3.5, 2.0 (`net48` and etc)

カメラデバイスが使用できるプラットフォーム:

* Windows (DirectShowデバイス)
* Windows (Video for Windowsデバイス)
* Linux (V4L2デバイス)

### テスト済みデバイス

サンプルコードを動作させて確認。

確認したキャプチャユニット:

* Elgato CamLink 4K (Windows/Linux)
* Logitech WebCam C930e (Windows/Linux)
* Unnamed cheap USB capture module (Windows/Linux)

確認したコンピューター:

* Generic PC Core i9-9960X (x64, Windows)
* Generic PC Core i9-11900K (x64, Linux)
* Microsoft Surface Go Gen1 内蔵カメラ (x64, Windows)
* VAIO Z VJZ131A11N 内蔵カメラ (x64, Windows)
* clockworks DevTerm A06 (arm64, Linux)
* Raspberry Pi 400 (armhf, Linux)
* Teclast X89 E7ED Tablet PC inside camera (x86, Windows)

テスト中:

* Seeed reTerminal (arm64, Linux)
* NVIDIA Jetson TX2 評価ボード 内蔵カメラ (arm64, Linux)

確認した、動作しない環境:

* Surface2 (Windows RT 8.1 JB'd)

----

## 使い方

最初に、対象デバイスと映像の特性を列挙します:

```csharp
using FlashCap;

// キャプチャデバイスを列挙します
var devices = new CaptureDevices();

foreach (var descriptor in devices.EnumerateDescriptors())
{
    // "Logicool Webcam C930e: DirectShow device, Characteristics=34"
    // "Default: VideoForWindows default, Characteristics=1"
    Console.WriteLine(descriptor);

    foreach (var characteristics in descriptor.Characteristics)
    {
        // "1920x1080 [JPEG, 30.000fps]"
        // "640x480 [YUYV, 60.000fps]"
        Console.WriteLine(characteristics);
    }
}
```

次に、キャプチャーを実行します:

```csharp
// 映像特性を指定して、デバイスを開きます:
var descriptor0 = devices.EnumerateDescriptors().ElementAt(0);

using var device = await descriptor0.OpenAsync(
    descriptor0.Characteristics[0],
    async buffer =>
    {
        // 引数に渡されるピクセルバッファにキャプチャされている:

        // イメージデータを取得 (恐らくDIB/Jpeg/PNGフォーマットのバイナリ):
        byte[] image = buffer.ExtractImage();

        // 後はお好きに...
        var ms = new MemoryStream(image);
        var bitmap = Bitmap.FromStream(ms);

        // ...
    });

// 処理を開始:
device.Start();

// ...

// 処理を停止:
device.Stop();
```

完全なサンプルコードはこちらです:

* [Windowsフォームアプリケーション](samples/FlashCap.WindowsForms/)
* [Avaloniaアプリケーション](samples/FlashCap.Avalonia/)

TODO:

* [WPFアプリケーション](samples/FlashCap.WPF/)

Avaloniaのサンプルコードは、単一のコードで、WindowsとLinuxの両方で動作します。
ユーザーモードプロセスでリアルタイムにキャプチャを行い、
（MJPEGから）ビットマップをデコードし、ウィンドウにレンダリングします。
AvaloniaはSkiaを使ったレンダラーを使用しています。かなり高速です。

![FlashCap.Avalonia](Images/FlashCap.Avalonia_Windows.png)

![FlashCap.Avalonia](Images/FlashCap.Avalonia_Linux.png)

----

## コールバックハンドラと処理方法

TODO: rewrite to what is handler strategies.

----

## データコピーの削減

TODO: rewrite

もう一つのトピックは、`PixelBuffer.ReferImage()` メソッドが
`ArraySegment<byte>` を返すことです。
これは、画像データのコピーを回避するために利用できます（トランスコードが適用されない場合）。

**注意**: 結果の配列セグメントの有効期間は、次の `Capture()` が実行されるまでです。

```csharp
// デコードを実行:
ArraySegment<byte> image = buffer.ReferImage();
var bitmap = SkiaSharp.SKBitmap.Decode(image);

// デコードが完了すると、ピクセルバッファは不要となる:
reserver.Push(buffer);

// (ビットマップを何かに使う...)
```

画像抽出操作を比較する表を示します:

| メソッド             | 速度        | スレッドセーフ | イメージの型               |
|:-----------------|:----------|:--------|:---------------------|
| `CopyImage()`    | 遅い        | 安全      | `byte[]`             |
| `ExtractImage()` | 場合によっては遅い | 保護が必要   | `byte[]`             |
| `ReferImage()`   | 高速        | 保護が必要   | `ArraySegment<byte>` |

また、"YUV" 形式の場合でもトランスコードを実行しないように無効化し、
完全な生画像データを参照するようにします。
(もちろん、生データをデコードするのはあなたの責任となります...）

```csharp
// トランスコーダを無効にしてデバイスを開く:
using var device = await descriptor0.OpenAsync(
    descriptor0.Characteristics[0],
    false,   // transcodeIfYUV == false
    async bufer =>
    {
        // ...
    });

// ...
```

---

## フレームプロセッサをマスターする (Advanced topic)

TODO: rewrite to what is frame processor.

---

## 制限

* Video for Windowsでは、プログラムで「ソースデバイス」を選択することはできません。VFWの論理構成は:
  1. VFWデバイスドライバ(常に1つのドライバのみ、最新のWindowsではデフォルトのWDMデバイス): `ICaptureDevices.EnumerateDevices()` がこのデバイスを列挙します。
  2. ソースデバイス（本当のカメラデバイス）に対応する各ドライバ。しかし、プログラマブルに選択することはできません。
     複数のカメラデバイスが検出された場合、自動的に選択ダイアログが表示されます。

---

## License

Apache-v2.

---

## 履歴

* 0.10.0:
  * フレームプロセッサを実装し、より使いやすく、拡張性のあるフレーム/ピクセル取得方法を実装できるようにしました。
  * イベントベースのインターフェイスを削除し、コールバックのインターフェイスを追加しました。
  * net35/net40 プラットフォームでサポートする非同期メソッドを追加しました (公式の非同期パッケージが必要です)。
  * net461 以上のプラットフォームでサポートする ValueTask非同期メソッドを追加しました (公式の非同期パッケージが必要です)。
  * 同期と非同期のメソッドを完全に分離しました。
  * インターフェース型を削除しました。
  * mono-linux環境でランダムに例外が発生するのを修正。
  * (1.0.0までもうすぐです)
* 0.9.0:
  * Linux V4L2に対応しました 🎉
* 0.8.0:
  * フレームレート計算を改善しました。
  * より簡単に画像を撮影できるメソッド `CaptureOneShot` を追加しました。
  * ビデオ特性のインターフェイスを変更しました(フレームレートとピクセルフォーマット関連。V4L2の実装を反映)。
* 0.7.0:
  * DirectShowで無効なビデオフレームを送信するビデオデバイスをかわす機能を改善しました。
  * 32bit環境でRtlCopyMemoryのエントリポイントが見つからない問題を修正しました。
  * LimitedExecutorクラスを追加しました。
* 0.6.0:
  * DirectShowデバイスをサポート 🎉
  * STAスレッドを分離して使用するVFWの安定性を向上させました。
* 0.5.0:
  * netstandard1.3を再有効化しました。
  * より便利なメソッドとインタフェースを追加しました。
  * VFWで無効なビデオフレームを送信するビデオデバイスをかわす機能を改善しました。
  * メディアタイプ構造へのアクセスの不具合を修正しました。
* 0.4.0:
  * DirectShow APIを使用した実装 (ただし、まだ作業中なので、このバージョンは無効です)
  * バッファの取り扱いを改善しました。
  * フレーム到着イベントでのアロケーションを廃止。
  * 到着した画像が jpeg/mjpeg の場合、トランスコードされたデータが不正になるのを修正。
  * VFWホストウィンドウが本当にデスクトップ領域外にある場合、フレーム到着イベントを呼び出さないように修正しました。
* 0.3.0:
  * 共通のデバイス列挙を実装。
  * サポートデバイスの特性列挙を追加。
  * DirectShowとV2L2の為に、インターフェイスメンバーのブラッシュアップ。
* 0.2.0:
  * YUV変換式をMSの技術論文に準拠させました。
  * PixelBufferをスレッドセーフにしました。
* 0.1.10:
  * 平行変換時のスキャッターの高さを増加させました。
  * height modがない場合の変換オーバーランを修正しました。
* 0.1.0:
  * 初期リリース。
