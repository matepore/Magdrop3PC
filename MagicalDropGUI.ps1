# ============================================================
# Magical Drop III - Native Recompilation Launcher
# Main menu, keyboard/controller configuration, game launch
# ============================================================

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# ------------------------------------------------------------
# Paths and default keyboard controls
# ------------------------------------------------------------

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$game = Join-Path $root 'magdrop3'
$cfg  = Join-Path $game 'controls.cfg'
$captureExe = Join-Path $game 'build-sdl2-game-vs-x64\Release\magdrop3.exe'

$defaults = [ordered]@{
    P1_UP    = 'Up'
    P1_DOWN  = 'Down'
    P1_LEFT  = 'Left'
    P1_RIGHT = 'Right'
    P1_A     = 'Z'
    P1_B     = 'X'
    P1_C     = 'C'
    P1_D     = 'V'

    P2_UP    = 'W'
    P2_DOWN  = 'S'
    P2_LEFT  = 'A'
    P2_RIGHT = 'D'
    P2_A     = 'F'
    P2_B     = 'G'
    P2_C     = 'H'
    P2_D     = 'J'

    START1   = '1'
    START2   = '2'
    SELECT   = '3'
    COIN1    = '5'
    COIN2    = '6'
    SERVICE  = '9'
    TEST     = 'F2'
}

$labels = [ordered]@{
    P1_UP    = 'Up'
    P1_DOWN  = 'Down'
    P1_LEFT  = 'Left'
    P1_RIGHT = 'Right'
    P1_A     = 'Button A'
    P1_B     = 'Button B'
    P1_C     = 'Button C'
    P1_D     = 'Button D'

    P2_UP    = 'Up'
    P2_DOWN  = 'Down'
    P2_LEFT  = 'Left'
    P2_RIGHT = 'Right'
    P2_A     = 'Button A'
    P2_B     = 'Button B'
    P2_C     = 'Button C'
    P2_D     = 'Button D'

    START1   = 'Start Player 1'
    START2   = 'Start Player 2'
    SELECT   = 'Select'
    COIN1    = 'Coin 1'
    COIN2    = 'Coin 2'
    SERVICE  = 'Service'
    TEST     = 'Test switch'
}

# ------------------------------------------------------------
# Read keyboard and controller configuration
# ------------------------------------------------------------

function Read-Map {
    $map = [ordered]@{}

    foreach ($key in $defaults.Keys) {
        $map[$key] = $defaults[$key]
    }

    if (Test-Path -LiteralPath $cfg) {
        foreach ($line in Get-Content -LiteralPath $cfg) {
            if ($line -match '^\s*([A-Z0-9_]+)\s*=\s*(.*?)\s*$') {
                $key = $matches[1]
                $value = $matches[2]

                if (
                    $map.Contains($key) -and
                    -not [string]::IsNullOrWhiteSpace($value)
                ) {
                    $map[$key] = $value
                }
            }
        }
    }

    return $map
}

function Read-PadMap {
    $padMap = [ordered]@{}

    if (Test-Path -LiteralPath $cfg) {
        foreach ($line in Get-Content -LiteralPath $cfg) {
            if ($line -match '^\s*(PAD_[A-Z0-9_]+)\s*=\s*(.*?)\s*$') {
                $key = $matches[1]
                $value = $matches[2]

                if (-not [string]::IsNullOrWhiteSpace($value)) {
                    $padMap[$key] = $value
                }
            }
        }
    }

    return $padMap
}

