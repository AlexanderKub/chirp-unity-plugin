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

public enum ChirpErrorsEnum {
    CHIRP_CONNECT_OK = 0,
    CHIRP_CONNECT_OUT_OF_MEMORY,
    CHIRP_CONNECT_NOT_INITIALISED,
    CHIRP_CONNECT_NOT_RUNNING,
    CHIRP_CONNECT_ALREADY_RUNNING,
    CHIRP_CONNECT_ALREADY_STOPPED,

    CHIRP_CONNECT_INVALID_SAMPLE_RATE = 20,
    CHIRP_CONNECT_NULL_BUFFER,
    CHIRP_CONNECT_NULL_POINTER,
    CHIRP_CONNECT_INVALID_CHANNEL,
    CHIRP_CONNECT_INVALID_FREQUENCY_CORRECTION,

    CHIRP_CONNECT_INVALID_KEY = 40,
    CHIRP_CONNECT_INVALID_SECRET,
    CHIRP_CONNECT_INVALID_CREDENTIALS,
    CHIRP_CONNECT_MISSING_SIGNATURE,
    CHIRP_CONNECT_INVALID_SIGNATURE,
    CHIRP_CONNECT_MISSING_CONFIG,
    CHIRP_CONNECT_INVALID_CONFIG,
    CHIRP_CONNECT_EXPIRED_CONFIG,
    CHIRP_CONNECT_INVALID_VERSION,
    CHIRP_CONNECT_INVALID_PROJECT,
    CHIRP_CONNECT_INVALID_CONFIG_CHARACTER,

    CHIRP_CONNECT_PAYLOAD_EMPTY_MESSAGE = 80,
    CHIRP_CONNECT_PAYLOAD_INVALID_MESSAGE,
    CHIRP_CONNECT_PAYLOAD_UNKNOWN_SYMBOLS,
    CHIRP_CONNECT_PAYLOAD_DECODE_FAILED,
    CHIRP_CONNECT_PAYLOAD_TOO_LONG,
    CHIRP_CONNECT_PAYLOAD_TOO_SHORT,

    CHIRP_CONNECT_INVALID_VOLUME = 99,
    CHIRP_CONNECT_UNKNOWN_ERROR = 100,

    CHIRP_CONNECT_NETWORK_ERROR = 200,
    CHIRP_CONNECT_NETWORK_NO_NETWORK,
    CHIRP_CONNECT_NETWORK_PERMISSIONS_NOT_GRANTED,
    CHIRP_CONNECT_ACCOUNT_DISABLED,
    CHIRP_CONNECT_AUDIO_IO,
    CHIRP_CONNECT_SEND_MODE_DISABLED,
    CHIRP_CONNECT_RECIEVE_MODE_DISABLED,
    CHIRP_CONNECT_DEVICE_MUTED,

    CHIRP_PLUGIN_NOT_INIT = 404,
}

public class ChirpPlugin : MonoBehaviour {
    #region iOS Plugin Import

    delegate void MonoPStateChangeDelegate(ChirpStateEnum state);
    delegate void MonoPDataDelegate(string data);
    private static bool IsPluginInit;

    #if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern int ChirpInitSDK(string key, string secret, string config);
    [DllImport("__Internal")]
    private static extern int ChirpStartSDK();
    [DllImport("__Internal")]
    private static extern int ChirpStopSDK();
    [DllImport("__Internal")]
    private static extern int ChirpSendData(int length, string payload);
    [DllImport("__Internal")]
    private static extern void RegisterPluginHandlers(MonoPStateChangeDelegate state, MonoPDataDelegate recieve ,MonoPDataDelegate sent);
    #endif

    #endregion

    #region Android Plugin Import

    #if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject PluginBridge;
    public class ChirpPluginJavaMessageHandler : AndroidJavaProxy {
        public ChirpPluginJavaMessageHandler() : base("com.alexanderkub.plugins.unitychirpio.ChirpPluginJavaMessageHandler") { }

