# LibGit2Sharp DLL Files

This folder contains the LibGit2Sharp library DLL files required for Git operations in Unity.

## Required DLL Files

You need to add the following DLL files to this directory:

### Core Library
- `LibGit2Sharp.dll` - Main LibGit2Sharp library

### Native Dependencies (Platform-Specific)
LibGit2Sharp requires native git2 binaries. Download and place them based on your target platform:

#### Windows
- `git2-e632535.dll` (or latest version)
- Place in: `Assets/Plugins/LibGit2Sharp/`

For 32-bit and 64-bit specific versions:
- 32-bit: `Assets/Plugins/x86/git2-xxxxx.dll`
- 64-bit: `Assets/Plugins/x86_64/git2-xxxxx.dll`

#### macOS
- `libgit2-xxxxx.dylib`
- Place in: `Assets/Plugins/LibGit2Sharp/`

#### Linux
- `libgit2-xxxxx.so`
- Place in: `Assets/Plugins/LibGit2Sharp/`

## Installation Instructions

1. **Download LibGit2Sharp NuGet Package**
   - Visit: https://www.nuget.org/packages/LibGit2Sharp/
   - Download the latest version
   - Extract the `.nupkg` file (it's a ZIP file)

2. **Extract DLL Files**
   - From the NuGet package, locate:
     - `lib/net46/LibGit2Sharp.dll` (or appropriate .NET version)
     - `runtimes/win-x64/native/git2-xxxxx.dll` (Windows 64-bit)
     - Similar paths for other platforms

3. **Copy to Unity Project**
   - Copy `LibGit2Sharp.dll` to `Assets/Plugins/LibGit2Sharp/`
   - Copy native DLLs to appropriate platform folders

4. **Configure Import Settings in Unity**
   - Select each DLL in Unity Project window
   - In the Inspector:
     - Check "Any Platform" or select specific platforms
     - For Editor: Enable "Editor" and "Standalone"
     - For native DLLs: Match with your build targets

## Platform Settings

### LibGit2Sharp.dll
- **Any Platform**: Enabled
- **Include Platforms**: Editor, Standalone
- **Asset Labels**: Add custom labels if needed

### Native DLLs (git2-*.dll, *.dylib, *.so)
- **Platform**: Match your build target (Windows/macOS/Linux)
- **CPU**: x86_64 (64-bit) or x86 (32-bit)
- **Include Platforms**: Standalone

## Folder Structure

```
Assets/
├── Editor/
│   └── GitHelper.cs
└── Plugins/
    ├── LibGit2Sharp/
    │   ├── README.md (this file)
    │   ├── LibGit2Sharp.dll
    │   └── git2-xxxxx.dll (Windows, any)
    ├── x86/
    │   └── git2-xxxxx.dll (Windows 32-bit)
    └── x86_64/
        └── git2-xxxxx.dll (Windows 64-bit)
```

## Usage

Once the DLLs are properly installed, the `GitHelper` class in `Assets/Editor/GitHelper.cs` will be able to:
- Initialize Git repositories
- Stage and commit changes
- Create and manage branches
- Add remotes and push to GitHub

## Troubleshooting

### DLL Not Found Error
- Ensure all required DLLs are in the correct folders
- Check Unity's import settings for each DLL
- Verify platform compatibility

### LibGit2Sharp Initialization Failed
- Make sure native dependencies (git2 DLLs) are present
- Check that DLL versions are compatible
- Ensure proper folder structure

### Build Errors
- Verify that native DLLs are included in build
- Check Build Settings → Player → Other Settings
- Ensure API Compatibility Level is set to .NET Framework or .NET Standard 2.1

## Version Information

- **Recommended LibGit2Sharp Version**: 0.27.0 or later
- **Unity Version**: 2020.3 LTS or later
- **.NET Target**: .NET Framework 4.6+ or .NET Standard 2.1

## License

LibGit2Sharp is released under the MIT License. See: https://github.com/libgit2/libgit2sharp

## Additional Resources

- LibGit2Sharp GitHub: https://github.com/libgit2/libgit2sharp
- LibGit2Sharp Documentation: https://github.com/libgit2/libgit2sharp/wiki
- NuGet Package: https://www.nuget.org/packages/LibGit2Sharp/
