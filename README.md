# Confusious

> Will your project survive dependency confusion?

**Confusious** is a .NET proof-of-concept tool designed to detect [dependency confusion](https://medium.com/@alex.birsan/dependency-confusion-4a5d60fec610) vulnerabilities in projects using NuGet packages.

It identifies internal packages, publishes fake equivalents to a local feed, manipulates the NuGet resolution order, and verifies if the project accidentally pulls these dummy packages.

---

## Features

- Parses your project's `project.assets.json` to extract dependencies and NuGet sources
- Segregates **public** vs **internal** packages
- Automatically:
  - Injects a local "fake" NuGet feed
  - Publishes dummy packages with same names/versions as internal packages
  - Performs restore to detect vulnerable package resolutions
  - Cleans up everything afterward
- Flags vulnerable dependencies clearly

---

## Requirements

- .NET 8.0 SDK or later
- NuGet CLI (for publishing dummy packages)
- Your project must have already run `dotnet restore` once (so `project.assets.json` exists)

---

## How It Works

1. You provide a path to your `project.assets.json` file.
2. The tool:
   - Extracts dependencies and feed sources
   - Identifies internal packages (non-public feeds)
   - Creates and publishes dummy `.nupkg` files for them
   - Adds the fake feed to `NuGet.config`
   - Triggers a `dotnet restore`
   - Checks if any dummy packages were pulled
3. If any package was resolved from the fake feed, it flags them as **vulnerable**.
4. Then it:
   - Removes the fake feed
   - Deletes all dummy packages and related folders
   - Restores to revert back to original state

---

## Installation

Clone the repo and build:

```bash
git clone https://github.com/vrushalned/Confusious.git
cd Confusious
dotnet build
```

## Run
```bash
dotnet run --path <path-to-project.assets.json>
```

## Sample Output
```bash
Reading /path/to/your/project/obj/project.assets.json ...
Config Path: /path/to/your/project/NuGet.config

Public packages:
PackageName: Newtonsoft.Json, Version: 13.0.1

Internal packages:
PackageName: Internal.Logging, Version: 1.0.0, Source: https://internal.nuget/feed

Adding fake feed...
Creating fake packages...
Running dotnet restore...

!!!VULNERABLE PACKAGES!!!
PackageName: Internal.Logging, Version: 1.0.0, Source: file:///fakefeed/

Cleaning up...
Restoring original packages...
```



## Ethical Usage Advisory

Confusious is intended **solely for authorized security testing** on projects and environments you own or have explicit permission to analyze.  
Unauthorized use against third-party systems, services, or software is strictly prohibited and may violate applicable laws and regulations.

By using Confusious, you agree to act responsibly and ethically. The authors disclaim any liability for misuse or damages arising from improper use.

Always obtain proper consent before testing any software or infrastructure.


