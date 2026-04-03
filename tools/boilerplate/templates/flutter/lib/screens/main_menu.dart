import 'package:flutter/material.dart';

import '../config.dart';
import '../widgets/energy_bar.dart';
import '../widgets/progression_bar.dart';
import '../widgets/retention_manager.dart';

class MainMenu extends StatefulWidget {
  static const route = '/menu';

  const MainMenu({super.key});

  @override
  State<MainMenu> createState() => _MainMenuState();
}

class _MainMenuState extends State<MainMenu> {
  int _index = 0;

  @override
  Widget build(BuildContext context) {
    return RetentionManager(
      child: Scaffold(
        appBar: AppBar(
          title: Text('{{game_name}}'),
          actions: [
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 12),
              child: Center(
                child: Row(
                  children: [
                    const Icon(Icons.monetization_on, color: Colors.amber, size: 20),
                    const SizedBox(width: 4),
                    Text('${IvxConfig.initialCoins}', style: const TextStyle(fontWeight: FontWeight.bold)),
                    const SizedBox(width: 12),
                    const Icon(Icons.diamond, color: Colors.purpleAccent, size: 20),
                    const SizedBox(width: 4),
                    Text('${IvxConfig.initialGems}', style: const TextStyle(fontWeight: FontWeight.bold)),
                    const SizedBox(width: 12),
                    EnergyBar(currentEnergy: IvxConfig.maxEnergy, maxEnergy: IvxConfig.maxEnergy),
                  ],
                ),
              ),
            ),
          ],
        ),
        body: _buildBody(),
        bottomNavigationBar: BottomNavigationBar(
          currentIndex: _index,
          onTap: (i) => setState(() => _index = i),
          type: BottomNavigationBarType.fixed,
          items: const [
            BottomNavigationBarItem(icon: Icon(Icons.home), label: 'Home'),
            BottomNavigationBarItem(icon: Icon(Icons.store), label: 'Store'),
            BottomNavigationBarItem(icon: Icon(Icons.emoji_events), label: 'Achieve'),
            BottomNavigationBarItem(icon: Icon(Icons.card_giftcard), label: 'Daily'),
            BottomNavigationBarItem(icon: Icon(Icons.leaderboard), label: 'Board'),
            BottomNavigationBarItem(icon: Icon(Icons.settings), label: 'Settings'),
          ],
        ),
      ),
    );
  }

  Widget _buildBody() {
    switch (_index) {
      case 0:
        return _buildHomeTab();
      case 1:
        return _buildStoreTab();
      case 2:
        return _buildAchievementsTab();
      case 3:
        return _buildDailyTab();
      case 4:
        return _buildLeaderboardTab();
      case 5:
        return _buildSettingsTab();
      default:
        return const SizedBox.shrink();
    }
  }

  Widget _buildHomeTab() {
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        const ProgressionBar(level: 1, progress: 0.25),
        const SizedBox(height: 32),
        Center(
          child: FilledButton.icon(
            onPressed: () {},
            icon: const Icon(Icons.play_arrow),
            label: const Text('PLAY NOW'),
            style: FilledButton.styleFrom(
              padding: const EdgeInsets.symmetric(horizontal: 48, vertical: 16),
              textStyle: const TextStyle(fontSize: 24, fontWeight: FontWeight.bold),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildStoreTab() {
    return GridView.builder(
      padding: const EdgeInsets.all(16),
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 2,
        mainAxisSpacing: 16,
        crossAxisSpacing: 16,
      ),
      itemCount: 6,
      itemBuilder: (context, index) {
        return Card(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Icon(Icons.inventory, size: 48),
              const SizedBox(height: 8),
              Text('Item $index'),
              ElevatedButton(onPressed: () {}, child: const Text('Buy')),
            ],
          ),
        );
      },
    );
  }

  Widget _buildAchievementsTab() {
    return ListView.builder(
      padding: const EdgeInsets.all(16),
      itemCount: 10,
      itemBuilder: (context, index) {
        return ListTile(
          leading: const CircleAvatar(child: Icon(Icons.emoji_events)),
          title: Text('Achievement ${index + 1}'),
          subtitle: const Text('Complete task to unlock'),
          trailing: const Icon(Icons.lock),
        );
      },
    );
  }

  Widget _buildDailyTab() {
    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text('Daily Quests', style: Theme.of(context).textTheme.headlineSmall),
          const SizedBox(height: 16),
          Expanded(
            child: ListView.builder(
              itemCount: 3,
              itemBuilder: (context, index) {
                return Card(
                  child: ListTile(
                    title: Text('Quest ${index + 1}'),
                    subtitle: const Text('Reward: 100 Coins'),
                    trailing: ElevatedButton(onPressed: () {}, child: const Text('Claim')),
                  ),
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildLeaderboardTab() {
    return ListView.builder(
      itemCount: 20,
      itemBuilder: (context, index) {
        return ListTile(
          leading: Text('#${index + 1}', style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
          title: Text('Player ${index + 1}'),
          trailing: Text('${10000 - index * 100} pts'),
        );
      },
    );
  }

  Widget _buildSettingsTab() {
    return ListView(
      children: [
        SwitchListTile(
          title: const Text('Sound Effects'),
          value: true,
          onChanged: (v) {},
        ),
        SwitchListTile(
          title: const Text('Music'),
          value: true,
          onChanged: (v) {},
        ),
        const Divider(),
        ListTile(
          title: const Text('Log out'),
          leading: const Icon(Icons.logout),
          onTap: () {
            Navigator.pushReplacementNamed(context, '/');
          },
        ),
      ],
    );
  }
}
