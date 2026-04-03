package com.intelliversex.sdk;

public class IVXSatori {
    private static IVXSatori instance;
    public static IVXSatori getInstance() {
        if (instance == null) instance = new IVXSatori();
        return instance;
    }
    public void postEvent(String eventName) {}
    public void getFlag(String flagName) {}
}