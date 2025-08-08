# VanillaStyleDiplomacy

This mod aims to give a more vanilla feel to the various features provided by the already existing and more popular Diplomacy mod.
In contrast to the already existing Diplomacy mod this mod is made with cross-platform compatibility in mind and thus will not use Harmony or other Windows-only dependencies (or any other platform specific elements for that matter).

# How to install

## Using the precompiled binaries

1. Locate your Mount & Blade 2: Bannerlord installation
    1.1 [In Steam] Right click M&B 2: Bannerlord -> Properties -> Installed Files -> Browse
2. Open the "Modules" folder in your Mount & Blade 2: Bannerlord installation
3. Download the VanillaStyleDiplomacy.zip file from the Packages page and extract the contents
4. Copy the extracted folder into your "Modules" folder
5. Start Mount & Blade 2: Bannerlord and enable "VanillaStyleDiplomacy" in the "Mods" tab, located just below the Singleplayer/Multiplayer selection
6. Win

## Building from source

Clone the git repository:
```
git clone https://github.com/KaySteinhoff/VanillaStyleDiplomacy.git
```

### Building on Windows

Open the Project in either Visual Studio or Visual Studio Code and build the dll.
Make sure the resulting file targets .NET Framework 4.7.2.

### Building on Linux

**_SDK_**<br>

First you'll need to install the .NET SDK.<br>
Luckily it is rather easy to install as Microsoft has a comprehensive tutorial [here](https://learn.microsoft.com/en-us/dotnet/core/install/linux?WT.mc_id=dotnet-35129-website) on how to do it both manually and scripted.<br>
You can choose either but the scirpted one is easier to do.<br>
<br>
After installing the latest .NET version run the following command in the terminal:
```
which dotnet
```

If a path is printed the SDK was successfully installed. If not check if the directory `/home/<YourUserName>/.dotnet` exists. If it does run the following command to add it to your `PATH` variable.<br>
**Note that this will not be carried over into the next session. Please check how to permanently add PATH variables if you wish to do that.**
```
export PATH=$PATH:/home/<YourUserName>/.dotnet/
```
If you now run the previous `which` command again you should see a path being printed.<br>

**_IDE_**<br>

As Visual Studio is a Windows-only application we'll use Visual Studio Code.<br>
It is of the utmost importance that you install VSCode using the command line as it is otherwise possible that VSCode is unable to open a terminal instance, making it impossible for it to find the installed .NET SDK.<br>
Luckily Visual Studio Code has a tutorial [here](https://code.visualstudio.com/docs/setup/linux) which covers multiple distros such as Ubuntu, Fedora, Arch, etc.<br>
<br>
Once you've followed your distobution specific installation guide run the following command:
```
code --version
```

A result simiar to this should be printed:
```
1.102.3
488a1f239235055e34e673291fb8d8c810886f81
x64
```
This will confirm that Visual Studio Code has been successfully installed.<br>
<br>
Once done start Visual Studio Code by typing:
```
code
```

**_Final setup_**<br>
<br>
Once Visual Studio Code has started and you have **not** received an error message of the .NET SDK not being located a few extensions have to be installed:
- C# Dev Kit
- vscode-solution-explorer(Optional but very helpful)

Once all extensions are installed create a new Class Library project.<br>
Edit the *.csproj file(either provided as a plain text file in the Explorer or Right Click->'Open file' using the vscode-solution-explorer) and modify it to look like this:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- We target multiple Frameworks just to make sure we can at least compile for .NET 8.0(might be a different version depending on what you've installed) -->
    <TargetFrameworks>net8.0;net472</TargetFrameworks>
  </PropertyGroup>

  <ItemGroup>
    <!-- This NuGet Package enables us to compile for the .NET Framework 4.7.2 by creating a dependency to the requested version -->
    <PackageReference Include="Microsoft.NETFramework.ReferenceAssemblies" PrivateAssets="All" Version="1.0.0-preview.2"/>

    <!-- You can also reference the TaleWorlds.*.dll files in the actual bin/Win64_Shipping_Client/ directory of the game files but you'll get some unnecessary Warnings because of some C++ *.dlls that way -->
    <Reference Include="bin/Debug/net472/TaleWorlds.*.dll"/>
  </ItemGroup>
</Project>
```

Now just open the project in Visual Studio and build it.<br>
My advice is to use a seperate terminal instance and using the following command:
```
clear && dotnet build
```
This way you won't mess with any other outputs and will always have a clear overview of the last build process.

# Changelogs

## v 0.0.1

- Initial publication
- Optional patch for more sensible execution logic
  - Reputation loss/gain depends on various elements listed here in decreasing value order: Family, Ally(in the same Kingdom), War status