#pragma once
#include "BaseEnemy.h"
#include "BossAnimation.h"

enum class BossAttackType {
    None,
    Attack,
    Attack1,
    Attack2
};

class Enemy1 : public BaseEnemy {
private:
    // === Animation ===
    BossAnimation idleAnim;
    BossAnimation walkAnim;
	BossAnimation attackAnim;
    BossAnimation attack1Anim;
    BossAnimation attack2Anim;
    BossAnimation deathAnim;

    //Âm thanh
    sf::SoundBuffer at_buffer;
    sf::SoundBuffer at1_buffer;
    sf::SoundBuffer at2_buffer;
    sf::Sound attackSound;

    // === Tấn công ===
    BossAttackType attackType = BossAttackType::None;
    // --- Hành vi ---
    float moveSpeed = 100.f;
    float detectionRange = 700.f;
    float attackRange = 200.f;

    float attackCooldown = 0.2f;
    float attackCooldownTimer = 0.f;
    float attackTimer = 0.f;

    float attackActiveTime_Attack = 0.5f;
    float attackDuration_Attack = 0.5f;
    float attackActiveTime_Attack1 = 0.8f;
    float attackDuration_Attack1 = 0.5f;
    float attackActiveTime_Attack2 = 0.5f;
    float attackDuration_Attack2 = 1.0f;
    bool hitboxActive = false;

    float groundY = 550.f;

    // === Hàm nội bộ ===
    void UpdateAttackAnim(float deltaTime, const sf::Texture*& tex, sf::IntRect& rect);
    void ChangeState(EnemyState newState);

public:
    // === Constructor ===
    Enemy1(sf::Texture& texIdle,
        sf::Texture& texWalk,
        sf::Texture& texAttack,
        sf::Texture& texAttack1,
        sf::Texture& texAttack2,
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
