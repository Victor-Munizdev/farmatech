import http.server
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
        elif self.path == '/tickets':
            self.send_response(200)
            self.send_header('Content-type', 'application/json')
            self.end_headers()
            tickets = [
                {'Id': 1, 'Title': 'Erro no login', 'Status': 'Aberto'},
                {'Id': 2, 'Title': 'Falha no envio de email', 'Status': 'Em andamento'}
            ]
            self.wfile.write(json.dumps(tickets).encode())
        elif self.path.startswith('/fragments/'):
            frag = self.path[11:]
            if frag.endswith('.html'):
                frag = frag[:-5]
            file_path = os.path.join('fragments', frag + '.html')
            if os.path.exists(file_path):
                self.send_response(200)
                self.send_header('Content-type', 'text/html')
                self.end_headers()
                with open(file_path, 'r') as f:
                    self.wfile.write(f.read().encode())
            else:
                self.send_error(404)
        else:
            self.send_error(404)

    def do_POST(self):
        if self.path == '/login':
            content_length = int(self.headers['Content-Length'])
            post_data = self.rfile.read(content_length)
            data = json.loads(post_data)
            if data.get('email') == 'gabriel.damiao@farmatech.com' and data.get('password') == '123456':
                response = {'success': True, 'token': 'mock-token'}
            else:
                response = {'success': False, 'message': 'Email ou senha incorretos'}
            self.send_response(200)
            self.send_header('Content-type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps(response).encode())
        elif self.path == '/chat':
            content_length = int(self.headers['Content-Length'])
            post_data = self.rfile.read(content_length)
            data = json.loads(post_data)
            message = data.get('message', '')
            ai_response = self.get_gemini_response(message)
            response = {'Response': ai_response}
            self.send_response(200)
            self.send_header('Content-type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps(response).encode())
        else:
            self.send_error(405)

    def get_gemini_response(self, message):
        try:
            api_key = 'AIzaSyDafrIdzLzhIfOdK8oRFmwyleYpmDcK3Mc'
            url = f'https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={api_key}'
            headers = {'Content-Type': 'application/json'}
            payload = {
                'contents': [{
                    'parts': [{'text': message}]
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

with socketserver.TCPServer(('', 5001), Handler) as httpd:
    print('Servidor rodando em http://localhost:5001/')
    httpd.serve_forever()