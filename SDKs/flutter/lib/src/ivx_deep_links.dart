// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

import 'dart:async';

/// Parsed result of a deep link URL.
class DeepLinkResult {
  final bool matched;
  final String scheme;
  final String host;
  final String route;
  final Map<String, String> params;
  final String raw;

  const DeepLinkResult({
    required this.matched,
    this.scheme = '',
    this.host = '',
    this.route = '',
    this.params = const {},
    this.raw = '',
  });
}

/// Handler signature for registered deep link routes.
typedef DeepLinkHandler = void Function(
    Map<String, String> params, DeepLinkResult result);

/// Lightweight deep link parser and dispatcher.
///
/// Parses URLs in the format `{scheme}://{host}/{route}?key=value`
/// and fires registered handlers for matching routes.
class IVXDeepLinks {
  static IVXDeepLinks? _instance;

  String _scheme = '';
  String _host = '';
  bool _initialized = false;

  final Map<String, List<DeepLinkHandler>> _handlers = {};
  final StreamController<DeepLinkResult> _linkController =
      StreamController<DeepLinkResult>.broadcast();

  IVXDeepLinks._();

  /// Shared singleton instance.
  static IVXDeepLinks get instance => _instance ??= IVXDeepLinks._();

  /// Stream of all successfully matched deep links.
  Stream<DeepLinkResult> get onDeepLink => _linkController.stream;

  /// Whether [initialize] has been called.
  bool get isInitialized => _initialized;

  /// Configure the expected scheme and host.
  void initialize({required String scheme, required String host}) {
    _scheme = scheme;
    _host = host;
    _initialized = true;
  }

  /// Parse [url] and dispatch to registered handlers.
  DeepLinkResult handleUrl(String url) {
    final result = _parse(url);
    if (result.matched) {
      _dispatch(result);
      _linkController.add(result);
    }
    return result;
  }

  /// Register a [handler] for a specific [route].
  void registerHandler(String route, DeepLinkHandler handler) {
    _handlers.putIfAbsent(route, () => []);
    _handlers[route]!.add(handler);
  }

  /// Remove a previously registered [handler] from a [route].
  void removeHandler(String route, DeepLinkHandler handler) {
    _handlers[route]?.remove(handler);
  }

  /// Remove all handlers, or only those for a specific [route].
  void removeAllHandlers([String? route]) {
    if (route != null) {
      _handlers.remove(route);
    } else {
      _handlers.clear();
    }
  }

  /// Release resources. After calling this, [instance] returns a fresh object.
  void dispose() {
    _linkController.close();
    _handlers.clear();
    _instance = null;
  }

  DeepLinkResult _parse(String url) {
    const empty = DeepLinkResult(matched: false);

    final schemeEnd = url.indexOf('://');
    if (schemeEnd == -1) return DeepLinkResult(matched: false, raw: url);

    final scheme = url.substring(0, schemeEnd);
    final rest = url.substring(schemeEnd + 3);

    final pathStart = rest.indexOf('/');
    final host = pathStart == -1 ? rest : rest.substring(0, pathStart);

    if (_initialized && (scheme != _scheme || host != _host)) {
      return DeepLinkResult(matched: false, raw: url);
    }

    final pathAndQuery = pathStart == -1 ? '' : rest.substring(pathStart + 1);
    final queryStart = pathAndQuery.indexOf('?');
    final route =
        queryStart == -1 ? pathAndQuery : pathAndQuery.substring(0, queryStart);
    final queryString =
        queryStart == -1 ? '' : pathAndQuery.substring(queryStart + 1);

    final params = <String, String>{};
    if (queryString.isNotEmpty) {
      for (final pair in queryString.split('&')) {
        final eqIdx = pair.indexOf('=');
        if (eqIdx == -1) {
          params[Uri.decodeComponent(pair)] = '';
        } else {
          params[Uri.decodeComponent(pair.substring(0, eqIdx))] =
              Uri.decodeComponent(pair.substring(eqIdx + 1));
        }
      }
    }

    return DeepLinkResult(
      matched: true,
      scheme: scheme,
      host: host,
      route: route,
      params: params,
      raw: url,
    );
  }

  void _dispatch(DeepLinkResult result) {
    final handlers = _handlers[result.route];
    if (handlers == null) return;
    for (final handler in List<DeepLinkHandler>.of(handlers)) {
      try {
        handler(result.params, result);
      } catch (_) {
        // Handler errors are silently swallowed to avoid cascading failures.
      }
    }
  }
}
