param(
    [string]$BotToken = "",
    [switch]$Apply,
    [switch]$PromptForToken
)

function Read-MaskedInput {
    param(
        [string]$Prompt = "Enter Discord bot token"
    )

    $readHost = Get-Command Read-Host -ErrorAction Stop
    if ($readHost.Parameters.ContainsKey("MaskInput")) {
        return (Read-Host -Prompt $Prompt -MaskInput)
    }

    $secureToken = Read-Host -Prompt $Prompt -AsSecureString
    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureToken)
    try {
        return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
    }
}

$tokenWasSetForThisRun = $false

if (-not [string]::IsNullOrWhiteSpace($BotToken)) {
    Write-Warning "Using -BotToken can expose secrets in shell history. Prefer -PromptForToken."
    $env:AGF_DISCORD_BOT_TOKEN = $BotToken
    $tokenWasSetForThisRun = $true
}

if ($PromptForToken -or [string]::IsNullOrWhiteSpace($env:AGF_DISCORD_BOT_TOKEN)) {
    $plainToken = Read-MaskedInput -Prompt "Enter Discord bot token"
    if ([string]::IsNullOrWhiteSpace($plainToken)) {
        Write-Error "Bot token missing. Set AGF_DISCORD_BOT_TOKEN, pass -BotToken, or use -PromptForToken."
        exit 1
    }
    $env:AGF_DISCORD_BOT_TOKEN = $plainToken
    $tokenWasSetForThisRun = $true
}

$scriptPath = Join-Path $PSScriptRoot "apply_discord_server_plan.py"
$configPath = Join-Path $PSScriptRoot "discord_server_plan.json"

if (-not (Test-Path $scriptPath)) {
    Write-Error "Script not found: $scriptPath"
    exit 1
}

if (-not (Test-Path $configPath)) {
    Write-Error "Config not found: $configPath"
    exit 1
}


$args = @(
    $scriptPath,
    "--config", $configPath
)

if ($Apply) {
    $args += "--apply"
}

Write-Host ("Running Discord plan in " + ($(if ($Apply) { "APPLY" } else { "DRY-RUN" }) + " mode..."))

try {
    & python @args
}
finally {
    if ($tokenWasSetForThisRun) {
        Remove-Item Env:AGF_DISCORD_BOT_TOKEN -ErrorAction SilentlyContinue
    }
}
