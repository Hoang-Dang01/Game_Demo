#pragma once
#include <SFML/Graphics.hpp>
#include <nlohmann/json.hpp>
#include <string>
#include <vector>

using namespace sf;
using namespace std;
using json = nlohmann::json;

class Map
{
private:
    Texture tileset;                // Ảnh tileset chung
    vector<Sprite> groundTiles;     // Layer Ground
    vector<Sprite> backgroundTiles; // Layer Background

    std::vector<sf::Sprite> tileSprites;   // các tile hiển thị
    std::vector<sf::FloatRect> collisions; // vùng va chạm
    std::vector<sf::Sprite> backgroundSprites;


public:
    bool Load(const string& filePath, const string& levelName);
    void Draw(RenderWindow& window);

};