function Save-Map($map, $padMap) {
    # Preserve comments, unrelated settings, and unknown configuration
    # entries instead of silently deleting them.
    $extraLines = @()

    if (Test-Path -LiteralPath $cfg) {
        foreach ($line in Get-Content -LiteralPath $cfg) {
            if ($line -match '^\s*([A-Z0-9_]+)\s*=') {
                $key = $matches[1]

                if (
                    $defaults.Contains($key) -or
                    $key -match '^PAD_'
                ) {
                    continue
                }
            }

            $extraLines += $line
        }
    }

    $lines = @(
        '# Magical Drop III controls'
        '# Keyboard SDL key names; Escape is reserved.'
        '# Controller bindings use BTN, HAT, and AXIS names.'
        ''
    )

    foreach ($key in $defaults.Keys) {
        $lines += "$key=$($map[$key])"
    }

    $lines += ''

    foreach ($key in $padMap.Keys) {
        if (-not [string]::IsNullOrWhiteSpace([string]$padMap[$key])) {
            $lines += "$key=$($padMap[$key])"
        }
    }

    if ($extraLines.Count -gt 0) {
        $lines += ''
        $lines += $extraLines
    }

    Set-Content -LiteralPath $cfg -Value $lines -Encoding ASCII
}

# ------------------------------------------------------------
# Convert Windows key names to configuration key names
# ------------------------------------------------------------

function Key-Name($key) {
    if ($key -eq 'Escape') {
        return 'Escape'
    }

    if ($key -match '^D([0-9])$') {
        return $matches[1]
    }

    if ($key -match '^NumPad([0-9])$') {
        return 'Keypad ' + $matches[1]
    }

    if ($key -eq 'Oemcomma') {
        return ','
    }

    if ($key -eq 'OemPeriod') {
        return '.'
    }

    if ($key -eq 'OemMinus') {
        return '-'
    }

    if ($key -eq 'Oemplus') {
        return '+'
    }

    if ($key -match '^Oem[0-9]$') {
        return $null
    }

    if ($key -eq 'Return') {
        return 'Return'
    }

    return $key
}

# ------------------------------------------------------------
# Controller binding display names
# ------------------------------------------------------------

function Pad-DisplayName($binding) {
    if ([string]::IsNullOrWhiteSpace([string]$binding)) {
        return 'Not set'
    }

    if ($binding -match '^BTN(\d+)$') {
        return "Button $($matches[1])"
    }

    if ($binding -match '^HAT(\d+)_UP$') {
        return "D-pad Up (Hat $($matches[1]))"
    }

    if ($binding -match '^HAT(\d+)_DOWN$') {
        return "D-pad Down (Hat $($matches[1]))"
    }

    if ($binding -match '^HAT(\d+)_LEFT$') {
        return "D-pad Left (Hat $($matches[1]))"
    }

    if ($binding -match '^HAT(\d+)_RIGHT$') {
        return "D-pad Right (Hat $($matches[1]))"
    }

    if ($binding -match '^AXIS(\d+)_NEG$') {
        return "Axis $($matches[1]) -"
    }

    if ($binding -match '^AXIS(\d+)_POS$') {
        return "Axis $($matches[1]) +"
    }

    return [string]$binding
}

# ------------------------------------------------------------
# Capture a controller binding using the native SDL2 executable
# ------------------------------------------------------------




