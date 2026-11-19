# 던전 게임 전투 시뮬레이터 실행 (Unity 없이 실제 전투 코드를 돌린다)
#
#   .\Tools\BattleSim\run.ps1                    # 승률표 (층 3개 x 상대 5종 x 직업 4종, 각 200판)
#   .\Tools\BattleSim\run.ps1 check              # 전투 흐름 18항목 확인
#   .\Tools\BattleSim\run.ps1 balance --layer 1 --class 마법사 --battles 500
#   .\Tools\BattleSim\run.ps1 balance --move 0.5 --card 0.25 --recovery 0.5 --recovery-move 1   # 시간 숫자 바꿔 보기
#
# .NET SDK는 Unity 에디터에 들어 있는 것을 먼저 쓰고, 없으면 PATH의 dotnet을 쓴다.
param([Parameter(ValueFromRemainingArguments = $true)] [string[]] $SimArgs)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path

$dotnet = $null
$unity = Get-ChildItem "$env:ProgramFiles\Unity\Hub\Editor\*\Editor\Data\DotNetSdk\dotnet.exe" -ErrorAction SilentlyContinue |
         Sort-Object FullName -Descending | Select-Object -First 1
if ($unity) { $dotnet = $unity.FullName }
if (-not $dotnet) {
    $onPath = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($onPath) { $dotnet = $onPath.Source }
}
if (-not $dotnet) {
    Write-Host '.NET SDK를 찾지 못했습니다. Unity 6 에디터(Unity Hub)나 .NET 8 SDK를 설치해 주세요.'
    exit 2
}

$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'

& $dotnet build "$here\BattleSim.csproj" -c Release -v q -nologo | Out-Host
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
& $dotnet "$here\bin\Release\net8.0\BattleSim.dll" @SimArgs
exit $LASTEXITCODE
