# Script para reemplazar ref por out en llamadas a TryParse y TryGetValue
$sourceFolder = "PowerBIModelingMCP.Library"
$files = Get-ChildItem -Path $sourceFolder -Filter "*.cs" -Recurse

$totalFixed = 0

Write-Host "Corrigiendo ref -> out en $($files.Count) archivos..." -ForegroundColor Cyan

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content

    # Reemplazar ", ref " por ", out " en contextos de TryParse y TryGetValue
    $content = $content -replace ',\s*ref\s+(\w+)\s*\)', ', out $1)'

    if ($content -ne $originalContent) {
        Set-Content $file.FullName -Value $content -Encoding UTF8 -NoNewline
        $totalFixed++
        Write-Host "  Corregido: $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "`nArchivos corregidos: $totalFixed" -ForegroundColor Yellow
Write-Host "Completado!" -ForegroundColor Green
