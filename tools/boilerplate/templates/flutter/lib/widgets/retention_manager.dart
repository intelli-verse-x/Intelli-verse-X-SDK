import 'package:flutter/material.dart';

class RetentionManager extends StatefulWidget {
  final Widget child;

  const RetentionManager({super.key, required this.child});

  @override
  State<RetentionManager> createState() => _RetentionManagerState();
}

class _RetentionManagerState extends State<RetentionManager> {
  @override
  void initState() {
    super.initState();
    // Simulate retention init
    _checkDailyRewards();
  }

  Future<void> _checkDailyRewards() async {
    // Stub
  }

  @override
  Widget build(BuildContext context) {
    return widget.child;
  }
}