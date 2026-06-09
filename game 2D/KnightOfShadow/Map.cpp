#include "Map.h"
#include <fstream>
#include <iostream>

using namespace std;
using namespace sf;

bool Map::Load(const string& filePath, const string& levelName)
{
    tileSprites.clear();
    collisions.clear();
    backgroundSprites.clear(); // nếu bạn có vector background

    // 1️⃣ Đọc file JSON
    ifstream file(filePath);
    if (!file.is_open()) {
        cerr << " Khong the mo file map: " << filePath << endl;
        return false;
    }

    json ldtk;
    file >> ldtk;
    file.close();

    // 2️⃣ Tìm level theo tên
    json targetLevel;
    bool found = false;
    for (auto& level : ldtk["levels"]) {
        if (level["identifier"] == levelName) {
            targetLevel = level;
            std::cout << " Loaded level: " << (std::string)level["identifier"] << std::endl;
            found = true;
            break;
        }
    }

    if (!found) {
        cerr << "Khong tim thay level: " << levelName << endl;
        return false;
    }

    // 3️⃣ Load tileset
    std::string tilesetPath;

    if (levelName == "Level_0")
        tilesetPath = "Assets/Images/Map/start (2).png";
    else if (levelName == "Level_1")
        tilesetPath = "Assets/Images/Map/iceblas.png";
    else if (levelName == "Level_2")
        tilesetPath = "Assets/Images/Map/hell.png";
    else if (levelName == "Level_3")
        tilesetPath = "Assets/Images/Map/King.jpg";
    else
        tilesetPath = "Assets/Images/Map/start (2).png"; // fallback

    if (!tileset.loadFromFile(tilesetPath)) {
        cerr << " Khong the tai tileset: " << tilesetPath << endl;
        return false;
    }

    // Xóa các tile cũ nếu có
    groundTiles.clear();
    backgroundTiles.clear();

    // 4️⃣ Duyệt từng layer trong level
    for (auto& layer : targetLevel["layerInstances"]) {
        string layerName = layer["__identifier"];
        int gridSize = layer["__gridSize"];

        // --- Layer Background ---
        if (layerName == "Backround" || layerName == "Background" || layerName == "BG") {

            for (auto& tile : layer["gridTiles"]) {
                int px = tile["px"][0];
                int py = tile["px"][1];
                int srcX = tile["src"][0];
                int srcY = tile["src"][1];

                Sprite sprite;
                sprite.setTexture(tileset);
                sprite.setTextureRect(IntRect(srcX, srcY, gridSize, gridSize));
                sprite.setPosition((float)px, (float)py);

                backgroundTiles.push_back(sprite);
            }
        }

        // --- Layer Ground ---
        else if (layerName == "Ground") {
            for (auto& tile : layer["gridTiles"]) {
                int px = tile["px"][0];
                int py = tile["px"][1];
                int srcX = tile["src"][0];
                int srcY = tile["src"][1];

                Sprite sprite;
                sprite.setTexture(tileset);
                sprite.setTextureRect(IntRect(srcX, srcY, gridSize, gridSize));
                sprite.setPosition((float)px, (float)py);

                groundTiles.push_back(sprite);
            }
        }
    }

    cout << "Da load map: " << levelName
        << " | Background: " << backgroundTiles.size()
        << " | Ground: " << groundTiles.size() << endl;

    return true;
}

void Map::Draw(RenderWindow& window)
{
    // Vẽ Background trước
    for (auto& s : backgroundTiles)
        window.draw(s);

    // Sau đó mới vẽ Ground
    for (auto& s : groundTiles)
        window.draw(s);
}


