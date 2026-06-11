<#
.SYNOPSIS
  Drives a single Android crash scenario via adb (uiautomator + input), captures logcat.

.DESCRIPTION
  Fallback UI driver for when the MAUI MCP has no connected DevFlow agent (the playground
  app does not embed Microsoft.Maui.DevFlow.Agent). Pure adb: dumps the a11y tree, finds the
  scenario card / trigger button by text, taps it, and captures `adb logcat -d` after the crash.

  Hardened from a full 4-cell matrix run:
   - Tap-Trigger SCROLLS the detail page down until the "Trigger scenario" button is visible
     (it is the last element on a scrollable page and is often below the fold).
   - Relaunch-ToGallery retries the launch up to 3x (30 polls each) because the device can be
     slow to render the gallery after a terminating scenario.

.PARAMETER Title    Gallery card title (exact display text).
.PARAMETER Key      Scenario key used for output file names.
.PARAMETER OutDir   Cell output directory; writes logcat-<key>.log and process-<key>.log.
.PARAMETER Special  none | startup | resume (lifecycle scenarios need force-stop / home+resume).
.PARAMETER SettleSeconds  Seconds to wait after triggering before capturing logcat.
.PARAMETER Package  App id (default dev.redth.maui.diagnostics.playground).
.PARAMETER Activity Launch activity (default crc64...MainActivity for this app).
#>
param(
  [Parameter(Mandatory)][string]$Title,
  [Parameter(Mandatory)][string]$Key,
  [Parameter(Mandatory)][string]$OutDir,
  [ValidateSet('none','startup','resume')][string]$Special = 'none',
  [int]$SettleSeconds = 3,
  [string]$Package = 'dev.redth.maui.diagnostics.playground',
  [string]$Activity = 'dev.redth.maui.diagnostics.playground/crc642843c7e8ee259013.MainActivity'
)
$ErrorActionPreference = 'Stop'
$pkg = $Package
$act = $Activity
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

function Dump-Ui {
  adb shell uiautomator dump /sdcard/ui.xml *> $null
  adb pull /sdcard/ui.xml "$OutDir/_ui.xml" *> $null
  return (Get-Content "$OutDir/_ui.xml" -Raw)
}
function Get-Center([string]$xml, [string]$text, [switch]$exact) {
  foreach ($n in [regex]::Matches($xml, '<node[^>]*>')) {
    $s = $n.Value
    $m = [regex]::Match($s, 'text="([^"]*)"')
    if (-not $m.Success) { continue }
    $t = $m.Groups[1].Value
    $hit = if ($exact) { $t -ieq $text } else { $t -like "*$text*" }
    if ($hit -and $t -ne '') {
      $b = [regex]::Match($s, 'bounds="\[(\d+),(\d+)\]\[(\d+),(\d+)\]"')
      if ($b.Success) {
        return @([int](([int]$b.Groups[1].Value + [int]$b.Groups[3].Value) / 2),
                 [int](([int]$b.Groups[2].Value + [int]$b.Groups[4].Value) / 2))
      }
    }
  }
  return $null
}
function Tap($p) { adb shell input tap $p[0] $p[1] *> $null; Start-Sleep -Milliseconds 500 }
function Relaunch-ToGallery {
  adb shell am force-stop $pkg *> $null
  Start-Sleep -Milliseconds 800
  for ($attempt = 0; $attempt -lt 3; $attempt++) {
    adb shell am start -n $act *> $null
    for ($i = 0; $i -lt 30; $i++) {
      Start-Sleep -Milliseconds 600
      $xml = Dump-Ui
      if (Get-Center $xml 'Scenario gallery') { return $true }
    }
    # not up yet; force-stop and try launching again
    adb shell am force-stop $pkg *> $null
    Start-Sleep -Milliseconds 800
  }
  return $false
}
function Open-Scenario {
  $found = $null
  for ($i = 0; $i -lt 14; $i++) {
    $xml = Dump-Ui
    $found = Get-Center $xml $Title -exact
    if ($found) { break }
    adb shell input swipe 540 1700 540 700 250 *> $null
    Start-Sleep -Milliseconds 450
  }
  if (-not $found) { throw "card not found: $Title" }
  Tap $found
  Start-Sleep -Milliseconds 700
}
function Tap-Trigger {
  # The "Trigger scenario" button is the last element on a scrollable detail
  # page; longer "Expected artifacts" lists push it below the fold, so scroll
  # the page down until uiautomator can see it.
  $btn = $null
  for ($i = 0; $i -lt 10; $i++) {
    $xml = Dump-Ui
    $btn = Get-Center $xml 'Trigger scenario' -exact
    if ($btn) { break }
    # scroll the detail content down to reveal the button
    adb shell input swipe 540 1800 540 600 250 *> $null
    Start-Sleep -Milliseconds 500
  }
  if (-not $btn) { throw "Trigger scenario button not found" }
  Tap $btn
  Start-Sleep -Milliseconds 600
  $ok = $null
  for ($i = 0; $i -lt 8; $i++) {
    $xml = Dump-Ui
    $ok = Get-Center $xml 'Trigger' -exact
    if ($ok) { break }
    Start-Sleep -Milliseconds 350
  }
  if (-not $ok) { throw "confirm Trigger button not found" }
  Tap $ok
}

$logcat = "$OutDir/logcat-$Key.log"
$proc   = "$OutDir/process-$Key.log"

if ($Special -eq 'none') {
  if (-not (Relaunch-ToGallery)) { throw "gallery did not render" }
  adb logcat -c
  Open-Scenario
  Tap-Trigger
  Start-Sleep -Seconds $SettleSeconds
  adb logcat -d *> $logcat
}
elseif ($Special -eq 'startup') {
  if (-not (Relaunch-ToGallery)) { throw "gallery did not render" }
  Open-Scenario
  Tap-Trigger           # arms the startup crash
  Start-Sleep -Seconds 2
  adb shell am force-stop $pkg *> $null
  Start-Sleep -Milliseconds 500
  adb logcat -c
  adb shell am start -n $act *> $null   # should crash during startup
  Start-Sleep -Seconds 5
  adb logcat -d *> $logcat
}
elseif ($Special -eq 'resume') {
  if (-not (Relaunch-ToGallery)) { throw "gallery did not render" }
  Open-Scenario
  Tap-Trigger           # arms the resume crash
  Start-Sleep -Seconds 2
  adb logcat -c
  adb shell input keyevent KEYCODE_HOME *> $null   # background
  Start-Sleep -Seconds 2
  adb shell am start -n $act *> $null              # foreground -> crash on resume
  Start-Sleep -Seconds 5
  adb logcat -d *> $logcat
}

# Process-view: the managed/runtime slice of logcat (runtime self-report + managed tags).
Select-String -Path $logcat -Pattern 'DOTNET|mono-rt|monodroid|MonoDroid|AndroidRuntime|FATAL|System\.|Unhandled|Exception|Runtime|libmonosgen|libcoreclr|nativeloader.*lib(mono|core)' `
  | ForEach-Object { $_.Line } | Set-Content $proc

Write-Output "DONE $Key -> $logcat ($((Get-Item $logcat).Length) bytes)"
