#include "MapManager.h"
#include <iostream>

void MapManager::Init() {
    // Danh sách tên map
    levelNames = { "Level_0", "Level_1", "Level_2", "Level_3" };
    LoadLevel(0); // load map đầu tiên
}

void MapManager::LoadLevel(int index) {
    currentLevel = index;
    map.Load("Assets/Images/Map/Map.ldtk", levelNames[currentLevel]);
}

void MapManager::Update(float deltaTime) {
    // Nếu muốn hiệu ứng nhạc theo map, fade in/out hoặc logic khác, thêm ở đây
}

void MapManager::Draw(sf::RenderWindow& window) {
    map.Draw(window); // vẽ map
}
