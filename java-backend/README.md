Java 17 Spring Boot backend (mirrors .NET sample)

Endpoints
- POST /api/auth/login — returns JWT if username=admin and password=admin; logs attempts to MySQL
- GET  /weatherforecast — sample data
- Swagger UI: /swagger-ui.html

Config
- Edit src/main/resources/application.yml
  - spring.datasource.url/username/password for MySQL
  - jwt.key/issuer/audience/expiresHours

Run
1) Ensure JDK 17 and Maven are installed (java -version, mvn -v)
2) From java-backend folder: mvn spring-boot:run
3) Open http://localhost:5000/swagger-ui.html

Notes
- CORS: allow all (dev convenience)
- Security: JWT filter protects endpoints except /api/auth/login and swagger
