package com.intelliversex.sdk;

public class IVXWallet {
    private static IVXWallet instance;
    public static IVXWallet getInstance() {
        if (instance == null) instance = new IVXWallet();
        return instance;
    }
    public int getCoins() { return 100; }
    public int getGems() { return 50; }
}