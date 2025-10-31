package com.example.login.controller;

import com.example.login.service.LoginLogger;
import io.jsonwebtoken.Jwts;
import io.jsonwebtoken.SignatureAlgorithm;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.ResponseEntity;
import org.springframework.validation.annotation.Validated;
import org.springframework.web.bind.annotation.*;

import javax.crypto.SecretKey;
import javax.crypto.spec.SecretKeySpec;
import javax.validation.constraints.NotBlank;
import java.nio.charset.StandardCharsets;
import java.time.Instant;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;

@RestController
@RequestMapping("/api/auth")
@Validated
public class AuthController {

    private final LoginLogger loginLogger;

    @Value("${jwt.key}")
    private String jwtKey;
    @Value("${jwt.issuer}")
    private String issuer;
    @Value("${jwt.audience}")
    private String audience;
    @Value("${jwt.expiresHours:2}")
    private int expiresHours;

    public AuthController(LoginLogger loginLogger) {
        this.loginLogger = loginLogger;
    }

    public static class LoginRequest {
        @NotBlank
        public String username;
        @NotBlank
        public String password;
    }

    @PostMapping("/login")
    public ResponseEntity<?> login(@RequestBody LoginRequest req, @RequestHeader(value = "User-Agent", required = false) String userAgent, @RequestHeader(value = "X-Forwarded-For", required = false) String xff, javax.servlet.http.HttpServletRequest httpReq) {
        String username = req.username == null ? "" : req.username.trim();
        String password = req.password == null ? "" : req.password;

        String ip = xff != null && !xff.isEmpty() ? xff.split(",")[0].trim() : (httpReq.getRemoteAddr());
        String ua = userAgent;

        boolean success = "admin".equals(username) && "admin".equals(password);
        try {
            loginLogger.log(username, success, ip, ua);
        } catch (Exception ignored) { }

        if (!success) {
            Map<String, Object> body = new HashMap<>();
            body.put("message", "用户名或密码错误");
            return ResponseEntity.status(401).body(body);
        }

        String token = generateJwtToken(username);
        return ResponseEntity.ok(Map.of("token", token));
    }

    private String generateJwtToken(String username) {
        SecretKey key = new SecretKeySpec(jwtKey.getBytes(StandardCharsets.UTF_8), "HmacSHA256");
        Instant now = Instant.now();
        Date expiry = Date.from(now.plusSeconds(expiresHours * 3600L));
        return Jwts.builder()
                .setSubject(username)
                .setId(java.util.UUID.randomUUID().toString())
                .setIssuer(issuer)
                .setAudience(audience)
                .setIssuedAt(Date.from(now))
                .setExpiration(expiry)
                .signWith(key, SignatureAlgorithm.HS256)
                .compact();
    }
}

