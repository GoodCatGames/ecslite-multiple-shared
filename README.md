# Shared injection for LeoEcsLite C# Entity Component System framework
Dependency injection for [LeoECS Lite](https://github.com/Leopotam/ecslite).

> Tested on unity 2020.3 (not dependent on it) and contains assembly definition for compiling to separate assembly file for performance reason.

> **Important!** Don't forget to use `DEBUG` builds for development and `RELEASE` builds in production: all internal error checks / exception throwing works only in `DEBUG` builds and eleminated for performance reasons in `RELEASE`.

# Table of content
* [Socials](#socials)
* [Installation](#installation)
    * [As unity module](#as-unity-module)
    * [As source](#as-source)
* [Integration to startup](#integration-to-startup)
* [License](#license)

# Socials
[![discord](https://img.shields.io/discord/404358247621853185.svg?label=enter%20to%20discord%20server&style=for-the-badge&logo=discord)](https://discord.gg/5GZVde6)

# Installation

## As unity module
This repository can be installed as unity module directly from git url. In this way new line should be added to `Packages/manifest.json`:
```
"com.goodcat.ecslite.shared": "https://github.com/GoodCatGames/ecslite-shared.git",
```

## As source
If you can't / don't want to use unity modules, code can be downloaded as sources archive from `Releases` page.

# Integration to startup
```csharp
var battleLog = new BattleLog();
IBattleLogView battleLogView = null;
#if DEBUG
   battleLogView = new BattleLogViewText(battleLog);         
#else
   battleLogView = new BattleLogViewHtml(battleLog);         
#endif

// If you use InjectShared() you cannot create systems with SharedCustom:
//    var systems = new EcsSystems (new EcsWorld(), new SharedCustom());
// You will get an exception.

var systems = new EcsSystems(new EcsWorld());
systems
    .Add (new System1())    
    .Add (new System2())    
    .Add (new BattleLogShowSystem())    
    // ...
    
    // InjectShared() methods should be placed after
    // all systems/worlds registration 
    .InjectShared(battleLog)
    .InjectShared<IBattleLogView>(battleLogView)    
    
    // InitShared() method should be placed after 
    // all InjectShared() methods
    .InitShared()        
    .Init();
```
```csharp
public class BattleLogShowSystem : IEcsRunSystem
{
  [EcsInject] private readonly BattleLogViewBase _battleLogView;
  
  public void Run(EcsSystems systems)
  {
    _battleLogView.Show();
  }
}
```
```csharp
// Getting Shared directly from systems
var shared = systems.GetShared<shared.Shared>();
var battleLogView = shared.Get<IBattleLogView>();
battleLogView.Show();
```

# License
The software is released under the terms of the [MIT license](./LICENSE.md).

No personal support or any guarantees.