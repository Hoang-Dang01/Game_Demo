#pragma once
#include <SFML/Graphics.hpp>
#include <SFML/Window.hpp>
#include <SFML/Audio.hpp>
#include <iostream>
#include <functional>

class MenuScreen {
private:
    sf::Texture bgTexture;
    sf::Sprite bgSprite;
    sf::Font font;
    sf::Font titleFont;
    sf::Text titleText;
    sf::Text pressText;

    float blinkTimer = 0.f;
    float blinkSpeed = 2.f;
    bool showText = true;

    sf::SoundBuffer introBuffer;
    sf::Sound introSound;
    sf::SoundBuffer startBuffer;
    sf::Sound startSound;
    sf::Music bgMusic;
    float musicVolume = 60.f;

    sf::RectangleShape fadeRect;

    float mapFadeAlpha = 0.f;         // dùng chung cho fade-out/fade-in
    float fadeSpeed = 300.f;          // tốc độ fade-out/fade-in

    std::function<void()> fadeCallback; // callback khi fade-out map xong

    bool isFadingInMap = false;
    bool isFadingOutMap = false;

    float fadeInDelayTimer = 0.f;     // delay trước khi fade-in

public:
    float fadeAlpha = 255.f;
    float fadeOutAlpha = 0.f;
    bool isActive = true;
    bool isFadingOut = false;

    MenuScreen(sf::RenderWindow& window);
    void Update(float dt, sf::RenderWindow& window);
    void Draw(sf::RenderWindow& window);

    // --- FADE MAP ---
    void StartFadeOutMap(std::function<void()> callback, float delayInSec = 0.f) {
        if (!isFadingOutMap && !isFadingInMap) {
            isFadingOutMap = true;
            mapFadeAlpha = 0.f;
            fadeCallback = callback;
            fadeInDelayTimer = delayInSec; // thiết lập thời gian delay trước fade-in
        }
    }

    void UpdateMapFade(float dt) {
        // --- Fade-out ---
        if (isFadingOutMap) {
            mapFadeAlpha += fadeSpeed * dt;
            if (mapFadeAlpha >= 255.f) {
                mapFadeAlpha = 255.f;
                isFadingOutMap = false;
                if (fadeCallback) fadeCallback(); // load map mới

                // nếu có delay trước fade-in
                if (fadeInDelayTimer <= 0.f)
                    isFadingInMap = true;
            }
        }
        // --- Delay trước fade-in ---
        else if (fadeInDelayTimer > 0.f) {
            fadeInDelayTimer -= dt;
            if (fadeInDelayTimer <= 0.f) {
                fadeInDelayTimer = 0.f;
                isFadingInMap = true;
            }
        }
        // --- Fade-in ---
        else if (isFadingInMap) {
            mapFadeAlpha -= fadeSpeed * dt;
            if (mapFadeAlpha <= 0.f) {
                mapFadeAlpha = 0.f;
                isFadingInMap = false;
            }
        }
    }

    void DrawMapFade(sf::RenderWindow& window) {
        if (isFadingOutMap || isFadingInMap || fadeInDelayTimer > 0.f) {
            sf::RectangleShape rect(sf::Vector2f(static_cast<float>(window.getSize().x),
                static_cast<float>(window.getSize().y)));
            sf::Uint8 alpha = static_cast<sf::Uint8>(mapFadeAlpha);
            rect.setFillColor(sf::Color(0, 0, 0, alpha));
            window.draw(rect);
        }
    }

    // --- KHAI BÁO HÀM XÓA HIỆU ỨNG FADE ---
    void ResetFade();
    // --------------------------------------

    // --- getters ---
    bool IsMapFadingOut() const { return isFadingOutMap; }
    bool IsMapFadingIn() const { return isFadingInMap; }
};