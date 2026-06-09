#include<iostream>
#include "Player.h"
#include "Animation.h"
#include<SFML/Window/Keyboard.hpp>

std::vector<DashTrail> dashTrails;
float trailSpawnTimer = 0.f;
float trailSpawnInterval = 0.025f;

Player::Player(sf::Texture& texIdle, sf::Texture& texWalk,
    sf::Texture& texAttack1, sf::Texture& texAttack2, sf::Texture& texAttack3,
    sf::Texture& texSkill1, sf::Texture& texDeath)
    : idleAnim(texIdle, { 7, 1 }, 0.2f),
    walkAnim(texWalk, { 7, 1 }, 0.15f),
    attackAnim1(texAttack1, { 4, 1 }, 0.12f),
    attackAnim2(texAttack2, { 4, 1 }, 0.15f),
    attackAnim3(texAttack3, { 4, 1 }, 0.12f),
    skill1Anim(texSkill1, { 8, 1 }, 0.1f),
    deathAnim(texDeath, { 10, 1 }, 0.2f)

{
    // Load âm thanh 
    dashBuffer.loadFromFile("Assets/Sound effect/Player/dash1.ogg");
    dashSound.setBuffer(dashBuffer);
    dashSound.setPitch(1.7f);
    dashSound.setVolume(7.f);

    jumpBuffer.loadFromFile("Assets/Sound effect/Player/jump.ogg");
    jumpSound.setBuffer(jumpBuffer);
    jumpSound.setVolume(60.f);

    at1_buffer.loadFromFile("Assets/Sound effect/Player/attack.ogg");
    at2_buffer.loadFromFile("Assets/Sound effect/Player/attack.ogg");
    at3_buffer.loadFromFile("Assets/Sound effect/Player/attack.ogg");
    skill_buffer.loadFromFile("Assets/Sound effect/Player/Skill.ogg");

    // Bắt đầu với Idle
    state = Idle;

    // Texture ban đầu
    sprite.setTexture(texIdle);
    sprite.setTextureRect(idleAnim.GetRect());

    // Đặt origin theo frame đầu tiên để nhân vật đứng vững
    auto rect = idleAnim.GetRect();
    sprite.setOrigin(384 / 2.f, 192 / 2.f);
    sprite.setScale(1.f, 1.f);

    //======================HP BAR======================
    font.loadFromFile("Assets/Font/fontTitle.TTF");
    avatarTex.loadFromFile("Assets/Images/Player/avatar.png"); // hình đại diện player
    avatarSprite.setTexture(avatarTex);
    avatarSprite.setScale(0.5f, 0.5f); // tuỳ chỉnh size
    avatarSprite.setPosition(1.f, 10.f); // góc trái màn hình

    maxHealth = 300.f;
    health = maxHealth;

    // Nền
    hpBarBack.setSize(sf::Vector2f(200.f, 20.f)); // chiều dài 200, cao 20
    hpBarBack.setFillColor(sf::Color(50, 50, 50, 200)); // xám mờ
    hpBarBack.setPosition(20.f, 20.f); // góc trái trên

    // Thanh HP đỏ
    hpBarFront.setSize(sf::Vector2f(200.f, 20.f));
    hpBarFront.setFillColor(sf::Color::Red);
    hpBarFront.setPosition(hpBarBack.getPosition());
}

void Player::HandleInput(float deltaTime)
{
    if (state == Death) return;

    velocity.x = 0.f;
    bool moving = false;

    float moveSpeed = 250.f;
    bool leftClick = sf::Keyboard::isKeyPressed(sf::Keyboard::J);

    if (dashCooldownTimer > 0)
        dashCooldownTimer -= deltaTime;

    // Nhấn Z để dash (nếu chưa dash và cooldown xong)
    if ((sf::Keyboard::isKeyPressed(sf::Keyboard::L) || (sf::Keyboard::isKeyPressed(sf::Keyboard::LShift)))&& !isDashing && dashCooldownTimer <= 0.f)
    {
        isDashing = true;
        dashTime = dashDuration;
        dashCooldownTimer = dashCooldown;
        dashSound.play();
    }
    if ((sf::Keyboard::isKeyPressed(sf::Keyboard::I) || (sf::Keyboard::isKeyPressed(sf::Keyboard::Q)))
        && skill1CooldownTimer <= 0.f
        && state != Skill1
        && state != Attacking1 && state != Attacking2 && state != Attacking3)
    {
        ChangeState(Skill1);
        skill1CooldownTimer = skill1Cooldown;
    }
    // Chỉ cho di chuyển khi không tấn công
    if (state != Attacking1 && state != Attacking2 && state != Attacking3 && state != Skill1)
    {
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::A)) {
            velocity.x = -moveSpeed;
            facingRight = false;
            moving = true;
        }
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::D)) {
            velocity.x = +moveSpeed;
            facingRight = true;
            moving = true;
        }
        if ((sf::Keyboard::isKeyPressed(sf::Keyboard::K) || (sf::Keyboard::isKeyPressed(sf::Keyboard::Space))) && isOnGround)
        {
            velocity.y = jumpStrength;
            jumpSound.play();
            isOnGround = false;
        }
        // Flip sprite theo hướng
        sprite.setScale(facingRight ? 1.f : -1.f, 1.f);


        if (leftClick && !wasMousePressedLastFrame)
            ChangeState(Attacking1);
        else if (!leftClick)
            ChangeState(moving ? Walking : Idle);
    }
    else {
        // Nếu đang tấn công và người chơi nhấn click nữa → gắn combo
        if (leftClick && !wasMousePressedLastFrame) {
            comboQueued = true;
        }
    }

    wasMousePressedLastFrame = leftClick;
}

