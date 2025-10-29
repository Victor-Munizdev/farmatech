# FarmaTech — Sistema de Chamados

Sistema de automação de chamados com IA integrada para a empresa FarmaTech.

## 📋 Sobre o Projeto

Este é um projeto acadêmico desenvolvido para a disciplina PIM III da Universidade Paulista. O sistema permite aos usuários da FarmaTech criar, gerenciar e acompanhar chamados técnicos, além de contar com um assistente de IA para suporte.

## 🚀 Funcionalidades

- **Sistema de Login**: Autenticação segura de usuários
- **Gerenciamento de Chamados**: Criar, visualizar e atualizar status de chamados
- **Chat com IA**: Assistente virtual integrado com Google Gemini AI
- **Interface Responsiva**: Design moderno e adaptável a diferentes dispositivos
- **Análise de Jogos**: Ferramenta adicional de análise esportiva com IA

## 🛠️ Tecnologias Utilizadas

### Backend
- **C#** com HttpListener
- **Google Gemini AI** para integração com IA
- **JSON** para comunicação de dados

### Frontend
- **HTML5** e **CSS3**
- **JavaScript** (Vanilla)
- **Chart.js** para gráficos
- Design responsivo com **CSS Grid** e **Flexbox**

## 📁 Estrutura do Projeto

```
/
├── index.html              # Landing page principal
├── back.cs                 # Servidor backend C#
├── jogo/                   # Análise de jogos esportivos
│   ├── backend.cs         # Servidor do analisador
│   ├── index.html         # Interface do analisador
│   └── README.md          # Documentação específica
├── paginas/               # Páginas do sistema
│   ├── login.html         # Tela de login
│   ├── tickets.html       # Lista de chamados
│   └── chat-ia.html       # Chat com IA
└── README.md              # Esta documentação
```

## 🏃‍♂️ Como Executar

### Pré-requisitos
- **Windows**: .NET Framework 4.5+ ou .NET 6+
- **Linux/Mac**: Mono ou .NET Core

### Execução do Sistema Principal

#### Windows
```cmd
# Compilar e executar
csc /out:back.exe back.cs
back.exe
```

#### Linux/Mac
```bash
# Instalar Mono (se necessário)
sudo apt update && sudo apt install mono-complete

# Compilar e executar
mcs -r:System.Net.Http.dll -r:System.Web.Extensions.dll back.cs && mono back.exe
```

O servidor será iniciado em **http://localhost:5001**

### Execução do Analisador de Jogos

#### Windows
```cmd
cd jogo
dotnet run backend.cs
```

#### Linux/Mac
```bash
cd jogo
mcs -r:System.Net.Http.dll -r:System.Web.Extensions.dll backend.cs && mono backend.exe
```

O analisador será iniciado em **http://localhost:8002**

## 👥 Usuários de Teste

- **Email**: gabriel.veiga@farmatech.com
- **Senha**: 123456

## 📊 Funcionalidades do Sistema

### Sistema de Chamados
- ✅ Criar novos chamados
- ✅ Visualizar lista de chamados
- ✅ Atualizar status dos chamados
- ✅ Detalhes individuais de cada chamado

### Chat com IA
- ✅ Integração com Google Gemini AI
- ✅ Respostas contextuais sobre chamados
- ✅ Suporte técnico automatizado

### Análise de Jogos
- ✅ Análise de jogos esportivos
- ✅ Probabilidades de vitória/empate/derrota
- ✅ Estatísticas visuais
- ✅ Suporte a múltiplos times brasileiros

## 🎨 Design

O sistema utiliza um design moderno com:
- **Tema Dark**: Cores escuras para redução de fadiga visual
- **Glassmorphism**: Efeitos de transparência e blur
- **Gradientes**: Cores dinâmicas e modernas
- **Animações**: Transições suaves e interativas
- **Responsividade**: Adaptação perfeita a mobile e desktop

## 📚 Documentação Adicional

- [Google Gemini AI](https://ai.google.dev/docs) - Documentação da API de IA
- [C# HttpListener](https://docs.microsoft.com/dotnet/api/system.net.httplistener) - Documentação do servidor
- [Chart.js](https://www.chartjs.org/) - Documentação dos gráficos

## 👨‍💻 Equipe de Desenvolvimento

- Ana
- Anita
- Ester
- Gabriel
- Kaua
- Ryan

## 📄 Licença

Este projeto foi desenvolvido para fins acadêmicos na Universidade Paulista — PIM III • 2025.

---

**Nota**: Este é um protótipo acadêmico. Para produção, considere implementar autenticação real, banco de dados e medidas de segurança adicionais.