function Capture-Controller($state) {
    if (-not (Test-Path -LiteralPath $captureExe)) {
        [System.Windows.Forms.MessageBox]::Show(
            "Controller capture executable not found:`n$captureExe`n`nBuild the SDL2 version first.",
            'Controller capture unavailable',
            [System.Windows.Forms.MessageBoxButtons]::OK,
            [System.Windows.Forms.MessageBoxIcon]::Error
        ) | Out-Null
        return $null
    }

    $process = $null

    try {
        $startInfo = New-Object System.Diagnostics.ProcessStartInfo
        $startInfo.FileName = $captureExe
        $startInfo.Arguments = '--capture-controller'
        $startInfo.WorkingDirectory = $game
        $startInfo.UseShellExecute = $false
        $startInfo.CreateNoWindow = $true
        $startInfo.RedirectStandardOutput = $true
        $startInfo.RedirectStandardError = $true

        $process = New-Object System.Diagnostics.Process
        $process.StartInfo = $startInfo

        if (-not $process.Start()) {
            throw 'Failed to start the controller capture process.'
        }

        # Read both streams asynchronously to avoid blocking the process.
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()

        $cancelled = $false

        while (-not $process.HasExited) {
            [System.Windows.Forms.Application]::DoEvents()

            if ($state.cancelCapture) {
                $cancelled = $true

                try {
                    $process.Kill()
                    [void]$process.WaitForExit(2000)
                }
                catch {
                    # The process may already have exited.
                }

                break
            }

            Start-Sleep -Milliseconds 30
        }

        if ($cancelled) {
            if (-not $process.HasExited) {
                try {
                    $process.Kill()
                    $process.WaitForExit()
                }
                catch {}
            }

            # Finish consuming redirected streams before disposing.
            $null = $stdoutTask.GetAwaiter().GetResult()
            $null = $stderrTask.GetAwaiter().GetResult()
            return $null
        }

        $process.WaitForExit()
        $exitCode = $process.ExitCode

        $output = $stdoutTask.GetAwaiter().GetResult().Trim()
        $errorOutput = $stderrTask.GetAwaiter().GetResult().Trim()

        if (
            $exitCode -eq 0 -and
            $output -match '^(BTN\d+|HAT\d+_(UP|DOWN|LEFT|RIGHT)|AXIS\d+_(NEG|POS))$'
        ) {
            return $output
        }

        if ($errorOutput -match 'No controller detected') {
            $message = 'No controller was detected. Connect your controller and try again.'
            $title = 'Controller not detected'
        }
        elseif ($exitCode -eq 0) {
            $message = "Controller capture returned an unexpected result:`n$output"
            $title = 'Controller capture'
        }
        else {
            $message = "Controller capture failed (exit code $exitCode).`n`nOutput:`n$output`n`nError:`n$errorOutput"
            $title = 'Controller capture failed'
        }

        [System.Windows.Forms.MessageBox]::Show(
            $message,
            $title,
            [System.Windows.Forms.MessageBoxButtons]::OK,
            [System.Windows.Forms.MessageBoxIcon]::Warning
        ) | Out-Null

        return $null
    }
    catch {
        [System.Windows.Forms.MessageBox]::Show(
            "Could not run controller capture:`n$($_.Exception.Message)",
            'Controller capture failed',
            [System.Windows.Forms.MessageBoxButtons]::OK,
            [System.Windows.Forms.MessageBoxIcon]::Error
        ) | Out-Null
        return $null
    }
    finally {
        if ($null -ne $process) {
            $process.Dispose()
        }
    }
}

# ------------------------------------------------------------
# Unified keyboard and controller configuration window
# ------------------------------------------------------------

