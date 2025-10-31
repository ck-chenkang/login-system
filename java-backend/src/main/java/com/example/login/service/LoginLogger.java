package com.example.login.service;

public interface LoginLogger {
    void ensureTable();
    void log(String username, boolean success, String ip, String userAgent);
}

