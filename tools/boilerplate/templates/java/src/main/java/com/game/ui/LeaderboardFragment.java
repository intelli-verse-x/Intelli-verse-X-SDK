package com.game.ui;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import com.game.R;
import com.intelliversex.sdk.IVXSatori;

public class LeaderboardFragment extends Fragment {
    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_leaderboard, container, false);
        Button btn = view.findViewById(R.id.btn_leaderboard_action);
        btn.setOnClickListener(v -> {
            IVXSatori.getInstance().postEvent("view_leaderboard");
            Toast.makeText(getContext(), "Leaderboard Loaded", Toast.LENGTH_SHORT).show();
        });
        return view;
    }
}