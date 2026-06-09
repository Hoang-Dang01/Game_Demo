#pragma once
#include "BaseEnemy.h"
#include "BossAnimation.h"

class Enemy2 : public BaseEnemy {
private:
    // === Animation ===
    BossAnimation idleAnim;
    BossAnimation walkAnim;
    BossAnimation attackAnim;
    BossAnimation deathAnim;

    //Âm thanh
    sf::SoundBuffer at_buffer;
    sf::Sound attackSound;

    // --- HĂ nh vi ---
    float moveSpeed = 100.f;
    float detectionRange = 700.f;
    float attackRange = 150.f;

    float attackCooldown = 0.2f;
    float attackCooldownTimer = 0.f;
    float attackTimer = 0.f;

    float attackActiveTime_Attack = 0.5f;
    float attackDuration_Attack = 0.7f;

    float groundY = 610.f;
    bool hitboxActive = false;

    // === HĂ m ná»™i bá»™ ===
    void UpdateAttackAnim(float deltaTime, const sf::Texture*& tex, sf::IntRect& rect);
    void ChangeState(EnemyState newState);

public:
    // === Constructor ===
    Enemy2(sf::Texture& texIdle,
        sf::Texture& texWalk,
        sf::Texture& texAttack,
        sf::Texture& texDeath);

    // === Override ===
    void HandleInput(float deltaTime, const sf::Vector2f& playerPosition);
    void Update(float deltaTime) override;
    void Draw(sf::RenderWindow& window) override;
    void TakeDamage(int damage) override;

    // **Thêm hàm DrawHP**
    void DrawHP(sf::RenderWindow& window) {
        window.draw(hpBack);
        window.draw(hpFront);
    }
};