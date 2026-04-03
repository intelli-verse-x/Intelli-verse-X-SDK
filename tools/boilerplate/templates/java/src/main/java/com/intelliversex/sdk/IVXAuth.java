package com.intelliversex.sdk;

public class IVXAuth {
    private static IVXAuth instance;
    public static IVXAuth getInstance() {
        if (instance == null) instance = new IVXAuth();
        return instance;
    }
    public void login(String deviceId, AuthCallback callback) {
        if (callback != null) callback.onResult(true);
    }
    public interface AuthCallback { void onResult(boolean success); }
}