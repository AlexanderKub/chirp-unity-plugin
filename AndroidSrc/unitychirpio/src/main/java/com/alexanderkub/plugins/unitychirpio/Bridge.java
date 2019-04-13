package com.alexanderkub.plugins.unitychirpio;

import android.app.Activity;
import android.content.Context;
import android.os.Handler;

public final class Bridge {
    private static ChirpPluginJavaMessageHandler javaMessageHandler;
    private static Handler unityMainThreadHandler;
    private ChirpConnector connector;

    //region USED FROM UNITY METHODS
    @SuppressWarnings("unused")
    public void registerPluginHandlers(final Activity act,
                                       ChirpPluginJavaMessageHandler handler) {
        javaMessageHandler = handler;
        if(unityMainThreadHandler == null) {
            unityMainThreadHandler = new Handler();
        }
        this.connector = new ChirpConnector(act.getBaseContext(), act);
    }

    @SuppressWarnings("unused")
    public void ChirpInitSDK(String key, String secret, String config) {
        this.connector.InitSDK(key, secret, config);
    }

    @SuppressWarnings("unused")
    public int ChirpStartSDK() {
        return this.connector.StartSDK();
    }

    @SuppressWarnings("unused")
    public int ChirpStopSDK() {
        return this.connector.StopSDK();
    }

    @SuppressWarnings("unused")
    public int ChirpSendData(int length, String payload) {
        return this.connector.sendPayload(payload);
    }
    //endregion

    private static void runOnUnityThread(Runnable runnable) {
        if(unityMainThreadHandler != null && runnable != null) {
            unityMainThreadHandler.post(runnable);
        }
    }

    static void SendReceiveEventToUnity(final String data) {
        runOnUnityThread(new Runnable() {
            @Override
            public void run() {
                if(javaMessageHandler != null) {
                    javaMessageHandler.OnReceiveDataHandler(data);
                }
            }
        });
    }

    static void SendSentEventToUnity(final String data) {
        runOnUnityThread(new Runnable() {
            @Override
            public void run() {
                if(javaMessageHandler != null) {
                    javaMessageHandler.OnSentDataHandler(data);
                }
            }
        });
    }

    static void SendChangeStateEventToUnity(int state) {
        final int tmpState = state;
        runOnUnityThread(new Runnable() {
            @Override
            public void run() {
                if(javaMessageHandler != null) {
                    javaMessageHandler.OnChangeStateHandler(tmpState);
                }
            }
        });
    }
}