void Player::Update(float deltaTime)
{
    if (isDashing)
    {
        dashTime -= deltaTime;
        velocity.x = (facingRight ? dashSpeed : -dashSpeed);
        velocity.y = 0;
        sprite.move(velocity * deltaTime);

        trailSpawnTimer -= deltaTime;
        if (trailSpawnTimer <= 0.f)
        {
            DashTrail trail;
            trail.sprite = sprite; // copy sprite hiện tại
            trail.alpha = 200.f;   // độ trong suốt ban đầu (0–255)
            dashTrails.push_back(trail);
            trailSpawnTimer = trailSpawnInterval;
        }

        if (dashTime <= 0)
        {
            isDashing = false;
            velocity.x = 0;
            ChangeState(Idle);
        }
    }

    if (skill1CooldownTimer > 0.f)
        skill1CooldownTimer -= deltaTime;

    for (auto& t : dashTrails)
    {
        t.alpha -= 1000.f * deltaTime;
        if (t.alpha < 0) t.alpha = 0;
        t.sprite.setColor(sf::Color(255, 255, 255, static_cast<sf::Uint8>(t.alpha)));
    }

    dashTrails.erase(std::remove_if(dashTrails.begin(), dashTrails.end(),
        [](const DashTrail& t) { return t.alpha <= 0; }), dashTrails.end());

    if (!isOnGround) {
        velocity.y += gravity * deltaTime;
    }
    if (state != Skill1) {
        sprite.move(velocity * deltaTime);
    }

    // Kiểm tra chạm đất
    if (sprite.getPosition().y >= groundY) {
        sprite.setPosition(sprite.getPosition().x, groundY);
        velocity.y = 0;
        isOnGround = true;
    }

    const sf::Texture* currentTexture = nullptr;
    sf::IntRect currentRect;

    switch (state)
    {
    case Idle:
        idleAnim.Update(deltaTime, true);
        currentTexture = idleAnim.getTexture();
        currentRect = idleAnim.GetRect();
        break;

    case Walking:
        walkAnim.Update(deltaTime, true);
        currentTexture = walkAnim.getTexture();
        currentRect = walkAnim.GetRect();
        break;
    case Attacking1:
        attackAnim1.Update(deltaTime, false);
        currentTexture = attackAnim1.getTexture();
        currentRect = attackAnim1.GetRect();
        if (attackAnim1.IsFinished()) {
            if (comboQueued) {
                comboQueued = false;
                ChangeState(Attacking2);
            }
            else {
                ChangeState(Idle);
            }
        }
        break;

    case Attacking2:
        attackAnim2.Update(deltaTime, false);
        currentTexture = attackAnim2.getTexture();
        currentRect = attackAnim2.GetRect();
        if (attackAnim2.IsFinished()) {
            if (comboQueued) {
                comboQueued = false;
                ChangeState(Attacking3);
            }
            else {
                ChangeState(Idle);
            }
        }
        break;

    case Attacking3:
        attackAnim3.Update(deltaTime, false);
        currentTexture = attackAnim3.getTexture();
        currentRect = attackAnim3.GetRect();
        if (attackAnim3.IsFinished()) {
            comboQueued = false;
            ChangeState(Idle);
        }
        break;

    case Skill1:
    {
        skill1Anim.Update(deltaTime, false);
        currentTexture = skill1Anim.getTexture();
        currentRect = skill1Anim.GetRect();

        // Lấy frame hiện tại
        int currentFrame = skill1Anim.GetCurrentFrame();

        // Khi đến frame 2 -> bắt đầu dash (1 lần)
        if (currentFrame == 7 && !hasDashedInSkill1)
        {
            float dashDistance = 120.f; // tùy chỉnh độ xa
            sprite.move((facingRight ? 1.f : -1.f) * dashDistance, 0.f);
            hasDashedInSkill1 = true;
        }

        // Khi animation kết thúc -> trở về Idle
        if (skill1Anim.IsFinished())
        {
            ChangeState(Idle);
            hasDashedInSkill1 = false;
        }

        break;
    }
    case Death:
        deathAnim.Update(deltaTime, false);
        sprite.setTexture(*deathAnim.getTexture());
        sprite.setTextureRect(deathAnim.GetRect());
        return;

    }
    if (currentTexture && sprite.getTexture() != currentTexture)
        sprite.setTexture(*currentTexture);

    sprite.setTextureRect(currentRect);

    if (state == Attacking1 || state == Attacking2 || state == Attacking3 || state == Skill1)
    {
        sf::FloatRect attackBox;

        // Kích thước vùng đánh (tùy bạn chỉnh)
        attackBox.width = 90.f;   // độ rộng của vùng tấn công
        attackBox.height = 80.f;  // độ cao (thường bằng nửa chiều cao nhân vật)

        // Căn theo vị trí nhân vật
        float playerX = sprite.getPosition().x;
        float playerY = sprite.getPosition().y;

        attackBox.top = playerY - attackBox.height / 2.f;

        // Dời ra trước mặt tùy hướng
        float offsetX = 0.f; // khoảng cách từ giữa người ra phía trước (bạn có thể thử 30–50)
        if (facingRight)
            attackBox.left = playerX + offsetX;
        else
            attackBox.left = playerX - (attackBox.width + offsetX);

        currentAttackBox = attackBox;
    }
    else
    {
        currentAttackBox = sf::FloatRect();
    }
    //==================SKILL 1====================
    if (state == Skill1)
    {
        // Cập nhật hitbox riêng của Skill1
        sf::FloatRect sBox;
        sBox.width = 200.f;   // tầm đánh Skill1
        sBox.height = 60.f;

        float playerX = sprite.getPosition().x;
        float playerY = sprite.getPosition().y;
        sBox.top = playerY - sBox.height / 2.f;

        float offsetX = 0.f; // đánh xa hơn thường
        if (facingRight)
            sBox.left = playerX + offsetX;
        else
            sBox.left = playerX - (sBox.width + offsetX);

        skill1Hitbox = sBox;
    }
    else
    {
        skill1Hitbox = sf::FloatRect();
    }

    bodyHitbox.width = 40.f;   // chiều ngang thân
    bodyHitbox.height = 70.f; // chiều cao thân
    bodyOffset = sf::Vector2f(-bodyHitbox.width / 2.f, -bodyHitbox.height + 50.f);
    sf::Vector2f pos = sprite.getPosition();
    bodyHitbox.left = pos.x + bodyOffset.x;
    bodyHitbox.top = pos.y + bodyOffset.y;

    UpdateHitCooldown(deltaTime);
}

