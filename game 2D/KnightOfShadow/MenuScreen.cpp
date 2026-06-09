#include "MenuScreen.h"

MenuScreen::MenuScreen(sf::RenderWindow& window)
    : fadeAlpha(255.f), blinkTimer(0.f), blinkSpeed(1.f),
    showText(true), isActive(true)
{
    // Background
    if (!bgTexture.loadFromFile("Assets/Images/Background/bg8.jpg")) {
        std::cout << "Error loading background!\n";
    }
    bgSprite.setTexture(bgTexture);
    bgSprite.setScale(
        (float)window.getSize().x / bgSprite.getLocalBounds().width,
        (float)window.getSize().y / bgSprite.getLocalBounds().height
    );
    //Font Title
    if (!titleFont.loadFromFile("Assets/Font/fontTitle.TTF")) { // Font riêng cho tiêu đề
        std::cout << "Error loading title font!\n";
    }

    titleText.setFont(titleFont);
    titleText.setString("Knight of Shadow");
    titleText.setCharacterSize(100);
    titleText.setStyle(sf::Text::Italic | sf::Text::Bold);

    // Màu chữ chính (đỏ sáng, hơi neon)
    titleText.setFillColor(sf::Color(220, 20, 60));

    // Thêm viền phát sáng (đỏ đậm mờ)
    titleText.setOutlineThickness(3.f);
    titleText.setOutlineColor(sf::Color::Black);

    // Căn giữa và đặt vị trí
    sf::FloatRect titleBounds = titleText.getLocalBounds();
    titleText.setOrigin(titleBounds.left + titleBounds.width / 2.f, titleBounds.top + titleBounds.height / 2.f);
    titleText.setPosition((float)window.getSize().x / 2.f, (float)window.getSize().y - 200); // ngay trên pressText
    // Font Text
    if (!font.loadFromFile("Assets/Font/font.ttf")) {
        std::cout << "Error loading font!\n";
    }

    // Text
    pressText.setFont(font);
    pressText.setString("Press Any Key to Start");
    pressText.setCharacterSize(40);
    pressText.setFillColor(sf::Color::White);
    pressText.setPosition(
        (float)window.getSize().x / 2.f - pressText.getGlobalBounds().width / 2.f,
        (float)window.getSize().y - 120
    );

    // Fade-in
    fadeRect.setSize(sf::Vector2f((float)window.getSize().x, (float)window.getSize().y));
    fadeRect.setFillColor(sf::Color(0, 0, 0, 255));

    fadeRect.setSize(sf::Vector2f(window.getSize()));
    fadeRect.setFillColor(sf::Color(0, 0, 0, 0)); // ban đầu trong suốt

    //Âm thanh Intro + Start
    if (!introBuffer.loadFromFile("Assets/Music/boom.ogg")) {
        std::cout << "Error loading intro sound!\n";
    }
    introSound.setBuffer(introBuffer);
    introSound.setVolume(100.f);
    introSound.play();


    if (!startBuffer.loadFromFile("Assets/Music/boom.ogg")) {
        std::cout << "Error loading start sound!\n";
    }
    startSound.setBuffer(startBuffer);
    startSound.setVolume(100.f);

    //Theme
    musicVolume = 40.f;
    if (!bgMusic.openFromFile("Assets/Music/bg_music.ogg")) {
        std::cout << "Error loading menu theme!\n";
    }
    bgMusic.setLoop(true);     // Lặp vô hạn
    bgMusic.setVolume(musicVolume);   // Âm lượng 0–100
    bgMusic.play();
}

void MenuScreen::Update(float dt, sf::RenderWindow& window) {
    // Fade-in effect
    if (fadeAlpha > 0) {
        fadeAlpha -= 50 * dt;
        if (fadeAlpha < 0) fadeAlpha = 0;
        fadeRect.setFillColor(sf::Color(0, 0, 0, (sf::Uint8)fadeAlpha));
    }

    // ================================
    //   NHẤN PHÍM BẤT KÌ ĐỂ START
    // ================================
    if (!isFadingOut) {
        for (int k = 0; k < sf::Keyboard::KeyCount; k++) {
            if (sf::Keyboard::isKeyPressed((sf::Keyboard::Key)k)) {
                isFadingOut = true;
                introSound.stop();   // Tắt sound intro
                break;
            }
        }
    }

    // ================================
    //       HIỆU ỨNG FADE-OUT
    // ================================
    if (isFadingOut) {
        static bool startSoundPlayed = false;

        // Play sound start 1 lần duy nhất
        if (!startSoundPlayed) {
            startSound.play();
            startSoundPlayed = true;
        }

        // Fade-out screen
        fadeOutAlpha += 150 * dt;
        if (fadeOutAlpha > 255) fadeOutAlpha = 255;
        fadeRect.setFillColor(sf::Color(0, 0, 0, (sf::Uint8)fadeOutAlpha));

        // Fade-out nhạc menu
        if (musicVolume > 0.f) {
            musicVolume -= 80.f * dt;
            if (musicVolume < 0.f) musicVolume = 0.f;
            bgMusic.setVolume(musicVolume);
        }

        // Fade-out xong
        if (fadeOutAlpha >= 255) {
            bgMusic.stop();      // ❗ Nhạc dừng hoàn toàn → KHÔNG TRÙNG NỮA
            isActive = false;    // → Qua game
        }
    }
    else {
        // Blink text (chỉ chạy khi chưa fade-out)
        blinkTimer += dt * blinkSpeed;
        if (blinkTimer >= 1.f) {
            showText = !showText;
            blinkTimer = 0.f;
        }
    }
}

void MenuScreen::Draw(sf::RenderWindow& window) {
    // Vẽ background
    window.draw(bgSprite);
    window.draw(titleText);
    // Vẽ chữ "Press any key..."
    if (showText)
        window.draw(pressText);

    // Vẽ hiệu ứng mờ dần (fade-in hoặc fade-out)
    if (fadeAlpha > 0 || isFadingOut)
        window.draw(fadeRect);
}

// === CÀI ĐẶT HÀM RESETFADE TẠI ĐÂY ===
void MenuScreen::ResetFade() {
    // Đặt lại các cờ về false
    isFadingOutMap = false;
    isFadingInMap = false;

    // Xóa màu đen che màn hình
    mapFadeAlpha = 0.f;
    fadeInDelayTimer = 0.f;

    // Hủy callback (để game không load map mới khi đang reset)
    fadeCallback = nullptr;
}