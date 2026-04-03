package com.game.ui;

import android.content.Intent;
import android.os.Bundle;
import android.widget.Button;
import androidx.appcompat.app.AppCompatActivity;
import com.game.R;
import com.intelliversex.sdk.IVXSatori;
import com.game.MainActivity;

public class FTUEActivity extends AppCompatActivity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_ftue);
        
        IVXSatori.getInstance().postEvent("ftue_start");

        Button btnComplete = findViewById(R.id.btn_complete_ftue);
        btnComplete.setOnClickListener(v -> {
            IVXSatori.getInstance().postEvent("ftue_complete");
            startActivity(new Intent(this, MainActivity.class));
            finish();
        });
    }
}