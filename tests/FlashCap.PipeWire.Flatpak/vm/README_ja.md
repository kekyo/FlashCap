# Flatpakで初めてカメラを使うときのテスト

Ubuntu 24.04の仮想マシンでAvaloniaサンプルを起動し、GNOME 46のカメラ利用ダイアログを操作する。カメラにはLinuxの仮想カメラドライバーakvcamを使う。動くテストパターンをPipeWire経由で撮影できるので、実行するPCにWebカメラを接続する必要はない。[akvcamのドキュメント](https://github.com/webcamoid/akvcam/wiki)

## 実行する

x86-64のLinuxホストに.NET 10 SDK、Node.js 18以降、Flatpak、flatpak-builder、OSTree、QEMU、genisoimage、e2fsprogs、curlを用意する。UbuntuのQEMUパッケージは `qemu-system-x86` と `qemu-utils`。QEMU 8.2に含まれるマシン型 `pc-q35-8.2` とCPUモデル `Nehalem-v1` を使う。ゲストには4個の仮想CPUと4 GiBのメモリを割り当てる。[QEMUのCPUモデル](https://www.qemu.org/docs/master/system/qemu-cpu-models.html)

通常はLinuxの仮想化機能KVMを使う。`/dev/kvm` を利用できないホストでは、QEMUのソフトウェアエミュレーション（TCG）で実行する。別のホストでも同じ方式を使うには `FLASHCAP_VM_ACCEL=kvm` または `FLASHCAP_VM_ACCEL=tcg` を指定する。

リポジトリのルートから次のコマンドを実行する。最初にFlathubを登録してビルドに使うSDKとランタイムを入れる。サンプルは `--bundle-only` でFlatpakファイルにまとめ、ゲスト内にインストールする。

```sh
flatpak remote-add --user --if-not-exists flathub https://dl.flathub.org/repo/flathub.flatpakrepo
flatpak install --user flathub org.freedesktop.Sdk//25.08 org.freedesktop.Platform//25.08
node scripts/build-pipewire-flatpak.mjs --bundle-only
node scripts/test-pipewire-vm.mjs prepare
node scripts/test-pipewire-vm.mjs test
```

`prepare`は初回にUbuntuと依存パッケージを取得し、カメラ権限が未設定のディスクイメージを保存する。以降は保存済みイメージのSHA-256を照合して使う。`test`は許可と拒否を順に試す。それぞれ新しい差分ディスクで起動し、ネットワークを切った状態でテストする。片方だけ再実行する場合は `test` を `allow` または `deny` に置き換える。

## 何を確認するか

両方のケースで、サンドボックス内から `/dev/video*` とPipeWireのソケットに直接アクセスできないことを確認する。続いてGNOMEのダイアログで選択し、ポータルがその選択を保存したことを確かめる。[Cameraポータルの仕様](https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.Camera.html)

ゲストではFFmpegが動くカラーバーを仮想カメラに送る。Ubuntu標準のPipeWireとWirePlumberがカメラを入力として登録したら、GNOMEのアプリ検索でサンプルを開いて利用権限を選ぶ。[akvcamで映像を送る方法](https://github.com/webcamoid/akvcam/wiki/Usage-and-examples)

| 選択 | 合格条件 |
| --- | --- |
| Allow | サンプルにカメラが表示される。動画の中央部分を比較すると、撮影中は画像が動き、停止中は同じ画像が続き、再開するとまた動く。撮影中にカメラ設定を開いて明るさを確認できる。 |
| Deny | 同じFlatpak内から公開APIでカメラを列挙すると0台になる。サンプルを正常に閉じられる。 |

## 結果を見直す

実行のたびに `artifacts/pipewire-vm/allow-*/results/` または `deny-*/results/` ができる。`result` は合否、`session.log` は操作の記録、`first-permission.png` は選択する直前の画面。許可ケースでは撮影中・停止中・再開後の動画と比較結果も残る。環境を調べるには `packages.tsv` と `flatpaks.tsv`、同じ入力で実行したかを確かめるには `inputs.txt` を見る。

ゲストの構築に失敗した場合は `prepare-*/provision.log`、起動に失敗した場合は該当するケースの `serial.log` に原因が記録される。

Ubuntuイメージは配布版とSHA-256、APTパッケージは2026年11月1日0時UTCのスナップショット、Flatpakのランタイムと描画ドライバーは `runtimes.lock` のコミットで固定している。仮想カメラはakvcam 2.0.0のソースのSHA-256を照合してから、ゲストのカーネル向けにビルドする。[Ubuntuイメージのチェックサム](https://cloud-images.ubuntu.com/releases/24.04/release-20261017/SHA256SUMS)・[Ubuntu Snapshot Service](https://snapshot.ubuntu.com/)・[akvcam 2.0.0](https://github.com/webcamoid/akvcam/tree/d1243f277d0a10efd9fec04574ad6fcb4265f7b0)

別のPCで同じディスクを使うには、`artifacts/pipewire-vm/gnome-46-*/` のディレクトリごとコピーする。サンプルの `.flatpak` も `artifacts/pipewire-flatpak/` にコピーすれば、ビルドやダウンロードを繰り返さずに同じ入力でテストできる。ディスクの保存先は環境変数 `FLASHCAP_VM_DIR` で変更できる。
