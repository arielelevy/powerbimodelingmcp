# Script para corregir llamadas explícitas a operadores
$sourceFolder = "PowerBIModelingMCP.Library"
$files = Get-ChildItem -Path $sourceFolder -Filter "*.cs" -Recurse

$totalFixed = 0

Write-Host "Corrigiendo llamadas a operadores en $($files.Count) archivos..." -ForegroundColor Cyan

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content

    # DateTime.operator != -> !=
    $content = $content -replace 'DateTime\.operator\s*!=\s*\(([^,]+),\s*([^)]+)\)', '($1 != $2)'

    # DateTime.operator == -> ==
    $content = $content -replace 'DateTime\.operator\s*==\s*\(([^,]+),\s*([^)]+)\)', '($1 == $2)'

    # int.operator != -> !=
    $content = $content -replace 'int\.operator\s*!=\s*\(([^,]+),\s*([^)]+)\)', '($1 != $2)'

    # int.operator == -> ==
    $content = $content -replace 'int\.operator\s*==\s*\(([^,]+),\s*([^)]+)\)', '($1 == $2)'

    # bool.operator != -> !=
    $content = $content -replace 'bool\.operator\s*!=\s*\(([^,]+),\s*([^)]+)\)', '($1 != $2)'

    # bool.operator == -> ==
    $content = $content -replace 'bool\.operator\s*==\s*\(([^,]+),\s*([^)]+)\)', '($1 == $2)'

    if ($content -ne $originalContent) {
        Set-Content $file.FullName -Value $content -Encoding UTF8 -NoNewline
        $totalFixed++
        Write-Host "  Corregido: $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "`nArchivos corregidos: $totalFixed" -ForegroundColor Yellow
Write-Host "Completado!" -ForegroundColor Green
