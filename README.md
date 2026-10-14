# HAAVK ChatGPT Launcher

给 Windows 上的 **ChatGPT 桌面版**加一段 HAAVK 加载动画。

双击桌面图标后，全屏播放 HAAVK 02 的 **4K 动画**（带原声），同时在后台启动 ChatGPT；动画结束后画面淡出，ChatGPT 窗口会被切到最前面。按 `Esc` 可以随时跳过。

v1.0.1 使用与原版本相同的 HAAVK 02 动画，保持画面顺序、播放节奏和声音。画面取自作者提供的 4K 文件，音轨逐字节沿用原 ChatGPT 版本；视频和音频均直接复制，没有重新编码。

这是一个独立的启动器，不修改 ChatGPT 本身，也不注入任何代码。原来的 ChatGPT 图标照常可用，只有从本启动器打开时才会播放动画。

## 使用

1. 先从 Microsoft Store 安装 ChatGPT 桌面版。
2. 在 [Releases](../../releases) 下载最新的 `HAAVK-ChatGPT-*.zip`，解压到任意位置（保持文件夹完整）。
3. 双击 `create-desktop-shortcut.vbs`，桌面会出现 **ChatGPT HAAVK** 图标。
4. 以后从这个图标打开 ChatGPT。想每次都有动画，可以把它固定到任务栏。

| 操作 | 效果 |
| --- | --- |
| 等待动画播完（约 31 秒） | 画面淡出，ChatGPT 切到最前面 |
| `Esc` | 跳过剩余动画，立即进入 ChatGPT |

ChatGPT 已经在运行（包括缩在托盘里）时，启动器会把它的窗口恢复并切到最前面。

### 可能遇到的提示

- **"Windows 已保护你的电脑"**：启动器没有付费数字签名，第一次运行时 SmartScreen 会提示。点 **更多信息 → 仍要运行** 即可。
- **安全软件拦截 `.vbs` 脚本**：选择允许；或者跳过脚本，手动给 `HAAVK-ChatGPT.exe` 创建桌面快捷方式。

移动解压后的文件夹以后，请重新运行 `create-desktop-shortcut.vbs`，并删掉指向旧位置的图标。卸载时删除桌面图标和解压的文件夹即可，启动器不驻留、不添加开机启动项。

## 文件

| 文件 | 说明 |
| --- | --- |
| `HAAVK-ChatGPT.exe` | 启动器 |
| `haavk_02.mp4` | 动画视频，3840×2160 / 30 fps / H.264 + AAC，约 30.8 秒 |
| `haavk.ico` | 图标 |
| `create-desktop-shortcut.vbs` | 创建桌面快捷方式 |

视频体积较大（约 744 MB），只放在 Release 压缩包里，不在代码仓库中。普通显示器会按屏幕尺寸缩放显示，文件本身保持 4K 分辨率。

v1.0.1 中 `haavk_02.mp4` 的 SHA-256 为 `cec067a28208b3bdb6a5df6a6ab3fc27c58937307484ba3db02da17184bb8a81`。完整压缩包的校验值见同版本 Release 的 `SHA256SUMS.txt`。

## 从源码构建

需要 Windows 10/11 自带的 .NET Framework 4.x（含 C# 编译器），不需要安装 Visual Studio。

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1 -Video path\to\haavk_02.mp4
```

输出在 `dist\` 文件夹。代码只有一个文件：[`src/HaavkChatGPT.cs`](src/HaavkChatGPT.cs)（WPF 全屏播放 + 通过 `ApplicationActivationManager` 启动 ChatGPT）。

启动目标写在源码常量 `ChatGptAppId` 里（`OpenAI.Codex_2p2nqsd0c76g0!App`，即 Microsoft Store 版 ChatGPT 的应用 ID）。如果你的 ChatGPT 应用 ID 不同，可以在 PowerShell 里运行 `Get-StartApps | Where-Object Name -match ChatGPT` 查看，改掉常量后重新构建。

## 许可

- 启动器代码、脚本与文档：[MIT License](LICENSE)。
- HAAVK 动画视频与 HAAVK 图标：作者原创，采用 [CC BY 4.0](LICENSE-ASSETS.md)。

ChatGPT 是 OpenAI 的商标，此处仅用于说明启动目标。本项目与 OpenAI 无官方关联，也不包含 OpenAI 的任何标识。
