#pragma once
#include <SFML/Graphics.hpp>
#include <vector>
#include <string>

class WinScreen {
public:
    WinScreen(float width, float height);

    bool HandleInput(sf::RenderWindow& window, sf::Event& event);
    void Update(sf::RenderWindow& window);
    void Draw(sf::RenderWindow& window);

    void SetScoreInfo(int currentScore, const std::vector<int>& history);

private:
    sf::Font font;
    sf::Text textTitle;
    sf::Text textReset;

    sf::Text textScore;
    sf::Text textHistory;

    sf::RectangleShape background;
};