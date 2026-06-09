#pragma once
#include <vector>
#include <string>

class ScoreManager {
private:
    int currentScore;
    std::vector<int> highScores; // Danh sách điểm cao
    std::string fileName;

    // Hàm nội bộ để lưu/tải file
    void SaveToFile();
    void LoadScores();

public:
    ScoreManager(); // Constructor

    // Cộng điểm
    void AddScore(int points);

    // Lấy điểm hiện tại
    int GetCurrentScore() const;

    // Reset điểm về 0 (khi chơi lại)
    void ResetCurrentScore();

    // Lưu điểm hiện tại vào danh sách High Score
    void SaveScore();

    // Lấy danh sách High Score để hiển thị
    const std::vector<int>& GetHighScores() const;

    // Lấy điểm cao nhất (Top 1)
    int GetHighScore() const;
};