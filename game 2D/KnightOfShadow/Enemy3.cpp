#include "Enemy3.h"
#include <cmath>
#include <iostream>

Enemy3::Enemy3(sf::Texture& texIdle, sf::Texture& texWalk, sf::Texture& texAttack,
    sf::Texture& texAttack1, sf::Texture& texAttack2, sf::Texture& texTele, sf::Texture& texDeath)
    : idleAnim(texIdle, { 9, 1 }, 0.15f),
    walkAnim(texWalk, { 8, 1 }, 0.1f),
    attackAnim(texAttack, { 10, 4 }, 0.1f, false),
    attack1Anim(texAttack1, { 10, 2 }, 0.1f, false),
    attack2Anim(texAttack2, { 7, 7 }, 0.1f, false),
    teleAnim(texTele, { 10, 1 }, 0.1f, false),
    deathAnim(texDeath, { 7, 6 }, 0.15f, false)
{
    state = EnemyState::Idle;
    attackType = Boss3AttackType::None;

    sprite.setTexture(texIdle);
    sprite.setTextureRect(idleAnim.GetRect());
    auto rect = idleAnim.GetRect();
    sprite.setPosition(800, 400);
    sprite.setOrigin(rect.width / 2.f, rect.height / 2.f);
    sprite.setScale(0.7f, 0.7f);

    currentAttackBox = sf::FloatRect();
    bodyHitbox = sf::FloatRect();

    maxHealth = 3000.f;   // ví dụ máu boss 2
    health = maxHealth;

    font.loadFromFile("Assets/Font/1.ttf");
    InitHPBar({ 500.f, 70.f }, 600.f, 20.f, "Selvaria: Vesperyn the Bloodveil Empress");
    bossNameText.setPosition(hpBack.getPosition().x + 110.f, hpBack.getPosition().y - 35.f);
}

void Enemy3::HandleInput(float deltaTime, const sf::Vector2f& playerPosition, bool playerIsOnGround)
{
    velocity.x = 0.f;

    // ==== Nếu chuẩn bị Attack2 thì teleport lên đầu player ====
    if (isTeleportForAttack2)
    {
        if (!playerIsOnGround) return; // chỉ teleport khi player dưới đất
            isTeleporting = true;
            teleTarget = attack2Target;
            isTeleportForAttack2 = false; // reset
            teleAnim.Reset();
            teleTimer = 0.f;
    }


    // Nếu đang tấn công hoặc chết thì không di chuyển
    if (state == EnemyState::Attacking || state == EnemyState::Death || isDead)
    {
        if (attackCooldownTimer > 0)
            attackCooldownTimer -= deltaTime;
        return;
    }

    // Nếu đang teleport thì dừng hết
    if (isTeleporting)
        return;

    sf::Vector2f direction = playerPosition - sprite.getPosition();
    float distance = std::hypot(direction.x, direction.y);
    float verticalDistance = std::abs(direction.y);
    const float verticalDetectionRange = 200.f; // boss có thể phát hiện player cao hơn
    const float verticalAttackRange = 100.f;    // attack vẫn chỉ trong khoảng này
    facingRight = direction.x > 0;

    // Kiểm tra cooldown teleport
    float currentCooldown = justFinishedAttack2 ? teleCooldownAfterAttack2 : teleCooldownNormal;
    teleTimer += deltaTime;

    if (teleTimer >= currentCooldown && !isTeleporting && state != EnemyState::Attacking)
    {
        if (!playerIsOnGround) return; // chỉ teleport khi player dưới đất
            teleTimer = 0.f;
            justFinishedAttack2 = false; // reset flag
            isTeleporting = true;

            float offsetX = (rand() % 2 == 0 ? -150.f : 150.f);
            teleTarget = sf::Vector2f(playerPosition.x + offsetX, playerPosition.y);
            teleAnim.Reset();
    }

    // Kiểm tra tấn công
    if (distance <= attackRange && verticalDistance <= 150)
    {
        attackTimer += deltaTime;

        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0.f;
            int r = rand() % 100; // số từ 0 -> 99
            if (r < 40)            // 0-49 -> 50%
            {
                if (!playerIsOnGround) return;
                attackType = Boss3AttackType::Attack2;
                isTeleportForAttack2 = true;
                attack2Target = sf::Vector2f(playerPosition.x, playerPosition.y - 250.f);
            }
            else if (r < 70)       // 50-79 -> 30%
                attackType = Boss3AttackType::Attack1;
            else                   // 80-99 -> 20%
                attackType = Boss3AttackType::Attack;

            ChangeState(EnemyState::Attacking);
            hitboxActive = false;
            attackCooldownTimer = 0.f;
        }
    }
    else if (distance <= detectionRange && verticalDistance <= 150)
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

    sprite.setScale(facingRight ? 0.7f : -0.7f, 0.7f);

    if (attackCooldownTimer > 0)
        attackCooldownTimer -= deltaTime;
}

