package com.alexanderkub.plugins.unitychirpio;

import android.Manifest;
import android.annotation.TargetApi;
import android.app.Activity;
import android.content.Context;
import android.content.pm.PackageManager;
import android.util.Log;
import android.support.annotation.NonNull;


import io.chirp.connect.ChirpConnect;
import io.chirp.connect.interfaces.ConnectEventListener;
import io.chirp.connect.models.ChirpError;

class ChirpConnector {
    private ChirpConnect chirpConnect;
    private Context Ctx;
    private Activity Act;

    ChirpConnector(final Context ctx, final Activity act) {
        this.Ctx = ctx;
        this.Act = act;
    }

    void InitSDK(String key, String secret, String config) {
        if (this.chirpConnect != null) {
            this.chirpConnect.close();
            this.chirpConnect = null;
        }

        final ConnectEventListener connectEventListener = new ConnectEventListener() {

            @Override
            public void onSending(@NonNull byte[] payload, int channel) {
                //String hexData = new String(payload);
                //Log.v("connectDemoApp", "ConnectCallback: onSending: " + hexData + " on channel: " + channel);
            }

            @Override
            public void onSent(@NonNull byte[] data, int channel) {
                /*
                 * onSent is called when a send event has completed.
                 * The data argument contains the payload that was sent.
                 */
                String hexData = new String(data);
                Bridge.SendSentEventToUnity(hexData);
                //Log.v("connectDemoApp", "ConnectCallback: onSent: " + hexData + " on channel: " + channel);
            }

            @Override
            public void onReceiving(int channel) {
                /*
                 * onReceiving is called when a receive event begins.
                 * No data has yet been received.
                 */
                //Log.v("connectDemoApp", "ConnectCallback: onReceiving on channel: " + channel);
            }

            @Override
            public void onReceived(byte[] data, int channel) {
                /*
                 * onReceived is called when a receive event has completed.
                 * If the payload was decoded successfully, it is passed in data.
                 * Otherwise, data is null.
                 */
                String hexData = "null";
                if (data != null) {
                    hexData = new String(data);
                }
                Bridge.SendReceiveEventToUnity(hexData);
                //Log.v("connectDemoApp", "ConnectCallback: onReceived: " + hexData + " on channel: " + channel);
            }

            @Override
            public void onStateChanged(int oldState, int newState) {
                /*
                 * onStateChanged is called when the SDK changes state.
                 */
                Bridge.SendChangeStateEventToUnity(newState);
            }

            @Override
            public void onSystemVolumeChanged(int oldVolume, int newVolume) {
                /*
                 * onSystemVolumeChanged is called when the system volume is changed.
                 */
            }

        };

        this.chirpConnect = new ChirpConnect(Ctx, key, secret);
        ChirpError error = this.chirpConnect.setConfig(config);
        if (error.getCode() == 0) {
            chirpConnect.setListener(connectEventListener);
        } else {
            Log.e("ChirpError: ", error.getMessage());
        }
        GrantPermissions();
    }

    @TargetApi(23)
    private static final int RESULT_REQUEST_RECORD_AUDIO = 1095;

    @TargetApi(23)
    private void GrantPermissions() {
        if (Ctx.checkSelfPermission(Manifest.permission.RECORD_AUDIO) != PackageManager.PERMISSION_GRANTED) {
            Act.requestPermissions(new String[] {Manifest.permission.RECORD_AUDIO}, RESULT_REQUEST_RECORD_AUDIO);
        }
    }

    int StartSDK() {
        if (this.chirpConnect == null) {
            return 404;
        }
        ChirpError error = this.chirpConnect.start();
        int errorCode = error.getCode();
        if (errorCode > 0) {
            return errorCode;
        }
        return 0;
    }

    int StopSDK() {
        if (this.chirpConnect == null) {
            return 404;
        }
        ChirpError error = this.chirpConnect.stop();
        int errorCode = error.getCode();
        if (errorCode > 0) {
            return errorCode;
        }
        return 0;
    }


    int sendPayload(String message) {
        /*
         * A payload is a byte array dynamic size with a maximum size defined by the config string.
         *
         */
        byte[] payload = message.getBytes();

        long maxSize = chirpConnect.maxPayloadLength();
        if (maxSize < payload.length) {
            return 84;
        }
        ChirpError error = chirpConnect.send(payload);
        int errorCode = error.getCode();
        if (errorCode > 0) {
            return errorCode;
        }
        return 0;
    }
}
