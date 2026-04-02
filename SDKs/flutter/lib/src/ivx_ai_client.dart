import 'dart:async';
import 'dart:convert';

import 'package:http/http.dart' as http;

import 'types.dart';

// ---------------------------------------------------------------------------
// Models
// ---------------------------------------------------------------------------

/// Represents a response when starting an AI voice or host session.
class IVXAISessionResponse {
  final String sessionId;
  final String status;
  final Map<String, dynamic> metadata;

  const IVXAISessionResponse({
    required this.sessionId,
    this.status = 'active',
    this.metadata = const {},
  });

  factory IVXAISessionResponse.fromJson(Map<String, dynamic> json) =>
      IVXAISessionResponse(
        sessionId: json['session_id'] as String? ?? '',
        status: json['status'] as String? ?? 'active',
        metadata: json['metadata'] as Map<String, dynamic>? ?? const {},
      );

  @override
  String toString() =>
      'IVXAISessionResponse(sessionId: $sessionId, status: $status)';
}

/// A single message within an AI conversation.
class IVXAIMessage {
  final String id;
  final String role;
  final String content;
  final DateTime timestamp;
  final Map<String, dynamic> metadata;

  const IVXAIMessage({
    required this.id,
    required this.role,
    required this.content,
    required this.timestamp,
    this.metadata = const {},
  });

  factory IVXAIMessage.fromJson(Map<String, dynamic> json) => IVXAIMessage(
        id: json['id'] as String? ?? '',
        role: json['role'] as String? ?? 'assistant',
        content: json['content'] as String? ?? '',
        timestamp: DateTime.tryParse(json['timestamp'] as String? ?? '') ??
            DateTime.now(),
        metadata: json['metadata'] as Map<String, dynamic>? ?? const {},
      );

  @override
  String toString() => 'IVXAIMessage(id: $id, role: $role)';
}

/// Profile data supplied when creating an AI-hosted session.
class IVXHostProfile {
  final String name;
  final String personaId;
  final String style;
  final Map<String, dynamic> config;

  const IVXHostProfile({
    required this.name,
    required this.personaId,
    this.style = 'default',
    this.config = const {},
  });

  Map<String, dynamic> toJson() => {
        'name': name,
        'persona_id': personaId,
        'style': style,
        'config': config,
      };

  @override
  String toString() => 'IVXHostProfile(name: $name, personaId: $personaId)';
}

/// Entitlement status for AI features.
class IVXAIEntitlement {
  final bool hasAccess;
  final String tier;
  final int remainingCredits;
  final DateTime? expiresAt;

  const IVXAIEntitlement({
    required this.hasAccess,
    this.tier = 'free',
    this.remainingCredits = 0,
    this.expiresAt,
  });

  factory IVXAIEntitlement.fromJson(Map<String, dynamic> json) =>
      IVXAIEntitlement(
        hasAccess: json['has_access'] as bool? ?? false,
        tier: json['tier'] as String? ?? 'free',
        remainingCredits: json['remaining_credits'] as int? ?? 0,
        expiresAt: json['expires_at'] != null
            ? DateTime.tryParse(json['expires_at'] as String)
            : null,
      );

  @override
  String toString() =>
      'IVXAIEntitlement(tier: $tier, hasAccess: $hasAccess, credits: $remainingCredits)';
}

/// Descriptor for an available AI persona.
class IVXAIPersona {
  final String id;
  final String name;
  final String description;
  final String avatarUrl;
  final List<String> supportedLanguages;

  const IVXAIPersona({
    required this.id,
    required this.name,
    this.description = '',
    this.avatarUrl = '',
    this.supportedLanguages = const ['en'],
  });

  factory IVXAIPersona.fromJson(Map<String, dynamic> json) => IVXAIPersona(
        id: json['id'] as String? ?? '',
        name: json['name'] as String? ?? '',
        description: json['description'] as String? ?? '',
        avatarUrl: json['avatar_url'] as String? ?? '',
        supportedLanguages:
            (json['supported_languages'] as List<dynamic>?)
                    ?.cast<String>() ??
                const ['en'],
      );

  @override
  String toString() => 'IVXAIPersona(id: $id, name: $name)';
}

// ---------------------------------------------------------------------------
// Client
// ---------------------------------------------------------------------------

/// HTTP client for the IntelliVerseX AI service.
///
/// Provides voice-session management, text messaging, AI-hosted game
/// sessions, entitlement checks, and persona queries against a remote API.
///
/// ```dart
/// final ai = IVXAIClient.instance;
/// await ai.initialize(apiBaseUrl: 'https://ai.example.com');
/// final session = await ai.startVoiceSession('persona_1', userId);
/// ```
class IVXAIClient {
  static IVXAIClient? _instance;

  late String _baseUrl;
  String? _apiKey;
  bool _initialized = false;
  late http.Client _httpClient;

  IVXAIClient._();

  /// Singleton accessor.
  static IVXAIClient get instance => _instance ??= IVXAIClient._();

  /// Reset the singleton (useful for testing).
  static void resetInstance() => _instance = null;

  /// Whether [initialize] has been called successfully.
  bool get isInitialized => _initialized;

  // ---------------------------------------------------------------------------
  // Initialization
  // ---------------------------------------------------------------------------

