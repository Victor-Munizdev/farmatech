using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
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

    static void Main()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8002/");
        listener.Start();
        Console.WriteLine("Servidor de Análise de Jogos rodando em http://localhost:8002/");
        Console.WriteLine("Pressione Ctrl+C para parar.");

        while (true)
        {
            HttpListenerContext context = listener.GetContext();
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
            using (StreamReader reader = new StreamReader(context.Request.InputStream))
            {
                string body = reader.ReadToEnd();
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, string> data = serializer.Deserialize<Dictionary<string, string>>(body);
            }

            // Simular análise (em produção, buscar dados reais de API de futebol)
            GameAnalysis analysis = new GameAnalysis
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
            using (StreamReader reader = new StreamReader(context.Request.InputStream))
            {
                string body = reader.ReadToEnd();
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, string> data = serializer.Deserialize<Dictionary<string, string>>(body);

                string aiResponse = GetGeminiAnalysis(data["prompt"]);

                var response = new
                {
                    analysis = aiResponse
                };
                WriteResponse(context, response);
            }
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

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string jsonRequest = serializer.Serialize(requestBody);
            StringContent content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={geminiApiKey}", content).Result;
            response.EnsureSuccessStatusCode();

            string responseString = response.Content.ReadAsStringAsync().Result;
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            Dictionary<string, object> geminiResponse = jsonSerializer.Deserialize<Dictionary<string, object>>(responseString);

            if (geminiResponse.ContainsKey("candidates"))
            {
                var candidates = (object[])geminiResponse["candidates"];
                if (candidates.Length > 0)
                {
                    var candidate = (Dictionary<string, object>)candidates[0];
                    if (candidate.ContainsKey("content"))
                    {
                        var contentObj = (Dictionary<string, object>)candidate["content"];
                        if (contentObj.ContainsKey("parts"))
                        {
                            var parts = (object[])contentObj["parts"];
                            if (parts.Length > 0)
                            {
                                var part = (Dictionary<string, object>)parts[0];
                                if (part.ContainsKey("text"))
                                {
                                    return (string)part["text"];
                                }
                            }
                        }
                    }
                }
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
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        string json = serializer.Serialize(data);
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
    }
}