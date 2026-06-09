#include "ScoreManager.h"
#include <fstream>
#include <iostream>
#include <algorithm> // Dùng cho std::swap

ScoreManager::ScoreManager() {
    currentScore = 0;
    fileName = "scores.txt";
    // Khởi tạo
    std::ofstream file(fileName, std::ofstream::trunc);
    file.close();
    highScores.clear();

}

void ScoreManager::AddScore(int points) {
    currentScore += points;
}

int ScoreManager::GetCurrentScore() const {
    return currentScore;
}

void ScoreManager::ResetCurrentScore() {
    currentScore = 0;
}

void ScoreManager::SaveScore() {
    if (currentScore <= 0) return;

    // Kiểm tra trùng lặp
    bool exists = false;
    for (int s : highScores) {
        if (s == currentScore) {
            exists = true;
            break;
        }
    }

    if (!exists) {
        highScores.push_back(currentScore);

        // BUBBLE SORT (Giảm dần)
        size_t n = highScores.size();
        for (size_t i = 0; i < n - 1; i++) {
            for (size_t j = 0; j < n - i - 1; j++) {
                if (highScores[j] < highScores[j + 1]) {
                    std::swap(highScores[j], highScores[j + 1]);
                }
            }
        }

        if (highScores.size() > 10) {
            highScores.resize(10);
        }

        SaveToFile();
    }
}

const std::vector<int>& ScoreManager::GetHighScores() const {
    return highScores;
}

int ScoreManager::GetHighScore() const {
    if (highScores.empty()) return 0;
    return highScores[0];
}

void ScoreManager::SaveToFile() {
    std::ofstream file(fileName);
    if (file.is_open()) {
        for (int score : highScores) {
            file << score << "\n";
        }
        file.close();
    }
}

void ScoreManager::LoadScores() {
    highScores.clear();
    std::ifstream file(fileName);
    if (file.is_open()) {
        int score;
        while (file >> score) {
            highScores.push_back(score);
        }
		// BUBBLE SORT (Giảm dần)
        size_t n = highScores.size();
        for (size_t i = 0; i < n - 1; i++) {
            for (size_t j = 0; j < n - i - 1; j++) {
                if (highScores[j] < highScores[j + 1]) {
                    std::swap(highScores[j], highScores[j + 1]);
                }
            }
        }
        file.close();
    }
}