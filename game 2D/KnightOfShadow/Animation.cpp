#include "Animation.h"
#include<iostream>

using namespace std;
Animation::Animation(sf::Texture& texture, sf::Vector2i frameNum, float frameTime)
	: m_texture(&texture), m_frameNum(frameNum), m_frameTime(frameTime), m_currentTime(0.f)
{
	setTexture(texture);
	CalculateRectSize();
	CalculateRectUV();
	ApplyRect();
}

void Animation::Update(float deltaTime, bool loop)
{
	m_currentTime += deltaTime;

	if (m_currentTime >= m_frameTime) {
		m_currentTime -= m_frameTime; m_currentFrame.x++;

		if (m_currentFrame.x >= m_frameNum.x) {
			if (loop)
				m_currentFrame.x = 0;
			else
				m_currentFrame.x = m_frameNum.x - 1;
		}
	}

	CalculateRectUV();
	ApplyRect();
}


void Animation::CalculateRectSize()
{
	m_rectSize.x = static_cast<int>(m_texture->getSize().x / m_frameNum.x);
	m_rectSize.y = static_cast<int>(m_texture->getSize().y / m_frameNum.y);
}


void Animation::CalculateRectUV()
{
	m_rectUV.x = m_currentFrame.x * m_rectSize.x;
	m_rectUV.y = m_currentFrame.y * m_rectSize.y;
}

void Animation::ApplyRect()
{
	m_rect = sf::IntRect(m_rectUV, m_rectSize);
	setTextureRect(m_rect);
}


sf::IntRect Animation::GetRect() const
{
	return m_rect;
}

void Animation::Reset()
{
	m_currentFrame = { 0, 0 };
	m_currentTime = 0.f;
	CalculateRectUV();
	ApplyRect();
}
bool Animation::IsFinished() const {
	return (m_currentFrame.x >= m_frameNum.x - 1);
}
const sf::Texture* Animation::getTexture() const
{
	return m_texture;
}