/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_SATORI_URL: string;
  readonly VITE_SATORI_API_KEY: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
