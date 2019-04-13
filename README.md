# Chirp Unity Plugin

Plugin for integrate data over sound SDK [Chirp.io](https://chirp.io/) into Unity project. Support iOS and Android projects. For more information see [Chirp documentation](https://developers.chirp.io/docs).

## Setup
For setup plugin you will need to

- Import `UnityChirpIOPlugin.unitypackage`([download](https://github.com/AlexanderKub/chirp-unity-plugin/releases/download/v0.0.3/UnityChirpIOPlugin.unitypackage)) into your Unity project.
- Sign up at [developers.chirp.io](https://developers.chirp.io) and download the latest Android and iOS SDK from downloads.
- Copy the `chirpsdk-release.aar` file into the `ChirpIO/Plugins/Android` folder of the Unity project.
- Copy the `ChirpConnect.framework` file into the `ChirpIO/Plugins/iOS` folder of the Unity project.
- Copy your app key, secret and config into TestPlugin script fields on ChirpGO game object in ChirpIOExampleScene scene.

## Build
For Android build you will need to

- Set `Minimum API Level` to level 19 or higher

For iOS build you will need to

- Go `Edit\Project Settings\Audio` check `Disable Unity Audio`. Cause `Deactivating an audio session that has running I/O` issue.
- Go iOS Player Settings and define `Microphone Usage Description`.
- In XCode project set `Enable Bitcode: NO`.
- Replace `@import AudioToolbox; @import Foundation;` to
`#import "AudioToolbox/AudioToolbox.h" #import "Foundation/Foundation.h"`
 in `ChirpConnect.h` file inside `ChirpConnect.framework`.

## Usage
At moment plugin allow send and receive string data over sound, see example scene.
This plugin not full implement SDK features.
If you need more features, you can create issue, also feel free to contribute.

## C# API
`ChirpPlugin` is static class for work with ChirpSDK.

### Methods
Method for initialize SDK with given key, secret and config.
```c#
    ChirpPlugin.InitSDK(string key, string secret, string config)
```
Method for start SDK work.
```c#
    ChirpPlugin.StartSDK()
```
Method for stop SDK work.
```c#
    ChirpPlugin.StopSDK()
```
Method for send given string data.
```c#
    ChirpPlugin.SendData(string payload)
```

### Properties
Current SDK state property.
```c#
   ChirpStateEnum ChirpPlugin.ChirpState
```

### Delegates
Action delegate for state change handling.
```c#
  Action<ChirpStateEnum, ChirpStateEnum> ChirpPlugin.OnChangeStateDataEvent
```

Action delegate for receive data handling. Data in HEX string format!
```c#
  Action<string> ChirpPlugin.OnRecieveDataEvent
```

Action delegate for sent data handling. Data in HEX string format!
```c#
   Action<string> ChirpPlugin.OnSentDataEvent
```
## Test
For testing data receiving you can use [Chirp Audio API](https://audio.chirp.io/v3/docs/). Use your developer key and secret for auth.
Send [Hello world!](https://audio.chirp.io/v3/standard/48656c6c6f20776f726c6421.wav) by standart protocol.

## Build from sources

Plugin source code for iOS it's just Objective-C++ files in Plugins folder, you don't need to build it.

Plugin source code for Android it's Gradle project.For a correct build you must place `chirpsdk-release.aar` to `unitychirpio/libs` and build `unitychirpio` module by Gradle.

----