void Player::UpdateHPBar() {
    float hpPercent = health / maxHealth; // tỉ lệ còn lại
    hpBarFront.setSize(sf::Vector2f(hpBarBack.getSize().x * hpPercent, hpBarFront.getSize().y));
}

void Player::Draw(sf::RenderWindow& window)
{
    for (const auto& t : dashTrails)
        window.draw(t.sprite);
    window.draw(sprite);

    // Vẽ avatar
    window.draw(avatarSprite);

    // Vẽ thanh máu
    sf::RectangleShape backBar;
    backBar.setSize({ 250.f, 20.f }); // size thanh máu
    backBar.setFillColor(sf::Color(100, 100, 100)); // nền xám
    backBar.setOutlineColor(sf::Color::Black);       // viền trắng
    backBar.setOutlineThickness(3.f);
    backBar.setPosition(avatarSprite.getPosition().x + avatarSprite.getGlobalBounds().width - 7.f,
        avatarSprite.getPosition().y + 28.f);
    window.draw(backBar);

    sf::RectangleShape frontBar;
    float hpRatio = health / maxHealth;
    frontBar.setSize({ 250.f * hpRatio, 20.f }); // scale theo HP
    frontBar.setFillColor(sf::Color::Red);
    frontBar.setPosition(backBar.getPosition());
    window.draw(frontBar);

    // 3. Vẽ số máu lên giữa thanh
    sf::Text hpText;
    hpText.setFont(font);
    hpText.setCharacterSize(20); // size chữ
    hpText.setFillColor(sf::Color::White);
    hpText.setString(std::to_string(static_cast<int>(health)) + "/" + std::to_string(static_cast<int>(maxHealth)));

    // Canh giữa thanh HP
    sf::FloatRect textRect = hpText.getLocalBounds();
    hpText.setOrigin(textRect.left + textRect.width / 2.f, textRect.top + textRect.height / 2.f);
    hpText.setPosition(backBar.getPosition().x + backBar.getSize().x / 2.f,
        backBar.getPosition().y + backBar.getSize().y / 2.f - 1.f); // -1 để hơi chính giữa

    window.draw(hpText);

    //if (state == Attacking1 || state == Attacking2 || state == Attacking3)
    //{
    //    sf::RectangleShape atkBoxShape;
    //    atkBoxShape.setPosition(currentAttackBox.left, currentAttackBox.top);
    //    atkBoxShape.setSize({ currentAttackBox.width, currentAttackBox.height });
    //    atkBoxShape.setFillColor(sf::Color(255, 255, 0, 60)); // vàng trong suốt
    //    atkBoxShape.setOutlineColor(sf::Color::Yellow);
    //    atkBoxShape.setOutlineThickness(1.f);
    //    window.draw(atkBoxShape);
    //}

    //if (state == Skill1)
    //{
    //    sf::RectangleShape skillBoxShape;
    //    skillBoxShape.setPosition(skill1Hitbox.left, skill1Hitbox.top);
    //    skillBoxShape.setSize({ skill1Hitbox.width, skill1Hitbox.height });
    //    skillBoxShape.setFillColor(sf::Color(0, 255, 255, 60)); // xanh cyan
    //    skillBoxShape.setOutlineColor(sf::Color::Cyan);
    //    skillBoxShape.setOutlineThickness(1.f);
    //    window.draw(skillBoxShape);
    //}

    //// Vẽ vùng thân người (để so sánh)
    //sf::RectangleShape bodyBox;
    //bodyBox.setPosition(bodyHitbox.left, bodyHitbox.top);
    //bodyBox.setSize({ bodyHitbox.width, bodyHitbox.height });
    //bodyBox.setFillColor(sf::Color(255, 0, 0, 40)); // đỏ nhạt trong suốt
    //bodyBox.setOutlineColor(sf::Color::Red);
    //bodyBox.setOutlineThickness(1.f);
    //window.draw(bodyBox);
}

