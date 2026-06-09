#include "GameOverScreen.h"
#include <iostream>

GameOverScreen::GameOverScreen(float width, float height) {
    // 1. Load Font
    if (!font.loadFromFile("Assets/Font/fontTutorial.ttf")) {
        std::cerr << "Error loading font inside GameOverScreen" << std::endl;
    }

    // 2. Nền đen mờ
    background.setSize(sf::Vector2f(width, height));
    background.setFillColor(sf::Color(0, 0, 0, 220)); // Đen đậm hơn chút

    // 3. Chữ GAME OVER
    textTitle.setFont(font);
    textTitle.setString("GAME OVER");
    textTitle.setCharacterSize(90);
    textTitle.setFillColor(sf::Color::Red);
    textTitle.setOutlineColor(sf::Color::Black);
    textTitle.setOutlineThickness(4);

    sf::FloatRect titleRect = textTitle.getLocalBounds();
    textTitle.setOrigin(titleRect.left + titleRect.width / 2.0f, titleRect.top + titleRect.height / 2.0f);
    textTitle.setPosition(width / 2.0f, height / 2.0f - 250); // Dời lên cao

    // 4. Setup Text Score (Điểm hiện tại)
    textScore.setFont(font);
    textScore.setCharacterSize(40);
    textScore.setFillColor(sf::Color::Yellow);

    // 5. Setup Text History (Lịch sử đấu)
    textHistory.setFont(font);
    textHistory.setCharacterSize(28);
    textHistory.setFillColor(sf::Color(200, 200, 200)); // Màu xám trắng
    textHistory.setLineSpacing(1.2f); // Giãn dòng cho dễ đọc

    // 6. Nút TRY AGAIN
    textReset.setFont(font);
    textReset.setString("TRY AGAIN");
    textReset.setCharacterSize(50);
    textReset.setFillColor(sf::Color::White);

    sf::FloatRect resetRect = textReset.getLocalBounds();
    textReset.setOrigin(resetRect.left + resetRect.width / 2.0f, resetRect.top + resetRect.height / 2.0f);
    textReset.setPosition(width / 2.0f, height / 2.0f + 300); // Dời xuống dưới cùng
}

void GameOverScreen::SetScoreInfo(int currentScore, const std::vector<int>& history) {
    // --- Cập nhật điểm hiện tại ---
    textScore.setString("YOUR SCORE: " + std::to_string(currentScore));
    sf::FloatRect sRect = textScore.getLocalBounds();
    textScore.setOrigin(sRect.left + sRect.width / 2.0f, sRect.top + sRect.height / 2.0f);
    textScore.setPosition(1600 / 2.0f, 896 / 2.0f - 150);

    // --- Cập nhật bảng xếp hạng ---
    std::string historyStr = "TOP HIGH SCORES:\n";
    int count = 0;
    for (int s : history) {
        count++;
        if (count == 1) historyStr += "[TOP 1]   " + std::to_string(s) + "\n";
        else            historyStr += "  #" + std::to_string(count) + "       " + std::to_string(s) + "\n";

        if (count >= 5) break; // Chỉ hiện Top 5
    }
    textHistory.setString(historyStr);

    sf::FloatRect hRect = textHistory.getLocalBounds();
    textHistory.setOrigin(hRect.left + hRect.width / 2.0f, hRect.top); // Căn giữa ngang, top dọc
    textHistory.setPosition(1600 / 2.0f, 896 / 2.0f - 80);
}

void GameOverScreen::Update(sf::RenderWindow& window) {
    sf::Vector2i mousePos = sf::Mouse::getPosition(window);
    sf::Vector2f mousePosF(static_cast<float>(mousePos.x), static_cast<float>(mousePos.y));

    // Hiệu ứng hover nút Try Again
    if (textReset.getGlobalBounds().contains(mousePosF)) {
        textReset.setFillColor(sf::Color::Yellow);
        textReset.setScale(1.1f, 1.1f);
    }
    else {
        textReset.setFillColor(sf::Color::White);
        textReset.setScale(1.0f, 1.0f);
    }
}

bool GameOverScreen::HandleInput(sf::RenderWindow& window, sf::Event& event) {
    if (event.type == sf::Event::MouseButtonPressed) {
        if (event.mouseButton.button == sf::Mouse::Left) {
            sf::Vector2i mousePos = sf::Mouse::getPosition(window);
            sf::Vector2f mousePosF(static_cast<float>(mousePos.x), static_cast<float>(mousePos.y));

            if (textReset.getGlobalBounds().contains(mousePosF)) {
                return true;
            }
        }
    }
    return false;
}

void GameOverScreen::Draw(sf::RenderWindow& window) {
    window.draw(background);
    window.draw(textTitle);
    window.draw(textScore);
    window.draw(textHistory);
    window.draw(textReset);
}