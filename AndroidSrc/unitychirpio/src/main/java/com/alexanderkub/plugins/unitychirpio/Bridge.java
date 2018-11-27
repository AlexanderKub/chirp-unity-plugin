package com.alexanderkub.plugins.unitychirpio;

import android.content.Context;
import android.os.Handler;

import io.chirp.connect.models.ConnectState;

public final class Bridge {
    private static ChirpPluginJavaMessageHandler javaMessageHandler;
    private static Handler unityMainThreadHandler;
    private ChirpConnector connector;

    public void registerPluginHandlers(final Context ctx, ChirpPluginJavaMessageHandler handler) {
        javaMessageHandler = handler;
        if(unityMainThreadHandler == null) {
            unityMainThreadHandler = new Handler();
        }
        this.connector = new ChirpConnector(ctx);
    }

    public void ChirpInitSDK(String key, String secret, String config) {
        this.connector.InitSDK(key, secret, config);
    }

    public void ChirpStartSDK() {
        this.connector.StartSDK();
    }

    public void ChirpStopSDK() {
        this.connector.StopSDK();
    }

    public void ChirpSendData(int length, String payload) {
        this.connector.sendPayload(length, payload);
    }

    private static void runOnUnityThread(Runnable runnable) {
        if(unityMainThreadHandler != null && runnable != null) {
            unityMainThreadHandler.post(runnable);
        }
    }

    protected static void SendReceiveEventToUnity(final String data) {
        runOnUnityThread(new Runnable() {
            @Override
            public void run() {
                if(javaMessageHandler != null) {
                    javaMessageHandler.OnReceiveDataHandler(data);
                }
            }
        });
    }

    protected static void SendSentEventToUnity(final String data) {
        runOnUnityThread(new Runnable() {
            @Override
            public void run() {
                if(javaMessageHandler != null) {
                    javaMessageHandler.OnSentDataHandler(data);
                }
            }
        });
    }

    protected static void SendChangeStateEventToUnity(byte state) {
        final byte tmpState = state;
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
