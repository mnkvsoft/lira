Write-Host "Begin..." -ForegroundColor Yellow

$path = $PSScriptRoot
Write-Host "Current path: $path"

$templates = Get-ChildItem -Path $path -Filter "*.template.md" -Recurse -File
$regex = [regex]'\[([^\]]*#?[^\]]*)\]\(([^)]+)\)'

Write-Host "Found templates: $templates"

foreach ($template in $templates) {
    $dir = $template.DirectoryName
    if ($null -eq $dir) {
        throw "Directory is null"
    }

    $content = Get-Content -Path $template.FullName -Raw
    $newContent = $content;

    $uniqueMatches = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($match in $regex.Matches($content)) {
       $null = $uniqueMatches.Add($match.Value)
    }

    $counter = 0
    foreach ($match in $uniqueMatches) {
        $splitted = $match.Split("](");

        $linkPath = $splitted[-1].Replace(")", "")
        $linkName = $splitted[0].Replace("[", "")

        if ($linkPath -notmatch "($(@(".rules", ".cs", ".json", ".declare", ".dic") -join '|'))$") {
           continue
        }

        if($linkName.EndsWith("#ignore")){
            $newLinkName = $linkName.Replace("#ignore", "");
            $newLink = "[$newLinkName]($linkPath)"

            $newContent = $newContent.Replace($match, $newLink)
            continue;
        }

        $blockType = ""
        if($linkPath.EndsWith(".cs")){
           $blockType = "cs"
        }

        $linkPathFull = Join-Path $dir $linkPath
        $linkContent = Get-Content -Path $linkPathFull -Raw

        $nl = "`r`n"
        $newContentPart = $match + $nl + $nl + "``````" + $blockType + $nl + $linkContent + $nl + "``````"

        $newContent = $newContent.Replace($match, $newContentPart)
        $counter++
    }

    if ($counter -gt 0) {
        $newFile = Join-Path $dir $($template.Name).Replace(".template.md", ".md")
        $newContent | Set-Content -Path $newFile

        Write-Host "File $newFile was writed"
    }
}