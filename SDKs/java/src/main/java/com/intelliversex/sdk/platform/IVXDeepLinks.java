// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

package com.intelliversex.sdk.platform;

import java.io.UnsupportedEncodingException;
import java.net.URLDecoder;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.CopyOnWriteArrayList;

/**
 * Lightweight deep link parser and dispatcher.
 *
 * <p>Parses URLs in the format {@code {scheme}://{host}/{route}?key=value}
 * and fires registered handlers for matching routes.</p>
 */
public final class IVXDeepLinks {

    /** Callback interface for deep link route handlers. */
    @FunctionalInterface
    public interface DeepLinkHandler {
        void onDeepLink(Map<String, String> params, DeepLinkResult result);
    }

    /** Parsed result of a deep link URL. */
    public static final class DeepLinkResult {
        private final boolean matched;
        private final String scheme;
        private final String host;
        private final String route;
        private final Map<String, String> params;
        private final String raw;

        DeepLinkResult(boolean matched, String scheme, String host,
                       String route, Map<String, String> params, String raw) {
            this.matched = matched;
            this.scheme = scheme;
            this.host = host;
            this.route = route;
            this.params = Collections.unmodifiableMap(params);
            this.raw = raw;
        }

        public boolean isMatched()             { return matched; }
        public String getScheme()              { return scheme; }
        public String getHost()                { return host; }
        public String getRoute()               { return route; }
        public Map<String, String> getParams() { return params; }
        public String getRaw()                 { return raw; }
    }

    private static volatile IVXDeepLinks instance;

    private String scheme = "";
    private String host = "";
    private volatile boolean initialized;
    private final Map<String, List<DeepLinkHandler>> handlers = new HashMap<>();

    private IVXDeepLinks() {}

    /** Thread-safe singleton accessor. */
    public static IVXDeepLinks getInstance() {
        if (instance == null) {
            synchronized (IVXDeepLinks.class) {
                if (instance == null) {
                    instance = new IVXDeepLinks();
                }
            }
        }
        return instance;
    }

    /** Configure the expected scheme and host. */
    public void initialize(String scheme, String host) {
        this.scheme = scheme;
        this.host = host;
        this.initialized = true;
    }

    /** Whether {@link #initialize} has been called. */
    public boolean isInitialized() {
        return initialized;
    }

    /**
     * Parse {@code url} and dispatch to registered handlers.
     *
     * @param url the deep link URL to handle
     * @return parsed result with route and query parameters
     */
    public DeepLinkResult handleUrl(String url) {
        DeepLinkResult result = parse(url);
        if (result.matched) {
            dispatch(result);
        }
        return result;
    }

    /** Register a {@code handler} for a specific {@code route}. */
    public synchronized void registerHandler(String route, DeepLinkHandler handler) {
        handlers.computeIfAbsent(route, k -> new CopyOnWriteArrayList<>()).add(handler);
    }

    /** Remove a previously registered {@code handler} from a {@code route}. */
    public synchronized void removeHandler(String route, DeepLinkHandler handler) {
        List<DeepLinkHandler> list = handlers.get(route);
        if (list != null) {
            list.remove(handler);
        }
    }

    /** Remove all handlers, or only those for a specific {@code route}. */
    public synchronized void removeAllHandlers(String route) {
        if (route != null) {
            handlers.remove(route);
        } else {
            handlers.clear();
        }
    }

    /** Remove all handlers for every route. */
    public synchronized void removeAllHandlers() {
        handlers.clear();
    }

    // ------------------------------------------------------------------

    private DeepLinkResult parse(String url) {
        Map<String, String> emptyParams = Collections.emptyMap();

        int schemeEnd = url.indexOf("://");
        if (schemeEnd == -1) {
            return new DeepLinkResult(false, "", "", "", emptyParams, url);
        }

        String parsedScheme = url.substring(0, schemeEnd);
        String rest = url.substring(schemeEnd + 3);

        int pathStart = rest.indexOf('/');
        String parsedHost = pathStart == -1 ? rest : rest.substring(0, pathStart);

        if (initialized && (!parsedScheme.equals(scheme) || !parsedHost.equals(host))) {
            return new DeepLinkResult(false, "", "", "", emptyParams, url);
        }

        String pathAndQuery = pathStart == -1 ? "" : rest.substring(pathStart + 1);
        int queryStart = pathAndQuery.indexOf('?');
        String route = queryStart == -1 ? pathAndQuery : pathAndQuery.substring(0, queryStart);
        String queryString = queryStart == -1 ? "" : pathAndQuery.substring(queryStart + 1);

        Map<String, String> params = new HashMap<>();
        if (!queryString.isEmpty()) {
            for (String pair : queryString.split("&")) {
                int eq = pair.indexOf('=');
                if (eq == -1) {
                    params.put(urlDecode(pair), "");
                } else {
                    params.put(urlDecode(pair.substring(0, eq)), urlDecode(pair.substring(eq + 1)));
                }
            }
        }

        return new DeepLinkResult(true, parsedScheme, parsedHost, route, params, url);
    }

    private void dispatch(DeepLinkResult result) {
        List<DeepLinkHandler> list;
        synchronized (this) {
            list = handlers.get(result.route);
        }
        if (list == null) return;
        for (DeepLinkHandler handler : list) {
            try {
                handler.onDeepLink(result.params, result);
            } catch (Exception ignored) {
                // Handler errors are silently swallowed to avoid cascading failures.
            }
        }
    }

    private static String urlDecode(String s) {
        try {
            return URLDecoder.decode(s, "UTF-8");
        } catch (UnsupportedEncodingException e) {
            return s;
        }
    }
}
