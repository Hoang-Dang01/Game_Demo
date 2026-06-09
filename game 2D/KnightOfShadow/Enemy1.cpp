#include "Enemy1.h"
#include <cmath>
#include <iostream>

Enemy1::Enemy1(sf::Texture& texIdle, sf::Texture& texWalk, sf::Texture& texAttack,
    sf::Texture& texAttack1, sf::Texture& texAttack2,
    sf::Texture& texDeath)
    :idleAnim(texIdle, { 6, 1 }, 0.2f),
    walkAnim(texWalk, { 12, 1 }, 0.1f),
    attackAnim(texAttack, { 15, 1 }, 0.08f, false),
    attack1Anim(texAttack1, { 9, 2 }, 0.08f, false),
    attack2Anim(texAttack2, { 10, 2 }, 0.1f, false),
    deathAnim(texDeath, { 10, 2 }, 0.15f, false)
{
    state = EnemyState::Idle;
    attackType = BossAttackType::None;

    sprite.setTexture(texIdle);
    sprite.setTextureRect(idleAnim.GetRect());
    auto rect = idleAnim.GetRect();
    sprite.setPosition(800, 400);
    sprite.setOrigin(rect.width / 2.f, rect.height / 2.f);
    sprite.setScale(1.f, 1.f);

    currentAttackBox = sf::FloatRect();
    bodyHitbox = sf::FloatRect();

    // Load âm thanh
    at_buffer.loadFromFile("Assets/Sound effect/Enemy/Boss2/attack.mp3");
    at1_buffer.loadFromFile("Assets/Sound effect/Enemy/Boss2/at1.mp3");
    at2_buffer.loadFromFile("Assets/Sound effect/Enemy/Boss2/at2.mp3");

    maxHealth = 2000.f;
    health = maxHealth;

    font.loadFromFile("Assets/Font/1.ttf");
    InitHPBar({ 500.f, 70.f }, 600.f, 20.f, "Malzakar: Lord of the Eternal Hellforge");
    bossNameText.setPosition(hpBack.getPosition().x + 95.f, hpBack.getPosition().y - 35.f);
}

void Enemy1::HandleInput(float deltaTime, const sf::Vector2f& playerPosition)
{
    velocity.x = 0.f;

    if (state == EnemyState::Attacking || state == EnemyState::Death || isDead)
    {
        if (attackCooldownTimer > 0)
            attackCooldownTimer -= deltaTime;
        return;
    }

    sf::Vector2f direction = playerPosition - sprite.getPosition();
    float distance = std::hypot(direction.x, direction.y);
    facingRight = direction.x < 0;

    if (distance <= attackRange)
    {
        attackTimer += deltaTime;

        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0.f;

            // Random 1 trong 3 đòn tấn công
            int randomAttack = rand() % 3; // 0,1,2
            switch (randomAttack)
            {
            case 2:
                attackType = BossAttackType::Attack;
                break;
            case 1:
                attackType = BossAttackType::Attack1;
                break;
            case 0:
                attackType = BossAttackType::Attack2;
                break;
            }

            ChangeState(EnemyState::Attacking);
            hitboxActive = false;
            attackCooldownTimer = 0.f;
        }
    }
    else if (distance <= detectionRange)
    {
        ChangeState(EnemyState::Walking);
        if (distance > 0.f)
        {
            direction /= distance;
            velocity.x = direction.x * moveSpeed;
        }
    }
    else
    {
        ChangeState(EnemyState::Idle);
    }

    sprite.setScale(facingRight ? 1.f : -1.f, 1.f);

    if (attackCooldownTimer > 0)
        attackCooldownTimer -= deltaTime;
}

