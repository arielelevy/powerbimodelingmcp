# HTTP/SSE Server Setup with OAuth Authentication

## Overview

El servidor Power BI Modeling MCP ahora soporta HTTP/SSE con autenticación OAuth JWT. Esto permite acceder al servidor a través de endpoints HTTP REST y Server-Sent Events (SSE) en lugar de solo STDIO.

## Inicio Rápido

### Ejecutar el servidor en modo HTTP/SSE

```bash
powerbi-modeling-mcp.exe --http --port=8004 --readwrite
```

### Parámetros disponibles

- `--http` o `--sse`: Activa el modo HTTP/SSE
- `--port=<puerto>` o `-p=<puerto>`: Especifica el puerto (default: 5000)
- `--readwrite`: Modo lectura-escritura
- `--readonly`: Modo solo lectura
- `--skip-confirmation`: Omite confirmaciones para operaciones de escritura

## Endpoints Disponibles

### 1. Health Check (Sin autenticación)
```bash
GET http://localhost:8004/health
```

Respuesta:
```json
{
  "status": "healthy",
  "mode": "ReadWrite"
}
```

### 2. Server-Sent Events (SSE) - Con autenticación
```bash
GET http://localhost:8004/sse
```

Headers:
```
Authorization: Bearer <your_jwt_token>
```

O mediante query parameter:
```bash
GET http://localhost:8004/sse?access_token=<your_jwt_token>
```

### 3. MCP HTTP POST - Con autenticación
```bash
POST http://localhost:8004/mcp
```

Headers:
```
Authorization: Bearer <your_jwt_token>
Content-Type: application/json
```

Body: MCP protocol JSON request

## Autenticación OAuth/JWT

### Configuración de Variables de Entorno

El servidor usa las siguientes variables de entorno para la configuración JWT:

```bash
# Clave secreta (mínimo 32 caracteres)
set MCP_JWT_SECRET=your-secret-key-min-32-chars-long-12345

# Emisor del token (issuer)
set MCP_JWT_ISSUER=powerbi-mcp-server

# Audiencia del token (audience)
set MCP_JWT_AUDIENCE=powerbi-mcp-client
```

**Valores por defecto:**
- Secret: `your-secret-key-min-32-chars-long-12345`
- Issuer: `powerbi-mcp-server`
- Audience: `powerbi-mcp-client`

### Generar un Token JWT

Puedes generar un token JWT usando cualquier librería JWT. Ejemplo con Python:

```python
import jwt
import datetime

secret = "your-secret-key-min-32-chars-long-12345"
payload = {
    "iss": "powerbi-mcp-server",
    "aud": "powerbi-mcp-client",
    "exp": datetime.datetime.utcnow() + datetime.timedelta(hours=24),
    "iat": datetime.datetime.utcnow(),
    "sub": "user@example.com"
}

token = jwt.encode(payload, secret, algorithm="HS256")
print(token)
```

O con Node.js:

```javascript
const jwt = require('jsonwebtoken');

const secret = 'your-secret-key-min-32-chars-long-12345';
const payload = {
  iss: 'powerbi-mcp-server',
  aud: 'powerbi-mcp-client',
  exp: Math.floor(Date.now() / 1000) + (24 * 60 * 60), // 24 horas
  iat: Math.floor(Date.now() / 1000),
  sub: 'user@example.com'
};

const token = jwt.sign(payload, secret);
console.log(token);
```

## Ejemplos de Uso

### Ejemplo con curl - Health Check

```bash
curl http://localhost:8004/health
```

### Ejemplo con curl - SSE Stream con token

```bash
curl -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -N http://localhost:8004/sse
```

### Ejemplo con curl - MCP Request

```bash
curl -X POST http://localhost:8004/mcp \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -H "Content-Type: application/json" \
     -d '{
       "jsonrpc": "2.0",
       "method": "tools/list",
       "id": 1
     }'
```

### Ejemplo con JavaScript (SSE)

```javascript
const token = 'YOUR_JWT_TOKEN';
const eventSource = new EventSource(`http://localhost:8004/sse?access_token=${token}`);

eventSource.onmessage = (event) => {
  console.log('Message:', event.data);
};

eventSource.onerror = (error) => {
  console.error('Error:', error);
};
```

### Ejemplo con Python (requests)

```python
import requests

token = "YOUR_JWT_TOKEN"
headers = {
    "Authorization": f"Bearer {token}",
    "Content-Type": "application/json"
}

# Health check
response = requests.get("http://localhost:8004/health")
print(response.json())

# MCP request
mcp_request = {
    "jsonrpc": "2.0",
    "method": "tools/list",
    "id": 1
}

response = requests.post(
    "http://localhost:8004/mcp",
    headers=headers,
    json=mcp_request
)
print(response.json())
```

## CORS

El servidor está configurado con CORS habilitado para permitir peticiones desde cualquier origen. En producción, se recomienda configurar orígenes específicos.

## Seguridad

### Recomendaciones de Producción:

1. **Cambiar la clave secreta**: NO uses la clave por defecto en producción
2. **Usar HTTPS**: Implementa SSL/TLS para conexiones seguras
3. **Configurar CORS**: Limita los orígenes permitidos
4. **Tokens de corta duración**: Usa tiempos de expiración apropiados
5. **Rotar claves**: Implementa rotación regular de claves JWT
6. **Variables de entorno**: Almacena la configuración de forma segura

### Ejemplo de configuración segura:

```bash
# Generar una clave segura (ejemplo con PowerShell)
$bytes = New-Object byte[] 32
[Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($bytes)
$key = [Convert]::ToBase64String($bytes)
echo $key

# Configurar las variables de entorno
set MCP_JWT_SECRET=%key%
set MCP_JWT_ISSUER=production-powerbi-mcp
set MCP_JWT_AUDIENCE=production-client
```

## Logs

El servidor registra todos los eventos importantes:

- Inicio del servidor
- Configuración OAuth
- Requests autenticados
- Errores de autenticación
- Estado de las herramientas MCP

Los logs se muestran en la salida estándar (stdout/stderr).

## Troubleshooting

### Error 401 Unauthorized

- Verifica que el token JWT sea válido
- Confirma que el issuer y audience coincidan con la configuración del servidor
- Verifica que el token no haya expirado

### El servidor no inicia

- Verifica que el puerto no esté en uso
- Confirma que tengas permisos para escuchar en el puerto
- Revisa los logs para errores específicos

### SSE se desconecta

- El servidor envía heartbeats cada 30 segundos
- Verifica la estabilidad de la conexión de red
- Confirma que el token siga válido durante toda la sesión

## Diferencias con STDIO Mode

- **STDIO**: Comunicación a través de entrada/salida estándar (pipes)
- **HTTP/SSE**: Comunicación a través de HTTP REST y Server-Sent Events
- **Autenticación**: HTTP/SSE requiere OAuth JWT, STDIO no

Ambos modos soportan las mismas herramientas MCP y funcionalidades.
