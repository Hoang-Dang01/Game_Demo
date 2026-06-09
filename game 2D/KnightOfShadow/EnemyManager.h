#pragma once
#include <vector>
#include <memory>
#include <algorithm>
#include "BaseEnemy.h"
#include "Enemy1.h"
#include "Enemy2.h"
#include "Enemy3.h"
#include "Player.h"

class EnemyManager {
private:
    std::vector<std::unique_ptr<BaseEnemy>> enemies;

public:
    EnemyManager() = default;

    void AddEnemy(std::unique_ptr<BaseEnemy> enemy);
    void UpdateAll(float deltaTime, const sf::Vector2f& playerPos, const Player& player);
    void DrawAll(sf::RenderWindow& window);
    void RemoveDeadEnemies();

    const std::vector<std::unique_ptr<BaseEnemy>>& GetEnemies() const { return enemies; }
};