void Enemy1::Update(float deltaTime)
{
    UpdateHitCooldown(deltaTime);

    if (isDead)
    {
        if (state != EnemyState::Death)
            ChangeState(EnemyState::Death);
    }

    if (state == EnemyState::Death && deathAnim.IsFinished())
        return;

    if (!isOnGround)
        velocity.y += gravity * deltaTime;

    sprite.move(velocity * deltaTime);

    if (sprite.getPosition().y >= groundY)
    {
        sprite.setPosition(sprite.getPosition().x, groundY);
        velocity.y = 0;
        isOnGround = true;
    }

    const sf::Texture* currentTexture = nullptr;
    sf::IntRect currentRect;

    switch (state)
    {
    case EnemyState::Idle:
        idleAnim.Update(deltaTime);
        currentTexture = idleAnim.GetTexture();
        currentRect = idleAnim.GetRect();
        break;

    case EnemyState::Walking:
        walkAnim.Update(deltaTime);
        currentTexture = walkAnim.GetTexture();
        currentRect = walkAnim.GetRect();
        break;

    case EnemyState::Attacking:
        UpdateAttackAnim(deltaTime, currentTexture, currentRect);
        break;

    case EnemyState::Death:
        deathAnim.Update(deltaTime);
        currentTexture = deathAnim.GetTexture();
        currentRect = deathAnim.GetRect();
        if (deathAnim.IsFinished())
            deathAnimFinished = true;
        break;
    }

    if (currentTexture && sprite.getTexture() != currentTexture)
        sprite.setTexture(*currentTexture);
    sprite.setTextureRect(currentRect);

    // Hitbox tấn công
    if (state == EnemyState::Attacking && hitboxActive)
    {
        sf::FloatRect attackBox;
        float enemyX = sprite.getPosition().x;
        float enemyY = sprite.getPosition().y;

        switch (attackType)
        {
        case BossAttackType::Attack:
            attackBox.width = 250.f;
            attackBox.height = 250.f;
            attackBox.top = enemyY - 100.f;
            attackBox.left = facingRight ? enemyX - 220.f : enemyX + 20.f;
            break;

        case BossAttackType::Attack1:
            attackBox.width = 500.f;
            attackBox.height = 50.f;
            attackBox.top = enemyY + 130.f;
            attackBox.left = enemyX - attackBox.width / 2.f;
            break;

        case BossAttackType::Attack2:
            attackBox.width = 300.f;
            attackBox.height = 400.f;
            attackBox.top = enemyY - 200.f;
            attackBox.left = facingRight ? enemyX - 300.f : enemyX + 20.f;
            break;

        default:
            attackBox = sf::FloatRect();
            break;
        }

        currentAttackBox = attackBox;
    }
    else
        currentAttackBox = sf::FloatRect();

    // Hitbox thân
    bodyHitbox.width = 200.f;
    bodyHitbox.height = 200.f;
    bodyOffset = sf::Vector2f(-bodyHitbox.width / 2.f, -bodyHitbox.height / 2.f);
    sf::Vector2f pos = sprite.getPosition();
    bodyHitbox.left = pos.x + bodyOffset.x;
    bodyHitbox.top = pos.y + (bodyOffset.y + 50.f);
}

void Enemy1::UpdateAttackAnim(float deltaTime, const sf::Texture*& tex, sf::IntRect& rect)
{
    attackTimer += deltaTime;

    switch (attackType)
    {
    case BossAttackType::Attack:
        attackAnim.Update(deltaTime);
        tex = attackAnim.GetTexture();
        rect = attackAnim.GetRect();
        break;
    case BossAttackType::Attack1:
        attack1Anim.Update(deltaTime);
        tex = attack1Anim.GetTexture();
        rect = attack1Anim.GetRect();
        break;
    case BossAttackType::Attack2:
        attack2Anim.Update(deltaTime);
        tex = attack2Anim.GetTexture();
        rect = attack2Anim.GetRect();
        break;
    default:
        tex = idleAnim.GetTexture();
        rect = idleAnim.GetRect();
        break;
    }

    switch (attackType)
    {
    case BossAttackType::Attack:
        hitboxActive = (attackTimer >= attackActiveTime_Attack &&
            attackTimer <= attackActiveTime_Attack + attackDuration_Attack);
        break;

    case BossAttackType::Attack1:
        hitboxActive = (attackTimer >= attackActiveTime_Attack1 &&
            attackTimer <= attackActiveTime_Attack1 + attackDuration_Attack1);
        break;

    case BossAttackType::Attack2:
        hitboxActive = (attackTimer >= attackActiveTime_Attack2 &&
            attackTimer <= attackActiveTime_Attack2 + attackDuration_Attack2);
        break;

    default:
        hitboxActive = false;
        break;
    }

    bool finished = false;
    if (attackType == BossAttackType::Attack) finished = attackAnim.IsFinished();
    if (attackType == BossAttackType::Attack1) finished = attack1Anim.IsFinished();
    if (attackType == BossAttackType::Attack2) finished = attack2Anim.IsFinished();

    if (finished)
    {
        ChangeState(EnemyState::Idle);
        attackType = BossAttackType::None;
        attackTimer = 0.f;
        hitboxActive = false;
    }
}

