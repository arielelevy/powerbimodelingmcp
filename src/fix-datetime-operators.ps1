# Script to fix DateTime.operator - calls
$sourceFolder = "PowerBIModelingMCP.Library"
$files = Get-ChildItem -Path $sourceFolder -Filter "*.cs" -Recurse

$totalFixed = 0

Write-Host "Fixing DateTime operator calls in $($files.Count) files..." -ForegroundColor Cyan

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content

    # DateTime.op_Subtraction -> subtraction
    $content = $content -replace 'DateTime\.op_Subtraction\s*\(([^,]+),\s*([^)]+)\)', '($1 - $2)'

    if ($content -ne $originalContent) {
        Set-Content $file.FullName -Value $content -Encoding UTF8 -NoNewline
        $totalFixed++
        Write-Host "  Fixed: $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "`nFiles fixed: $totalFixed" -ForegroundColor Yellow
Write-Host "Complete!" -ForegroundColor Green
