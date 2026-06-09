#include <SFML/Graphics.hpp>
#include "Player.h"
#include "Enemy1.h"
#include "Enemy2.h"
#include "Enemy3.h"
#include "EnemyManager.h"
#include "MenuScreen.h"
#include "Map.h"
#include "PauseMenu.h"
#include "GameOverScreen.h"
#include "WinScreen.h"
#include "ScoreManager.h" 
#include <vector>
#include <string>

int main() {
    sf::RenderWindow window(sf::VideoMode(1600, 896), "KoS", sf::Style::Close);
    window.setFramerateLimit(60);

    // --- MAP ---
    Map map;
    std::vector<std::string> levelNames = { "Level_0", "Level_1", "Level_2", "Level_3" };
    int currentLevel = 0;
    map.Load("Assets/Images/Map/Map.ldtk", levelNames[currentLevel]);

    sf::Music bgMusic;
    bool mapMusicStarted = false;
    std::vector<std::string> musicPaths = {
        "Assets/Music/intro.mp3", "Assets/Music/boss2.mp3",
        "Assets/Music/boss22.mp3", "Assets/Music/boss3.mp3"
    };

    // --- PLAYER ---
    sf::Texture t1, t2, t3, t4, t5, t6, t7;
    t1.loadFromFile("Assets/Images/Player/idle.png");
    t2.loadFromFile("Assets/Images/Player/run.png");
    t3.loadFromFile("Assets/Images/Player/at1.png");
    t4.loadFromFile("Assets/Images/Player/at2.png");
    t5.loadFromFile("Assets/Images/Player/at3.png");
    t6.loadFromFile("Assets/Images/Player/skill1.png");
    t7.loadFromFile("Assets/Images/Player/death.png");
    Player player(t1, t2, t3, t4, t5, t6, t7);
    player.SetPosition({ 200.f, 655.f });

    // --- MANAGERS ---
    EnemyManager enemyManager;
    ScoreManager scoreManager;

    MenuScreen menu(window);
    PauseMenu pauseMenu(window);
    GameOverScreen gameOverScreen(1600, 896);
    WinScreen winScreen(1600, 896);

    // --- FONT CHUNG ---
    sf::Font fontUI;
    if (!fontUI.loadFromFile("Assets/Font/fontTutorial.ttf")) {
        // Xử lý nếu không load được font
    }

    // --- UI ĐIỂM SỐ TRONG GAME (HUD) ---
    sf::Text scoreTextHUD;
    scoreTextHUD.setFont(fontUI);
    scoreTextHUD.setCharacterSize(30);
    scoreTextHUD.setFillColor(sf::Color::Yellow);
    scoreTextHUD.setOutlineThickness(2);
    scoreTextHUD.setOutlineColor(sf::Color::Black);
    scoreTextHUD.setPosition(1350.f, 30.f); // Góc phải trên

    sf::Clock clock;

    // --- TUTORIAL TEXT (Đã sửa lại định dạng đẹp) ---
    sf::Text tutorialText;
    tutorialText.setFont(fontUI);
    tutorialText.setCharacterSize(28);
    tutorialText.setFillColor(sf::Color(255, 220, 50)); // Màu vàng
    tutorialText.setOutlineThickness(2);
    tutorialText.setOutlineColor(sf::Color::Black);

    // Trả lại định dạng xuống dòng như cũ
    tutorialText.setString(
        "GAME TUTORIAL:\n"
        "- A / D        : Move\n"
        "- L / LSHIFT   : Dash\n"
        "- J            : Hit\n"
        "- K / SPACE    : Jump\n"
        "- I / Q        : Skill"
    );
    tutorialText.setPosition(50.f, 80.f);

    bool needReset = false;
    bool isVictory = false;
    bool scoreSaved = false;

    // Biến theo dõi số lượng quái để tính điểm
    int previousEnemyCount = 0;

    while (window.isOpen()) {
        sf::Event event;
        while (window.pollEvent(event)) {
            if (event.type == sf::Event::Closed) window.close();

            if (menu.isActive) {
                if (event.type == sf::Event::KeyPressed) menu.isFadingOut = true;
            }
            else {
                if (isVictory) {
                    if (winScreen.HandleInput(window, event)) needReset = true;
                }
                else if (player.IsDead()) {
                    if (gameOverScreen.HandleInput(window, event)) needReset = true;
                }
                else {
                    PauseMenu::ButtonResult action = pauseMenu.HandleInput(event, window);
                    if (action == PauseMenu::Reset) needReset = true;
                }
            }
        }

        // --- RESET GAME ---
        if (needReset) {
            currentLevel = 0;
            map.Load("Assets/Images/Map/Map.ldtk", levelNames[currentLevel]);
            player.SetPosition({ 200.f, 655.f });
            player.SetHP(300);
            enemyManager = EnemyManager();

            mapMusicStarted = false;
            bgMusic.stop();
            menu.ResetFade();

            isVictory = false;
            scoreSaved = false;
            scoreManager.ResetCurrentScore();

            previousEnemyCount = 0;

            needReset = false;
        }

        float deltaTime = clock.restart().asSeconds();
        window.clear(sf::Color(70, 100, 150));

        if (menu.isActive) {
            menu.Update(deltaTime, window);
            menu.Draw(window);
        }
        else {
            scoreTextHUD.setString("SCORE: " + std::to_string(scoreManager.GetCurrentScore()));

            // --- LOGIC GAME LOOP ---
            if (!pauseMenu.IsPaused() && !player.IsDead() && !isVictory) {
                if (!mapMusicStarted) {
                    mapMusicStarted = true;
                    if (!musicPaths[currentLevel].empty()) { 
                        bgMusic.openFromFile(musicPaths[currentLevel]);
                        bgMusic.setVolume(30.f);
                        bgMusic.play();
                        bgMusic.setLoop(true);
                    }
                }

                player.HandleInput(deltaTime);
                player.Update(deltaTime);
                sf::Vector2f pos = player.GetPosition();
                if (pos.x < 50) pos.x = 50; if (pos.x > 1550) pos.x = 1550;
                player.SetPosition(pos);

                enemyManager.UpdateAll(deltaTime, player.GetPosition(), player);

                // --- LOGIC TÍNH ĐIỂM ---
                int currentLivingEnemies = 0;
                for (const auto& e : enemyManager.GetEnemies()) {
                    if (!e->IsDead()) currentLivingEnemies++;
                }

                if (currentLivingEnemies < previousEnemyCount) {
                    int killedCount = previousEnemyCount - currentLivingEnemies;
                    scoreManager.AddScore(killedCount * 100);
                }
                previousEnemyCount = currentLivingEnemies;
                enemyManager.RemoveDeadEnemies();
                // -----------------------

                // Collision
                for (const auto& e : enemyManager.GetEnemies()) {
                    if ((player.GetState() == Attacking1 ||
                        player.GetState() == Attacking2 ||
                        player.GetState() == Attacking3) &&
                        player.GetAttackBox().intersects(e->GetBodyHitbox()))
                    {
                        e->TakeDamage(20);
                    }
                    if (player.GetState() == Skill1 &&
                        player.GetSkill1Hitbox().intersects(e->GetBodyHitbox()))
                    {
                        e->TakeDamage(50);
                    }
                    if (e->IsAttacking() && e->GetAttackBox().intersects(player.GetBodyHitbox())) {
                        bool inSafe = false;
                        if (currentLevel == 3)
                        { 
                            for (auto& z : e->GetSafeZones())
                                if (z.intersects(player.GetBodyHitbox()))
                                    inSafe = true;
                        }
                        if (!inSafe)
                            player.TakeDamage(15);
                    }
                }

                // --- CHUYỂN MAP ---
                bool allDead = (enemyManager.GetEnemies().size() == 0);

                if (pos.x >= 1550.f && (currentLevel == 0 || allDead)) {
                    if (currentLevel < 3) {
                        if (!menu.IsMapFadingOut() && !menu.IsMapFadingIn()) {
                            menu.StartFadeOutMap([&]() {
                                currentLevel++;
                                if (currentLevel < levelNames.size()) {
                                    map.Load("Assets/Images/Map/Map.ldtk", levelNames[currentLevel]);
                                    player.SetPosition({ 200.f, 650.f });

                                    enemyManager = EnemyManager();
                                    previousEnemyCount = 0;

                                    mapMusicStarted = false;

                                    // Spawn Boss
                                    if (currentLevel == 1) {
                                        static sf::Texture tI, tW, tA, tD;
                                        static bool l = false;
                                        if (!l) { 
                                            tI.loadFromFile("Assets/Images/Enemy/idle_1.png");
                                            tW.loadFromFile("Assets/Images/Enemy/walk_1.png");
                                            tA.loadFromFile("Assets/Images/Enemy/attack_1.png");
                                            tD.loadFromFile("Assets/Images/Enemy/death_1.png");
                                            l = true;
                                        }
                                        enemyManager.AddEnemy(std::make_unique<Enemy2>(tI, tW, tA, tD));
                                        enemyManager.GetEnemies().back()->SetPosition({ 1000.f, 610.f });
                                    }
                                    else if (currentLevel == 2) {
                                        static sf::Texture tI, tW, tA, tA1, tA2, tD;
                                        static bool l = false;
                                        if (!l) { 
                                            tI.loadFromFile("Assets/Images/Enemy/idle.png");
                                            tW.loadFromFile("Assets/Images/Enemy/walk.png");
                                            tA.loadFromFile("Assets/Images/Enemy/attack.png");
                                            tA1.loadFromFile("Assets/Images/Enemy/at1.png");
                                            tA2.loadFromFile("Assets/Images/Enemy/at2.png");
                                            tD.loadFromFile("Assets/Images/Enemy/death.png");
                                            l = true;
                                        }
                                        enemyManager.AddEnemy(std::make_unique<Enemy1>(tI, tW, tA, tA1, tA2, tD));
                                        enemyManager.GetEnemies().back()->SetPosition({ 1000.f, 550.f });
                                    }
                                    else if (currentLevel == 3) {
                                        static sf::Texture tI, tW, tA, tA1, tA2, tT, tD;
                                        static bool l = false;
                                        if (!l) {
                                            tI.loadFromFile("Assets/Images/Enemy/idle_2.png");
                                            tW.loadFromFile("Assets/Images/Enemy/walk_2.png");
                                            tA.loadFromFile("Assets/Images/Enemy/attack_2.png");
                                            tA1.loadFromFile("Assets/Images/Enemy/at1_2.png");
                                            tA2.loadFromFile("Assets/Images/Enemy/at2_2.png");
                                            tT.loadFromFile("Assets/Images/Enemy/tele.png");
                                            tD.loadFromFile("Assets/Images/Enemy/death_2.png");
                                            l = true;
                                        }
                                        enemyManager.AddEnemy(std::make_unique<Enemy3>(tI, tW, tA, tA1, tA2, tT, tD));
                                        enemyManager.GetEnemies().back()->SetPosition({ 850.f, 330.f });
                                    }
                                }
                                }, 0.5f);
                        }
                    }
                }
                menu.UpdateMapFade(deltaTime);
            }
            // Logic animation chết cho Player
            else if (player.IsDead()) player.Update(deltaTime);

            // --- CHECK CHIẾN THẮNG ---
            if (currentLevel == 3 && enemyManager.GetEnemies().empty() && !isVictory && mapMusicStarted) {
                isVictory = true;
            }

            // --- LƯU ĐIỂM ---
            if ((isVictory || player.IsDead()) && !scoreSaved) {
                scoreManager.SaveScore();
                if (isVictory)
                    winScreen.SetScoreInfo(scoreManager.GetCurrentScore(), scoreManager.GetHighScores());
                else
                    gameOverScreen.SetScoreInfo(scoreManager.GetCurrentScore(), scoreManager.GetHighScores());
                scoreSaved = true;
            }

            // --- DRAW ---
            map.Draw(window);

            // Chỉ vẽ Tutorial nếu ở Map 0 (Level Intro)
            if (currentLevel == 0) window.draw(tutorialText);

            enemyManager.DrawAll(window);
            player.Draw(window);
            menu.DrawMapFade(window);
            for (auto& e : enemyManager.GetEnemies()) e->DrawHP(window);

            // Vẽ HUD Score (nếu đang chơi)
            if (!isVictory && !player.IsDead() && !menu.isActive) {
                window.draw(scoreTextHUD);
            }

            // Vẽ màn hình UI (Win/Lose/Pause)
            if (isVictory) { winScreen.Update(window); winScreen.Draw(window); }
            else if (player.IsDead()) { gameOverScreen.Update(window); gameOverScreen.Draw(window); }
            else { pauseMenu.Update(window); pauseMenu.Draw(window); }
        }
        window.display();
    }
    return 0;
}