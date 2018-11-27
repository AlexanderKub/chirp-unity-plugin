package com.alexanderkub.plugins.unitychirpio;

public interface ChirpPluginJavaMessageHandler {
    void OnChangeStateHandler(byte state);
    void OnReceiveDataHandler(String message);
    void OnSentDataHandler(String message);
}
