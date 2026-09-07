/** Teaching example: one public room, phone controls a dot on the kiosk. No rewards or payments. */
import http from 'node:http';
import { randomBytes } from 'node:crypto';
import { readFile } from 'node:fs/promises';

const room = randomBytes(16).toString('hex');
const state = { x: 50, moves: 0 };
const html = await readFile(new URL('./screen.html', import.meta.url));
const port = Number(process.env.PORT || 4141);
const publicOrigin = process.env.PUBLIC_ORIGIN || `http://localhost:${port}`;
let origin = new URL(publicOrigin);
if (origin.pathname !== '/' || origin.search || origin.hash || origin.username || origin.password) throw new Error('PUBLIC_ORIGIN must be an origin');
const server = http.createServer(async (req, res) => {
  const url = new URL(req.url, publicOrigin);
  res.setHeader('Cache-Control', 'no-store');
  res.setHeader('Referrer-Policy', 'no-referrer');
  res.setHeader('Content-Security-Policy', "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; object-src 'none'; base-uri 'none'");
  if (url.searchParams.get('room') !== room) { res.writeHead(404).end('Room not found'); return; }
  if (req.method === 'GET' && ['/display', '/controller'].includes(url.pathname)) {
    res.setHeader('Content-Type', 'text/html'); res.end(html); return;
  }
  if (req.method === 'GET' && url.pathname === '/api/state') {
    res.setHeader('Content-Type', 'application/json'); res.end(JSON.stringify(state)); return;
  }
  if (req.method === 'POST' && url.pathname === '/api/input') {
    if (req.headers.origin !== origin.origin) { res.writeHead(403).end(); return; }
    let input = ''; let bytes = 0;
    try {
      for await (const chunk of req) { bytes += chunk.length; if (bytes > 128) { res.writeHead(413).end(); return; } input += chunk; }
      const {direction} = JSON.parse(input);
      if (direction !== -1 && direction !== 1) { res.writeHead(400).end(); return; }
      state.x = Math.max(5, Math.min(95, state.x + direction * 5)); state.moves++;
      res.writeHead(204).end();
    } catch { res.writeHead(400).end(); }
    return;
  }
  res.writeHead(404).end();
});
server.listen(port, '0.0.0.0', () => {
  if (!process.env.PUBLIC_ORIGIN) origin = new URL(`http://localhost:${server.address().port}`);
  const manifest = {schemaVersion:1, appId:'two-screen-demo', name:'Move beyond mobile',
    kioskUrl:`${origin.origin}/display?room=${room}`, controllerUrl:`${origin.origin}/controller?room=${room}`, sessionSeconds:180};
  console.log('Open the display in one window and controller in another. Host behind HTTPS before publishing.');
  console.log(JSON.stringify(manifest, null, 2));
});
