/** Run on your server/terminal only. Never put KIOSKX_API_KEY in a browser build. */
import {readFile} from 'node:fs/promises';
import {IVXKioskPublisher} from '../../dist/index.mjs';
const [machineNo, manifestPath, mode = 'stage'] = process.argv.slice(2);
if (!machineNo || !manifestPath || !['stage','publish'].includes(mode)) throw new Error('Usage: node publish.mjs MACHINE manifest.json [stage|publish]');
const client = new IVXKioskPublisher({apiKey:()=>process.env.KIOSKX_API_KEY || ''});
const manifest = JSON.parse(await readFile(manifestPath, 'utf8'));
const current = await client.get(machineNo);
console.log(await client.publish(machineNo, current.revision, manifest, mode === 'publish'));
