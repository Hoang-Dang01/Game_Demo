#pragma once
#include <SFML/Graphics.hpp>
#include <vector>
#include <string>

class GameOverScreen {
public:
    GameOverScreen(float width, float height);

    // Xử lý sự kiện click chuột
    bool HandleInput(sf::RenderWindow& window, sf::Event& event);

    void Update(sf::RenderWindow& window);
    void Draw(sf::RenderWindow& window);

    // HÀM MỚI: Nhận điểm hiện tại và danh sách điểm cao để hiển thị
    void SetScoreInfo(int currentScore, const std::vector<int>& history);

private:
    sf::Font font;
    sf::Text textTitle;
    sf::Text textReset;

    // Text hiển thị điểm
    sf::Text textScore;
    sf::Text textHistory;

    sf::RectangleShape background;
};