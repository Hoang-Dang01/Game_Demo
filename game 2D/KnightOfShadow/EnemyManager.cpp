#include "EnemyManager.h"
#include "Player.h"

void EnemyManager::AddEnemy(std::unique_ptr<BaseEnemy> enemy) {
    enemies.push_back(std::move(enemy));
}

void EnemyManager::UpdateAll(float deltaTime, const sf::Vector2f& playerPos, const Player& player) {
    bool isPlayerOnGround = player.IsOnGround();
    for (auto& e : enemies) {
        // Nếu là Enemy1 thì gọi HandleInput riêng (để giữ logic cũ)
        if (auto enemy1 = dynamic_cast<Enemy1*>(e.get())) {
            enemy1->HandleInput(deltaTime, playerPos);
        }
        else if (auto enemy2 = dynamic_cast<Enemy2*>(e.get())) {
            enemy2->HandleInput(deltaTime, playerPos);
        }
        else if (auto enemy3 = dynamic_cast<Enemy3*>(e.get())) {
            enemy3->HandleInput(deltaTime, playerPos, isPlayerOnGround);
        }
        e->Update(deltaTime);

        sf::Vector2f pos = e->GetPosition();
        float mapLeft = 0.f;
        float mapRight = 1550.f; // chỉnh theo map
        float mapTop = 0.f;
        float mapBottom = 896.f;

        if (pos.x < mapLeft) pos.x = mapLeft;
        if (pos.x > mapRight) pos.x = mapRight;
        if (pos.y < mapTop) pos.y = mapTop;
        if (pos.y > mapBottom) pos.y = mapBottom;

        e->SetPosition(pos);
    }

    RemoveDeadEnemies();
}

void EnemyManager::DrawAll(sf::RenderWindow& window) {
    for (auto& e : enemies) {
        e->Draw(window);
    }
}

void EnemyManager::RemoveDeadEnemies() {
    enemies.erase(
        std::remove_if(enemies.begin(), enemies.end(),
            [](const std::unique_ptr<BaseEnemy>& e) {
                return e->IsDead() && e->IsDeathAnimFinished(); // chỉ xóa khi anim chết xong
            }),
        enemies.end()
    );
}
