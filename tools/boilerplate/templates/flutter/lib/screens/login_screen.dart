import 'package:flutter/material.dart';
import 'package:intelliversex_sdk/intelliversex_sdk.dart';

import '../config.dart';
import 'main_menu.dart';

class LoginScreen extends StatefulWidget {
  static const route = '/';

  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  bool _isLoading = false;

  Future<void> _loginGuest() async {
    setState(() => _isLoading = true);
    try {
      await IvxManager.instance.initialize(
        host: IvxConfig.serverHost,
        port: IvxConfig.serverPort,
        serverKey: IvxConfig.serverKey,
      );
      await IvxManager.instance.loginAsGuest();
      if (!mounted) return;
      Navigator.pushReplacementNamed(context, MainMenu.route);
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Login failed: $e')),
      );
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('{{game_name}}')),
      body: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(IvxConfig.tagline, style: Theme.of(context).textTheme.titleMedium),
            const SizedBox(height: 24),
            if (_isLoading)
              const Center(child: CircularProgressIndicator())
            else ...[
              FilledButton(
                onPressed: _loginGuest,
                child: const Text('Continue as guest'),
              ),
              const SizedBox(height: 12),
              OutlinedButton(
                onPressed: () {},
                child: const Text('Sign in with email'),
              ),
              const SizedBox(height: 12),
              OutlinedButton(
                onPressed: () {},
                child: const Text('Social sign-in'),
              ),
            ]
          ],
        ),
      ),
    );
  }
}
