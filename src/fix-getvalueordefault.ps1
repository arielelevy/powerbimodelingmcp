# Script to fix GetValueOrDefault() calls at the end of ?? chains
$sourceFolder = "PowerBIModelingMCP.Library\Tools"
$files = Get-ChildItem -Path $sourceFolder -Filter "Batch*.cs"

$totalFixed = 0

Write-Host "Fixing GetValueOrDefault() calls in $($files.Count) files..." -ForegroundColor Cyan

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content

    # Replace .GetValueOrDefault() with ?? 0 at the end of ?? chains
    $content = $content -replace '\.GetValueOrDefault\(\)', ' ?? 0'

    if ($content -ne $originalContent) {
        Set-Content $file.FullName -Value $content -Encoding UTF8 -NoNewline
        $totalFixed++
        Write-Host "  Fixed: $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "`nFiles fixed: $totalFixed" -ForegroundColor Yellow
Write-Host "Complete!" -ForegroundColor Green
