// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

export interface DeepLinkConfig {
  scheme: string;
  host: string;
}

export interface DeepLinkResult {
  matched: boolean;
  scheme: string;
  host: string;
  route: string;
  params: Record<string, string>;
  raw: string;
}

export type DeepLinkHandler = (params: Record<string, string>, result: DeepLinkResult) => void;

export class IVXDeepLinks {
  private static _instance: IVXDeepLinks | null = null;
  private _scheme = '';
  private _host = '';
  private _initialized = false;
  private _handlers = new Map<string, Set<DeepLinkHandler>>();

  private constructor() {}

  static getInstance(): IVXDeepLinks {
    if (!IVXDeepLinks._instance) {
      IVXDeepLinks._instance = new IVXDeepLinks();
    }
    return IVXDeepLinks._instance;
  }

  static initialize(config: DeepLinkConfig): void {
    const inst = IVXDeepLinks.getInstance();
    inst._scheme = config.scheme;
    inst._host = config.host;
    inst._initialized = true;
  }

  static handleUrl(url: string): DeepLinkResult {
    const inst = IVXDeepLinks.getInstance();
    const result = inst._parse(url);
    if (result.matched) {
      inst._dispatch(result);
    }
    return result;
  }

  static registerHandler(route: string, handler: DeepLinkHandler): void {
    const inst = IVXDeepLinks.getInstance();
    if (!inst._handlers.has(route)) {
      inst._handlers.set(route, new Set());
    }
    inst._handlers.get(route)!.add(handler);
  }

  static removeHandler(route: string, handler: DeepLinkHandler): void {
    const inst = IVXDeepLinks.getInstance();
    inst._handlers.get(route)?.delete(handler);
  }

  static removeAllHandlers(route?: string): void {
    const inst = IVXDeepLinks.getInstance();
    if (route) {
      inst._handlers.delete(route);
    } else {
      inst._handlers.clear();
    }
  }

  static get isInitialized(): boolean {
    return IVXDeepLinks.getInstance()._initialized;
  }

  private _parse(url: string): DeepLinkResult {
    const empty: DeepLinkResult = {
      matched: false,
      scheme: '',
      host: '',
      route: '',
      params: {},
      raw: url,
    };

    const schemeEnd = url.indexOf('://');
    if (schemeEnd === -1) return empty;

    const scheme = url.substring(0, schemeEnd);
    const rest = url.substring(schemeEnd + 3);

    const pathStart = rest.indexOf('/');
    const host = pathStart === -1 ? rest : rest.substring(0, pathStart);

    if (this._initialized && (scheme !== this._scheme || host !== this._host)) {
      return empty;
    }

    let pathAndQuery = pathStart === -1 ? '' : rest.substring(pathStart + 1);
    const queryStart = pathAndQuery.indexOf('?');
    const route = queryStart === -1 ? pathAndQuery : pathAndQuery.substring(0, queryStart);
    const queryString = queryStart === -1 ? '' : pathAndQuery.substring(queryStart + 1);

    const params: Record<string, string> = {};
    if (queryString) {
      for (const pair of queryString.split('&')) {
        const eqIdx = pair.indexOf('=');
        if (eqIdx === -1) {
          params[decodeURIComponent(pair)] = '';
        } else {
          params[decodeURIComponent(pair.substring(0, eqIdx))] =
            decodeURIComponent(pair.substring(eqIdx + 1));
        }
      }
    }

    return { matched: true, scheme, host, route, params, raw: url };
  }

  private _dispatch(result: DeepLinkResult): void {
    const handlers = this._handlers.get(result.route);
    if (handlers) {
      for (const handler of handlers) {
        try {
          handler(result.params, result);
        } catch (_) {
          // Handler errors are silently swallowed to avoid cascading failures.
        }
      }
    }
  }
}