void Enemy3::Update(float deltaTime)
{
    UpdateHitCooldown(deltaTime);

    // ===== TELEPORT CHO ATTACK2 =====
    if (isTeleportForAttack2)
    {
        teleAnim.Reset();
        isTeleporting = true;      // dùng lại cơ chế teleport animation
        teleTarget = attack2Target;
        isTeleportForAttack2 = false;  // reset flag để không lặp lại
        teleTimer = 0.f;
        return; // ngừng các hành vi khác khi teleport
    }

    if (isDead)
    {
        if (state != EnemyState::Death)
            ChangeState(EnemyState::Death);
    }

    if (isTeleporting)
    {
        teleAnim.Update(deltaTime);
        sprite.setTexture(*teleAnim.GetTexture());
        sprite.setTextureRect(teleAnim.GetRect());
        sprite.setScale(facingRight ? 0.7f : -0.7f, 0.7f);

        if (teleAnim.IsFinished())
        {
            sprite.setPosition(teleTarget);
            isTeleporting = false;

            // Nếu teleport chuẩn bị Attack2
            if (attackType == Boss3AttackType::Attack2)
            {
                ChangeState(EnemyState::Attacking);
                attackTimer = 0.f;
                hitboxActive = false;
            }
            else
            {
                // Thực hiện teleport bình thường (attack ngẫu nhiên)
                attackType = (rand() % 2 == 0) ? Boss3AttackType::Attack : Boss3AttackType::Attack1;
                ChangeState(EnemyState::Attacking);
                attackTimer = 0.f;
                hitboxActive = false;
            }
        }
        return;
    }



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
    sprite.setScale(facingRight ? 0.7f : -0.7f, 0.7f);

    // Hitbox tấn công
    if (state == EnemyState::Attacking && hitboxActive)
    {
        sf::FloatRect attackBox;
        float enemyX = sprite.getPosition().x;
        float enemyY = sprite.getPosition().y;

        switch (attackType)
        {
        case Boss3AttackType::Attack:
        {
            attackBox.width = 220.f;
            attackBox.height = 130.f;
            attackBox.top = enemyY - 65.f;
            if (facingRight)
                attackBox.left = enemyX - 50.f;
            else
                attackBox.left = enemyX - attackBox.width + 30.f;

            break;
        }
        case Boss3AttackType::Attack1:
            attackBox.width = 800.f; attackBox.height = 120.f;
            attackBox.top = enemyY - 50.f;
            attackBox.left = enemyX - attackBox.width / 2.f;
            break;
        case Boss3AttackType::Attack2:
        {
            // Vùng tấn công chính (màu vàng)
            attackBox.width = 900.f;
            attackBox.height = 300.f;
            attackBox.top = enemyY;
            attackBox.left = enemyX - attackBox.width / 2.f;

            // Safe zone ở giữa (player đứng đây sẽ không bị trúng)
            safeZoneLeft.width = 120.f;
            safeZoneLeft.height = 120.f;
            safeZoneLeft.left = enemyX - 250.f;
            safeZoneLeft.top = enemyY + 200.f;

            safeZoneRight.width = 120.f;
            safeZoneRight.height = 120.f;
            safeZoneRight.left = enemyX + 250.f - safeZoneRight.width; // căn đối xứng bên phải
            safeZoneRight.top = enemyY + 200.f;
            break;
        }
        default:
            attackBox = sf::FloatRect();
            break;
        }

        currentAttackBox = attackBox;
    }
    else
        currentAttackBox = sf::FloatRect();

    // Hitbox thân
    bodyHitbox.width = 150.f;
    bodyHitbox.height = 200.f;
    bodyOffset = sf::Vector2f(-bodyHitbox.width / 2.f, -bodyHitbox.height / 2.f);
    sf::Vector2f pos = sprite.getPosition();
    bodyHitbox.left = pos.x + bodyOffset.x;
    bodyHitbox.top = pos.y + (bodyOffset.y - 30.f);
}


