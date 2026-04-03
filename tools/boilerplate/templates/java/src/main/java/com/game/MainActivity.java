package com.game;

import android.graphics.Color;
import android.os.Bundle;
import android.view.ViewGroup;
import android.widget.FrameLayout;

import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.FragmentTransaction;

import com.game.ui.MainMenuFragment;
import com.intelliversex.sdk.IVXClient;

/**
 * Initializes IVXClient, guest auth, then hosts {@link MainMenuFragment}.
 */
public class MainActivity extends AppCompatActivity {
    private IVXClient ivx;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        FrameLayout content = new FrameLayout(this);
        content.setId(android.view.View.generateViewId());
        content.setLayoutParams(new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT));
        String bg = "{{background_color}}".replace("#", "");
        content.setBackgroundColor((int) Long.parseLong(bg.length() == 6 ? "FF" + bg : bg, 16));
        setContentView(content);

        ivx = IVXClient.getInstance(this);
        ivx.configure(Config.GAME_ID, Config.SERVER_HOST, Config.SERVER_PORT, Config.SERVER_KEY);
        ivx.authenticateGuest(session -> runOnUiThread(() -> {
            if (session == null || !session.isValid()) {
                finish();
                return;
            }
            FragmentTransaction tx = getSupportFragmentManager().beginTransaction();
            tx.replace(content.getId(), MainMenuFragment.newInstance());
            tx.commit();
        }));
    }

    @Override
    protected void onDestroy() {
        if (ivx != null) {
            ivx.dispose();
        }
        super.onDestroy();
    }
}
