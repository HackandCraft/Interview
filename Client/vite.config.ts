// Note this only includes basic configuration for development mode.
// For a more comprehensive configuration check:
// https://github.com/fable-compiler/webpack-config-template

import { defineConfig } from 'vite';

export default defineConfig({
    server: {
        port: 5174,
        watch: {
            ignored: (p => { return p.includes("ace-builds") || p.endsWith(".fs"); }),
            usePolling: true,
        },
        open: true
    }
});
