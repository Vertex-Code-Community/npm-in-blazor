import { defineConfig } from "vite";

export default defineConfig({
    build: {
        lib: {
            entry: "src/index.js",
            name: "simpleStats",          // global name
            formats: ["iife"],            // one bundle for <script>
            fileName: () => "index.js",
        },
        outDir: "dist",
        emptyOutDir: true,
        sourcemap: true,
        minify: true,
    },
});