void Enemy3::UpdateAttackAnim(float deltaTime, const sf::Texture*& tex, sf::IntRect& rect)
{
    attackTimer += deltaTime;

    switch (attackType)
    {
    case Boss3AttackType::Attack:
        attackAnim.Update(deltaTime);
        tex = attackAnim.GetTexture();
        rect = attackAnim.GetRect();
        break;
    case Boss3AttackType::Attack1:
        attack1Anim.Update(deltaTime);
        tex = attack1Anim.GetTexture();
        rect = attack1Anim.GetRect();
        break;
    case Boss3AttackType::Attack2:
        attack2Anim.Update(deltaTime);
        tex = attack2Anim.GetTexture();
        rect = attack2Anim.GetRect();
        break;
    default:
        tex = idleAnim.GetTexture();
        rect = idleAnim.GetRect();
        break;
    }

    if (attackType == Boss3AttackType::Attack)
    {
        float moveSpeedWhileAttacking = 250.f; // tốc độ di chuyển khi đánh
        sprite.move((facingRight ? 0.7f : -0.7f) * moveSpeedWhileAttacking * deltaTime, 0.f);
    }

    switch (attackType)
    {
    case Boss3AttackType::Attack:
        hitboxActive = (attackTimer >= attackActiveTime_Attack &&
            attackTimer <= attackActiveTime_Attack + attackDuration_Attack);
        break;

    case Boss3AttackType::Attack1:
        hitboxActive = (attackTimer >= attackActiveTime_Attack1 &&
            attackTimer <= attackActiveTime_Attack1 + attackDuration_Attack1);
        break;

    case Boss3AttackType::Attack2:
        hitboxActive = (attackTimer >= attackActiveTime_Attack2 &&
            attackTimer <= attackActiveTime_Attack2 + attackDuration_Attack2);
        break;

    default:
        hitboxActive = false;
        break;
    }

    bool finished = false;
    if (attackType == Boss3AttackType::Attack) finished = attackAnim.IsFinished();
    if (attackType == Boss3AttackType::Attack1) finished = attack1Anim.IsFinished();
    if (attackType == Boss3AttackType::Attack2) finished = attack2Anim.IsFinished();

    if (finished)
    {
        if (attackType == Boss3AttackType::Attack2)
            justFinishedAttack2 = true;

        ChangeState(EnemyState::Idle);
        attackType = Boss3AttackType::None;
        attackTimer = 0.f;
        hitboxActive = false;
    }
}


void Enemy3::Draw(sf::RenderWindow& window)
{
    window.draw(sprite);

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

  /*  if (state == EnemyState::Attacking && hitboxActive)
    {
        sf::RectangleShape atkBoxShape;
        atkBoxShape.setPosition(currentAttackBox.left, currentAttackBox.top);
        atkBoxShape.setSize({ currentAttackBox.width, currentAttackBox.height });
        atkBoxShape.setFillColor(sf::Color(255, 255, 0, 60));
        atkBoxShape.setOutlineColor(sf::Color::Yellow);
        atkBoxShape.setOutlineThickness(1.f);
        window.draw(atkBoxShape);*/

        //if (attackType == Boss3AttackType::Attack2)
        //{
        //    auto drawSafeZone = [&](const sf::FloatRect& zone)
        //        {
        //            sf::RectangleShape shape;
        //            shape.setPosition(zone.left, zone.top);
        //            shape.setSize({ zone.width, zone.height });
        //            shape.setFillColor(sf::Color(0, 255, 0, 60)); // xanh mờ
        //            shape.setOutlineColor(sf::Color::Green);
        //            shape.setOutlineThickness(1.f);
        //            window.draw(shape);
        //        };

 /*           drawSafeZone(safeZoneLeft);
            drawSafeZone(safeZoneRight);
        }
    }*/

    // Hitbox thân
 /*   sf::RectangleShape bodyBox;
    bodyBox.setPosition(bodyHitbox.left, bodyHitbox.top);
    bodyBox.setSize({ bodyHitbox.width, bodyHitbox.height });
    bodyBox.setFillColor(sf::Color(255, 0, 0, 40));
    bodyBox.setOutlineColor(sf::Color::Red);
    bodyBox.setOutlineThickness(1.f);
    window.draw(bodyBox);*/
}

void Enemy3::ChangeState(EnemyState newState)
{
    if (state == newState || state == EnemyState::Death)
        return;

    state = newState;

    switch (state)
    {
    case EnemyState::Idle: idleAnim.Reset(); break;
    case EnemyState::Walking: walkAnim.Reset(); break;
    case EnemyState::Attacking:
        if (attackType == Boss3AttackType::Attack) {
            attackAnim.Reset();
            attackSound.setBuffer(tele_buffer);
            attackSound.setVolume(70.f);
            attackSound.play();
        }
        else if (attackType == Boss3AttackType::Attack1) {
            attack1Anim.Reset();
            attackSound.setBuffer(at_buffer);
            attackSound.setVolume(30.f);
            attackSound.play();
        }
        else if (attackType == Boss3AttackType::Attack2) {
            attack2Anim.Reset();
            attackSound.setBuffer(at_buffer);
            attackSound.setVolume(30.f);
            attackSound.play();
        }
        break;
    case EnemyState::Death: deathAnim.Reset();
        isTeleporting = false;
        deathAnim.Reset();
        break;
    }
}

void Enemy3::TakeDamage(int damage)
{
    BaseEnemy::TakeDamage(damage); // trừ máu

    // Cập nhật thanh đỏ
    float ratio = health / maxHealth;
    hpFront.setSize(sf::Vector2f(600.f * ratio, 20.f));
}