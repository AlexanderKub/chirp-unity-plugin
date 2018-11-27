package com.alexanderkub.plugins.unitychirpio;

import android.content.Context;
import android.util.Log;


import io.chirp.connect.ChirpConnect;
import io.chirp.connect.interfaces.ConnectEventListener;
import io.chirp.connect.interfaces.ConnectSetConfigListener;
import io.chirp.connect.models.ChirpError;
import io.chirp.connect.models.ConnectState;

public class ChirpConnector {
    private ChirpConnect chirpConnect;
    private Context Ctx;

    public ChirpConnector(final Context ctx) {
        this.Ctx = ctx;
    }

    protected void InitSDK(String key, String secret, String config) {
        if (this.chirpConnect != null) {
            this.chirpConnect.close();
            this.chirpConnect = null;
        }

        final ConnectEventListener connectEventListener = new ConnectEventListener() {
            @Override
            public void onSending(byte[] data, byte channel) {
                /**
                 * onSending is called when a send event begins.
                 * The data argument contains the payload being sent.
                 */
                String hexData = "null";
                if (data != null) {
                    hexData = chirpConnect.payloadToHexString(data);
                }
                Log.v("connectdemoapp", "ConnectCallback: onSending: " + hexData + " on channel: " + channel);
            }

            @Override
            public void onSent(byte[] data, byte channel) {
                /**
                 * onSent is called when a send event has completed.
                 * The data argument contains the payload that was sent.
                 */
                String hexData = "null";
                if (data != null) {
                    hexData = chirpConnect.payloadToHexString(data);
                }
                Bridge.SendSentEventToUnity(hexData);
                Log.v("connectdemoapp", "ConnectCallback: onSent: " + hexData + " on channel: " + channel);
            }

            @Override
            public void onReceiving(byte channel) {
                /**
                 * onReceiving is called when a receive event begins.
                 * No data has yet been received.
                 */
                Log.v("connectdemoapp", "ConnectCallback: onReceiving on channel: " + channel);
            }

            @Override
            public void onReceived(byte[] data, byte channel) {
                /**
                 * onReceived is called when a receive event has completed.
                 * If the payload was decoded successfully, it is passed in data.
                 * Otherwise, data is null.
                 */
                String hexData = "null";
                if (data != null) {
                    hexData = chirpConnect.payloadToHexString(data);
                }
                Bridge.SendReceiveEventToUnity(hexData);
                Log.v("connectdemoapp", "ConnectCallback: onReceived: " + hexData + " on channel: " + channel);
            }

            @Override
            public void onStateChanged(byte oldState, byte newState) {
                /**
                 * onStateChanged is called when the SDK changes state.
                 */
                ConnectState state = ConnectState.createConnectState(newState);
                Log.v("connectdemoapp", "ConnectCallback: onStateChanged " + oldState + " -> " + newState);
                Bridge.SendChangeStateEventToUnity(newState);
            }

            @Override
            public void onSystemVolumeChanged(int oldVolume, int newVolume) {
                /**
                 * onSystemVolumeChanged is called when the system volume is changed.
                 */
                Log.v("connectdemoapp", "System volume has been changed, notify user to increase the volume when sending data");
            }

        };

        this.chirpConnect = new ChirpConnect(Ctx, key, secret);
        this.chirpConnect.setConfig(config, new ConnectSetConfigListener() {
            @Override
            public void onSuccess() {
                //Set-up the connect callbacks
                chirpConnect.setListener(connectEventListener);
            }

            @Override
            public void onError(ChirpError setConfigError) {
                Log.e("setConfigError", setConfigError.getMessage());
            }
        });
    }

    public void StartSDK() {
        if (this.chirpConnect == null) {
            return;
        }
        this.chirpConnect.start();
    }

    public void StopSDK() {
        if (this.chirpConnect == null) {
            return;
        }
        this.chirpConnect.stop();
    }


    protected void sendPayload(int length, String message) {
        /**
         * A payload is a byte array dynamic size with a maximum size defined by the config string.
         *
         */
        byte[] payload = message.getBytes();

        long maxSize = chirpConnect.getMaxPayloadLength();
        if (maxSize < payload.length) {
            Log.e("ConnectError: ", "Invalid Payload");
            return;
        }
        ChirpError error = chirpConnect.send(payload);
        if (error.getCode() > 0) {
            Log.e("ConnectError: ", error.getMessage());
        }
    }
}
