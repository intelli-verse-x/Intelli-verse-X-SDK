import 'package:flutter/material.dart';

class ProgressionBar extends StatelessWidget {
  final int level;
  final double progress; // 0.0 to 1.0

  const ProgressionBar({super.key, required this.level, required this.progress});

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      mainAxisSize: MainAxisSize.min,
      children: [
        Text('Level $level', style: const TextStyle(fontWeight: FontWeight.bold)),
        const SizedBox(height: 4),
        LinearProgressIndicator(value: progress),
      ],
    );
  }
}