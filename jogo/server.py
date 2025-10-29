5import http.server
import socketserver
import json
import os
import urllib.request
import urllib.parse

class Handler(http.server.SimpleHTTPRequestHandler):
    def do_GET(self):
        if self.path == '/':
            self.path = 'index.html'
            return super().do_GET()
        else:
            return super().do_GET()

    def do_POST(self):
        if self.path == '/ai-analysis':
            content_length = int(self.headers['Content-Length'])
            post_data = self.rfile.read(content_length)
            data = json.loads(post_data)
            prompt = data.get('prompt', '')

            ai_response = self.get_gemini_response(prompt)

            response = {'analysis': ai_response}
            self.send_response(200)
            self.send_header('Content-type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps(response).encode())
        else:
            self.send_error(405)

    def get_gemini_response(self, prompt):
        try:
            api_key = 'AIzaSyDafrIdzLzhIfOdK8oRFmwyleYpmDcK3Mc'
            url = f'https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={api_key}'
            headers = {'Content-Type': 'application/json'}
            payload = {
                'contents': [{
                    'parts': [{'text': prompt}]
                }]
            }
            data = json.dumps(payload).encode('utf-8')
            req = urllib.request.Request(url, data=data, headers=headers, method='POST')
            with urllib.request.urlopen(req) as response:
                result = json.loads(response.read().decode('utf-8'))
                if 'candidates' in result and result['candidates']:
                    content = result['candidates'][0].get('content', {})
                    parts = content.get('parts', [])
                    if parts:
                        return parts[0].get('text', 'Erro na resposta da IA')
            return 'Erro ao processar resposta da IA'
        except Exception as e:
            return f'Erro na integração com Gemini AI: {str(e)}'

with socketserver.TCPServer(('', 8002), Handler) as httpd:
    print('Servidor de Jogos rodando em http://localhost:8002/')
    httpd.serve_forever()