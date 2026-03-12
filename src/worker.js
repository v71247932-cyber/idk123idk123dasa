// src/worker.js
// Worker simplu — servește assets-urile statice (index.html etc.)
// Cloudflare Workers Assets gestionează toate fișierele din directorul definit în wrangler.toml

export default {
    async fetch(request, env) {
        // env.ASSETS e injectat automat de Cloudflare când [assets] e definit în wrangler.toml
        return env.ASSETS.fetch(request);
    },
};
