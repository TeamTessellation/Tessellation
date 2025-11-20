# 사용자별 코드 줄수 변경 분석 스크립트
# 바이너리 파일 제외

# UTF-8 인코딩 설정
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
chcp 65001 | Out-Null

# 사용자 이름 매핑 (같은 사람을 하나로 통합)
$authorMapping = @{
    "machamy" = "macham"
    "KimSeungWoo" = "macham"
    "자반자바" = "catsnakedog"
}

$excludePatterns = @(
    "*.wav", "*.mp3", "*.ogg",           # Audio
    "*.png", "*.jpg", "*.jpeg", "*.gif", # Images
    "*.asset", "*.prefab", "*.unity",    # Unity Binary
    "*.mat", "*.controller", "*.anim",   # Unity Assets
    "*.fbx", "*.obj", "*.blend",         # 3D Models
    "*.ttf", "*.otf",                    # Fonts
    "*.psd", "*.ai",                     # Design files
    "*.dll", "*.exe", "*.so",            # Binaries
    "*.meta",                            # Unity Meta files
    "*.shadergraph",                     # Unity Shader Graph files
    "*.shadersubgraph",                  # Unity Shader Subgraph files
    "*.df",                              # Database files
    "*.json"                             # JSON files
)

# Git 명령어로 데이터 수집
$gitCommand = "git log --all --numstat --pretty=format:`"COMMIT:%an`""
foreach ($pattern in $excludePatterns) {
    $gitCommand += " `":(exclude)$pattern`""
}
# Sample 폴더 제외
$gitCommand += " `":(exclude)**/Samples/**`""
$gitCommand += " `":(exclude)**/Sample/**`""

Write-Host "데이터 수집 중..." -ForegroundColor Cyan
$output = Invoke-Expression $gitCommand

# 데이터 파싱
$stats = @{}
$currentAuthor = ""

foreach ($line in $output -split "`n") {
    if ($line -match "^COMMIT:(.+)$") {
        $currentAuthor = $Matches[1]
        
        # 사용자 이름 매핑 적용
        if ($authorMapping.ContainsKey($currentAuthor)) {
            $currentAuthor = $authorMapping[$currentAuthor]
        }
        
        if (-not $stats.ContainsKey($currentAuthor)) {
            $stats[$currentAuthor] = @{
                Added = 0
                Removed = 0
                Commits = 0
            }
        }
        $stats[$currentAuthor].Commits++
    }
    elseif ($line -match "^(\d+)\s+(\d+)\s+(.+)$") {
        if ($currentAuthor -ne "") {
            $added = [int]$Matches[1]
            $removed = [int]$Matches[2]
            $file = $Matches[3]
            
            # .meta 파일은 이미 Git 명령어에서 제외됨
            $stats[$currentAuthor].Added += $added
            $stats[$currentAuthor].Removed += $removed
        }
    }
}

# 결과 출력
Write-Host "`n========================================" -ForegroundColor Green
Write-Host "   Code Contribution Analysis (Binary Excluded)" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green

$sortedStats = $stats.GetEnumerator() | Sort-Object { $_.Value.Added + $_.Value.Removed } -Descending

$totalAdded = 0
$totalRemoved = 0
$totalCommits = 0

foreach ($entry in $sortedStats) {
    $author = $entry.Key
    $data = $entry.Value
    $net = $data.Added - $data.Removed
    $netSign = if ($net -ge 0) { "+" } else { "" }
    
    $totalAdded += $data.Added
    $totalRemoved += $data.Removed
    $totalCommits += $data.Commits
    
    Write-Host "Author: " -NoNewline -ForegroundColor Yellow
    Write-Host $author -ForegroundColor White
    Write-Host "  Commits:   " -NoNewline
    Write-Host ("{0,6}" -f $data.Commits) -ForegroundColor Cyan
    Write-Host "  Added:     " -NoNewline
    Write-Host ("+{0,6}" -f $data.Added) -ForegroundColor Green
    Write-Host "  Removed:   " -NoNewline
    Write-Host ("-{0,6}" -f $data.Removed) -ForegroundColor Red
    Write-Host "  Net:       " -NoNewline
    Write-Host ("{0}{1,6}" -f $netSign, $net) -ForegroundColor $(if ($net -ge 0) { "Green" } else { "Red" })
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Green
Write-Host "Total Statistics" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Total Commits: " -NoNewline
Write-Host ("{0,6}" -f $totalCommits) -ForegroundColor Cyan
Write-Host "  Total Added:   " -NoNewline
Write-Host ("+{0,6}" -f $totalAdded) -ForegroundColor Green
Write-Host "  Total Removed: " -NoNewline
Write-Host ("-{0,6}" -f $totalRemoved) -ForegroundColor Red
Write-Host "  Net Change:    " -NoNewline
Write-Host ("+{0,6}" -f ($totalAdded - $totalRemoved)) -ForegroundColor Green
Write-Host ""
