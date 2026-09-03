ROB'S MUSIC COPIER v1.05 - .NET 10
=================================

Rob's Music Copier is a free Windows utility for copying music files referenced
by M3U and M3U8 playlists and for updating playlist paths when music is moved to
a different drive or folder.

FEATURES
- Copy every music file referenced by an M3U/M3U8 playlist.
- Preserve the original folder hierarchy or flatten the files into one folder.
- Optionally number flattened files in playlist order.
- Report missing files in MissingFiles.txt.
- Replace old playlist locations with a new drive or folder location.
- Save the updated playlist in a separately selected destination folder.
- Create a numbered TXT list of the playlist in play order.
- Open the copy destination or newly created playlist directly from the app.
- Clear all entries separately on either tab.

BUILD THE STANDALONE EXE
1. Install the .NET 10 SDK on the Windows computer used to build the program.
2. Double-click Publish-Standalone.bat.
3. The finished self-contained file will be:
   bin\Release\net10.0-windows\win-x64\publish\RobsMusicCopier-v1.05.exe

The finished EXE includes the .NET runtime. Computers running it do not need
.NET installed, and the EXE can be copied and run by itself.

Manual build command:
dotnet publish RobsMusicCopier.csproj -c Release -r win-x64 --self-contained true

ICONS AND SUPPORT
The Rob's Music Copier headphones artwork appears inside the app and is embedded
as the Windows EXE, title-bar, and taskbar icon. The PayPal logo appears only on
the in-app support controls. Donations are completely optional and are not
required to download, use, copy, or modify the program.

VIRUS SCANNING
You can independently scan the published EXE at https://www.virustotal.com/.
New unsigned self-contained applications can occasionally receive generic
false-positive detections. Review the individual detection names and results.

LICENSE
Copyright (c) 2026 Rob Cannell.
This project is licensed under the MIT License. See LICENSE.txt.

VERSION 1.05
- Added the headphones logo inside the app and as the Windows application icon.
- Retained the PayPal logo only on the internal support controls.
- Added an About window with version, copyright, license, and support details.
- Added the version to the standalone EXE filename.
- Includes Clear All controls, separate edit destination selection, playlist TXT
  export, and direct access to the created playlist.
