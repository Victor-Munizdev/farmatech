using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Net.Http;

class GameAnalysis
{
    public string Team1;
    public string Team2;
    public string Analysis;
    public Dictionary<string, double> Probabilities;
    public List<GameStats> RecentGames;
}

class GameStats
{
    public string Date;
    public string HomeTeam;
    public string AwayTeam;
    public int HomeScore;
    public int AwayScore;
    public string Result;
}

class Program
{
    static readonly HttpClient httpClient = new HttpClient();
    static readonly string geminiApiKey = "AIzaSyDafrIdzLzhIfOdK8oRFmwyleYpmDcK3Mc";

    static async Task Main()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:5002/");
        listener.Start();
        Console.WriteLine("Servidor de Análise de Jogos rodando em http://localhost:5002/");
        Console.WriteLine("Pressione Ctrl+C para parar.");

        while (true)
        {
            var context = await listener.GetContextAsync();
            Task.Run(() => HandleRequest(context));
        }
    }

    static void HandleRequest(HttpListenerContext context)
    {
        string path = context.Request.Url.AbsolutePath;
        string method = context.Request.HttpMethod;
        context.Response.ContentType = "application/json; charset=utf-8";

        if (path == "/" && method == "GET")
        {
            // Retornar HTML
            context.Response.ContentType = "text/html; charset=utf-8";
            byte[] html = Encoding.UTF8.GetBytes(File.ReadAllText("index.html"));
            context.Response.OutputStream.Write(html, 0, html.Length);
        }
        else if (path == "/analyze-game" && method == "POST")
        {
            using var reader = new StreamReader(context.Request.InputStream);
            var body = reader.ReadToEnd();
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);

            // Simular análise (em produção, buscar dados reais de API de futebol)
            var analysis = new GameAnalysis
            {
                Team1 = "Bahia",
                Team2 = "Vitória",
                Analysis = "Análise baseada nos últimos 10 jogos...",
                Probabilities = new Dictionary<string, double>
                {
                    ["home_win"] = 65.0,
                    ["draw"] = 20.0,
                    ["away_win"] = 15.0
                },
                RecentGames = new List<GameStats>
                {
                    new GameStats { Date = "2025-01-01", HomeTeam = "Bahia", AwayTeam = "Vitória", HomeScore = 2, AwayScore = 1, Result = "Casa" },
                    new GameStats { Date = "2025-01-08", HomeTeam = "Vitória", AwayTeam = "Bahia", HomeScore = 1, AwayScore = 1, Result = "Empate" }
                }
            };

            WriteResponse(context, analysis);
        }
        else if (path == "/ai-analysis" && method == "POST")
        {
            using var reader = new StreamReader(context.Request.InputStream);
            var body = reader.ReadToEnd();
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);

            string aiResponse = GetGeminiAnalysis(data["prompt"]);

            var response = new
            {
                analysis = aiResponse
            };
            WriteResponse(context, response);
        }
        else
        {
            WriteResponse(context, new { error = "Rota não encontrada" });
        }

        context.Response.OutputStream.Close();
    }

    static string GetGeminiAnalysis(string prompt)
    {
        try
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={geminiApiKey}", content).Result;
            response.EnsureSuccessStatusCode();

            var responseString = response.Content.ReadAsStringAsync().Result;
            var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseString);

            if (geminiResponse.TryGetProperty("candidates", out var candidates) &&
                candidates[0].TryGetProperty("content", out var contentProp) &&
                contentProp.TryGetProperty("parts", out var parts) &&
                parts[0].TryGetProperty("text", out var text))
            {
                return text.GetString();
            }

            return "Erro ao processar resposta da IA.";
        }
        catch (Exception ex)
        {
            return $"Erro na integração com Gemini AI: {ex.Message}";
        }
    }

    static void WriteResponse(HttpListenerContext context, object data)
    {
        string json = JsonSerializer.Serialize(data);
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
    }
}