  /// Set up the AI client with the remote [apiBaseUrl].
  ///
  /// An optional [apiKey] is sent as a `Bearer` token on every request.
  Future<void> initialize({
    required String apiBaseUrl,
    String? apiKey,
  }) async {
    if (apiBaseUrl.trim().isEmpty) {
      throw const IVXError(code: -1, message: 'apiBaseUrl cannot be empty.');
    }
    _baseUrl = apiBaseUrl.endsWith('/') ? apiBaseUrl : '$apiBaseUrl/';
    _apiKey = apiKey;
    _httpClient = http.Client();
    _initialized = true;
    _log('AI client initialized — $_baseUrl');
  }

  // ---------------------------------------------------------------------------
  // Voice sessions
  // ---------------------------------------------------------------------------

  /// Start a voice conversation session for [personaId] on behalf of [userId].
  ///
  /// An optional [language] tag (e.g. `"en"`, `"es"`) sets the session locale.
  Future<IVXAISessionResponse> startVoiceSession(
    String personaId,
    String userId, {
    String? language,
  }) async {
    _ensureInitialized();
    final body = {
      'persona_id': personaId,
      'user_id': userId,
      if (language != null) 'language': language,
    };
    final json = await _post('v1/voice/sessions', body);
    return IVXAISessionResponse.fromJson(json);
  }

  /// Gracefully end the voice session identified by [sessionId].
  Future<void> endVoiceSession(String sessionId) async {
    _ensureInitialized();
    await _post('v1/voice/sessions/$sessionId/end', {});
    _log('Voice session $sessionId ended');
  }

  // ---------------------------------------------------------------------------
  // Text messaging
  // ---------------------------------------------------------------------------

  /// Send a [text] message into an active session.
  Future<void> sendText(String sessionId, String text) async {
    _ensureInitialized();
    await _post('v1/sessions/$sessionId/messages', {'text': text});
  }

  /// Poll new messages for [sessionId], optionally filtering to those after
  /// [since].
  Future<List<IVXAIMessage>> pollMessages(
    String sessionId, {
    DateTime? since,
  }) async {
    _ensureInitialized();
    final query = <String, String>{};
    if (since != null) query['since'] = since.toUtc().toIso8601String();
    final json = await _get('v1/sessions/$sessionId/messages', query);
    final items = json['messages'] as List<dynamic>? ?? [];
    return items
        .cast<Map<String, dynamic>>()
        .map(IVXAIMessage.fromJson)
        .toList();
  }

  // ---------------------------------------------------------------------------
  // Host sessions
  // ---------------------------------------------------------------------------

  /// Create an AI-hosted game session tied to [matchId].
  Future<IVXAISessionResponse> startHostSession(
    String matchId,
    IVXHostProfile profile,
  ) async {
    _ensureInitialized();
    final body = {
      'match_id': matchId,
      ...profile.toJson(),
    };
    final json = await _post('v1/host/sessions', body);
    return IVXAISessionResponse.fromJson(json);
  }

  /// Push a game event of [eventType] with JSON [data] into a host session.
  Future<void> sendHostEvent(
    String sessionId,
    String eventType,
    String data,
  ) async {
    _ensureInitialized();
    await _post('v1/host/sessions/$sessionId/events', {
      'event_type': eventType,
      'data': data,
    });
  }

  // ---------------------------------------------------------------------------
  // Entitlements & personas
  // ---------------------------------------------------------------------------

  /// Check AI feature entitlement for [userId].
  Future<IVXAIEntitlement> checkEntitlement(String userId) async {
    _ensureInitialized();
    final json = await _get('v1/entitlements/$userId', {});
    return IVXAIEntitlement.fromJson(json);
  }

  /// Retrieve the list of available AI personas.
  Future<List<IVXAIPersona>> getPersonas() async {
    _ensureInitialized();
    final json = await _get('v1/personas', {});
    final items = json['personas'] as List<dynamic>? ?? [];
    return items
        .cast<Map<String, dynamic>>()
        .map(IVXAIPersona.fromJson)
        .toList();
  }

  // ---------------------------------------------------------------------------
  // HTTP helpers
  // ---------------------------------------------------------------------------

  Map<String, String> get _headers => {
        'Content-Type': 'application/json',
        if (_apiKey != null) 'Authorization': 'Bearer $_apiKey',
      };

  Future<Map<String, dynamic>> _post(
    String path,
    Map<String, dynamic> body,
  ) async {
    final uri = Uri.parse('$_baseUrl$path');
    final response = await _httpClient.post(
      uri,
      headers: _headers,
      body: jsonEncode(body),
    );
    return _handleResponse(response);
  }

  Future<Map<String, dynamic>> _get(
    String path,
    Map<String, String> queryParams,
  ) async {
    final uri =
        Uri.parse('$_baseUrl$path').replace(queryParameters: queryParams);
    final response = await _httpClient.get(uri, headers: _headers);
    return _handleResponse(response);
  }

  Map<String, dynamic> _handleResponse(http.Response response) {
    if (response.statusCode >= 200 && response.statusCode < 300) {
      if (response.body.isEmpty) return {};
      final decoded = jsonDecode(response.body);
      if (decoded is Map<String, dynamic>) return decoded;
      return {'data': decoded};
    }
    throw IVXError(
      code: response.statusCode,
      message: 'AI API error ${response.statusCode}: ${response.body}',
    );
  }

  // ---------------------------------------------------------------------------
  // Internal
  // ---------------------------------------------------------------------------

  void _ensureInitialized() {
    if (!_initialized) {
      throw const IVXError(
        code: -1,
        message: 'IVXAIClient not initialized. Call initialize() first.',
      );
    }
  }

  void _log(String message) {
    // ignore: avoid_print
    print('[IntelliVerseX:AI] $message');
  }
}
