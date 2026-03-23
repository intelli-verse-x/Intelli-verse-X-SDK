import 'package:test/test.dart';
import 'package:intelliversex_sdk/intelliversex_sdk.dart';

void main() {
  setUp(() {
    IVXManager.resetInstance();
  });

  group('IVXManager singleton', () {
    test('returns the same instance', () {
      final a = IVXManager.instance;
      final b = IVXManager.instance;
      expect(identical(a, b), isTrue);
    });

    test('resetInstance creates a new instance', () {
      final a = IVXManager.instance;
      IVXManager.resetInstance();
      final b = IVXManager.instance;
      expect(identical(a, b), isFalse);
    });
  });

  group('pre-initialization state', () {
    test('isInitialized is false', () {
      expect(IVXManager.instance.isInitialized, isFalse);
    });

    test('userId is empty', () {
      expect(IVXManager.instance.userId, isEmpty);
    });

    test('username is empty', () {
      expect(IVXManager.instance.username, isEmpty);
    });

    test('hasValidSession is false', () {
      expect(IVXManager.instance.hasValidSession, isFalse);
    });
  });

  group('initialize', () {
    test('sets isInitialized to true with valid config', () {
      IVXManager.instance.initialize(const IVXConfig(
        nakamaHost: '127.0.0.1',
        nakamaPort: 7350,
        nakamaServerKey: 'testkey',
      ));
      expect(IVXManager.instance.isInitialized, isTrue);
      expect(IVXManager.instance.client, isNotNull);
    });

    test('emits initialized event', () {
      var called = false;
      IVXManager.instance.on(IVXEvent.initialized, (_) => called = true);
      IVXManager.instance.initialize(const IVXConfig());
      expect(called, isTrue);
    });
  });

  group('auth guard', () {
    test('authenticateDevice throws before initialize', () {
      expect(
        () => IVXManager.instance.authenticateDevice(),
        throwsA(isA<IVXError>()),
      );
    });

    test('fetchProfile throws without session', () {
      IVXManager.instance.initialize(const IVXConfig());
      expect(
        () => IVXManager.instance.fetchProfile(),
        throwsA(isA<IVXError>()),
      );
    });

    test('callRpc throws without session', () {
      IVXManager.instance.initialize(const IVXConfig());
      expect(
        () => IVXManager.instance.callRpc('test_rpc'),
        throwsA(isA<IVXError>()),
      );
    });
  });

  group('session management', () {
    test('clearSession resets session state', () {
      IVXManager.instance.initialize(const IVXConfig());
      IVXManager.instance.clearSession();
      expect(IVXManager.instance.hasValidSession, isFalse);
      expect(IVXManager.instance.userId, isEmpty);
    });
  });

  group('events', () {
    test('on/off handler registration', () {
      var count = 0;
      handler(dynamic _) => count++;

      IVXManager.instance.on(IVXEvent.error, handler);
      IVXManager.instance.off(IVXEvent.error, handler);

      IVXManager.instance.initialize(const IVXConfig());
      expect(count, 0);
    });
  });

  group('IVXConfig validation', () {
    test('accepts valid config', () {
      expect(
        () => const IVXConfig(nakamaHost: 'localhost', nakamaPort: 7350)
            .validate(),
        returnsNormally,
      );
    });

    test('rejects port 0', () {
      expect(
        () => const IVXConfig(nakamaPort: 0).validate(),
        throwsA(isA<ArgumentError>()),
      );
    });

    test('rejects port > 65535', () {
      expect(
        () => const IVXConfig(nakamaPort: 99999).validate(),
        throwsA(isA<ArgumentError>()),
      );
    });

    test('rejects empty host', () {
      expect(
        () => const IVXConfig(nakamaHost: '  ').validate(),
        throwsA(isA<ArgumentError>()),
      );
    });

    test('rejects empty server key', () {
      expect(
        () => const IVXConfig(nakamaServerKey: '').validate(),
        throwsA(isA<ArgumentError>()),
      );
    });
  });

  group('types', () {
    test('sdkVersion is a valid semver string', () {
      expect(sdkVersion, matches(RegExp(r'^\d+\.\d+\.\d+$')));
    });

    test('IVXProfile toString', () {
      const profile = IVXProfile(userId: 'abc', username: 'test');
      expect(profile.toString(), contains('abc'));
    });

    test('IVXError toString', () {
      const error = IVXError(code: 42, message: 'fail');
      expect(error.toString(), contains('42'));
      expect(error.toString(), contains('fail'));
    });
  });
}
