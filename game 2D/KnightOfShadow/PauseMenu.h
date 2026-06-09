#pragma once
#include <SFML/Graphics.hpp>

class PauseMenu {
public:
    // Kiểu dữ liệu để báo về main xem người dùng ấn nút nào
    enum ButtonResult {
        Nothing, // Chưa ấn gì
        Resume,  // Ấn nút Chơi tiếp
        Reset    // Ấn nút Reset
    };

    PauseMenu(sf::RenderWindow& window); // Constructor

    void Update(sf::RenderWindow& window); // Để làm hiệu ứng chuột (hover)
    void Draw(sf::RenderWindow& window);   // Vẽ menu

    // Hàm xử lý sự kiện (ấn phím ESC hoặc click chuột)
    ButtonResult HandleInput(sf::Event& event, sf::RenderWindow& window);

    void Toggle();       // Bật/Tắt menu
    bool IsPaused();     // Kiểm tra đang pause hay không

private:
    bool isPaused;
    sf::Font font;

    sf::Text textTitle;
    sf::Text btnResume;
    sf::Text btnReset;

    sf::RectangleShape background; // Nền đen mờ che game
};