# 七夕浪漫 3D 爱心粒子

一个可学习、可二创的全平台桌面粒子动画项目。紫色粒子从下方光盘向上汇聚，逐渐形成具有纵深感的粉紫色 3D 爱心，并在成型后缓慢旋转。

| Windows 版 | macOS 版 |
| --- | --- |
| ![Windows 效果预览](紫蓝星空最终预览.png) | ![macOS 效果预览](macos/preview.png) |

## 主要效果

- 10 秒平滑循环
- 紫蓝星空与低透明度极光背景
- 紫色粒子只向上运动并逐渐转为粉色
- 3D 爱心成型、内部增密、心跳与 Y 轴旋转
- 爱心中央显示“七夕快乐”
- 全屏运行，支持右上角关闭按钮和 `Esc` 退出
- 可选本地 MP3 背景音乐
- 不依赖网络或外部图片素材

## 项目结构

```text
.
├── QixiRomanticHeartParticles.cs   # Windows C# 完整源码
├── build.ps1                       # Windows 构建脚本
├── macos/
│   ├── main.swift                  # macOS AppKit/WebKit 外壳源码
│   ├── index.html                  # Canvas 粒子动画完整源码
│   ├── Info.plist                  # macOS 应用信息
│   └── build-macos.sh              # macOS 构建脚本
├── .github/workflows/
│   └── build-desktop.yml           # Windows 与 macOS 自动构建
├── 创作提示词.md                    # 从零生成类似效果的完整提示词
├── 使用说明.txt                    # 两个平台的使用说明
└── LICENSE                         # MIT 二创许可
```

## Windows 构建

系统要求：Windows，已安装 .NET Framework 4.x。

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

构建结果：`dist\七夕浪漫3D爱心粒子.exe`。

## macOS 构建

系统要求：macOS 11 或更高版本，并安装 Xcode Command Line Tools。

```bash
chmod +x macos/build-macos.sh
./macos/build-macos.sh
```

构建结果：

- `macos/dist-macos/七夕浪漫3D爱心粒子-macOS.dmg`
- `macos/dist-macos/七夕浪漫3D爱心粒子-macOS.zip`

macOS 版本使用系统 AppKit 与 WebKit，不依赖 Electron。

## GitHub Actions 自动构建

上传项目后，进入 GitHub 的 `Actions` 页面，选择 `Build desktop apps`，点击 `Run workflow`。完成后可以下载：

- `qixi-heart-windows`：Windows ZIP
- `qixi-heart-macos`：macOS DMG 和 ZIP

推送以 `v` 开头的标签时也会自动构建，例如 `v1.0.0`。

## 运行与音乐

- Windows：双击 EXE；按 `Esc` 或点击右上角关闭。
- macOS：双击 `.app`；按 `Esc` 或点击右上角关闭。
- 可选音乐：把自备的 `传奇.mp3` 放在 EXE 或 `.app` 同目录下。
- 音乐文件不包含在仓库中，请使用者自行确认使用权限。

## 学习与二创

完整源码和参数均已开放。建议从以下文件开始：

- Windows 动画：[QixiRomanticHeartParticles.cs](QixiRomanticHeartParticles.cs)
- macOS 动画：[macos/index.html](macos/index.html)
- macOS 桌面外壳：[macos/main.swift](macos/main.swift)
- 完整创作提示词：[创作提示词.md](创作提示词.md)

常用修改入口：搜索 `七夕快乐`、`5.2`、`8.75`、`9.92`、`#8E67FF` 和 `传奇.mp3`。

## 开源许可

源码采用 [MIT License](LICENSE)，可以学习、修改和二次创作。音乐及使用者自行添加的第三方素材不包含在许可范围内。
