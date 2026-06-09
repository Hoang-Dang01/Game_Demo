#include "BossAnimation.h"

BossAnimation::BossAnimation(sf::Texture& tex, sf::Vector2i frameCount, float switchTime, bool looping)
    : texture(&tex), frameTime(switchTime), currentTime(0.f), currentFrame(0), isLooping(looping), isFinished(false)
{
    // Tính kích thước mỗi frame
    unsigned int frameWidth = tex.getSize().x / frameCount.x;
    unsigned int frameHeight = tex.getSize().y / frameCount.y;

    // Tạo danh sách frame
    for (int y = 0; y < frameCount.y; ++y) {
        for (int x = 0; x < frameCount.x; ++x) {
            frames.push_back(sf::IntRect(
                x * static_cast<int>(frameWidth),
                y * static_cast<int>(frameHeight),
                static_cast<int>(frameWidth),
                static_cast<int>(frameHeight)
            ));
        }
    }
}

void BossAnimation::Update(float deltaTime)
{
    if (isFinished) return;

    currentTime += deltaTime;
    while (currentTime >= frameTime) {
        currentTime -= frameTime;
        currentFrame++;

        if (currentFrame >= static_cast<int>(frames.size())) {
            if (isLooping)
                currentFrame = 0;
            else {
                currentFrame = static_cast<int>(frames.size()) - 1;
                isFinished = true;
            }
        }
    }
}

void BossAnimation::Reset()
{
    currentFrame = 0;
    currentTime = 0.f;
    isFinished = false;
}
