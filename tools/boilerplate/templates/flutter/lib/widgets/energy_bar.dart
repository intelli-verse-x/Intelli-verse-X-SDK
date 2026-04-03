import 'package:flutter/material.dart';

class EnergyBar extends StatelessWidget {
  final int currentEnergy;
  final int maxEnergy;

  const EnergyBar({super.key, required this.currentEnergy, required this.maxEnergy});

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        const Icon(Icons.bolt, color: Colors.blue),
        const SizedBox(width: 4),
        Text('$currentEnergy / $maxEnergy', style: const TextStyle(fontWeight: FontWeight.bold)),
      ],
    );
  }
}