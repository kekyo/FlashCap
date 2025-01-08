# FlashCap

![FlashCap](Images/FlashCap.100.png)

FlashCap - シンプルで依存性のない、ビデオフレームキャプチャライブラリ

[![Project Status: Active – The project has reached a stable, usable state and is being actively developed.](https://www.repostatus.org/badges/latest/active.svg)](https://www.repostatus.org/#active)

## NuGet

| Package  | NuGet                                                                                                                |
|:---------|:---------------------------------------------------------------------------------------------------------------------|
| FlashCap | [![NuGet FlashCap](https://img.shields.io/nuget/v/FlashCap.svg?style=flat)](https://www.nuget.org/packages/FlashCap) |
| FSharp.FlashCap | [![NuGet FSharp.FlashCap](https://img.shields.io/nuget/v/FSharp.FlashCap.svg?style=flat)](https://www.nuget.org/packages/FSharp.FlashCap) |


----

[English language is here](https://github.com/kekyo/FlashCap)

## これは何?

.NETでビデオフレームキャプチャ機能を実装する必要がありますか？
.NETでのビデオフレームキャプチャライブラリに困っていますか？

このライブラリは、ビデオフレームのキャプチャ機能のみに特化した画像取り込みライブラリです（フレームグラバーと呼ばれることもあります）。
シンプルなAPIで使いやすく、簡素なアーキテクチャで、ネイティブライブラリを含んでいません。
また、公式以外の他のライブラリに依存することもありません。
[NuGetの依存ページを参照して下さい](https://www.nuget.org/packages/FlashCap)


----

## 簡単なコード例

`FlashCap` NuGetパッケージをインストールします。

* `FSharp.FlashCap`パッケージを使用すれば、F#向けに最適化されたAPIセットを使う事が出来ます。

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

        // イメージデータを取得 (恐らくDIB/JPEG/PNGフォーマットのバイナリ):
        byte[] image = bufferScope.Buffer.ExtractImage();

        // 後はお好きに...
        var ms = new MemoryStream(image);
        var bitmap = Bitmap.FromStream(ms);

        // ...
    });

// 処理を開始:
await device.StartAsync();

// ...

// 処理を停止:
await device.StopAsync();
```

Reactive Extensionを使う事も出来ます:

```csharp
// 映像特性を指定して、Observableを取得します:
var descriptor0 = devices.EnumerateDescriptors().ElementAt(0);

using var deviceObservable = await descriptor0.AsObservableAsync(
    descriptor0.Characteristics[0]);

// デバイスをサブスクライブします
deviceObservable.Subscribe(bufferScope =>
{
    // 引数に渡されるピクセルバッファにキャプチャされている:

    // イメージデータを取得 (恐らくDIB/JPEG/PNGフォーマットのバイナリ):
    byte[] image = bufferScope.Buffer.ExtractImage();

    // 後はお好きに...
    var ms = new MemoryStream(image);
    var bitmap = Bitmap.FromStream(ms);

    // ...
});

// 処理を開始:
await deviceObservable.StartAsync();
```

見ての通り、FlashCapはGUI要素に一切依存していません。
例えば、FlashCapをコンソールアプリケーションに応用したりする事が可能です。

解説記事はこちら（英語）: ["Easy to implement video image capture with FlashCap" (dev.to)](https://dev.to/kozy_kekyo/easy-to-implement-video-image-capture-with-flashcap-o5a)


----

## 動作環境


対応する.NETプラットフォームは以下の通りです（ほぼ全てです！）:

* .NET 8 to 5 (`net8.0` and etc)
* .NET Core 3.1, 3.0, 2.2, 2.1, 2.0 (`netcoreapp3.1` and etc)
* .NET Standard 2.1, 2.0, 1.3 (`netstandard2.1` and etc)
* .NET Framework 4.8, 4.6.1, 4.5, 4.0, 3.5 (`net48` and etc)

キャプチャデバイスが使用できるプラットフォーム:

* Windows (DirectShowデバイス, x64/x86)
* Windows (Video for Windowsデバイス, x64/x86)
* Linux (V4L2デバイス, x86_64/i686/aarch64/armv7l/mips)

## テスト済みデバイス

サンプルコードを動作させて確認(0.11.0)。

確認したキャプチャユニット/カメラ:

* Elgato CamLink 4K (Windows/Linux)
* BlackMagic Design ATEM Mini Pro (Windows/Linux)
* Logitech WebCam C930e (Windows/Linux)
* eMeet HD Webcam C970L (Windows/Linux)
* Microsoft LifeCam Cinema HD720 (Windows/Linux)
* Unnamed cheap USB capture module (Windows/Linux)
* Spirer RP28WD305 (Linux)

確認したコンピューター:

* Generic PC Core i9-9960X (x64, Windows)
* Generic PC Core i9-11900K (x64, Linux)
* Microsoft Surface Go Gen1 内蔵カメラ (x64, Windows)
* VAIO Z VJZ131A11N 内蔵カメラ (x64, Windows)
* clockworks DevTerm A06 (aarch64, Linux)
* Raspberry Pi 400 (armv7l/aarch64, Linux)
* Seeed reTerminal (armv7l, Linux, mono is unstable)
* Teclast X89 E7ED Tablet PC 内蔵カメラ (x86, Windows)
* NVIDIA Jetson TX2 評価ボード (aarch64, Linux)
* Acer Aspire One ZA3 inside camera (i686, Linux)
* Imagination Creator Ci20 (mipsel, Linux)
* Radxa ROCK5B (aarch64, Linux)
* Loongson-LS3A5000-7A2000-1w-EVB-V1.21 (loongarch64, Linux)

確認した、動作しない環境:

* Surface2 (arm32, Windows RT 8.1 JB'd)
  * デバイスが見つかりませんでした。VFWとDirectShowの両方に対応していない可能性があります。


----

## 完全なサンプルコード

完全なサンプルコードはこちらです:

* [Avalonia11アプリケーション](samples/FlashCap.Avalonia/)
* [WPFアプリケーション](samples/FlashCap.Wpf)
* [Windowsフォームアプリケーション](samples/FlashCap.WindowsForms/)
* [コンソールアプリケーション](samples/FlashCap.OneShot/)

Avaloniaのサンプルコードは、単一のコードで、WindowsとLinuxの両方で動作します。ユーザーモードプロセスでリアルタイムにキャプチャを行い、（MJPEGから）ビットマップをデコードし、ウィンドウにレンダリングします。AvaloniaはSkiaを使ったレンダラー [SkiaImageView](https://github.com/kekyo/SkiaImageView) を使用しています。かなり高速です。

![FlashCap.Avalonia](Images/FlashCap.Avalonia_Windows.png)

![FlashCap.Avalonia](Images/FlashCap.Avalonia_Linux.png)

### 一枚だけ撮りたい

イメージを一枚だけ撮りたい場合に、非常に簡単なメソッドがあります:

```csharp
// 映像特性を指定して、イメージを一枚だけ撮ります:
var descriptor0 = devices.EnumerateDescriptors().ElementAt(0);

byte[] imageData = await descriptor0.TakeOneShotAsync(
    descriptor0.Characteristics[0]);

// ファイルに保存します
await File.WriteAllBytesAsync("oneshot", imageData);
```

完全な実装は、[サンプルコード](samples/FlashCap.OneShot/)を参照して下さい。

### 対応できないフォーマットの除外

映像特性には、そのカメラがサポートしているフォーマットの一覧が入っています。
FlashCapは全てのフォーマットに対応しているわけではないため、デバイスをオープンする前に、正しいフォーマットを選択する必要があります。
対応していないフォーマットは、 `PixelFormats.Unknown` で示されるため、これを除外します:

```csharp
// 映像特性を指定して、デバイスを開きます:
var descriptor0 = devices.EnumerateDescriptors().ElementAt(0);

// 対応していないフォーマットを除外する:
var characteristics = descriptor0.Characteristics.
    Where(c => c.PixelFormat != PixelFormats.Unknown).
    ToArray();
```

FlashCapは、デバイスが返す全てのフォーマットを列挙します。
従って、 `PixelFormats.Unknown` である `VideoCharacteristics` の情報を確認する事で、デバイスがどのようなフォーマットに対応しているのかを分析することが出来ます。

### カメラデバイスのプロパティページを表示する

カメラデバイスのプロパティページを表示する事が出来ます。

![PropertyPage](Images/PropertyPage.png)

```csharp
using var device = await descriptor.OpenAsync(
    characteristics,
    async bufferScope =>
    {
        // ...
    });

// カメラデバイスがプロパティページをサポートしていれば
if (device.HasPropertyPage)
{
    // Avaloniaのウインドウから親ハンドルを取得
    if (window.TryGetPlatformHandle()?.Handle is { } handle)
    {
        // カメラデバイスのプロパティページを表示する
        await device.ShowPropertyPageAsync(handle);
    }
}
```

現在のところ、プロパティページを表示出来るのは、対象がDirectShowデバイスの場合のみです。

完全な実装は、[Avaloniaサンプルコード](samples/FlashCap.Avalonia/)や、[WPFサンプルコード](samples/FlashCap.Wpf/)を参照して下さい。


----

## 実装ガイドライン

以降では、FlashCapを使って、大容量の画像データを処理するための様々な手法を解説します。これは応用例なので、必ず読む必要はありませんが、実装のヒントにはなると思います。

## データコピーの削減

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
        PixelFormats.PNG => "output.png",
        _ => "output.bmp",
      });
    await fs.WriteAsync(image.Array, image.Offset, image.Count);
    await fs.FlushAsync();
  });
```

## トランスコードについて

デバイスから得られた「生の画像データ」は、私たちが扱いやすい、JPEGやRGB DIBビットマップではない場合があります。一般的に、動画形式の画像データは、MPEGのような連続ストリームではない場合、"MJPEG" (Motion JPEG)や"YUV"と呼ばれる形式です。

"MJPEG"は、中身が完全にJPEGと同じであるため、FlashCapはそのまま画像データとして返します。対して、"YUV"形式の場合は、データヘッダ形式はDIBビットマップと同じですが、中身は完全に別物です。従って、これをそのまま "output.bmp" のようなファイルで保存しても、多くの画像デコーダはこれを処理できません。

そこで、FlashCapは、"YUV"形式の画像データの場合は、自動的にRGB DIB形式に変換します。この処理の事を「トランスコード」と呼んでいます。先ほど、`ReferImage()`は「基本的にコピーが発生しない」と説明しましたが、"YUV"形式の場合は、トランスコードが発生するため、一種のコピーが行われます。（FlashCapはトランスコードをマルチスレッドで処理しますが、それでも画像データが大きい場合は、性能に影響します。）

もし、画像データが"YUV"形式であっても、そのままで問題ないのであれば、トランスコードを無効化することで、コピー処理を完全に1回のみにする事が出来ます:

```csharp
// トランスコードを無効にしてデバイスを開く:
using var device = await descriptor0.OpenAsync(
  descriptor0.Characteristics[0],
  TranscodeFormats.DoNotTranscode,   // トランスコードさせない
  async buferScope =>
  {
      // ...
  });

// ...
```

`TranscodeFormats` 列挙値には以下の選択肢があります:

| `TranscodeFormats` | 内容 |
|:----|:----|
| `Auto` | 必要であればトランスコードを行い、変換マトリックスを自動的に選択します。解像度に応じて、`BT601`, `BT709`, `BT2020` が選択されます。 |
| `DoNotTranscode` | トランスコードを全く行いません。JPEG又はPNG以外のフォーマットは、生データのままDIBビットマップに格納されます。 |
| `BT601` | 必要であれば、BT.601変換マトリックスを使用してトランスコードを行います。これはHDまでの解像度で標準的に使用されます。 |
| `BT709` | 必要であれば、BT.709変換マトリックスを使用してトランスコードを行います。これはFullHDまでの解像度で標準的に使用されます。 |
| `BT2020` | 必要であれば、BT.2020変換マトリックスを使用してトランスコードを行います。これは4K等のFullHDを超える解像度で標準的に使用されます。 |

上記のほかに、 `BT601FullRange`, `BT709FullRange`, `BT2020FullRange`, が存在します。
これらは、輝度信号の想定範囲を8ビット全体に広げますが、一般的ではありません。
`Auto` を選択した場合は、これらの `FullRange` を使用しません。

## コールバックハンドラと呼び出し契機

これまで説明してきたコールバックハンドラは、呼び出される契機が「フレームが得られた時」としていましたが、この契機をいくつかのパターンから選択する事が出来ます。この選択は、`isScattering`と`maxQueuingFrames`の引数で指定可能で、`OpenAsync`のオーバーロード引数で指定します:

```csharp
// ハンドラの呼び出し契機を指定する:
using var device = await descriptor0.OpenAsync(
  descriptor0.Characteristics[0],
  TranscodeFormats.Auto,
  true,   // 呼び出し契機の指定 (true: Scattering)
  10,     // 最大滞留フレーム数
  async buferScope =>
  {
      // ...
  });

// ...
```

以下に、パターンの種類を示します:

| `isScattering` | `maxQueuingFrames` | 概要 |
|:----|:----|:----|
| false | 1 | 引数省略時（デフォルト）の呼び出し契機。ハンドラが制御を返さない限り、後続のフレームを全て破棄する。一般的な使用方法に最適。 |
| false | n | ハンドラが制御を返さない場合でも、後続のフレームはキューに蓄えられる。コンピューターの性能が十分であれば、最大滞留フレーム数までは失わない。 |
| true | n | ハンドラは、マルチスレッドワーカーによって、並列処理される。対応するフレームの順序が保障されないが、CPUがマルチコアに対応していれば、処理を高速化出来る。 |

デフォルトの呼び出し契機は、多くの場合にとって適切です。例えば、画像をUIでプレビュー状況で、滞留フレーム数に過大な値を指定した場合、ハンドラの処理が遅いと、キューに古い画像データが保持されてしまい、現在のポーズとプレビューがどんどん乖離してしまいます。また、いつかプロセスがメモリ不足で強制終了する事になります。

同様に、`isScattering == true` を使いこなすのは、より難しくなります。あなたが書いたハンドラは、マルチスレッドで同時に呼び出されて、処理されます。従って、少なくともハンドラはスレッドセーフとなるように実装する必要があります。また、マルチスレッドで呼び出されるという事は、処理するバッファは、必ずしも順序が維持されない可能性があるという事です。例えば、UIにプレビューを表示する場合、動画が一瞬過去に戻ったり、ぎこちなく感じるはずです。

`isScattering == true` でフレームの順序が分からなくなる事に対処するため、`PixelBuffer`クラスには、`Timestamp`プロパティと`FrameIndex`プロパティが定義されています。これらのプロパティを参照すれば、フレーム順序の判定を行う事が出来ます。

## Reactive extensionの問題

ところで、`OpenAsync()`のハンドラ引数に、`PixelBufferArrivedDelegate`と`PixelBufferArrivedTaskDelegate`の両方のオーバーロードが存在する事に気が付いていますか？ これは、同期バージョンと非同期バージョンのハンドラ実装にそれぞれ対応していて、どちらでも正しくハンドラ処理の完了を認識するためです。

しかし、`AsObservableAsync()`の場合、ハンドラ実装に該当するのは、Reactive Extensionの`OnNext()`メソッドであり、これは同期バージョンしか存在しません。つまり、Reactive Extensionを使う場合は、オブザーバーの実装に非同期処理は使えません。`async void OnNext(...)`とマークする事で実装は可能ですが、ピクセルバッファの有効期限が、最初の`await`の直前までであることに十分注意して下さい。コンパイラはこの問題を検出できません。

最も安全な方法として、出来るだけ早く、ピクセルバッファから画像データを取り出す（コピーしてしまう）事でしょう。これは、射影演算子を使って簡単に実現できます:

```csharp
deviceObservable.
    // すぐに射影する
    Select(bufferScope =>
        Bitmap.FromStream(bufferScope.Buffer.ReferImage().AsStream())).
    // 後はお好きに...
    // ...
```

## バッファプーリングのカスタマイズ (Advanced topic)

FlashCapは、再利用されるバッファのための、バッファプーリングインターフェイスを持っています。
これは、`BufferPool` 基底クラスで、このクラスを継承して実装します。

既定の実装は `DefaultBufferPool` クラスで、自動的に使用されます。
このクラスは単純な実装ですが、弱参照を使用して、使われなくなったバッファをGCが回収可能にしています。

バッファプーリングを独自の実装で置き換えたい場合は、以下の2個の抽象メソッドを実装します:

```csharp
// バッファプーリングの基底クラス
public abstract class BufferPool
{
  protected BufferPool()
  { /* ... */ }

  // バッファを取得する
  public abstract byte[] Rent(int minimumSize);

  // バッファを解放する
  public abstract void Return(byte[] buffer);
}
```

* `Rent()`メソッドは、引数で指定されたサイズ以上のバッファを返す必要があります。
* `Return()`メソッドは、引数で指定されたバッファがもう使われないため、プーリングで引き取るようにします。

.NETにはGCがあるため、最も単純な（かつ、プーリングを行わない）実装は、以下のようになります:

```csharp
public sealed class FakeBufferPool : BufferPool
{
    public override byte[] Rent(int minimumSize) =>
        // 常に生成
        new byte[minimumSize];

    public override void Return(byte[] buffer)
    {
        // (`buffer` 参照を放置して、GCが回収するに任せる)
    }
}
```

例えば、.NET Core世代の `System.Buffers` には `ArrayPool` クラスがあることをご存じの方も居るでしょう。
`BufferPool`を拡張することで、このような既存のバッファプーリング実装や、独自の実装を使用することが出来ます。

このようにして独自のクラスを実装した場合は、`CaptureDevices`のコンストラクタに渡して、FlashCapに使用させます:

```csharp
// バッファプーリングインスタンスを生成して使用
var bufferPool = new FakeBufferPool();

var devices = new CaptureDevices(bufferPool);

// ...
```

この`CaptureDevices`のインスタンスから列挙された全てのデバイスで、共通のバッファプーリングとして使用されます。

## フレームプロセッサをマスターする (Advanced topic)

地下ダンジョンへようこそ。FlashCapのフレームプロセッサは、磨けば光る宝石です。しかし、余程のことが無い限り、フレームプロセッサを理解する必要はありません。この解説は、やむを得ずフレームプロセッサを扱う場合の参考にして下さい。また、FlashCapが[デフォルトで内蔵するフレームプロセッサの実装](https://github.com/kekyo/FlashCap/tree/main/FlashCap/FrameProcessors)も参考になるでしょう。

前節で解説した、コールバックハンドラの呼び出し契機は、内部的にはフレームプロセッサを切り替える事によって実現されています。つまり、フレームをどのように取り扱うのかや、その振る舞いを抽象化したものです。

フレームプロセッサは、非常に単純な基底クラスを継承して実装します:

```csharp
// (細かい定義は省きます)
public abstract class FrameProcessor
{
  // 必要なら実装する
  public virtual Task DisposeAsync()
  { /* ... */ }

  // ピクセルバッファを取得する
  protected PixelBuffer GetPixelBuffer()
  { /* ... */ }

  // ピクセルバッファを返却する
  public void ReleasePixelBuffer(PixelBuffer buffer)
  { /* ... */ }

  // デバイスを使用してキャプチャを実行する
  protected void Capture(
    CaptureDevice captureDevice,
    IntPtr pData, int size,
    long timestampMicroseconds, long frameIndex,
    PixelBuffer buffer)
  { /* ... */ }

  // フレームが到達した際に呼び出される
  public abstract void OnFrameArrived(
    CaptureDevice captureDevice,
    IntPtr pData, int size, long timestampMicroseconds, long frameIndex);
}
```

少なくとも実装する必要があるのは、`OnFrameArrived()`メソッドです。これは、文字通りフレームが到達した時に呼び出されます。シグネチャを見れば分かる通り、生のポインタと画像データのサイズ、タイムスタンプ、そしてフレーム番号が渡されます。

戻り値が`void`である事にも注意して下さい。このメソッドは非同期処理に出来ません。`async void`で修飾したとしても、引数で渡される情報が維持できません。

このメソッドの、典型的な実装例を示します:

```csharp
public sealed class CoolFrameProcessor : FrameProcessor
{
  private readonly Action<PixelBuffer> action;

  // キャプチャしたら実行するデリゲートを保持する
  public CoolFrameProcessor(Action<PixelBuffer> action) =>
    this.action = action;

  // フレームが到達した際に呼び出される
  public override void OnFrameArrived(
    CaptureDevice captureDevice,
    IntPtr pData, int size, long timestampMicroseconds, long frameIndex)
  {
    // ピクセルバッファを取得する
    var buffer = base.GetPixelBuffer();

    // キャプチャを実行する
    // ピクセルバッファに画像データが格納される (最初のコピーが発生)
    base.Capture(
      captureDevice,
      pData, size,
      timestampMicroseconds, frameIndex,
      buffer);

    // デリゲートを呼び出す
    this.action(buffer);

    // ピクセルバッファを返却する（任意。割り当てられたバッファを再利用します）
    base.ReleasePixelBuffer(buffer);
  }
}
```

このメソッドが、フレーム到達時に毎回呼び出されることを思い出してください。つまり、この実装例は、フレーム到達毎にピクセルバッファを生成し、キャプチャし、そしてデリゲートを呼び出します。

ではこれを使ってみます:

```csharp
var devices = new CaptureDevices();
var descriptor0 = devices.EnumerateDevices().ElementAt(0);

// フレームプロセッサを指定してオープンする
using var device = await descriptor0.OpenWitFrameProcessorAsync(
  descriptor0.Characteristics[0],
  TranscodeFormats.Auto,
  new CoolFrameProcessor(buffer =>   // カスタムのフレームプロセッサを使う
  {
    // キャプチャされたピクセルバッファが渡される
    var image = buffer.ReferImage();

    // デコードする
    var bitmap = Bitmap.FromStream(image.AsStream());

    // ...
  });

await device.StartAsync();

// ...
```

あなたの最初のフレームプロセッサを動かす準備が出来ました。そして、実際に動かさなくても、特徴と問題に気が付いていると思います:

* デリゲートは、フレーム到達時に最短で呼び出される。（呼び出されるところまでは最速である。）
* デリゲートの処理が完了するまで、`OnFrameArrived()`がブロックされる。
* デリゲートは、同期処理を想定している。そのため、デコード処理に時間がかかり、スレッドをブロックすると、容易にフレームドロップが発生する。
* もしここで、ブロックを回避するために`async void`を使用すると、デリゲートの完了を待てないので、ピクセルバッファへのアクセスは危険にさらされる。

このような理由で、FlashCapでは、ある程度安全に操作できる、標準のフレームプロセッサ群を使用するようになっています。では、カスタムフレームプロセッサを実装する利点がどこにあるのでしょうか？

それは、きわめて高度に最適化された、フレームと画像データの処理を実装できることです。例えば、ピクセルバッファは効率よく作られていますが、必ず使用しなければならないわけではありません。（`Capture()`メソッドの呼び出しは任意です。）引数によって、生の画像データへのポインタとサイズが与えられているため、画像データに直接アクセスすることは可能です。そこで、あなた独自の画像データ処理を実装すれば、最速の処理を実現する事が出来ます。


----

## 制限

* Video for Windowsでは、プログラムで「ソースデバイス」を選択することはできません。VFWの論理構成は:
  1. VFWデバイスドライバ(常に1つのドライバのみ、最新のWindowsではデフォルトのWDMデバイス): `ICaptureDevices.EnumerateDevices()` がこのデバイスを列挙します。
  2. ソースデバイス（本当のカメラデバイス）に対応する各ドライバ。しかし、プログラマブルに選択することはできません。
     複数のカメラデバイスが検出された場合、自動的に選択ダイアログが表示されます。


----

## 自分でビルドする

FlashCapは、ビルド環境をクリーンに保っています。
基本的に、Visual Studio 2022の.NET開発環境がインストールされていればビルド出来ると思います。
（WPF, Windows Formsのオプションは追加してください。これらはサンプルコードのビルドに必要です）

1. このリポジトリをクローンします。
2. `FlashCap.sln`をビルドします。
   * `dotnet build` でビルドします。
   * または、`FlashCap.sln`をVisual Studio 2022で開き、ビルドします。

注意: FlashCap自体は、.NET SDKを持つLinux環境でもビルドできるはずですが、サンプルコードにWindowsに依存する実装があるため、
開発環境にWindowsを想定しています。

プルリクエストを歓迎します! 開発は`develop`ブランチ上で行って、リリース時に`main`ブランチにマージしています。
そのため、プルリクエストを作る場合は、`develop`ブランチからあなたのトピックブランチを切って下さい。

### V4L2を未対応のプラットフォームに移植する

V4L2はLinuxのイメージキャプチャ標準APIです。
FlashCapはV4L2に対応していて、これによりLinuxの様々なプラットフォームで動作します。以下に対応プラットフォームを挙げます:

* i686, x86_64
* aarch64, armv7l
* mipsel
* loongarch64

ここに挙げた対応プラットフォームは、単に私やコントリビューターが動作確認出来た、つまり現実のハードウェアを持ち合わせていて、
FlashCapを使って実際にカメラのキャプチャに成功したものです。

それでは、他のプラットフォーム、例えばmips64,riscv32/64,sparc64で動作するか言えば、動作しません。
理由は、以下の通りです:

* 私が動作確認できない: 実際のハードウェアやSBC (Single Board Computer) コンポーネントを持ち合わせていないので、物理的な確認が出来ない。
* .NETランタイム、又はmonoが移植されていない。またはstableな移植が存在しない。

.NETランタイムの問題については、時間が解決する可能性はあります。
そこで、もしFlashCapを未対応のLinuxプラットフォームに移植するつもりがあるのであれば、
概要を示すので参考にしてください。

* `FlashCap.V4L2Generator` は、V4L2への移植に必要な相互運用ソースコードを自動生成するためのジェネレータです。
  このプロジェクトは、[Clang](https://clang.llvm.org/) が出力するJSON ASTファイルを使用して、
  V4L2のヘッダファイルから、正しいABI構造を厳密に適用したC#の相互運用定義ソースコードを生成します。
* 従って、まずターゲットとするLinuxのABIに対応したClangが必要になります。
  この理由により、安定的なABIが定まっていない環境に移植することはできません。
  * Clangのバージョンは、13,11,10を検証しています。これ以外のバージョンでもビルドできる場合があります。
    検証したところ、JSON ASTの出力方法に違いがある場合があるようです。
* 同様に、`FlashCap.V4L2Generator` を動作させるために、ターゲットとするLinuxで動作するmono又は.NETランタイムが必要です。
  * ターゲットLinuxがDebian系の移植であれば、これらは `apt` パッケージなどから入手可能かもしれません。
    `sudo apt install build-essential clang-13 mono-devel` などでインストール出来れば、可能性が高まります。
  * Linuxの安定的なビルド環境を準備するために、qemuを使用した仮想マシンの構築を強く推奨します。
    この目的のために、 `qemu-vm-builder/` ディレクトリに、環境構築用の自動化スクリプトを配置しています。
    スクリプトを参照して、あなたのターゲットとなるビルド環境を構築できるように、スクリプトを追加してください。
    スクリプトが追加されれば、我々もFlashCapの更新時に相互運用ソースコードのビルドが可能になります。
  * FlashCapの公式のビルド環境は、Debianの公式インストーラーを用いています。
    Debianのサイトから、 `netinst` ISOイメージをダウンロードして、必要な仮想マシンを生成しています。
  * Debianを使わない場合や、Debianのバージョンが一致しない場合は、生成される相互運用ソースコードに手動で修正を行う必要があるかもしれません。
* [#100](https://github.com/kekyo/FlashCap/issues/100) の取り組みも参考になると思います。

最初に、 `FlashCap.V4L2Generator` をビルドする必要があります。
ターゲットのLinux環境で.NET SDKが使用できない場合は、monoの `mcs` を使ってコードをコンパイルする `build-mono.sh` を使用して下さい。

その後、大まかな手順は、 `dumper.sh` というスクリプトに示されているので、
内容をターゲットの環境に合わせて書き換えて下さい。

`FlashCap.V4L2Generator` が生成したソースコードは、 `FlashCap.Core/Internal/V4L2/` に配置されます。
これを使用するには、 `NativeMethods_V4L2.cs` のタイプイニシャライザの `switch` 文に、
新しいプラットフォームの分岐を加えて下さい。

```csharp
switch (buf.machine)
{
    case "x86_64":
    case "amd64":
    case "i686":
    case "i586":
    case "i486":
    case "i386":
        Interop = IntPtr.Size == 8 ?
            new NativeMethods_V4L2_Interop_x86_64() :
            new NativeMethods_V4L2_Interop_i686();
        break;
    case "aarch64":
    case "armv9l":
    case "armv8l":
    case "armv7l":
    case "armv6l":
        Interop = IntPtr.Size == 8 ?
            new NativeMethods_V4L2_Interop_aarch64() :
            new NativeMethods_V4L2_Interop_armv7l();
        break;
    case "mips":
    case "mipsel":
        Interop = new NativeMethods_V4L2_Interop_mips();
        break;
    case "loongarch64":
        Interop = new NativeMethods_V4L2_Interop_loongarch64();
        break;

    // (ここに新しい移植を加えます...)

    default:
        throw new InvalidOperationException(
            $"FlashCap: Architecture '{buf.machine}' is not supported.");
}
```

あとは、神に祈ります :)
Avaloniaのサンプルコードを使用して確認すると良いでしょう。
Avaloniaが動作しない環境の場合は、OneShotサンプルコードで試した後、
これを拡張して連続したビットマップを保存して確認する方法もあります。

これで成功したら、PRを歓迎します。

このプロセスで生成されたコードは、他のプラットフォームとほぼ同一のコードと言えるため、
私が直接ハードウェアで検証出来ていませんが、恐らくPRを受け入れることが出来るでしょう。
以下の情報も提供してください（ドキュメントに記載されます）:

* ターゲットの物理マシン製品名
* キャプチャユニットやカメラの製品名
* これらの接続方法（例えばUSB接続・PCIe接続・内臓カメラなど）
* 一般的な小売り業者から入手出来ない場合は、具体的な入手先
* どうしても解消できなかった制約がある場合の説明（例えば特定の映像特性が動かないなど）

TIPS: なぜV4L2Generatorが必要になるかというと、.NET相互運用機能で想定されている各種デフォルトは、
Windows環境に最適化されていて、ターゲットのABIと互換性がないからです。


----

## License

Apache-v2.


----

## 履歴

* 1.10.0:
  * NV12フォーマットのトランスコードに対応しました。 [#132](https://github.com/kekyo/FlashCap/issues/132)
  * バッファプーリングに対応しました。 [#135](https://github.com/kekyo/FlashCap/issues/135) [#138](https://github.com/kekyo/FlashCap/issues/138)
  * 非同期ロックが待機する場合に、キャンセル要求がリークする事があるのを修正しました。 [#142](https://github.com/kekyo/FlashCap/issues/142)
  * V4L2で、x86_64やaarch64のような64/32ユーザーランド混在環境で、使用すべき相互運用ライブラリを誤って選択する事がある問題を修正しました。 [#43](https://github.com/kekyo/FlashCap/issues/43)
  * V4L2で、`StartAsync()`と`StopAsync()`を繰り返すと、フレームが発生しないくなる事がある問題を修正しました。 [#124](https://github.com/kekyo/FlashCap/issues/124)
  * V4L2で、デバイスや特性が列挙されない場合がある問題を修正しました。 [#126](https://github.com/kekyo/FlashCap/issues/126) [#127](https://github.com/kekyo/FlashCap/issues/127)
  * (もしかしたら、loongarch64はデグレードしているかもしれません。PRを歓迎します。参考: [#144](https://github.com/kekyo/FlashCap/pull/144))
* 1.9.0:
  * loongarch64 Linuxに対応しました [#100](https://github.com/kekyo/FlashCap/issues/100)
* 1.8.0:
  * .NET 8.0 SDKに対応しました。
  * トランスコードの変換マトリックス係数が一部誤っていたのを修正 [#107](https://github.com/kekyo/FlashCap/issues/107)
* 1.7.0:
  * DirectShowデバイスでプロパティページを表示出来るようになりました [#112](https://github.com/kekyo/FlashCap/issues/112)
  * `TranscodeFormats` 列挙型を使用して、BT.601, BT.709, BT.2020変換を指定できるようにしました [#107](https://github.com/kekyo/FlashCap/issues/107)
  * BlackMagic社固有のYUYVフォーマットに対応しました [#105](https://github.com/kekyo/FlashCap/issues/105)
  * いくつかのメソッド/関数は `Obsolete` としてマークされています。警告に従って変更してください。
  * .NET 8.0 RC2に対応しました。
* 1.6.0:
  * V4L2で一部のフォーマットが列挙されない問題を修正しました。
  * 未対応のフォーマットを暗黙に除外しないで、`PixelFormats.Unknown` として可視化されるようにしました。
  * 依存するF#パッケージを5.0.0にダウングレードしました。
* 1.5.0:
  * 簡単にイメージを一枚だけ撮影する、 `TakeOneShotAsync()` メソッドを追加し、対応するサンプルプロジェクトを追加しました。
  * Avaloniaサンプルコードで、FPSと撮影したイメージの情報をリアルタイムに表示するようにしました。
* 1.4.0:
  * 非同期メソッドにおいて、`CancellationToken`を指定できるようにしました。
  * `Start`と`Stop`を非同期処理に対応させました。
  * 暗黙に非同期操作が要求される箇所 (Async-Over-Sync) を修正しました。
  * V4L2において、キャプチャデバイスを再度開くと`ArgumentException`が発生する不具合を修正しました [#9](https://github.com/kekyo/FlashCap/issues/9)
  * Avaloniaサンプルコードで、デバイスと特性の切り替えが出来るようにしました。
* 1.3.0:
  * F#向けのAPIを公開する `FSharp.FlashCap` パッケージを追加。
* 1.2.0:
  * `AsObservableAsync()`で、Reactive Extensionに対応しました。
* 1.1.0:
  * ピクセルバッファプーリングの実装をFrameProcessorの基底クラスに移動しました。
  * CaptureDeviceでIDisposableが実装されていないのを修正。
* 1.0.0:
  * Reached 1.0.0 🎉
  * V4L2のmiplel環境をサポート。
  * 予定していた全ての環境でテストを行いました。
  * WPFのサンプルプロジェクトを追加しました。
  * サンプルプロジェクトでgraceful shutdownを反映させました。
* 0.15.0:
  * V4L2関係のブリッジコードの書き換えを完了し、i686でのV4L2の動作を修正した。
  * 完全非同期動作にしたため、net20のサポートを廃止。
  * `OpenAsync`に`maxQueuingFrames`を指定できるように変更しました。
* 0.14.0:
  * 内部フレームプロセッサで、フレーム到達ハンドラがDispose処理中に暗黙のデッドロックを起こした場合の問題を回避するようにしました。
* 0.12.0:
  * Openメソッドにトランスコードフラグを指定するオーバーロードを追加。
  * 内部フレームプロセッサの安全なシャットダウンを実装しました。
  * トランスコーディングにプロセッサカウントを適用。
  * タイムスタンプの計算コストを削減し、リソースの少ない環境にも対応。
  * `FrameIndex`プロパティを追加。
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
