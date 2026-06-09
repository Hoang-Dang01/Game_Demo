#pragma once
#include <SFML/Graphics.hpp>
#include <SFML/Audio.hpp>
#include "Animation.h"
#include <vector>
#include <iostream>

struct DashTrail {
    sf::Sprite sprite;
    float alpha = 0.f;
};

extern std::vector<DashTrail> dashTrails;
extern float trailSpawnTimer;
extern float trailSpawnInterval;

enum PlayerState {
    Idle,
    Walking,
    Attacking1,
    Attacking2,
    Attacking3,
    Skill1,
    Death
};

class Player {
private:
    sf::SoundBuffer dashBuffer;
    sf::Sound dashSound;

    sf::SoundBuffer jumpBuffer;
    sf::Sound jumpSound;

    sf::SoundBuffer at1_buffer;
    sf::SoundBuffer at2_buffer;
    sf::SoundBuffer at3_buffer;
    sf::Sound attackSound;

    sf::SoundBuffer skill_buffer;
    sf::Sound skillSound;

    sf::Sprite sprite;
    sf::Vector2f velocity;

    bool facingRight = true;
    bool comboQueued = false;
    bool wasMousePressedLastFrame = false; // để chống spam click combo

    PlayerState state;

    // Các animation
    Animation idleAnim;
    Animation walkAnim;
    Animation attackAnim1;
    Animation attackAnim2;
    Animation attackAnim3;
    Animation skill1Anim;
    Animation deathAnim;

    bool isOnGround = true;
    float gravity = 900.f;      // lực hấp dẫn
    float jumpStrength = -550.f; // lực nhảy (âm vì hướng lên trên)
    float groundY = 655.f; // y = vị trí mặt đất

    // Dash variables
    bool isDashing = false;
    float dashTime = 0.f;
    float dashDuration = 0.18f;   // thời gian dash (giây)
    float dashSpeed = 500.f;      // tốc độ dash
    float dashCooldown = 1.f;    // thời gian hồi dash
    float dashCooldownTimer = 0.f;

    bool hasDashedInSkill1 = false;

    sf::FloatRect currentAttackBox; //Hitbox đánh
    sf::FloatRect skill1Hitbox;

    sf::FloatRect bodyHitbox;   //Hitbox bị đánh
    sf::Vector2f bodyOffset;

    bool recentlyHit = false;
    float hitCooldown = 0.5f;
    float hitTimer = 0.f;
    bool isDead = false;

    float skill1Cooldown = 3.f;      // thời gian hồi chiêu (3s)
    float skill1CooldownTimer = 0.f;

    sf::RectangleShape hpBarBack;   // nền đen/màu tối
    sf::RectangleShape hpBarFront;  // thanh máu đỏ
    float maxHealth;                // HP tối đa
    float health;                   // HP hiện tại
    sf::Font font;

    sf::Texture avatarTex;
    sf::Sprite avatarSprite;

public:
    // Constructor đồng bộ với Player.cpp
    Player(sf::Texture& texIdle, sf::Texture& texWalk,
        sf::Texture& texAttack1, sf::Texture& texAttack2, sf::Texture& texAttack3,
        sf::Texture& texSkill1, sf::Texture& texDeath);

    void HandleInput(float deltaTime);
    void Update(float deltaTime);
    void Draw(sf::RenderWindow& window);

    // Quản lý trạng thái
    void ChangeState(PlayerState newState);

    // Hỗ trợ vị trí và sprite
    sf::Sprite& GetSprite();
    sf::Vector2f GetPosition() const;
    void SetPosition(const sf::Vector2f& pos);

    sf::FloatRect GetAttackBox() const { return currentAttackBox; }
    sf::FloatRect GetSkill1Hitbox() const { return skill1Hitbox; }
    sf::FloatRect GetGlobalBounds() const { return sprite.getGlobalBounds(); }
    PlayerState GetState() const { return state; }

    void TakeDamage(int dmg) {
        if (isDead || recentlyHit) return;
        health -= dmg;
        recentlyHit = true;
        std::cout << "Player HP: " << health << "\n";
        if (health < 0) health = 0;
        recentlyHit = true;
        UpdateHPBar();
        if (health <= 0) {
            isDead = true;
            isDashing = false;
            dashTime = 0.f;
            ChangeState(Death);
            std::cout << "Player da chet!\n";
        }
    }

    // --- KHAI BÁO HÀM HỒI MÁU ---
    void SetHP(float value);
    // ----------------------------

    void UpdateHitCooldown(float dt) {
        if (recentlyHit) {
            hitTimer += dt;
            if (hitTimer >= hitCooldown) {
                recentlyHit = false;
                hitTimer = 0.f;
            }
        }
    }
    bool IsOnGround() const { return isOnGround; }
    sf::FloatRect GetBodyHitbox() const { return bodyHitbox; }
    bool IsDead() const { return isDead; }
    float GetHealth() const { return health; }

    void UpdateHPBar();
};