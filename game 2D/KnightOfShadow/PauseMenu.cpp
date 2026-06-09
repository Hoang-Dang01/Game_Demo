#include "PauseMenu.h"
#include <iostream>

PauseMenu::PauseMenu(sf::RenderWindow& window) {
    isPaused = false;

    // Load font (Bạn nhớ chỉnh đúng đường dẫn font trong máy bạn)
    if (!font.loadFromFile("Assets/Font/fontTutorial.ttf")) {
        std::cerr << "Khong tim thay font cho Pause Menu!" << std::endl;
        // Nếu lỗi font, bạn có thể load font mặc định hoặc kệ nó (sẽ không hiện chữ)
    }

    // --- SETUP GIAO DIỆN ---

    // 1. Nền đen mờ (Alpha = 150)
    background.setSize(sf::Vector2f((float)window.getSize().x, (float)window.getSize().y));
    background.setFillColor(sf::Color(0, 0, 0, 150));

    // 2. Tiêu đề "PAUSED"
    textTitle.setFont(font);
    textTitle.setString("PAUSE GAME");
    textTitle.setCharacterSize(80);
    textTitle.setFillColor(sf::Color::White);
    // Căn giữa màn hình
    sf::FloatRect titleRect = textTitle.getLocalBounds();
    textTitle.setOrigin(titleRect.width / 2, titleRect.height / 2);
    textTitle.setPosition(window.getSize().x / 2.0f, window.getSize().y / 2.0f - 150);

    // 3. Nút "RESUME" (Chơi tiếp)
    btnResume.setFont(font);
    btnResume.setString("RESUME");
    btnResume.setCharacterSize(50);
    btnResume.setFillColor(sf::Color::White);
    sf::FloatRect resumeRect = btnResume.getLocalBounds();
    btnResume.setOrigin(resumeRect.width / 2, resumeRect.height / 2);
    btnResume.setPosition(window.getSize().x / 2.0f, window.getSize().y / 2.0f);

    // 4. Nút "RESET" (Chơi lại)
    btnReset.setFont(font);
    btnReset.setString("RESET");
    btnReset.setCharacterSize(50);
    btnReset.setFillColor(sf::Color::White);
    sf::FloatRect resetRect = btnReset.getLocalBounds();
    btnReset.setOrigin(resetRect.width / 2, resetRect.height / 2);
    btnReset.setPosition(window.getSize().x / 2.0f, window.getSize().y / 2.0f + 100);
}

void PauseMenu::Toggle() {
    isPaused = !isPaused;
}

bool PauseMenu::IsPaused() {
    return isPaused;
}

void PauseMenu::Update(sf::RenderWindow& window) {
    if (!isPaused) return;

    // Lấy vị trí chuột
    sf::Vector2i mousePosCtx = sf::Mouse::getPosition(window);
    sf::Vector2f mousePos = window.mapPixelToCoords(mousePosCtx);

    // Hiệu ứng: Nếu chuột đè lên nút nào thì nút đó đổi màu Vàng, không thì màu Trắng
    if (btnResume.getGlobalBounds().contains(mousePos))
        btnResume.setFillColor(sf::Color::Yellow);
    else
        btnResume.setFillColor(sf::Color::White);

    if (btnReset.getGlobalBounds().contains(mousePos))
        btnReset.setFillColor(sf::Color::Yellow);
    else
        btnReset.setFillColor(sf::Color::White);
}

PauseMenu::ButtonResult PauseMenu::HandleInput(sf::Event& event, sf::RenderWindow& window) {

    // 1. Xử lý phím ESC để bật/tắt
    if (event.type == sf::Event::KeyPressed && event.key.code == sf::Keyboard::Escape) {
        Toggle();
        return Nothing; // Chỉ bật tắt menu, chưa chọn nút nào
    }

    // 2. Xử lý Click chuột (chỉ khi menu đang hiện)
    if (isPaused && event.type == sf::Event::MouseButtonPressed) {
        if (event.mouseButton.button == sf::Mouse::Left) {
            // Lấy vị trí click
            sf::Vector2i mousePosCtx = sf::Mouse::getPosition(window);
            sf::Vector2f mousePos = window.mapPixelToCoords(mousePosCtx);

            // Check xem click vào nút nào
            if (btnResume.getGlobalBounds().contains(mousePos)) {
                Toggle(); // Tắt menu để chơi tiếp
                return Resume;
            }
            if (btnReset.getGlobalBounds().contains(mousePos)) {
                Toggle(); // Tắt menu để reset
                return Reset;
            }
        }
    }

    return Nothing;
}

void PauseMenu::Draw(sf::RenderWindow& window) {
    if (isPaused) {
        window.draw(background); // Vẽ nền đen mờ trước
        window.draw(textTitle);
        window.draw(btnResume);
        window.draw(btnReset);
    }
}