import {test} from 'node:test';
import assert from 'node:assert/strict';
import {spawn} from 'node:child_process';
test('phone input changes the display room and refuses foreign requests', async t => {
  const child = spawn(process.execPath, [new URL('./server.mjs', import.meta.url).pathname], {env:{...process.env,PORT:'0',PUBLIC_ORIGIN:''}});
  t.after(()=>child.kill());
  const manifest = await new Promise((resolve,reject)=>{
    let text=''; const timeout=setTimeout(()=>reject(new Error('Demo did not start')),5000);
    child.stdout.on('data', chunk=>{text+=chunk;try{const value=JSON.parse(text.slice(text.indexOf('{')));clearTimeout(timeout);resolve(value);}catch{}});
    child.on('error',reject);
  });
  const display = new URL(manifest.kioskUrl);
  const room = display.search;
  assert.equal((await fetch(manifest.kioskUrl)).status,200);
  assert.equal((await fetch(manifest.controllerUrl)).status,200);
  const input=display.origin+'/api/input'+room;
  assert.equal((await fetch(input,{method:'POST',headers:{Origin:'https://foreign.example'},body:'{"direction":1}'})).status,403);
  assert.equal((await fetch(input,{method:'POST',headers:{Origin:display.origin},body:'{"direction":1}'})).status,204);
  assert.deepEqual(await (await fetch(display.origin+'/api/state'+room)).json(),{x:55,moves:1});
  assert.equal((await fetch(display.origin+'/api/state?room=wrong')).status,404);
});