function Edit-Controls {
    $map = Read-Map
    $padMap = Read-PadMap

    $state = @{
        action   = $null
        mode     = $null
        saved    = $false
        cancelCapture = $false
        original = @{}
        originalPad = @{}
    }

    foreach ($key in $map.Keys) {
        $state.original[$key] = [string]$map[$key]
    }

    foreach ($key in $padMap.Keys) {
        $state.originalPad[$key] = [string]$padMap[$key]
    }

    $form = New-Object System.Windows.Forms.Form
    $form.Text = 'Controls'
    $form.Size = [System.Drawing.Size]::new(790, 680)
    $form.StartPosition = 'CenterScreen'
    $form.KeyPreview = $true
    $form.FormBorderStyle = 'FixedDialog'
    $form.MaximizeBox = $false
    $form.MinimizeBox = $false

    $tabs = New-Object System.Windows.Forms.TabControl
    $tabs.Location = [System.Drawing.Point]::new(10, 10)
    $tabs.Size = [System.Drawing.Size]::new(755, 535)
    $form.Controls.Add($tabs)

    $groups = @(
        @{
            Name = 'Player 1'
            Keys = @(
                'P1_UP', 'P1_DOWN', 'P1_LEFT', 'P1_RIGHT',
                'P1_A', 'P1_B', 'P1_C', 'P1_D'
            )
        },
        @{
            Name = 'Player 2'
            Keys = @(
                'P2_UP', 'P2_DOWN', 'P2_LEFT', 'P2_RIGHT',
                'P2_A', 'P2_B', 'P2_C', 'P2_D'
            )
        },
        @{
            Name = 'System'
            Keys = @(
                'START1', 'START2', 'SELECT',
                'COIN1', 'COIN2', 'SERVICE', 'TEST'
            )
        }
    )

    $keyboardButtons = @{}
    $controllerButtons = @{}

    foreach ($group in $groups) {
        $tab = New-Object System.Windows.Forms.TabPage
        $tab.Text = $group.Name
        [void]$tabs.TabPages.Add($tab)

        $headingAction = New-Object System.Windows.Forms.Label
        $headingAction.Text = 'Action'
        $headingAction.Font = [System.Drawing.Font]::new(
            'Segoe UI', 9, [System.Drawing.FontStyle]::Bold
        )
        $headingAction.Location = [System.Drawing.Point]::new(15, 15)
        $headingAction.Size = [System.Drawing.Size]::new(155, 25)
        $tab.Controls.Add($headingAction)

        $headingKeyboard = New-Object System.Windows.Forms.Label
        $headingKeyboard.Text = 'Keyboard'
        $headingKeyboard.Font = [System.Drawing.Font]::new(
            'Segoe UI', 9, [System.Drawing.FontStyle]::Bold
        )
        $headingKeyboard.Location = [System.Drawing.Point]::new(180, 15)
        $headingKeyboard.Size = [System.Drawing.Size]::new(245, 25)
        $tab.Controls.Add($headingKeyboard)

        $headingController = New-Object System.Windows.Forms.Label
        $headingController.Text = 'Controller'
        $headingController.Font = [System.Drawing.Font]::new(
            'Segoe UI', 9, [System.Drawing.FontStyle]::Bold
        )
        $headingController.Location = [System.Drawing.Point]::new(440, 15)
        $headingController.Size = [System.Drawing.Size]::new(275, 25)
        $tab.Controls.Add($headingController)

        $y = 48

        foreach ($key in $group.Keys) {
            $label = New-Object System.Windows.Forms.Label
            $label.Text = $labels[$key]
            $label.Location = [System.Drawing.Point]::new(15, $y + 7)
            $label.Size = [System.Drawing.Size]::new(155, 25)
            $tab.Controls.Add($label)

            # Keyboard binding button.
            $keyboardButton = New-Object System.Windows.Forms.Button
            $keyboardButton.Text = [string]$map[$key]
            $keyboardButton.Tag = $key
            $keyboardButton.Location = [System.Drawing.Point]::new(180, $y)
            $keyboardButton.Size = [System.Drawing.Size]::new(245, 32)

            $keyboardButton.Add_Click({
                param($sender, $eventArgs)

                $state.action = [string]$sender.Tag
                $state.mode = 'keyboard'
                $sender.Text = 'Press a key...'
                $status.Text = 'Press a keyboard key. Escape cancels capture.'
            }.GetNewClosure())

            $keyboardButton.Add_PreviewKeyDown({
                param($sender, $eventArgs)

                if (
                    $state.mode -eq 'keyboard' -and
                    $eventArgs.KeyCode -in @(
                        [System.Windows.Forms.Keys]::Up,
                        [System.Windows.Forms.Keys]::Down,
                        [System.Windows.Forms.Keys]::Left,
                        [System.Windows.Forms.Keys]::Right
                    )
                ) {
                    $eventArgs.IsInputKey = $true
                }
            }.GetNewClosure())

            $keyboardButtons[$key] = $keyboardButton
            $tab.Controls.Add($keyboardButton)

            # Controller binding button.
            $padKey = "PAD_$key"
            $padValue = ''

            if ($padMap.Contains($padKey)) {
                $padValue = [string]$padMap[$padKey]
            }

            $controllerButton = New-Object System.Windows.Forms.Button
            $controllerButton.Text = Pad-DisplayName $padValue
            $controllerButton.Tag = $key
            $controllerButton.Location = [System.Drawing.Point]::new(440, $y)
            $controllerButton.Size = [System.Drawing.Size]::new(275, 32)

            $controllerButton.Add_Click({
                param($sender, $eventArgs)

                if ($state.mode -eq 'controller') {
                    return
                }

                $action = [string]$sender.Tag
                $status.Text = "Listening for $($labels[$action])... Please press the controller button or direction you want to assign. Press Escape to cancel."
                $state.mode = 'controller'
                $sender.Text = 'Press a button...'
                $state.cancelCapture = $false
                $form.UseWaitCursor = $true
                $form.Refresh()

                try {
                    $binding = Capture-Controller $state
                }
                finally {
                    $form.UseWaitCursor = $false
                    $state.mode = $null
                }
                
                
                if ($state.cancelCapture) {
                    $current = ''
                    if ($padMap.Contains("PAD_$action")) {
                        $current = [string]$padMap["PAD_$action"]
                    }

                    $sender.Text = Pad-DisplayName $current
                    $status.Text = 'Controller capture cancelled.'
                    return
                }


                if ($binding -is [string] -and $binding -match '^(BTN\d+|HAT\d+_(UP|DOWN|LEFT|RIGHT)|AXIS\d+_(NEG|POS))$') {
                    
                    $padKey = "PAD_$action"
                    $conflictKey = $null

                    # Find another action that already uses this controller input.
                    foreach ($existingPadKey in @($padMap.Keys)) {
                        if (
                            $existingPadKey -ne $padKey -and
                            [string]$padMap[$existingPadKey] -eq [string]$binding
                        ) {
                            $conflictKey = $existingPadKey
                            break
                        }
                    }

                    if ($conflictKey) {
                        # Swap assignments, matching keyboard behavior.
                        $oldBinding = ''

                        if ($padMap.Contains($padKey)) {
                            $oldBinding = [string]$padMap[$padKey]
                        }

                        if ([string]::IsNullOrWhiteSpace($oldBinding)) {
                            # The newly assigned action had no previous binding.
                            $padMap.Remove($conflictKey)
                        }
                        else {
                            # Give the conflicting action the previous binding.
                            $padMap[$conflictKey] = $oldBinding
                        }

                        $conflictAction = $conflictKey -replace '^PAD_', ''

                        if ($controllerButtons.ContainsKey($conflictAction)) {
                            $controllerButtons[$conflictAction].Text =
                                Pad-DisplayName $oldBinding
                        }

                        $status.Text = "Swapped $($labels[$action]) and $($labels[$conflictAction]) controller bindings."
                    }
                    else {
                        $status.Text = "$($labels[$action]) controller binding: $(Pad-DisplayName $binding)"
                    }

                    $padMap[$padKey] = $binding
                    $sender.Text = Pad-DisplayName $binding

                }
                else {
                    $current = ''
                    if ($padMap.Contains("PAD_$action")) {
                        $current = [string]$padMap["PAD_$action"]
                    }

                    $sender.Text = Pad-DisplayName $current
                    $status.Text = 'Controller binding was not changed.'
                }
            }.GetNewClosure())

            # Right-click a controller binding to clear it.
            $controllerButton.Add_MouseDown({
                param($sender, $eventArgs)

                if ($eventArgs.Button -eq [System.Windows.Forms.MouseButtons]::Right) {
                    $action = [string]$sender.Tag
                    $padKey = "PAD_$action"
                    $padMap.Remove($padKey)
                    $sender.Text = 'Not set'
                    $status.Text = "$($labels[$action]) controller binding cleared."
                }
            }.GetNewClosure())

            $controllerButtons[$key] = $controllerButton
            $tab.Controls.Add($controllerButton)

            $y += 48
        }
    }

    # Status message.
    $status = New-Object System.Windows.Forms.Label
    $status.Text = 'Click a binding to assign it. Right-click a controller binding to clear it.'
    $status.Location = [System.Drawing.Point]::new(12, 550)
    $status.Size = [System.Drawing.Size]::new(750, 35)
    $form.Controls.Add($status)

    # Capture keyboard input.
    $form.Add_KeyDown({
        param($sender, $eventArgs)

        if ($state.mode -eq 'controller') {
            if ($eventArgs.KeyCode -eq [System.Windows.Forms.Keys]::Escape) {
                $state.cancelCapture = $true
                $eventArgs.Handled = $true
                $eventArgs.SuppressKeyPress = $true
            }
            return
        }

        if ($state.mode -ne 'keyboard' -or -not $state.action) {
            return
        }

        $keyName = Key-Name $eventArgs.KeyCode.ToString()
        $action = $state.action
        $state.action = $null
        $state.mode = $null

        if ($keyName -eq 'Escape') {
            $keyboardButtons[$action].Text = $map[$action]
            $status.Text = 'Keyboard capture cancelled. Escape is reserved for quitting.'
            $eventArgs.Handled = $true
            return
        }

        if (
            -not $keyName -or
            $keyName -in @('ShiftKey', 'ControlKey', 'Menu', 'LWin', 'RWin')
        ) {
            $keyboardButtons[$action].Text = $map[$action]
            $status.Text = 'Unsupported key. Try a letter, number, arrow, function or keypad key.'
            return
        }

        # Keep keyboard bindings unique by swapping duplicates.
        $duplicate = $null

        foreach ($key in $map.Keys) {
            if ($key -ne $action -and $map[$key] -eq $keyName) {
                $duplicate = $key
                break
            }
        }

        if ($duplicate) {
            $oldKey = $map[$action]
            $map[$duplicate] = $oldKey
            $keyboardButtons[$duplicate].Text = $oldKey
        }

        $map[$action] = $keyName
        $keyboardButtons[$action].Text = $keyName

        if ($duplicate) {
            $status.Text = "Swapped $($labels[$action]) and $($labels[$duplicate])."
        }
        else {
            $status.Text = "$($labels[$action]) keyboard binding: $keyName"
        }

        $eventArgs.Handled = $true
    }.GetNewClosure())

    
    # Save button.
    $saveButton = New-Object System.Windows.Forms.Button
    $saveButton.Text = 'Save'
    $saveButton.Location = [System.Drawing.Point]::new(12, 600)
    $saveButton.Size = [System.Drawing.Size]::new(85, 32)

    $saveButton.Add_Click({
        Save-Map $map $padMap
        $state.saved = $true

        [System.Windows.Forms.MessageBox]::Show(
            "Controls saved to:`n$cfg`n`nRestart the game to apply changes.",
            'Controls saved',
            [System.Windows.Forms.MessageBoxButtons]::OK,
            [System.Windows.Forms.MessageBoxIcon]::Information
        ) | Out-Null

        $form.Close()
    }.GetNewClosure())

    $form.Controls.Add($saveButton)

    # Clear all bindings.
    $clearAllButton = New-Object System.Windows.Forms.Button
    $clearAllButton.Text = 'Clear all'
    $clearAllButton.Location = [System.Drawing.Point]::new(105, 600)
    $clearAllButton.Size = [System.Drawing.Size]::new(90, 32)

    $clearAllButton.Add_Click({
        $result = [System.Windows.Forms.MessageBox]::Show(
            'Unassign every keyboard and controller binding?',
            'Clear all bindings',
            [System.Windows.Forms.MessageBoxButtons]::YesNo,
            [System.Windows.Forms.MessageBoxIcon]::Warning
        )

        if ($result -ne [System.Windows.Forms.DialogResult]::Yes) {
            return
        }

        foreach ($key in $defaults.Keys) {
            $map[$key] = 'NONE'
            $keyboardButtons[$key].Text = 'Not set'
        }

        foreach ($padKey in @($padMap.Keys)) {
            $padMap.Remove($padKey)
        }

        foreach ($key in $controllerButtons.Keys) {
            $controllerButtons[$key].Text = 'Not set'
        }

        $status.Text = 'All keyboard and controller bindings cleared.'
    }.GetNewClosure())

    $form.Controls.Add($clearAllButton)

    # Reset keyboard defaults only; preserve controller bindings.
    $resetButton = New-Object System.Windows.Forms.Button
    $resetButton.Text = 'Reset keyboard'
    $resetButton.Location = [System.Drawing.Point]::new(203, 600)
    $resetButton.Size = [System.Drawing.Size]::new(115, 32)

    $resetButton.Add_Click({
        foreach ($key in $defaults.Keys) {
            $map[$key] = $defaults[$key]
            $keyboardButtons[$key].Text = $defaults[$key]
        }

        $status.Text = 'Keyboard defaults restored. Controller bindings were not changed.'
    }.GetNewClosure())

    $form.Controls.Add($resetButton)

    # Reset controller overrides only; preserve keyboard bindings.
    $resetControllerButton = New-Object System.Windows.Forms.Button
    $resetControllerButton.Text = 'Reset controller'
    $resetControllerButton.Location = [System.Drawing.Point]::new(326, 600)
    $resetControllerButton.Size = [System.Drawing.Size]::new(120, 32)

    $resetControllerButton.Add_Click({
        foreach ($padKey in @($padMap.Keys)) {
            $padMap.Remove($padKey)
        }

        foreach ($key in $controllerButtons.Keys) {
            $controllerButtons[$key].Text = 'Auto'
        }

        $status.Text = 'Controller overrides cleared. Automatic mapping will be used.'
    }.GetNewClosure())

    $form.Controls.Add($resetControllerButton)

    # Reset both keyboard and controller.
    $resetAllButton = New-Object System.Windows.Forms.Button
    $resetAllButton.Text = 'Reset all'
    $resetAllButton.Location = [System.Drawing.Point]::new(454, 600)
    $resetAllButton.Size = [System.Drawing.Size]::new(90, 32)

    $resetAllButton.Add_Click({
        $result = [System.Windows.Forms.MessageBox]::Show(
            'Restore keyboard defaults and clear all controller overrides?',
            'Reset all controls',
            [System.Windows.Forms.MessageBoxButtons]::YesNo,
            [System.Windows.Forms.MessageBoxIcon]::Warning
        )

        if ($result -ne [System.Windows.Forms.DialogResult]::Yes) {
            return
        }

        foreach ($key in $defaults.Keys) {
            $map[$key] = $defaults[$key]
            $keyboardButtons[$key].Text = $defaults[$key]
        }

        foreach ($padKey in @($padMap.Keys)) {
            $padMap.Remove($padKey)
        }

        foreach ($key in $controllerButtons.Keys) {
            $controllerButtons[$key].Text = 'Auto'
        }

        $status.Text = 'Keyboard defaults restored and controller overrides cleared.'
    }.GetNewClosure())

    $form.Controls.Add($resetAllButton)

    # Cancel button.
    $cancelButton = New-Object System.Windows.Forms.Button
    $cancelButton.Text = 'Cancel'
    $cancelButton.Location = [System.Drawing.Point]::new(650, 600)
    $cancelButton.Size = [System.Drawing.Size]::new(100, 32)

    $cancelButton.Add_Click({
        $form.Close()
    }.GetNewClosure())

    $form.Controls.Add($cancelButton)

    # Warn when closing with unsaved keyboard or controller changes.
    $form.Add_FormClosing({
        param($sender, $eventArgs)

        if ($state.saved) {
            return
        }

        $changed = $false

        foreach ($key in $map.Keys) {
            if ([string]$map[$key] -ne [string]$state.original[$key]) {
                $changed = $true
                break
            }
        }

        if (-not $changed) {
            foreach ($key in $state.originalPad.Keys) {
                if (
                    -not $padMap.Contains($key) -or
                    [string]$padMap[$key] -ne [string]$state.originalPad[$key]
                ) {
                    $changed = $true
                    break
                }
            }
        }

        if (-not $changed) {
            foreach ($key in $padMap.Keys) {
                if (
                    -not $state.originalPad.ContainsKey($key) -or
                    [string]$padMap[$key] -ne [string]$state.originalPad[$key]
                ) {
                    $changed = $true
                    break
                }
            }
        }

        if (-not $changed) {
            return
        }

        $result = [System.Windows.Forms.MessageBox]::Show(
            'There are unsaved changes. Would you like to save them before closing?',
            'Unsaved changes',
            [System.Windows.Forms.MessageBoxButtons]::YesNoCancel,
            [System.Windows.Forms.MessageBoxIcon]::Warning
        )

        if ($result -eq [System.Windows.Forms.DialogResult]::Yes) {
            Save-Map $map $padMap
            $state.saved = $true
        }
        elseif ($result -eq [System.Windows.Forms.DialogResult]::Cancel) {
            $eventArgs.Cancel = $true
        }
    }.GetNewClosure())

    [void]$form.ShowDialog()
}

