package com.example.login.service;

import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.stereotype.Service;

import javax.annotation.PostConstruct;

@Service
public class MySqlLoginLogger implements LoginLogger {

    private final JdbcTemplate jdbcTemplate;

    public MySqlLoginLogger(JdbcTemplate jdbcTemplate) {
        this.jdbcTemplate = jdbcTemplate;
    }

    @PostConstruct
    public void init() {
        ensureTable();
    }

    @Override
    public void ensureTable() {
        String sql = "CREATE TABLE IF NOT EXISTS login_logs (" +
                "id BIGINT PRIMARY KEY AUTO_INCREMENT, " +
                "username VARCHAR(255) NOT NULL, " +
                "success TINYINT(1) NOT NULL, " +
                "ip VARCHAR(64) NULL, " +
                "user_agent VARCHAR(512) NULL, " +
                "created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP) " +
                "ENGINE=InnoDB DEFAULT CHARSET=utf8mb4";
        jdbcTemplate.execute(sql);
    }

    @Override
    public void log(String username, boolean success, String ip, String userAgent) {
        String insert = "INSERT INTO login_logs (username, success, ip, user_agent) VALUES (?,?,?,?)";
        jdbcTemplate.update(insert, username, success ? 1 : 0, ip, userAgent);
    }
}