void Player::ChangeState(PlayerState newState)
{
    if (state == newState) return;

    state = newState;

    switch (state)
    {
    case Idle:       idleAnim.Reset();      break;
    case Walking:    walkAnim.Reset();      break;
    case Attacking1:
        attackAnim1.Reset();
        attackSound.setBuffer(at1_buffer);
        attackSound.setPitch(0.95f + (rand() % 5 - 2) / 100.f);
        attackSound.setVolume(25.f);
        attackSound.play();
        {
            sf::Sound temp = attackSound;
            sf::sleep(sf::milliseconds(15));
            temp.play();
        }
        break;
    case Attacking2:
        attackAnim2.Reset();
        attackSound.setBuffer(at2_buffer);
        attackSound.setPitch(1.00f + (rand() % 5 - 2) / 100.f);
        attackSound.setVolume(15.f);
        attackSound.play();
        {
            sf::Sound temp = attackSound;
            sf::sleep(sf::milliseconds(10)); // lưu ý: trong game thực tế nên dùng timer
            temp.play();
        }
        break;
    case Attacking3:
        attackAnim3.Reset();
        attackSound.setBuffer(at3_buffer);
        attackSound.setPitch(1.05f + (rand() % 5 - 2) / 100.f); // ±2%
        attackSound.setVolume(10.f);
        attackSound.play();
        break;
    case Skill1:
        skill1Anim.Reset();
        hasDashedInSkill1 = false;

        if (skill1Anim.getTexture())
            sprite.setTexture(*skill1Anim.getTexture());
        sprite.setTextureRect(skill1Anim.GetRect());
        skillSound.setBuffer(skill_buffer);
        skillSound.setVolume(10.f);
        skillSound.play();
        break;
    case Death:
        deathAnim.Reset();
        velocity = { 0.f, 0.f }; // nhân vật đứng im
        break;
    }
}

// ----------------- Getter / Setter -----------------
sf::Sprite& Player::GetSprite() { return sprite; }
sf::Vector2f Player::GetPosition() const { return sprite.getPosition(); }
void Player::SetPosition(const sf::Vector2f& pos) { sprite.setPosition(pos); }

// === CÀI ĐẶT HÀM SETHP TẠI ĐÂY ===
void Player::SetHP(float value) {
    health = value;
    // Giới hạn máu không quá Max
    if (health > maxHealth) health = maxHealth;

    // Reset các cờ chết và bị đánh
    isDead = false;
    recentlyHit = false;

    // Đưa về trạng thái Idle để thoát khỏi animation chết
    ChangeState(Idle);

    // Cập nhật lại thanh máu
    UpdateHPBar();
}