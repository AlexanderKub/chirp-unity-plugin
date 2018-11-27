# Chirp Unity Plugin

Plugin for integrate data over sound SDK [Chirp.io](https://chirp.io/) into Unity project. Support iOS and Android projects. For more information see [Chirp documentation](https://developers.chirp.io/docs).

## Setup
For setup plugin you will need to

- Import `UnityChirpIOPlugin.unitypackage`([download](https://github.com/AlexanderKub/unity-chirp-plugin/releases/download/0.0.1/UnityChirpIOPlugin.unitypackage)) into your Unity project.
- Sign up at [developers.chirp.io](https://developers.chirp.io) and download the latest Android and iOS SDK from downloads.
- Copy the `chirp-connect-release.aar` file into the `ChirpIO/Plugins/Android` folder of the Unity project.
- Copy the `ChirpConnect.framework` file into the `ChirpIO/Plugins/iOS` folder of the Unity project.
- Copy your app key, secret and config into TestPlugin script fields on ChirpGO game object in ChirpIOExampleScene scene.

## Build 
For Android build you no need do extra steps, just build your project.
For iOS build you will need to

- Go `Edit\Project Settings\Audio` check `Disable Unity Audio`. Cause `Deactivating an audio session that has running I/O` issue.
- Go iOS Player Settings and define `Microphone Usage Description`.
- In XCode project set `Enable Bitcode: NO`.

## Usage
At moment plugin allow send and receive string data over sound, see example scene.
This plugin not full implement SDK features, and not support for example error handling.
If you need more features, you can create issue, also feel free to contribute.

## Test
For testing data receiving you can use [Chirp Audio API](https://audio.chirp.io/v3/docs/). Use your developer key and secret for auth.
Send [Hello world!](https://audio.chirp.io/v3/standard/48656c6c6f20776f726c6421.wav) by standart protocol.

----