# ------------------------------------------------------------
# Launch standard or Japanese version of the game
# ------------------------------------------------------------

function Play($japan) {
    if ($japan) {
        $exe = Join-Path $game 'build-japan\magdrop3.exe'
        $bios = 'sp-j2.rom'
    }
    else {
        $exe = Join-Path $game 'build-sdl2\magdrop3.exe'
        $bios = 'sp-s2.sp1'
    }

    if (-not (Test-Path -LiteralPath $exe)) {
        [System.Windows.Forms.MessageBox]::Show(
            "Executable not found:`n$exe`n`nBuild the game first.",
            'Game executable not found',
            [System.Windows.Forms.MessageBoxButtons]::OK,
            [System.Windows.Forms.MessageBoxIcon]::Error
        ) | Out-Null

        return
    }

    $arguments = @(
        '--rom-path'
        (Join-Path $game 'roms')
        '--bios-path'
        (Join-Path $game 'bios')
        '--bios'
        $bios
    )

    Start-Process `
        -FilePath $exe `
        -ArgumentList $arguments `
        -WorkingDirectory $game
}

# ------------------------------------------------------------
# Main launcher window
# ------------------------------------------------------------

$mainForm = New-Object System.Windows.Forms.Form
$mainForm.Text = 'Magical Drop III'
$mainForm.Size = [System.Drawing.Size]::new(430, 290)
$mainForm.StartPosition = 'CenterScreen'
$mainForm.FormBorderStyle = 'FixedDialog'
$mainForm.MaximizeBox = $false

$title = New-Object System.Windows.Forms.Label
$title.Text = 'Magical Drop III - Native Recompilation'
$title.Location = [System.Drawing.Point]::new(20, 18)
$title.Size = [System.Drawing.Size]::new(380, 30)
$title.Font = [System.Drawing.Font]::new(
    'Segoe UI',
    12,
    [System.Drawing.FontStyle]::Bold
)
$mainForm.Controls.Add($title)

$standardButton = New-Object System.Windows.Forms.Button
$standardButton.Text = 'Play - Standard BIOS'
$standardButton.Location = [System.Drawing.Point]::new(25, 65)
$standardButton.Size = [System.Drawing.Size]::new(365, 38)
$standardButton.Add_Click({
    Play $false
})
$mainForm.Controls.Add($standardButton)

$japanButton = New-Object System.Windows.Forms.Button
$japanButton.Text = 'Play - Japanese BIOS (Hard mode)'
$japanButton.Location = [System.Drawing.Point]::new(25, 112)
$japanButton.Size = [System.Drawing.Size]::new(365, 38)
$japanButton.Add_Click({
    Play $true
})
$mainForm.Controls.Add($japanButton)

$controlsButton = New-Object System.Windows.Forms.Button
$controlsButton.Text = 'Configure controls'
$controlsButton.Location = [System.Drawing.Point]::new(25, 159)
$controlsButton.Size = [System.Drawing.Size]::new(365, 38)
$controlsButton.Add_Click({
    Edit-Controls
})
$mainForm.Controls.Add($controlsButton)

$infoLabel = New-Object System.Windows.Forms.Label
$infoLabel.Text = 'Settings saved to magdrop3\controls.cfg'
$infoLabel.Location = [System.Drawing.Point]::new(25, 212)
$infoLabel.Size = [System.Drawing.Size]::new(370, 24)
$mainForm.Controls.Add($infoLabel)

[void]$mainForm.ShowDialog()
