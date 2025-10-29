using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Net.Http;

// Modelos
class User
{
    public string Email;
    public string Password;
}

class Ticket
{
    public int Id;
    public string Title;
    public string Description;
    public string Status;
}

class ChatMessage
{
    public string User;
    public string Message;
    public string Response;
}

class Program
{
    static readonly HttpClient httpClient = new HttpClient();
    static readonly string geminiApiKey = "----------API_KEY_AQUI----------";
    // Usuário mokado para teste!
    static User testUser = new User { Email = "gabriel.veiga@farmatech.com", Password = "123456" };
    // Chamados mokados!
    static List<Ticket> tickets = new List<Ticket>
    {
        new Ticket { Id = 1, Title = "Erro no login", Description = "Não é possível entrar no sistema", Status = "Aberto" },
        new Ticket { Id = 2, Title = "Falha no envio de email", Description = "Emails não estão sendo enviados", Status = "Em andamento" }
    };

    static async Task Main()
    {
    HttpListener listener = new HttpListener();
    // Porta alterada para 5001 para evitar conflitos locais com serviços que possam usar 5000
    listener.Prefixes.Add("http://localhost:5001/");
        listener.Start();
        Console.WriteLine("Servidor rodando em http://localhost:5001/");
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
        else if (path == "/login" && method == "POST")
        {
            using var reader = new StreamReader(context.Request.InputStream);
            var body = reader.ReadToEnd();
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);

            if (data["email"] == testUser.Email && data["password"] == testUser.Password)
                WriteResponse(context, new { success = true, token = "mock-token" });
            else
                WriteResponse(context, new { success = false, message = "Email ou senha incorretos" });
        }
        else if (path == "/tickets" && method == "GET")
        {
            WriteResponse(context, tickets);
        }
        else if (path.StartsWith("/ticket/") && method == "GET")
        {
            string idStr = path.Replace("/ticket/", "");
            if (int.TryParse(idStr, out int id))
            {
                var ticket = tickets.Find(t => t.Id == id);
                if (ticket != null)
                    WriteResponse(context, ticket);
                else
                    WriteResponse(context, new { error = "Ticket não encontrado" });
            }
        }
        else if (path == "/chat" && method == "POST")
        {
            using var reader = new StreamReader(context.Request.InputStream);
            var body = reader.ReadToEnd();
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);

            string aiResponse = await GetGeminiResponse(data["message"]);

            var response = new ChatMessage
            {
                User = data["user"],
                Message = data["message"],
                Response = aiResponse
            };
            WriteResponse(context, response);
        }
        else if (path == "/analyze-game" && method == "POST")
        {
            using var reader = new StreamReader(context.Request.InputStream);
            var body = reader.ReadToEnd();
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);

            string analysis = await GetGeminiResponse(data["prompt"]);

            var response = new
            {
                analysis = analysis
            };
            WriteResponse(context, response);
        }
        else if (path.StartsWith("/fragments/") && method == "GET")
        {
            try
            {
                string rel = path.Substring("/fragments/".Length);
                if (string.IsNullOrEmpty(rel))
                {
                    WriteResponse(context, new { error = "Fragmento inválido" });
                }
                else
                {
                    // Normaliza e evita traversal
                    rel = rel.Replace("/", Path.DirectorySeparatorChar.ToString()).TrimStart(Path.DirectorySeparatorChar);
                    if (!rel.EndsWith(".html", StringComparison.OrdinalIgnoreCase)) rel += ".html";
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "fragments", rel);
                    if (File.Exists(filePath))
                    {
                        context.Response.ContentType = "text/html; charset=utf-8";
                        byte[] html = Encoding.UTF8.GetBytes(File.ReadAllText(filePath));
                        context.Response.OutputStream.Write(html, 0, html.Length);
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                        WriteResponse(context, new { error = "Fragmento não encontrado" });
                    }
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                WriteResponse(context, new { error = "Erro ao ler fragmento", detail = ex.Message });
            }
        }
        else
        {
            WriteResponse(context, new { error = "Rota não encontrada" });
        }

        context.Response.OutputStream.Close();
    }

    static void WriteResponse(HttpListenerContext context, object data)
    {
        string json = JsonSerializer.Serialize(data);
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
    }

    static async Task<string> GetGeminiResponse(string message)
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
                            new { text = message }
                        }
                    }
                }
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={geminiApiKey}", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
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
}