void Enemy1::Draw(sf::RenderWindow& window)
{
    window.draw(sprite);

    // Vẽ thanh máu ở trên cùng giữa màn hình
    window.draw(hpBack);
    window.draw(hpFront);
    window.draw(hpBorder);
    UpdateHPBarText();

    // đặt text hơi cao hơn thanh
    sf::Vector2f hpPos = hpBack.getPosition();
    sf::Vector2f hpSize = hpBack.getSize();
    hpText.setPosition(hpPos.x + hpSize.x / 2.f, hpPos.y + hpSize.y / 2.f + 20.f); // -8 để lên trên
    window.draw(hpText);
    window.draw(bossNameText);

    // Hitbox tấn công (nếu muốn hiển thị)
  /*  if (state == EnemyState::Attacking && hitboxActive)
    {
        sf::RectangleShape atkBoxShape;
        atkBoxShape.setPosition(currentAttackBox.left, currentAttackBox.top);
        atkBoxShape.setSize({ currentAttackBox.width, currentAttackBox.height });
        atkBoxShape.setFillColor(sf::Color(255, 255, 0, 60));
        atkBoxShape.setOutlineColor(sf::Color::Yellow);
        atkBoxShape.setOutlineThickness(1.f);
        window.draw(atkBoxShape);
    }*/

    // Hitbox thân
   /* sf::RectangleShape bodyBox;
    bodyBox.setPosition(bodyHitbox.left, bodyHitbox.top);
    bodyBox.setSize({ bodyHitbox.width, bodyHitbox.height });
    bodyBox.setFillColor(sf::Color(255, 0, 0, 40));
    bodyBox.setOutlineColor(sf::Color::Red);
    bodyBox.setOutlineThickness(1.f);
    window.draw(bodyBox);*/
}

void Enemy1::ChangeState(EnemyState newState)
{
    if (state == newState || state == EnemyState::Death)
        return;

    state = newState;

    switch (state)
    {
    case EnemyState::Idle: idleAnim.Reset(); break;
    case EnemyState::Walking: walkAnim.Reset(); break;
    case EnemyState::Attacking:
        if (attackType == BossAttackType::Attack) {
            attackAnim.Reset();
            attackSound.setBuffer(at_buffer);
            attackSound.setVolume(15.f);
            attackSound.play();
        }
        else if (attackType == BossAttackType::Attack1) {
            attack1Anim.Reset();
            attackSound.setBuffer(at1_buffer);
            attackSound.setVolume(15.f);
            attackSound.play();
        }
        else if (attackType == BossAttackType::Attack2) {
            attack2Anim.Reset();
            attackSound.setBuffer(at2_buffer);
            attackSound.setVolume(15.f);
            attackSound.play();
        }
        break;
    case EnemyState::Death: deathAnim.Reset(); break;
    }
}

void Enemy1::TakeDamage(int damage)
{
    BaseEnemy::TakeDamage(damage);

    // cập nhật thanh đỏ
    float ratio = health / maxHealth;
    hpFront.setSize(sf::Vector2f(600.f * ratio, 20.f));
}