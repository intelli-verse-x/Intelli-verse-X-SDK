package com.intelliversex.sdk;

public class IVXClient {
    private static IVXClient instance;
    public static IVXClient getInstance() { 
        if (instance == null) instance = new IVXClient();
        return instance; 
    }
    public void initialize() {}
}