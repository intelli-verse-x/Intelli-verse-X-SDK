#include "config.h"

#include <imgui.h>
#include <imgui_impl_glfw.h>
#include <imgui_impl_opengl3.h>

#include <GLFW/glfw3.h>
#if defined(__APPLE__)
#define GL_SILENCE_DEPRECATION
#include <OpenGL/gl3.h>
#else
#include <GL/gl.h>
#endif

#include <ivx/ivx_client.hpp>

static char const* kTabs[] = {
	"Home", "Store", "Achievements", "Daily Rewards", "Leaderboard", "Settings",
};

struct FTUEOverlay {
	bool show = true;
	void render() {
		if (!show) return;
		ImGui::SetNextWindowPos(ImGui::GetMainViewport()->GetCenter(), ImGuiCond_Appearing, ImVec2(0.5f, 0.5f));
		ImGui::Begin("Welcome to {{game_name}}!", &show, ImGuiWindowFlags_AlwaysAutoResize);
		ImGui::Text("Here is your starter pack.");
		if (ImGui::Button("Claim 1000 Coins")) {
			show = false;
		}
		ImGui::End();
	}
};

struct RetentionManager {
	int current_streak = 1;
	bool claimed_today = false;
	void render_calendar() {
		ImGui::Text("Current Streak: %d days", current_streak);
		if (!claimed_today) {
			if (ImGui::Button("Claim Daily Reward")) {
				claimed_today = true;
				current_streak++;
			}
		} else {
			ImGui::TextDisabled("Reward claimed for today. Come back tomorrow!");
		}
	}
};

int main()
{
	if (!glfwInit())
		return -1;

	GLFWwindow* window = glfwCreateWindow(1280, 720, "{{game_name}}", nullptr, nullptr);
	if (!window)
	{
		glfwTerminate();
		return -1;
	}
	glfwMakeContextCurrent(window);
	glfwSwapInterval(1);

	IMGUI_CHECKVERSION();
	ImGui::CreateContext();
	ImGui::StyleColorsDark();
	ImGui_ImplGlfw_InitForOpenGL(window, true);
	ImGui_ImplOpenGL3_Init("#version 150");

	ivx::IVXClient client;
	client.configure(ivx_config::GAME_ID, ivx_config::SERVER_HOST, ivx_config::SERVER_PORT,
	               ivx_config::SERVER_KEY);
	client.authenticate_guest();
	client.load_hiro_systems();
	client.track_event("session_start", { { "game_id", ivx_config::GAME_ID } });

	FTUEOverlay ftue;
	RetentionManager retention;

	int energy = 100;
	float volume = 1.0f;
	bool fullscreen = false;

	while (!glfwWindowShouldClose(window))
	{
		glfwPollEvents();
		ImGui_ImplOpenGL3_NewFrame();
		ImGui_ImplGlfw_NewFrame();
		ImGui::NewFrame();

		ImGui::Begin("Wallet", nullptr, ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_AlwaysAutoResize);
		ImGui::Text("Coins: %lld  |  Gems: %lld", static_cast<long long>(ivx_config::INITIAL_COINS),
		            static_cast<long long>(ivx_config::INITIAL_GEMS));
		ImGui::End();

		ImGui::SetNextWindowPos(ImVec2(50, 100), ImGuiCond_FirstUseEver);
		ImGui::SetNextWindowSize(ImVec2(800, 500), ImGuiCond_FirstUseEver);
		ImGui::Begin("{{game_name}} Main Menu");
		ImGui::TextUnformatted(ivx_config::TAGLINE);
		ImGui::Separator();

		if (ImGui::BeginTabBar("main_tabs"))
		{
			if (ImGui::BeginTabItem("Home"))
			{
				ImGui::Text("Welcome back to {{game_name}}!");
				ImGui::Spacing();
				ImGui::Text("Energy: %d / 100", energy);
				ImGui::ProgressBar(energy / 100.0f, ImVec2(200.0f, 0.0f));
				if (ImGui::Button("Play Match (Cost: 10 Energy)") && energy >= 10) {
					energy -= 10;
				}
				ImGui::EndTabItem();
			}
			if (ImGui::BeginTabItem("Store"))
			{
				ImGui::Text("In-Game Store");
				ImGui::Separator();
				ImGui::Columns(2, "store_grid", false);
				ImGui::Text("Starter Pack");
				if (ImGui::Button("Buy ($0.99)##starter")) {}
				ImGui::NextColumn();
				ImGui::Text("1000 Coins");
				if (ImGui::Button("Buy (10 Gems)##coins")) {}
				ImGui::Columns(1);
				ImGui::EndTabItem();
			}
			if (ImGui::BeginTabItem("Achievements"))
			{
				ImGui::Text("Your Achievements");
				ImGui::Separator();
				ImGui::Text("[x] First Login");
				ImGui::Text("[ ] Play 10 Games");
				ImGui::Text("[ ] Spend 100 Coins");
				ImGui::EndTabItem();
			}
			if (ImGui::BeginTabItem("Daily Rewards"))
			{
				ImGui::Text("Calendar");
				ImGui::Separator();
				retention.render_calendar();
				ImGui::EndTabItem();
			}
			if (ImGui::BeginTabItem("Leaderboard"))
			{
				ImGui::Text("Global Rankings");
				ImGui::Separator();
				if (ImGui::BeginTable("lb_table", 3, ImGuiTableFlags_Borders | ImGuiTableFlags_RowBg))
				{
					ImGui::TableSetupColumn("Rank");
					ImGui::TableSetupColumn("Player");
					ImGui::TableSetupColumn("Score");
					ImGui::TableHeadersRow();

					ImGui::TableNextRow();
					ImGui::TableSetColumnIndex(0); ImGui::Text("1");
					ImGui::TableSetColumnIndex(1); ImGui::Text("PlayerOne");
					ImGui::TableSetColumnIndex(2); ImGui::Text("9999");

					ImGui::TableNextRow();
					ImGui::TableSetColumnIndex(0); ImGui::Text("2");
					ImGui::TableSetColumnIndex(1); ImGui::Text("Guest123");
					ImGui::TableSetColumnIndex(2); ImGui::Text("8500");

					ImGui::EndTable();
				}
				ImGui::EndTabItem();
			}
			if (ImGui::BeginTabItem("Settings"))
			{
				ImGui::Text("Audio & Display");
				ImGui::Separator();
				ImGui::SliderFloat("Master Volume", &volume, 0.0f, 1.0f);
				ImGui::Checkbox("Fullscreen", &fullscreen);
				ImGui::EndTabItem();
			}

			ImGui::EndTabBar();
		}
		ImGui::End();

		ftue.render();

		ImGui::Render();
		int display_w, display_h;
		glfwGetFramebufferSize(window, &display_w, &display_h);
		glViewport(0, 0, display_w, display_h);
		glClearColor(0.08f, 0.08f, 0.1f, 1.f);
		glClear(GL_COLOR_BUFFER_BIT);
		ImGui_ImplOpenGL3_RenderDrawData(ImGui::GetDrawData());
		glfwSwapBuffers(window);
	}

	ImGui_ImplOpenGL3_Shutdown();
	ImGui_ImplGlfw_Shutdown();
	ImGui::DestroyContext();
	glfwDestroyWindow(window);
	glfwTerminate();
	return 0;
}
