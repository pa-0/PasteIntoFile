# Paste Into File

[![Contributors](https://img.shields.io/github/contributors/eltos/PasteIntoFile)](https://github.com/eltos/PasteIntoFile/graphs/contributors)
[![Translations](https://badges.crowdin.net/PasteIntoFile/localized.svg)](https://crowdin.com/project/PasteIntoFile)
[![Maintainability](https://api.codeclimate.com/v1/badges/193a540baab0fcf6c916/maintainability)](https://codeclimate.com/github/eltos/PasteIntoFile/maintainability)
[![Build status](https://img.shields.io/github/actions/workflow/status/eltos/PasteIntoFile/dotnet-testbuild.yml?branch=main)](https://github.com/eltos/PasteIntoFile/actions)
[![Latest release](https://img.shields.io/github/v/release/eltos/PasteIntoFile)](https://github.com/eltos/PasteIntoFile/releases/latest)
[![Total downloads](https://img.shields.io/github/downloads/eltos/PasteIntoFile/total)](https://github.com/eltos/PasteIntoFile/releases)

## About

A Windows desktop application to paste clipboard contents into files and copy file contents to the clipboard via the context menu



_This is a fork of [sorge13248/PasteIntoFile](https://github.com/sorge13248/PasteIntoFile), itself being a fork of [EslaMx7/PasteIntoFile](https://github.com/EslaMx7/PasteIntoFile)._
_See the [contributors page](https://github.com/eltos/PasteIntoFile/graphs/contributors) for details on collaborators._
_It is simmilar to [Paste As File](https://pasteasfile.org) and [Paste To File](https://apps.microsoft.com/store/detail/paste-to-file/9PM34S06CFVJ)._
_This fork comes with many new features, a new fluid GUI and new console options._
_The full changelog can be found on the [release page](https://github.com/eltos/PasteIntoFile/releases)._



### Features

+ Explorer context menu entry: "Paste into file", "Replace with clipboard content" and "Copy file contents"
+ Hotkey `Win`+`Alt`+`V` to paste and `Win`+`Alt`+`C` to copy file contents
+ [Autosave mode](https://github.com/eltos/PasteIntoFile/discussions/2): rename inside file explorer without dialog
+ [Batch mode](https://github.com/eltos/PasteIntoFile/discussions/4): monitor clipboard and save on change
+ [Clipboard patching](https://github.com/eltos/PasteIntoFile/discussions/11): use regular paste options and `CTRL`+`V` (experimental)
+ Supports many formats: PNG, JPG, GIF, SVG, TXT, HTML, CSV, URL, PDF, RTF, DIF, SLK, EMF, TIF, ZIP, file drop list, ...
+ First launch wizard

![Paste Into File](screenshot.png)
![Paste Into File kontext menu](screenshot-1.png)


## Installation

Paste Into File is free

+ [**Download latest release** from GitHub](https://github.com/eltos/PasteIntoFile/releases)
  + We provide an **installer** (.msi file)
  + And a **portable version** (.zip file)
+ [Install from Microsoft Store](https://apps.microsoft.com/store/detail/XP88X1XTPKZJDJ)

| [🪙 DONATE](https://github.com/sponsors/eltos) | [❤️ SAY THANKS](https://saythanks.io/to/eltos) |
|---|---|

_Tested on Windows 10._
_If you are running an older Windows version make sure the .NET Framework 4.8+ is installed._
_If the Microsoft Defender SmartScreen promp appears read [this](https://github.com/eltos/PasteIntoFile/discussions/10)._


## Usage

When starting Paste Into File for the first time or after an update, the first launch wizard will guide through the configuration of context menu entry, hotkey, etc.

In addition to the instructions given below, help is also available via [GitHub discussions](https://github.com/eltos/PasteIntoFile/discussions/categories/q-a).

### Paste clipboard contents
If **clipboard patching** is enabled and Paste Into File runs in the system tray, the regular paste command and `CTRL`+`V` hotkey can be used.
Otherwise (or if you want to use the features described below), run the program from the file explorer context menu, via the start menu, with the hotkey `WIN`+`ALT`+`V` (if enabled) or via command line.

In **autosave mode**, the file to paste will directly be created and selected for renaming.
Otherwise, the dialog will prompt for filename and type.
By holding `SHIFT` when the program starts, the autosave mode setting can be temporarily inverted (show the dialog even though autosave is enabled, or skip the dialog even though autosave is disabled).
When running Paste Into File from the start menu, the dialog will always be shown.

The **filename template** can be edited from the dialog or via command line.
When holding `CTRL` while the program starts, the file will be saved to a subdirectory.
The corresponding template can be configured via command line.

The available **file extensions** depend on the formats available in the clipboard.
For example, if you copy a range of cells from a spreadsheet, the data is available not only as text, but also in DIF, RTF, SLK and HTML formats and even as screenshot.
Either select one of the suggested or enter a custom extension (which will be remembered).
An appropriate format is then chosen automatically[^save_plain_text] and a preview shown.
In autosave mode, the clipboard is saved as image, if available, or else as text.
The file extension is then determined by the last used extension for the respective filetype (which can also be set via command line).

A special **batch mode** exists to monitor the clipboard and save it every time new contents are copied.
If enabled, the filename is purely determined by the template (which supports a dedicated counter variable).

The context menu entry **Replace with clipboard content** allows you to paste clipboard contents into existing files by replacing them.
This works as long as the clipboard contains data compatible with the selected file type.
The old file is moved to the recycle bin and can be restored with usual undo option.

[^save_plain_text]: To force saving plain text data to a file with a special extension,
use uppercase letters or prepend a dot to the file extension (neither will change the actual filename).
For example, when copying syntax highlighted HTML code snippets from a browser,
using `html` will cause the html-formatted text to be saved,
while using `HTML` will save the plain text.


### Copy file contents
Run the program from the file explorer context menu, with the hotkey `WIN`+`ALT`+`C` (if enabled) or via command line.

Currently, image and text files are supported. If the file format is not understood, an error message will be shown.



### Command Line

Use `help`, `help paste`, `help config` etc. as argument to show available command line options, e.g.:
```
> .\PasteIntoFile.exe help
PasteIntoFile 5.0.0.0
Copyright © PasteIntoFile GitHub contributors

  config     Change configuration (without saving clipboard)
  copy       Copy file contents to clipboard
  help       Display more information on a specific command.
  paste      (Default Verb) Paste clipboard contents into file
  tray       Open in tray and wait for hotkey Win + Alt + V
  version    Display version information.
  wizard     Open the first-launch wizard
```
```
> .\PasteIntoFile.exe help paste
PasteIntoFile 5.0.0.0
Copyright © PasteIntoFile GitHub contributors

  -a, --autosave       Autosave file without prompt (true/false)
  -c, --clear          Clear clipboard after save (true/false)
  -d, --directory      Path of directory to save file into
  -f, --filename       Filename template with optional format variables such as
                       {0:yyyyMMdd HHmmSS} for current date and time
                       {1:000} for batch-mode save counter
                       May also contain a file extension and path fragment if
                       used in paste mode.
  --help               Display this help screen.
  --image-extension    Set default file extension for image contents
  --overwrite          (Default: false) Overwrite existing file without prompt.
                       Requires --autosave=true.
  --text-extension     Set default file extension for text contents
  --version            Display version information.
```

**Examples:**
- Add/remove the *Paste Into File* entry in the File Explorer context menu:
   ```powershell
   PasteIntoFile config --register
   PasteIntoFile config --unregister
   ```
- Start *Paste Into File* manually in system tray and react to hotkeys:
   ```powershell
   PasteIntoFile tray
   ```
- En-/disable autostart of *Paste Into File* in system tray on windows startup:
   ```powershell
   PasteIntoFile config --enable-autostart
   PasteIntoFile config --disable-autostart
   ```
- Configure the default filename template format (see [format specifiers](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings)):
   ```powershell
   PasteIntoFile config -f "{0:yyyy-MM-dd HH-mm-ss}"
   ```
- Save clipboard contents in autosave mode to specific location:
  ```powershell
  PasteIntoFile -d the/directory -f the_filename --autosave=true
  ```
- Copy file contents to clipboard:
  ```powershell
  PasteIntoFile copy path_to/the_file
  ```

## Developer notes

### Contributing
This project uses [pre-commit](https://pre-commit.com) with [dotnet-format](https://github.com/dotnet/format) to ensure consistent file formatting and clean diffs.
Install these tools before committing:
```bash
dotnet tool install --global dotnet-format --version 5.1.250801
pip install pre-commit
pre-commit install
```

### CI/CD
This project uses GitHub actions for test builds and releases.

##### Release a new version
1. Increment the version numbers in [`AssemblyInfo.cs`](PasteIntoFile/Properties/AssemblyInfo.cs#L34-L35)
1. [Create a release](https://github.com/eltos/PasteIntoFile/releases/new?title=Version%200.0&body=-%20...%0A%0AFull%20Changelog%3A%20https%3A%2F%2Fgithub.com%2Feltos%2FPasteIntoFile%2Fcompare%2Fv0.0...v0.0.0%0A%0A%5B!%5BGitHub%20release%20(by%20tag)%5D(https%3A%2F%2Fimg.shields.io%2Fgithub%2Fdownloads%2Feltos%2Fpasteintofile%2Fv0.0.0%2Ftotal)%5D(https%3A%2F%2Fmann1x.github.io%2Fgithub-release-stats%2F%3Fusername%3Deltos%26repository%3DPasteIntoFile)) on GitHub's release page. This will trigger the release workflow, which will autonomously build the binary and installer and attach it to the release.
1. [Submit](https://www.microsoft.com/en-us/wdsi/filesubmission?persona=HomeUser) this installer for analysis against Microsoft Defender Smartscreen as described [here](https://github.com/eltos/PasteIntoFile/discussions/10)




