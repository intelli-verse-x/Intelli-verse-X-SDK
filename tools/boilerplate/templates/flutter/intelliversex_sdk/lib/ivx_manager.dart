class IvxManager {
  static final IvxManager instance = IvxManager._();
  IvxManager._();

  Future<void> initialize({
    required String host,
    required int port,
    required String serverKey,
  }) async {
    // Stub
  }

  Future<void> loginAsGuest() async {
    // Stub
    await Future.delayed(const Duration(milliseconds: 500));
  }

  Future<void> loginWithEmail(String email, String password) async {
    // Stub
    await Future.delayed(const Duration(milliseconds: 500));
  }
}
