#pragma once
#include <SFML/Graphics.hpp>
#include <SFML/Audio.hpp>
#include <iostream>

// --- Trạng thái Enemy ---
enum class EnemyState {
    Idle,
    Walking,
    Attacking,
    Death
};

// --- Lớp cơ sở Enemy ---
class BaseEnemy {
protected:
    sf::Sprite sprite;
    EnemyState state = EnemyState::Idle;
    bool facingRight = true;
    bool isDead = false;

    // --- Máu ---
    float health=100.f;
    float maxHealth=100.f;

    // --- Vật lý ---
    sf::Vector2f velocity;
    bool isOnGround = true;
    float gravity = 900.f;
    float groundY = 400.f;

    // --- Hitbox ---
    sf::FloatRect currentAttackBox;
    sf::FloatRect bodyHitbox;
    sf::Vector2f bodyOffset;

    // --- Cooldown bị đánh ---
    bool recentlyHit = false;
    float hitCooldown = 0.4f;
    float hitTimer = 0.f;

    bool deathAnimFinished = false;

    sf::RectangleShape hpBack;  // nền xám
    sf::RectangleShape hpFront; // thanh đỏ
    sf::RectangleShape hpBorder;

    sf::Text hpText, bossNameText;
    sf::Font font;

public:
    virtual ~BaseEnemy() = default;

    virtual void Update(float deltaTime) = 0;
    virtual void Draw(sf::RenderWindow& window) = 0;

    // === Lấy máu ===
    float GetHealth() const { return health; }
    float GetMaxHealth() const { return maxHealth; }

    // === Enemy là Boss? ===
    virtual bool IsBoss() const { return false; }

    // === Nhận sát thương ===
    virtual void TakeDamage(int damage) {
        if (isDead || recentlyHit) return;

        health -= damage;
        if (health < 0) health = 0;

        std::cout << "Boss health: " << health << "\n";

        recentlyHit = true;

        if (health <= 0) {
            isDead = true;
            state = EnemyState::Death;
        }
    }

    // === Cooldown bị đánh ===
    void UpdateHitCooldown(float dt) {
        if (!recentlyHit) return;
        hitTimer += dt;
        if (hitTimer >= hitCooldown) {
            recentlyHit = false;
            hitTimer = 0.f;
        }
    }

    // === Getter cơ bản ===
    bool IsDead() const { return isDead; }
    EnemyState GetState() const { return state; }

    sf::Vector2f GetPosition() const { return sprite.getPosition(); }
    void SetPosition(const sf::Vector2f& pos) { sprite.setPosition(pos); }
    sf::Sprite& GetSprite() { return sprite; }

    bool IsAttacking() const { return state == EnemyState::Attacking; }

    sf::FloatRect GetAttackBox() const { return currentAttackBox; }
    sf::FloatRect GetBodyHitbox() const { return bodyHitbox; }

    // Safe zone (Boss 3 cần)
    virtual std::vector<sf::FloatRect> GetSafeZones() const { return {}; }

    bool IsDeathAnimFinished() const { return deathAnimFinished; }
    virtual void DrawHP(sf::RenderWindow& window) = 0; // hàm ảo thuần

    void InitHPBar(const sf::Vector2f& position, float width, float height, const std::string& bossName)
    {
        hpBack.setSize({ width, height });
        hpBack.setFillColor(sf::Color(50, 50, 50));
        hpBack.setPosition(position);

        hpFront.setSize({ width, height });
        hpFront.setFillColor(sf::Color::Red);
        hpFront.setPosition(position);

        hpBorder.setSize(hpBack.getSize());
        hpBorder.setFillColor(sf::Color::Transparent);
        hpBorder.setOutlineColor(sf::Color::White);
        hpBorder.setOutlineThickness(3.f);
        hpBorder.setPosition(position);

        // đảm bảo font đã load
        hpText.setFont(font);
        hpText.setCharacterSize(20);
        hpText.setFillColor(sf::Color::White);

        sf::FloatRect textBounds = bossNameText.getLocalBounds();

        bossNameText.setFont(font);
        bossNameText.setString(bossName);
        bossNameText.setCharacterSize(24);
        bossNameText.setFillColor(sf::Color::White);

        UpdateHPBarText();
    }
    void UpdateHPBarText()
    {
        float ratio = health / maxHealth;
        hpFront.setSize(sf::Vector2f(hpBack.getSize().x * ratio, hpBack.getSize().y));

        hpText.setString(std::to_string(static_cast<int>(health)) + "/" + std::to_string(static_cast<int>(maxHealth)));

        // Canh giữa thanh
        sf::FloatRect textRect = hpText.getLocalBounds();
        hpText.setOrigin(textRect.left + textRect.width / 2.f, textRect.top + textRect.height / 2.f);

        hpText.setPosition(
            hpBack.getPosition().x + hpBack.getSize().x,
            hpBack.getPosition().y + hpBack.getSize().y / 2.f - 1.f
        );
    }
};