#include "WinScreen.h"
#include <iostream>

WinScreen::WinScreen(float width, float height) {
    if (!font.loadFromFile("Assets/Font/fontTutorial.ttf")) {
        std::cerr << "Error loading font inside WinScreen" << std::endl;
    }

    // Nền đen mờ
    background.setSize(sf::Vector2f(width, height));
    background.setFillColor(sf::Color(0, 0, 0, 220));

    // Chữ VICTORY
    textTitle.setFont(font);
    textTitle.setString("VICTORY!");
    textTitle.setCharacterSize(100);

    // Màu Vàng Gold
    textTitle.setFillColor(sf::Color(255, 215, 0));
    textTitle.setOutlineColor(sf::Color::White);
    textTitle.setOutlineThickness(3);

    sf::FloatRect titleRect = textTitle.getLocalBounds();
    textTitle.setOrigin(titleRect.left + titleRect.width / 2.0f, titleRect.top + titleRect.height / 2.0f);
    textTitle.setPosition(width / 2.0f, height / 2.0f - 250);

    // Setup Text Score
    textScore.setFont(font);
    textScore.setCharacterSize(40);
    textScore.setFillColor(sf::Color::Cyan); // Màu xanh dương sáng

    // Setup Text History
    textHistory.setFont(font);
    textHistory.setCharacterSize(28);
    textHistory.setFillColor(sf::Color(200, 200, 200));
    textHistory.setLineSpacing(1.2f);

    // Nút PLAY AGAIN
    textReset.setFont(font);
    textReset.setString("PLAY AGAIN");
    textReset.setCharacterSize(50);
    textReset.setFillColor(sf::Color::White);

    sf::FloatRect resetRect = textReset.getLocalBounds();
    textReset.setOrigin(resetRect.left + resetRect.width / 2.0f, resetRect.top + resetRect.height / 2.0f);
    textReset.setPosition(width / 2.0f, height / 2.0f + 300);
}

void WinScreen::SetScoreInfo(int currentScore, const std::vector<int>& history) {
    // --- Cập nhật điểm ---
    textScore.setString("FINAL SCORE: " + std::to_string(currentScore));
    sf::FloatRect sRect = textScore.getLocalBounds();
    textScore.setOrigin(sRect.left + sRect.width / 2.0f, sRect.top + sRect.height / 2.0f);
    textScore.setPosition(1600 / 2.0f, 896 / 2.0f - 150);

    // --- Cập nhật lịch sử ---
    std::string historyStr = "HALL OF FAME:\n";
    int count = 0;
    for (int s : history) {
        count++;
        if (count == 1) historyStr += "[CHAMPION] " + std::to_string(s) + "\n";
        else            historyStr += "  #" + std::to_string(count) + "         " + std::to_string(s) + "\n";

        if (count >= 5) break;
    }
    textHistory.setString(historyStr);

    sf::FloatRect hRect = textHistory.getLocalBounds();
    textHistory.setOrigin(hRect.left + hRect.width / 2.0f, hRect.top);
    textHistory.setPosition(1600 / 2.0f, 896 / 2.0f - 80);
}

void WinScreen::Update(sf::RenderWindow& window) {
    sf::Vector2i mousePos = sf::Mouse::getPosition(window);
    sf::Vector2f mousePosF(static_cast<float>(mousePos.x), static_cast<float>(mousePos.y));

    if (textReset.getGlobalBounds().contains(mousePosF)) {
        textReset.setFillColor(sf::Color::Cyan);
        textReset.setScale(1.1f, 1.1f);
    }
    else {
        textReset.setFillColor(sf::Color::White);
        textReset.setScale(1.0f, 1.0f);
    }
}

bool WinScreen::HandleInput(sf::RenderWindow& window, sf::Event& event) {
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

void WinScreen::Draw(sf::RenderWindow& window) {
    window.draw(background);
    window.draw(textTitle);
    window.draw(textScore);
    window.draw(textHistory);
    window.draw(textReset);
}