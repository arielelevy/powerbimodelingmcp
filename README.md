# Power BI Modeling MCP Server

Un servidor MCP (Model Context Protocol) para trabajar con modelos semánticos de Power BI.

## Descripción

Este servidor MCP proporciona herramientas para interactuar con modelos semánticos de Power BI a través del protocolo MCP. Permite realizar operaciones de lectura y escritura en modelos tabulares, consultas DAX, gestión de tablas, columnas, medidas y más.

## Características

- ✅ **28 herramientas registradas** para operaciones con Power BI
- ✅ **Soporte para operaciones por lotes** (batch operations)
- ✅ **Consultas DAX** integradas
- ✅ **Gestión de traducciones** y culturas
- ✅ **Operaciones transaccionales**
- ✅ **Modo de solo lectura** y lectura-escritura
- ✅ **Compatibilidad con Power BI** y Analysis Services

## Herramientas Disponibles

### Operaciones de Base de Datos
- DatabaseOperationsTool
- ConnectionOperationsTool

### Operaciones de Tablas
- TableOperationsTool
- BatchTableOperationsTool

### Operaciones de Columnas
- ColumnOperationsTool
- BatchColumnOperationsTool

### Operaciones de Medidas
- MeasureOperationsTool
- BatchMeasureOperationsTool

### Otras Herramientas
- DaxQueryOperationsTool
- RelationshipOperationsTool
- PerspectiveOperationsTool
- CalculationGroupOperationsTool
- CalendarOperationsTool
- ObjectTranslationOperationsTool
- TransactionOperationsTool
- TraceOperationsTool
- Y más...

## Requisitos

- .NET 8.0 o superior
- Windows (para compatibilidad con Analysis Services)

## Instalación

### Compilar desde el código fuente

```bash
cd src
dotnet build PowerBIModelingMCP.sln
```

### Ejecutar el servidor

```bash
cd src/powerbi-modeling-mcp/bin/Debug/net8.0/win-x64
./powerbi-modeling-mcp.exe --start
```

## Uso

### Opciones de línea de comandos

```
--start                      Iniciar el servidor MCP (modo ReadWrite por defecto)
--read-only, --readonly      Iniciar en modo solo lectura
--read-write, --readwrite    Iniciar en modo lectura-escritura
--skip-confirmation          Omitir confirmaciones para operaciones de escritura
--compatibility=powerbi      Compatibilidad solo con PowerBI (por defecto)
--compatibility=full         Compatibilidad completa (PowerBI + Analysis Services)
--http, --sse                Iniciar en modo HTTP/SSE (no implementado aún)
--port=<puerto>, -p=<puerto> Especificar puerto HTTP (por defecto: 5000)
--help, -h                   Mostrar ayuda
```

### Ejemplos

```bash
# Iniciar en modo por defecto
powerbi-modeling-mcp --start

# Iniciar en modo solo lectura
powerbi-modeling-mcp --readonly

# Iniciar en modo lectura-escritura sin confirmaciones
powerbi-modeling-mcp --readwrite --skip-confirmation

# Iniciar con compatibilidad completa
powerbi-modeling-mcp --start --compatibility=full
```

## Arquitectura

El proyecto está organizado en dos componentes principales:

### powerbi-modeling-mcp (Ejecutable)
- Punto de entrada del servidor
- Configuración y registro de servicios
- Gestión del transporte MCP (STDIO)

### PowerBIModelingMCP.Library (Biblioteca)
- Core operations (operaciones principales)
- Tools (herramientas MCP)
- Prompts (plantillas de prompts)
- Services (servicios auxiliares)
- Common (estructuras de datos y utilidades)

## Dependencias Principales

- **ModelContextProtocol**: SDK de MCP
- **Microsoft.AnalysisServices.Tabular**: Interacción con modelos tabulares
- **Azure.Identity**: Autenticación Azure
- **Microsoft.Extensions.Hosting**: Hosting de servicios
- **YamlDotNet**: Parsing de archivos YAML/TMDL

## Scripts de Desarrollo

El directorio `src` incluye varios scripts PowerShell útiles para desarrollo:

- `fix-decompiled-code.ps1`: Corrección de código decompilado
- `fix-ref-to-out.ps1`: Corrección de parámetros ref/out
- `fix-datetime-operators.ps1`: Corrección de operadores DateTime
- `fix-required-members.ps1`: Corrección de miembros requeridos
- `fix-getvalueordefault.ps1`: Corrección de llamadas GetValueOrDefault
- `fix-remaining-operators.ps1`: Corrección de operadores restantes

## Licencia

Este proyecto fue reconstruido desde código decompilado con fines educativos y de desarrollo.

## Contribuir

Las contribuciones son bienvenidas. Por favor:

1. Fork el repositorio
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## Soporte

Para reportar problemas o solicitar características, por favor abre un issue en GitHub.
