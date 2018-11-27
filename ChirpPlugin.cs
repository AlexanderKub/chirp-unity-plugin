using UnityEngine;
using System;
using System.Runtime.InteropServices;

public enum ChirpStateEnum {
    NotInitialised,     
    Stopped,     
    Paused,   
    Running,
    Sending,
    Receiving,
}

public class ChirpPlugin : MonoBehaviour {

    #region iOS Plugin Import

    private delegate void MonoPStateChangeDelegate(ChirpStateEnum state);
    private delegate void MonoPDataDelegate(string data);

    #if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern int ChirpInitSDK(string key, string secret, string config);
    [DllImport("__Internal")]
    private static extern void ChirpStartSDK();
    [DllImport("__Internal")]
    private static extern void ChirpStopSDK();
    [DllImport("__Internal")]
    private static extern void ChirpSendData(int length, string payload);
    [DllImport("__Internal")]
    private static extern void RegisterPluginHandlers(MonoPStateChangeDelegate state, MonoPDataDelegate recieve ,MonoPDataDelegate sent);
    #endif

    #endregion

    #region Android Plugin Import
    #if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject PluginBridge;
    public class ChirpPluginJavaMessageHandler : AndroidJavaProxy {
        public ChirpPluginJavaMessageHandler() : base("com.alexanderkub.plugins.unitychirpio.ChirpPluginJavaMessageHandler") { }

        public void OnChangeStateHandler(ChirpStateEnum state) {
            ChirpPlugin.OnChangeState(state);
        }

        public void OnReceiveDataHandler(string data) {
            ChirpPlugin.OnReceiveData(HexStringToString(data));
        }

        public void OnSentDataHandler(string data) {
            ChirpPlugin.OnSentData(HexStringToString(data));
        }
    }
    #endif
    #endregion

    #region Chirp SDK API

    public static ChirpStateEnum ChirpState;
    public static Action<ChirpStateEnum, ChirpStateEnum> OnChangeStateDataEvent;
    public static Action<string> OnRecieveDataEvent;
    public static Action<string> OnSentDataEvent;

    public static void InitSDK(string key, string secret, string config) {
        #if UNITY_IOS && !UNITY_EDITOR
        ChirpInitSDK(key, secret, config);
        #elif UNITY_ANDROID && !UNITY_EDITOR
        object[] parameters = new object[3];
        parameters[0] = key;
        parameters[1] = secret;
        parameters[2] = config;
        PluginBridge.Call("ChirpInitSDK", parameters);
        #endif
    }

    public static void StartSDK() {
        #if UNITY_IOS && !UNITY_EDITOR
        ChirpStartSDK();
        #elif UNITY_ANDROID && !UNITY_EDITOR
        PluginBridge.Call("ChirpStartSDK");
        #endif
    }

    public static void StopSDK() {
        #if UNITY_IOS && !UNITY_EDITOR
        ChirpStopSDK();
        #elif UNITY_ANDROID && !UNITY_EDITOR
        PluginBridge.Call("ChirpStopSDK");
        #endif
    }

    public static void SendData(string payload) {
        #if UNITY_IOS && !UNITY_EDITOR
        ChirpSendData(payload.Length, payload);
        #elif UNITY_ANDROID && !UNITY_EDITOR
        PluginBridge.Call("ChirpSendData", payload.Length, payload);
        #endif
    }

    #endregion

    #region Native Callbacks

    [AOT.MonoPInvokeCallback(typeof(MonoPStateChangeDelegate))]
    private static void OnChangeState(ChirpStateEnum state) {
        if (OnChangeStateDataEvent != null) {
            OnChangeStateDataEvent(ChirpState, state);
        }
        ChirpState = state;
    }

    [AOT.MonoPInvokeCallback(typeof(MonoPDataDelegate))]
    private static void OnSentData(string data) {
        if (OnSentDataEvent == null) {
            return;
        }
        OnSentDataEvent(data);
    }

    [AOT.MonoPInvokeCallback(typeof(MonoPDataDelegate))]
    private static void OnReceiveData(string data) {
        if (OnRecieveDataEvent == null) {
            return;
        }
        OnRecieveDataEvent(data);
    }

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize() {
        #if UNITY_IOS && !UNITY_EDITOR
        RegisterPluginHandlers(OnChangeState, OnReceiveData, OnSentData);
        #elif UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        PluginBridge = new AndroidJavaObject("com.alexanderkub.plugins.unitychirpio.Bridge");
        object[] parameters = new object[2];
        parameters[0] = unityActivity;
        parameters[1] = new ChirpPluginJavaMessageHandler();
        PluginBridge.Call("registerPluginHandlers", parameters);
        #endif
    }

    #endregion

    private static string HexStringToString(string HexString) {
        string stringValue = "";
        for (int i = 0; i < HexString.Length / 2; i++)
        {
            string hexChar = HexString.Substring(i * 2, 2);
            int hexValue = Convert.ToInt32(hexChar, 16);
            stringValue += Char.ConvertFromUtf32(hexValue);
        }
        return stringValue;
    }
}
