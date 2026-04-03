import 'package:flutter/material.dart';

import 'screens/login_screen.dart';
import 'screens/main_menu.dart';

int _parseArgb(String hex) {
  final h = hex.replaceAll('#', '');
  final full = h.length == 6 ? 'ff$h' : h;
  return int.parse(full, radix: 16);
}

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(const IvxStarterApp());
}

class IvxStarterApp extends StatelessWidget {
  const IvxStarterApp({super.key});

  @override
  Widget build(BuildContext context) {
    final primary = Color(_parseArgb('{{primary_color}}'));
    final secondary = Color(_parseArgb('{{secondary_color}}'));
    final background = Color(_parseArgb('{{background_color}}'));

    return MaterialApp(
      title: '{{game_name}}',
      theme: ThemeData(
        brightness: Brightness.dark,
        colorScheme: ColorScheme.dark(
          primary: primary,
          secondary: secondary,
          surface: background,
        ),
        scaffoldBackgroundColor: background,
        useMaterial3: true,
      ),
      initialRoute: LoginScreen.route,
      routes: {
        LoginScreen.route: (_) => const LoginScreen(),
        MainMenu.route: (_) => const MainMenu(),
      },
    );
  }
}
