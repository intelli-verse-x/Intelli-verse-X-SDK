package com.game.ui;

import android.graphics.Color;
import android.os.Bundle;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.viewpager2.adapter.FragmentStateAdapter;
import androidx.viewpager2.widget.ViewPager2;

import com.game.Config;
import com.google.android.material.tabs.TabLayout;
import com.google.android.material.tabs.TabLayoutMediator;

/**
 * Main hub: wallet header + tabs (Home, Store, Achievements, Daily, Leaderboard, Settings).
 */
public class MainMenuFragment extends Fragment {
    public static MainMenuFragment newInstance() {
        return new MainMenuFragment();
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {
        int pad = (int) (12 * getResources().getDisplayMetrics().density);
        LinearLayout root = new LinearLayout(requireContext());
        root.setOrientation(LinearLayout.VERTICAL);
        root.setPadding(pad, pad, pad, pad);

        TextView wallet = new TextView(requireContext());
        wallet.setTextSize(16f);
        wallet.setTextColor(Color.WHITE);
        wallet.setGravity(Gravity.CENTER_VERTICAL);
        wallet.setPadding(pad, pad, pad, pad);
        wallet.setBackgroundColor(parseColor("{{primary_color}}"));
        wallet.setText("Wallet · coins " + Config.INITIAL_COINS + " · gems " + Config.INITIAL_GEMS);
        root.addView(wallet, new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));

        TabLayout tabs = new TabLayout(requireContext());
        tabs.setTabMode(TabLayout.MODE_SCROLLABLE);
        root.addView(tabs, new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));

        ViewPager2 pager = new ViewPager2(requireContext());
        pager.setAdapter(new FragmentStateAdapter(this) {
            @NonNull
            @Override
            public Fragment createFragment(int position) {
                switch (position) {
                    case 0: return new HomeFragment();
                    case 1: return new StoreFragment();
                    case 2: return new AchievementsFragment();
                    case 3: return new DailyRewardsFragment();
                    case 4: return new LeaderboardFragment();
                    case 5: return new SettingsFragment();
                    default: return new HomeFragment();
                }
            }

            @Override
            public int getItemCount() {
                return 6;
            }
        });
        root.addView(pager, new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, 0, 1f));

        String[] titles = {"Home", "Store", "Achievements", "DailyRewards", "Leaderboard", "Settings"};
        new TabLayoutMediator(tabs, pager, (tab, pos) -> tab.setText(titles[pos])).attach();

        return root;
    }

    private static int parseColor(String hex) {
        String h = hex.startsWith("#") ? hex.substring(1) : hex;
        return (int) Long.parseLong(h.length() == 6 ? "FF" + h : h, 16);
    }
}
