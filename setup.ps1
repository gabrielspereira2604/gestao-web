docker compose up -d
Write-Host "Aguardando SQL Server inicializar..."
Start-Sleep 30
dotnet ef database update --project GestaoWeb
