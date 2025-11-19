# Script para corregir parámetros out no inicializados
param(
    [string]$SourceFolder = "PowerBIModelingMCP.Library"
)

$files = Get-ChildItem -Path $SourceFolder -Filter "*.cs" -Recurse
$totalFixed = 0

Write-Host "Corrigiendo parámetros out en $($files.Count) archivos..." -ForegroundColor Cyan

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content
    $fileFixed = $false

    # Patrón 1: TryParse sin out keyword
    # Buscar patrones como: Type.TryParse(value, variable) y cambiar a Type.TryParse(value, out variable)
    if ($content -match '\.TryParse\s*\([^,]+,\s*(?!out\s+)(\w+)\s*\)') {
        # DataType.TryParse
        $content = $content -replace '(DataType\.TryParse\s*\([^,]+),\s*(?!out\s+)(\w+)\s*\)', '$1, out $2)'

        # Enum.TryParse<T>
        $content = $content -replace '(Enum\.TryParse<[^>]+>\s*\([^,]+),\s*(?!out\s+)(\w+)\s*\)', '$1, out $2)'

        # AggregateFunction.TryParse
        $content = $content -replace '(AggregateFunction\.TryParse\s*\([^,]+),\s*(?!out\s+)(\w+)\s*\)', '$1, out $2)'

        # Alignment.TryParse
        $content = $content -replace '(Alignment\.TryParse\s*\([^,]+),\s*(?!out\s+)(\w+)\s*\)', '$1, out $2)'

        # RefreshType.TryParse
        $content = $content -replace '(RefreshType\.TryParse\s*\([^,]+),\s*(?!out\s+)(\w+)\s*\)', '$1, out $2)'

        # CalendarOperationType.TryParse
        $content = $content -replace '(CalendarOperationType\.TryParse\s*\([^,]+),\s*(?!out\s+)(\w+)\s*\)', '$1, out $2)'

        $fileFixed = $true
    }

    # Patrón 2: Dictionary.TryGetValue sin out keyword
    if ($content -match '\.TryGetValue\s*\([^,]+,\s*(?!out\s+)(\w+)\s*\)') {
        $content = $content -replace '(\.TryGetValue\s*\([^,]+),\s*(?!out\s+)(\w+)\s*\)', '$1, out $2)'
        $fileFixed = $true
    }

    if ($content -ne $originalContent) {
        Set-Content $file.FullName -Value $content -Encoding UTF8 -NoNewline
        $totalFixed++
        Write-Host "  Corregido: $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "`nArchivos corregidos: $totalFixed" -ForegroundColor Yellow
Write-Host "Completado!" -ForegroundColor Green
