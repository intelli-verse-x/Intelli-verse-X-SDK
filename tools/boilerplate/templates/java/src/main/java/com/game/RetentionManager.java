package com.game;

import android.content.Context;
import android.content.SharedPreferences;
import com.intelliversex.sdk.IVXSatori;

public class RetentionManager {
    private static final String PREFS = "retention_prefs";
    public static void checkDailyRetention(Context context) {
        SharedPreferences prefs = context.getSharedPreferences(PREFS, Context.MODE_PRIVATE);
        long lastLogin = prefs.getLong("last_login", 0);
        long now = System.currentTimeMillis();
        
        IVXSatori.getInstance().postEvent("app_open");
        
        prefs.edit().putLong("last_login", now).apply();
    }
}