# Script para corregir código descompilado automáticamente
# Este script corrige patrones comunes de errores de descompilación

$sourceFolder = "PowerBIModelingMCP.Library"
$files = Get-ChildItem -Path $sourceFolder -Filter "*.cs" -Recurse

$totalFiles = $files.Count
$processedFiles = 0
$modifiedFiles = 0

Write-Host "Iniciando corrección de $totalFiles archivos..." -ForegroundColor Cyan

foreach ($file in $files) {
    $processedFiles++
    $modified = $false
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content

    # 1. Reemplazar string.op_Equality con ==
    if ($content -match 'string\.op_Equality') {
        $content = $content -replace 'string\.op_Equality\s*\(\s*([^,]+)\s*,\s*([^)]+)\s*\)', '($1 == $2)'
        $modified = $true
    }

    # 2. Reemplazar string.op_Inequality con !=
    if ($content -match 'string\.op_Inequality') {
        $content = $content -replace 'string\.op_Inequality\s*\(\s*([^,]+)\s*,\s*([^)]+)\s*\)', '($1 != $2)'
        $modified = $true
    }

    # 3. Reemplazar PropertyInfo.op_Inequality con !=
    if ($content -match 'PropertyInfo\.op_Inequality') {
        $content = $content -replace 'PropertyInfo\.op_Inequality\s*\(\s*([^,]+)\s*,\s*\(PropertyInfo\)\s*null\s*\)', '($1 != null)'
        $modified = $true
    }

    # 4. Reemplazar PropertyInfo.op_Equality con ==
    if ($content -match 'PropertyInfo\.op_Equality') {
        $content = $content -replace 'PropertyInfo\.op_Equality\s*\(\s*([^,]+)\s*,\s*\(PropertyInfo\)\s*null\s*\)', '($1 == null)'
        $modified = $true
    }

    # 5. Reemplazar (IEnumerable<T>) casts innecesarios en LINQ
    $content = $content -replace '\(IEnumerable<([^>]+)>\)\s*Enumerable\.', 'Enumerable.'

    # 6. Reemplazar (Func<...>) casts innecesarios en LINQ
    $content = $content -replace '\(Func<[^>]+>\)\s*\(', '('

    # 7. Limpiar casts de StringComparison
    $content = $content -replace '\(StringComparison\)\s*(\d+)', 'StringComparison.OrdinalIgnoreCase'

    # 8. Limpiar casts de SearchOption
    $content = $content -replace '\(SearchOption\)\s*1', 'SearchOption.AllDirectories'
    $content = $content -replace '\(SearchOption\)\s*0', 'SearchOption.TopDirectoryOnly'

    # 9. Simplificar expresiones LINQ complejas con Enumerable.
    # Esto es más complejo, por ahora lo dejamos

    if ($content -ne $originalContent) {
        Set-Content $file.FullName -Value $content -Encoding UTF8 -NoNewline
        $modifiedFiles++
        Write-Host "  [$processedFiles/$totalFiles] Modificado: $($file.Name)" -ForegroundColor Green
    } else {
        if ($processedFiles % 10 -eq 0) {
            Write-Host "  [$processedFiles/$totalFiles] Procesando..." -ForegroundColor Gray
        }
    }
}

Write-Host "`n=== Resumen ===" -ForegroundColor Cyan
Write-Host "Archivos procesados: $totalFiles" -ForegroundColor White
Write-Host "Archivos modificados: $modifiedFiles" -ForegroundColor Yellow
Write-Host "Corrección completada!" -ForegroundColor Green
