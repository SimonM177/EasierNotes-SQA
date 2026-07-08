// Servidor MOCK mínimo que simula la API de EasierNotes.
// SOLO para demostrar que la colección Postman pasa en verde.
// NO es el backend real (ese es el .NET del autor); reproduce el contrato HTTP.
const http = require('http');

let notes = [];
let nextId = 1;

function send(res, code, body) {
  res.writeHead(code, { 'Content-Type': 'application/json' });
  res.end(body !== undefined ? JSON.stringify(body) : '');
}

const server = http.createServer((req, res) => {
  const { method, url } = req;
  let bodyRaw = '';
  req.on('data', c => bodyRaw += c);
  req.on('end', () => {
    // POST /api/notes/create
    if (method === 'POST' && url === '/api/notes/create') {
      const qty = notes.filter(n => n.name.startsWith('Nueva Nota')).length;
      const note = {
        id: nextId++,
        name: qty > 0 ? `Nueva Nota (${qty + 1})` : 'Nueva Nota',
        html: '<p> Comienza a plasmar tus ideas aquí...</p>',
        categoryId: 1
      };
      notes.push(note);
      return send(res, 200, note);
    }
    // GET /api/notes
    if (method === 'GET' && url === '/api/notes') {
      return send(res, 200, notes);
    }
    // GET /api/notes/{id}
    if (method === 'GET' && /^\/api\/notes\/\d+$/.test(url)) {
      const id = Number(url.split('/').pop());
      const note = notes.find(n => n.id === id);
      return note ? send(res, 200, note) : send(res, 404);
    }
    // PUT /api/notes/update
    if (method === 'PUT' && url === '/api/notes/update') {
      const dto = JSON.parse(bodyRaw || '{}');
      const note = notes.find(n => n.id === Number(dto.id));
      if (!note) return send(res, 404);
      note.name = dto.name; note.html = dto.html;
      return send(res, 204);
    }
    // DELETE /api/notes/delete/{id}
    if (method === 'DELETE' && /^\/api\/notes\/delete\/\d+$/.test(url)) {
      const id = Number(url.split('/').pop());
      const idx = notes.findIndex(n => n.id === id);
      if (idx < 0) return send(res, 404);
      notes.splice(idx, 1);
      return send(res, 204);
    }
    // PATCH /api/notes/addToCategory/{noteId}/{categoryId}
    if (method === 'PATCH' && /^\/api\/notes\/addToCategory\/\d+\/\d+$/.test(url)) {
      const parts = url.split('/');
      const noteId = Number(parts[4]);
      const note = notes.find(n => n.id === noteId);
      if (!note) return send(res, 404);
      note.categoryId = Number(parts[5]);
      return send(res, 204);
    }
    send(res, 404);
  });
});

server.listen(5219, () => console.log('Mock API en http://localhost:5219'));
