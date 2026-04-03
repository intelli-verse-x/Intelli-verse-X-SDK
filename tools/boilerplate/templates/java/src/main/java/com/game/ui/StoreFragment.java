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
import com.intelliversex.sdk.IVXHiro;

public class StoreFragment extends Fragment {
    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_store, container, false);
        Button btn = view.findViewById(R.id.btn_store_action);
        btn.setOnClickListener(v -> {
            IVXHiro.getInstance().getStore();
            Toast.makeText(getContext(), "Store Items Loaded", Toast.LENGTH_SHORT).show();
        });
        return view;
    }
}