        public void OnChangeStateHandler(int state) {
            if (Enum.IsDefined(typeof(ChirpStateEnum), state)) {
                ChirpPlugin.OnChangeState((ChirpStateEnum)state);
            }
        }

        public void OnReceiveDataHandler(string data) {
            ChirpPlugin.OnReceiveData(data);
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
        int errorCode = 0;
        #if UNITY_EDITOR
        Debug.Log("ChirpPlugin.InitSDK with\n" + key + "\n" + secret + "\n" + config);
        #else
            #if UNITY_IOS
            errorCode = ChirpInitSDK(key, secret, config);
            #elif UNITY_ANDROID
            object[] parameters = new object[3];
            parameters[0] = key;
            parameters[1] = secret;
            parameters[2] = config;
            PluginBridge.Call("ChirpInitSDK", parameters);
            #endif
        #endif
        HandleError(errorCode);
        IsPluginInit = true;
    }

    public static void StartSDK() {
        int errorCode = 0;
        #if UNITY_EDITOR
        Debug.Log("ChirpPlugin.StartSDK");
        #else
            #if UNITY_IOS
            errorCode = ChirpStartSDK();
            #elif UNITY_ANDROID
            errorCode = PluginBridge.Call<int>("ChirpStartSDK");
            #endif
        #endif
        HandleError(errorCode);
    }

    public static void StopSDK() {
        if (!IsPluginInit) {
            return;
        }
        int errorCode = 0;
        #if UNITY_EDITOR
        Debug.Log("ChirpPlugin.StopSDK");
        #else
            #if UNITY_IOS
            errorCode = ChirpStopSDK();
            #elif UNITY_ANDROID
            errorCode = PluginBridge.Call<int>("ChirpStopSDK");
            #endif
        #endif
        HandleError(errorCode);
    }

    public static void SendData(string payload) {
        if (string.IsNullOrEmpty(payload)) {
            Debug.Log("ChirpPlugin.SendData Error: NullOrEmpty payload");
            return;
        }
        int errorCode = 0;
        #if UNITY_EDITOR
        Debug.Log("ChirpPlugin.SendData: " + payload);
        #else
            #if UNITY_IOS
            errorCode = ChirpSendData(payload.Length, payload);
            #elif UNITY_ANDROID
            errorCode = PluginBridge.Call<int>("ChirpSendData", payload.Length, payload);
            #endif
        #endif
        HandleError(errorCode);
    }

    #endregion

    #region Native Callbacks

    [AOT.MonoPInvokeCallback(typeof(MonoPStateChangeDelegate))]
    private static void OnChangeState(ChirpStateEnum state) {
        OnChangeStateDataEvent?.Invoke(ChirpState, state);
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
        #if UNITY_EDITOR
        Debug.Log("ChirpPlugin.Initialize");
        #else
            #if UNITY_IOS
            RegisterPluginHandlers(OnChangeState, OnReceiveData, OnSentData);
            #elif UNITY_ANDROID
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            PluginBridge = new AndroidJavaObject("com.alexanderkub.plugins.unitychirpio.Bridge");
            object[] parameters = new object[2];
            parameters[0] = unityActivity;
            parameters[1] = new ChirpPluginJavaMessageHandler();
            PluginBridge.Call("registerPluginHandlers", parameters);
            #endif
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

    private static void HandleError(int errorCode) {
        if (errorCode == 0) {
            return;
        }
        string errorString;
        if (Enum.IsDefined(typeof(ChirpErrorsEnum), errorCode)) {
            ChirpErrorsEnum error = (ChirpErrorsEnum)errorCode;
            errorString = "ChirpPlugin error(" + errorCode + "): " + error.ToString();
            Debug.LogError(errorString);
            throw new Exception(errorString);
        }
        errorString = "ChirpPlugin error(" + errorCode + "): CHIRP_PLUGIN_UNKNOW_ERROR";
        Debug.LogError(errorString);
        throw new Exception(errorString);
    }
}
