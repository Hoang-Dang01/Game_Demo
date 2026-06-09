#pragma once
#include <SFML/Graphics.hpp>
#include <vector>

class BossAnimation {
private:
    sf::Texture* texture;
    std::vector<sf::IntRect> frames; 
    float frameTime;
    float currentTime;
    int currentFrame;
    bool isLooping;
    bool isFinished;

public:
    // Constructor
    BossAnimation(sf::Texture& tex, sf::Vector2i frameCount, float switchTime, bool looping = true);

    // Cập nhật animation
    void Update(float deltaTime);

    // Reset animation về frame đầu
    void Reset();

    // Getter
    const sf::Texture* GetTexture() const { return texture; }
    sf::IntRect GetRect() const { return frames[currentFrame]; }
    bool IsFinished() const { return isFinished; }
};
