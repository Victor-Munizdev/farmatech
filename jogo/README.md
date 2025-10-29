# Analisador de Jogos Esportivos

Sistema de análise inteligente de jogos esportivos usando IA avançada.

## Como executar

### Método mais simples (.NET Core/6+ ou .NET 5+)
```bash
cd jogo
dotnet run backend.cs
```

**Nota:** Se aparecer erro sobre libhostfxr.so, use o método Mono abaixo.

### Windows (com .NET Framework)
```cmd
cd jogo
csc /out:backend.exe backend.cs
backend.exe
```

### Linux/Mac (com Mono)
```bash
cd jogo
# Instalar Mono primeiro
sudo apt update && sudo apt install mono-complete
# Compilar com referências necessárias
mcs -r:System.Net.Http.dll -r:System.Web.Extensions.dll backend.cs && mono backend.exe
```

### Pré-requisitos
- **.NET 6+** (recomendado): https://dotnet.microsoft.com/download
- Ou **.NET Framework 4.5+** (Windows)
- Ou **Mono** (Linux/Mac): `sudo apt install mono-complete` 

O servidor será iniciado em http://localhost:8002

### Funcionalidades
- Análise de jogos esportivos com IA
- Probabilidades de vitória, empate e derrota
- Estatísticas visuais dos últimos 10 jogos
- Interface moderna e responsiva

### Tecnologias
- C# com HttpListener
- Google Gemini AI
- Chart.js para gráficos
- CSS moderno com glassmorphism

### Estrutura do projeto
- `backend.cs` - Servidor backend C#
- `index.html` - Interface frontend