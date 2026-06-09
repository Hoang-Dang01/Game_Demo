#pragma once
#include "Map.h"
#include "Enemy1.h"
#include "Enemy2.h"
#include "Enemy3.h"
#include "EnemyManager.h"
#include <SFML/Audio.hpp>
#include <SFML/Graphics.hpp>
#include <string>
#include <vector>

class MapManager {
private:
    std::vector<std::string> levelNames;
    int currentLevel = 0;
    Map map;

    sf::Music bgMusic;
    bool canGoNext = true; // level 0 auto cho phép sang map sau

public:
    MapManager() = default;

    void Init();
    void LoadLevel(int index);
    void Update(float deltaTime);
    void Draw(sf::RenderWindow& window);

    // điều khiển cổng ảo
    void OpenNextGate() { canGoNext = true; }
    bool CanGoNext() const { return canGoNext; }

    int GetCurrentLevel() const { return currentLevel; }
};
