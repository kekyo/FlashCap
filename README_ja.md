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

サンプルコードを動作させて確認(0.11.0)。

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
* Raspberry Pi 400 (armhf/arm64, Linux)
* Seeed reTerminal (armhf, Linux)
* Teclast X89 E7ED Tablet PC 内蔵カメラ (x86, Windows)
* NVIDIA Jetson TX2 評価ボード (arm64, Linux)

確認した、動作しない環境:

* Surface2 (Windows RT 8.1 JB'd)
  * デバイスが見つかりませんでした。VFWとDirectShowの両方に対応していない可能性があります。

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
    async bufferScope =>
    {
        // 引数に渡されるピクセルバッファにキャプチャされている:

        // イメージデータを取得 (恐らくDIB/Jpeg/PNGフォーマットのバイナリ):
        byte[] image = bufferScope.Buffer.ExtractImage();

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

Avaloniaのサンプルコードは、単一のコードで、WindowsとLinuxの両方で動作します。ユーザーモードプロセスでリアルタイムにキャプチャを行い、（MJPEGから）ビットマップをデコードし、ウィンドウにレンダリングします。AvaloniaはSkiaを使ったレンダラーを使用しています。かなり高速です。

![FlashCap.Avalonia](Images/FlashCap.Avalonia_Windows.png)

![FlashCap.Avalonia](Images/FlashCap.Avalonia_Linux.png)

----

## データコピーの削減

以降では、FlashCapを使って、大容量の画像データを処理するための、様々な手法を解説します。これは、応用例なので、必ず読む必要はありませんが、実装のヒントになると思います。

動画を処理するには、大量のデータを扱う必要があります。FlashCapでは、動画の1枚1枚を「フレーム」と呼びます。そして、そのフレームが、1秒当たり60や30回という速さでやって来ます。

ここで重要になるのが、各フレームのデータを、いかにコピーする事なく処理できるか、という事です。現在のFlashCapは、最低1回のコピーが発生します。しかし、使用方法によっては、2回や3回、あるいはそれ以上のコピーが発生してしまう可能性があります。

`OpenAsync`メソッドを呼び出す際のコールバックは、`PixelBufferScope`の引数を渡してきます。この引数に、1回コピーされたフレームのデータが格納されています。ここで、「最も安全」なメソッドである、`CopyImage()` メソッドを呼び出してみます:

```csharp
using var device = await descriptor0.OpenAsync(
  descriptor0.Characteristics[0],
  async bufferScope =>  // <-- `PixelBufferScope` (この時点で1回コピー済み)
  {
    // ここで2回目のコピーが発生する
    byte[] image = bufferScope.Buffer.CopyImage();

    // 結果的に、ここで3回目のコピーが発生する
    var ms = new MemoryStream(image);
    var bitmap = Bitmap.FromStream(ms);

    // ...
  });
```

すると、少なくとも合計2回のコピーが発生することになります。更に、得られた画像データ（`image`）を、`Bitmap.FromStream()`でデコードする事で、結果的に3回のコピーが発生したことになります。

では、最初のコード例である、`ExtractImage()`を使った場合はどうでしょうか:

```csharp
using var device = await descriptor0.OpenAsync(
  descriptor0.Characteristics[0],
  async bufferScope =>  // <-- `PixelBufferScope` (この時点で1回コピー済み)
  {
    // ここで2回目のコピーが発生する（かも知れない）
    byte[] image = bufferScope.Buffer.ExtractImage();

    // Streamに変換する
    var ms = new MemoryStream(image);
    // デコードする。結果的に、ここで2回目、又は3回目のコピーが発生する
    var bitmap = Bitmap.FromStream(ms);

    // ...
  });
```

「コピーが発生する（かも知れない）」と言うのは、状況によっては、コピーが発生しない場合がある、という事です。そうであれば、`CopyImage()` を使わずに、`ExtractImage()`だけを使えば良い、と考えるかも知れません。しかし、`ExtractImage()`の場合は、得られたデータの有効期限が短い、と言う問題があります。

以下のようなコードを考えます:

```csharp
// コールバックのスコープ外に画像データを保存
byte[]? image = null;

using var device = await descriptor0.OpenAsync(
  descriptor0.Characteristics[0],
  bufferScope =>  // <-- `PixelBufferScope` (この時点で1回コピー済み)
  {
    // スコープ外に保存（2回目のコピー）
    image = bufferScope.Buffer.CopyImage();
    //image = bufferScope.ExtractImage();  // 危険!!!
  });

// スコープ外で使う
var ms = new MemoryStream(image);
// デコードする（3回目のコピー）
var bitmap = Bitmap.FromStream(ms);
```

このように、`CopyImage()`でコピーしていれば、コールバックのスコープ外でも安全に参照する事が出来ます。しかし、`ExtractImage()`を使った場合は、スコープ外で参照すると、画像データが破損している可能性があるため、注意が必要です。

同様に、`ReferImage()`メソッドを使うと、基本的にコピーが発生しません。（トランスコードが発生する場合を除きます。後述。）この場合も、スコープ外での参照は行えません。また、画像データはバイト配列ではなく、
`ArraySegment<byte>` で参照する事になります。

この型は、配列の部分参照を示しているため、そのまま使用する事が出来ません。例えば、`Stream`として使用したい場合は、以下のようにします:

```csharp
using var device = await descriptor0.OpenAsync(
  descriptor0.Characteristics[0],
  async bufferScope =>  // <-- `PixelBufferScope` (この時点で1回コピー済み)
  {
    // ここでは基本的にコピーが発生しない
    ArraySegment<byte> image =
      bufferScope.Buffer.ReferImage();

    // Streamに変換する
    var ms = new MemoryStream(
      image.Array, image.Offset, image.Count);
    // デコードする（2回目のコピー）
    var bitmap = Bitmap.LoadFrom(ms);

    // ...
  });
```

`MemoryStream`を使う場合は、このコード例と同様の拡張メソッド `AsStream()` が定義されているので、こちらを使用しても良いでしょう。また、SkiaSharpを使う場合は、`SKBitmap.Decode()`を使って、直接`ArraySegment<byte>`を渡す事が出来ます。

ここまでで説明した、画像データを取得する方法の一覧を示します:

| メソッド              | 速度        | スコープ外 | イメージの型               |
|:-----------------|:----------|:--------|:---------------------|
| `CopyImage()`    | 遅い        | 安全 | `byte[]`             |
| `ExtractImage()` | 場合によっては遅い | 使用不可 | `byte[]`             |
| `ReferImage()`   | 高速        | 使用不可 | `ArraySegment<byte>` |

`ReferImage()`を使用すれば、最低2回のコピーで実現することが分かりました。では、1回まで短縮するにはどうすれば良いでしょうか？

1回のコピーだけで実現するには、画像データのデコードを諦める必要があります。もしかしたら、画像データをハードウェアで処理できる環境であれば、画像データを直接、ハードウェアに渡すことで、2回目のコピーをオフロードできるかも知れません。

ここでは分かりやすい例として、以下のように、画像データを直接ファイルに保存する操作を考えます。この場合、デコードも行わないので、コピーは1回という事になります。（その代わり、I/O操作は途方もなく遅いのですが...）

```csharp
using var device = await descriptor0.OpenAsync(
  descriptor0.Characteristics[0],
  async bufferScope =>  // <-- `PixelBufferScope` (この時点で1回コピー済み)
  {
    // ここでは基本的にコピーが発生しない
    ArraySegment<byte> image = bufferScope.Buffer.ReferImage();

    // 画像データを直接ファイルに出力する
    using var fs = File.Create(
      descriptor0.Characteristics[0].PixelFormat switch
      {
        PixelFormats.JPEG => "output.jpg",
        PixelFormats.PNG => "output.jpg",
        _ => "output.bmp",
      });
    await fs.WriteAsync(image.Array, image.Offset, image.Count);
    await fs.FlushAsync();
  });
```

### トランスコードについて

デバイスから得られた「生の画像データ」は、私たちが扱いやすい、JPEGやDIBビットマップではない場合があります。一般的に、動画形式の画像データは、MPEGのような連続ストリームではない場合、"MJPEG" (Motion JPEG)や"YUV"と呼ばれる形式です。

"MJPEG"は、中身が完全にJPEGと同じであるため、FlashCapはそのまま画像データとして返します。対して、"YUV"形式の場合は、データヘッダ形式はDIBビットマップと同じですが、中身は完全に別物です。従って、これをそのまま "output.bmp" のようなファイルで保存しても、多くの画像デコーダはこれを処理できません。

そこで、FlashCapは、"YUV"形式の画像データの場合は、自動的に"RGB" DIB形式に変換します。この処理の事を「トランスコード」と呼んでいます。先ほど、`ReferImage()`は「基本的にコピーが発生しない」と説明しましたが、"YUV"形式の場合は、トランスコードが発生するため、一種のコピーが行われます。（FlashCapはトランスコードをマルチスレッドで処理しますが、それでも画像データが大きい場合は、性能に影響します。）

もし、画像データが"YUV"であっても、そのままで問題ないのであれば、トランスコードを無効化することで、コピー処理を完全に1回のみにする事が出来ます:

```csharp
// トランスコードを無効にしてデバイスを開く:
using var device = await descriptor0.OpenAsync(
  descriptor0.Characteristics[0],
  false,   // transcodeIfYUV == false
  async buferScope =>
  {
      // ...
  });

// ...
```

----

## コールバックハンドラと処理方法

TODO: rewrite to what is handler strategies.

----

## フレームプロセッサをマスターする (Advanced topic)

TODO: rewrite to what is frame processor.

----

## 制限

* Video for Windowsでは、プログラムで「ソースデバイス」を選択することはできません。VFWの論理構成は:
  1. VFWデバイスドライバ(常に1つのドライバのみ、最新のWindowsではデフォルトのWDMデバイス): `ICaptureDevices.EnumerateDevices()` がこのデバイスを列挙します。
  2. ソースデバイス（本当のカメラデバイス）に対応する各ドライバ。しかし、プログラマブルに選択することはできません。
     複数のカメラデバイスが検出された場合、自動的に選択ダイアログが表示されます。

----

## License

Apache-v2.

----

## 履歴

* 0.11.0:
  * `PixelBufferScope` を追加し、ピクセルバッファを早期に解放することが出来るようにした。
  * `IsDiscrete` を追加し、デバイスによって定義された画像特性かどうかを判別出来るようにした。
  * 様々なデバイスとコンピューターで動作検証を行い、不具合を修正。
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
