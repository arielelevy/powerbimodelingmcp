# Script to fix remaining operator issues
$sourceFolder = "PowerBIModelingMCP.Library"
$files = Get-ChildItem -Path $sourceFolder -Filter "*.cs" -Recurse

$totalFixed = 0

Write-Host "Fixing remaining operator issues in $($files.Count) files..." -ForegroundColor Cyan

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content

    # DateTime.op_Equality -> ==
    $content = $content -replace 'DateTime\.op_Equality\s*\(([^,]+),\s*([^)]+)\)', '($1 == $2)'

    # DateTime.op_GreaterThanOrEqual -> >=
    $content = $content -replace 'DateTime\.op_GreaterThanOrEqual\s*\(([^,]+),\s*([^)]+)\)', '($1 >= $2)'

    # DateTime.op_GreaterThan -> >
    $content = $content -replace 'DateTime\.op_GreaterThan\s*\(([^,]+),\s*([^)]+)\)', '($1 > $2)'

    # CultureTypes bitwise OR with int
    $content = $content -replace '(\w+\.CultureTypes)\s*\|\s*(\d+)', '$1 | (CultureTypes)$2'

    # CultureTypes bitwise AND with int
    $content = $content -replace '(\w+\.CultureTypes)\s*&\s*(\d+)', '$1 & (CultureTypes)$2'

    if ($content -ne $originalContent) {
        Set-Content $file.FullName -Value $content -Encoding UTF8 -NoNewline
        $totalFixed++
        Write-Host "  Fixed: $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "`nFiles fixed: $totalFixed" -ForegroundColor Yellow
Write-Host "Complete!" -ForegroundColor Green
