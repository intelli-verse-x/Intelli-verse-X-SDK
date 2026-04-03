import 'package:flutter/material.dart';

class FTUEOverlay extends StatelessWidget {
  final String message;
  final VoidCallback onDismiss;

  const FTUEOverlay({super.key, required this.message, required this.onDismiss});

  @override
  Widget build(BuildContext context) {
    return Container(
      color: Colors.black54,
      child: Center(
        child: Card(
          margin: const EdgeInsets.all(32),
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Text('Tutorial', style: Theme.of(context).textTheme.titleLarge),
                const SizedBox(height: 16),
                Text(message, textAlign: TextAlign.center),
                const SizedBox(height: 24),
                ElevatedButton(
                  onPressed: onDismiss,
                  child: const Text('Got it!'),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}