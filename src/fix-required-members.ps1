# Script to fix CS9035 errors - Required member initialization
$sourceFolder = "PowerBIModelingMCP.Library"
$files = Get-ChildItem -Path $sourceFolder -Filter "*.cs" -Recurse

$totalFixed = 0
$patterns = @(
    # Pattern for CultureCreate
    @{
        Old = '(?s)(CultureCreate\s+\w+\s*=\s*new\s+CultureCreate\(\);)\s+(\w+\.Name\s*=\s*[^;]+;)'
        New = {
            param($match)
            $varName = if ($match.Groups[1].Value -match '(\w+)\s*=') { $matches[1] } else { 'obj' }
            $assignment = $match.Groups[2].Value -replace "$varName\.", ''
            "CultureCreate $varName = new CultureCreate { $assignment }"
        }
    },
    # Pattern for ObjectTranslationGet with multiple properties
    @{
        Old = '(?s)(ObjectTranslationGet\s+\w+\s*=\s*new\s+ObjectTranslationGet\(\);)\s+((?:\w+\.\w+\s*=\s*[^;]+;\s*)+)'
        New = {
            param($match)
            $varName = if ($match.Groups[1].Value -match '(\w+)\s*=') { $matches[1] } else { 'obj' }
            $assignments = $match.Groups[2].Value -split ';\s*' | Where-Object { $_ -match '\S' }
            $properties = $assignments | ForEach-Object {
                $_ -replace "$varName\.", '' -replace '^\s+', ''
            }
            "ObjectTranslationGet $varName = new ObjectTranslationGet`n            {`n                " +
            ($properties -join ",`n                ") + "`n            };"
        }
    },
    # Generic pattern for single property initialization
    @{
        Old = '(?m)^(\s*)(\w+)\s+(\w+)\s*=\s*new\s+\2\(\);\s*\r?\n\s*\3\.(\w+)\s*=\s*([^;]+);'
        New = '$1$2 $3 = new $2 { $4 = $5 };'
    }
)

Write-Host "Fixing CS9035 errors in $($files.Count) files..." -ForegroundColor Cyan

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content

    # Apply all patterns
    foreach ($pattern in $patterns) {
        if ($pattern.New -is [scriptblock]) {
            $content = [regex]::Replace($content, $pattern.Old, $pattern.New)
        } else {
            $content = $content -replace $pattern.Old, $pattern.New
        }
    }

    if ($content -ne $originalContent) {
        Set-Content $file.FullName -Value $content -Encoding UTF8 -NoNewline
        $totalFixed++
        Write-Host "  Fixed: $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "`nFiles fixed: $totalFixed" -ForegroundColor Yellow
Write-Host "Complete!" -ForegroundColor Green
