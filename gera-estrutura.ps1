# Caminho do projeto - ajuste conforme necessário
$projectPath = "C:\dev\Android\MAUI\CantinaV1"

# Pastas de interesse
$includeFolders = @("Models", "Services", "ViewModels", "Views", "Resources")

# Arquivos de interesse na raiz
$includeRootFiles = @("App.xaml", "AppShell.xaml", "MauiProgram.cs")

# Arquivo de saída
$outputFile = "$projectPath\estrutura_simplificada.txt"

# Limpa o arquivo de saída se já existir
if (Test-Path $outputFile) {
    Remove-Item $outputFile
}

# Escreve título
"Projeto: MauiCantinaV1" | Out-File -FilePath $outputFile -Encoding utf8
"" | Out-File -FilePath $outputFile -Append

# Verifica pastas de interesse
foreach ($folder in $includeFolders) {
    $fullPath = Join-Path $projectPath $folder
    if (Test-Path $fullPath) {
        "[$folder]" | Out-File -FilePath $outputFile -Append
        Get-ChildItem $fullPath -Recurse -Include *.cs, *.xaml | ForEach-Object {
            "    " + $_.FullName.Replace($projectPath + "\", "") | Out-File -FilePath $outputFile -Append
        }
        "" | Out-File -FilePath $outputFile -Append
    }
}

# Adiciona arquivos .cs e .xaml na raiz do projeto
"[Root Files]" | Out-File -FilePath $outputFile -Append
Get-ChildItem $projectPath -Include $includeRootFiles | ForEach-Object {
    "    " + $_.Name | Out-File -FilePath $outputFile -Append
}
"" | Out-File -FilePath $outputFile -Append

Write-Host "Estrutura simplificada gerada em: $outputFile"
