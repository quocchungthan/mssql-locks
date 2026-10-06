import { copyFile, mkdir } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const projectRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const packageRoot = path.join(projectRoot, 'node_modules', '@microsoft', 'signalr');
const destination = path.join(projectRoot, 'wwwroot', 'lib', 'signalr');

await mkdir(destination, { recursive: true });
await copyFile(path.join(packageRoot, 'dist', 'browser', 'signalr.min.js'), path.join(destination, 'signalr.min.js'));
