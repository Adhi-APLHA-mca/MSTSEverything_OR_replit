# Open Rails

Open Rails is a free, open-source train simulator designed to be compatible with Microsoft Train Simulator (MSTS) content.

## Project Overview

This is the source code for the **Open Rails** train simulator — a Windows-only C# desktop application built with MonoGame (DirectX) for rendering, OpenAL for 3D audio, and Windows Forms/WPF for GUI tools.

**Because this project uses DirectX and Windows-specific .NET APIs, it can only be built and run on Windows using Visual Studio.** The Replit environment runs Linux and serves a project information page instead.

## Tech Stack

- **Language:** C# (.NET 4.7.2 / net6-windows)
- **Rendering:** MonoGame (WindowsDX / DirectX)
- **Audio:** OpenAL Soft
- **GUI:** Windows Forms, WPF
- **Build:** MSBuild / Visual Studio, NuGet
- **Docs:** Sphinx (reStructuredText)

## Replit Setup

The Replit environment serves a static project landing page via a simple Node.js HTTP server:

- `server.js` — lightweight HTTP server on port 5000
- `public/index.html` — project info page
- `public/or_logo.png` — project logo

## Building on Windows

1. Install Visual Studio 2019/2022 with the **.NET desktop development** workload
2. Clone the repo and open `Source/ORTS.sln`
3. Use *Build > Rebuild Solution* — outputs to `Program/`
4. Download content from openrails.org and run `OpenRails.exe`

## Key Source Directories

```
Source/
├── ORTS.sln              # Visual Studio solution
├── RunActivity/          # Main simulator executable
├── Launcher/             # Entry point (OpenRails.exe)
├── Menu/                 # Route/activity selection GUI
├── Orts.Simulation/      # Core physics & AI engine
├── Orts.Formats.Msts/    # MSTS file format parsers
├── Orts.Common/          # Shared utilities
├── MultiPlayerServer/    # Multiplayer server
├── Contrib/              # Tools: TrackViewer, TimetableEditor...
└── Documentation/Manual/ # reStructuredText manual source
```

## Links

- Website: https://www.openrails.org
- Manual: https://open-rails.readthedocs.io/en/latest/
- Bug tracker: https://bugs.launchpad.net/or
- Forum: https://www.elvastower.com/forums/

## User Preferences

- Keep the landing page simple and informative
