package com.intelliversex.sdk;

public class IVXHiro {
    private static IVXHiro instance;
    public static IVXHiro getInstance() {
        if (instance == null) instance = new IVXHiro();
        return instance;
    }
    public void getAchievements() {}
    public void getStore() {}
    public void getDailyRewards() {}
}