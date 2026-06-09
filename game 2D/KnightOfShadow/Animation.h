#pragma once
#include<SFML/Graphics.hpp>

class Animation :public sf::Sprite {
private:
	sf::Vector2i m_frameNum;
	sf::Vector2i m_currentFrame;

	float m_frameTime = 0.f;
	float m_currentTime = 0.f;

	sf::Vector2i m_rectUV;
	sf::Vector2i m_rectSize;
	sf::IntRect m_rect;

	const sf::Texture* m_texture = nullptr;
public:
	Animation() = default;
	Animation(sf::Texture& texture, sf::Vector2i frameNum, float frameTime);

	void Update(float deltaTime, bool loop);

	void CalculateRectSize();
	void CalculateRectUV();
	void ApplyRect();

	sf::IntRect GetRect() const;
	void Reset();
	bool IsFinished() const;
	const sf::Texture* getTexture() const;
	int GetCurrentFrame() const { return m_currentFrame.x; }
};
