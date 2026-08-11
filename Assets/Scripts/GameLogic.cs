using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TapTap
{
    [DefaultExecutionOrder(ExecutionOrder.GameLogic)]
    public class GameLogic : Entity, IInstance, ISceneResetEvent
    {
        private bool m_IsGameStarted = false;

        private bool m_IsGameOver = false;

        private int m_Score;

        private bool m_IsNewBestScore;

        private bool m_IsPaused;

        // Events.

        public UnityAction m_OnMouseClick;

        public UnityAction m_OnGameStarted;

        public UnityAction m_OnGameOver;

        public UnityAction m_OnScoreChanged;

        public UnityAction m_OnPauseChanged;

        // Properties.

        public bool IsGameStarted { get { return m_IsGameStarted; } }

        public bool IsGameOver { get { return m_IsGameOver; } }

        public int Score { get { return m_Score; } }

        /// <summary>Ván vừa rồi có phá kỷ lục không (chỉ có nghĩa sau khi game over).</summary>
        public bool IsNewBestScore { get { return m_IsNewBestScore; } }

        public bool IsPaused { get { return m_IsPaused; } }

        public void SetGameOver()
        {
            if (m_IsGameOver)
            {
                return;
            }

            m_IsGameOver = true;

            m_IsNewBestScore = BestScore.TrySet(m_Score);

            SoundManager.ResetCoinStreak();

            if (m_IsNewBestScore)
            {
                SoundManager.PlayBest();
            }
            else
            {
                SoundManager.PlayGameOver();
            }

            if (m_OnGameOver != null)
            {
                m_OnGameOver.Invoke();
            }
        }

        public void DoMouseClick()
        {
            if (!m_IsGameStarted)
            {
                m_IsGameStarted = true;

                if (m_OnGameStarted != null)
                {
                    m_OnGameStarted.Invoke();
                }

                return;
            }

            if (m_IsGameOver)
            {
                return;
            }

            SoundManager.PlayTap();

            if (m_OnMouseClick != null)
            {
                m_OnMouseClick.Invoke();
            }
        }

        public void DoReset()
        {
            SetPaused(false);

            Main.Get<UpdateSystem>().ResetScene();
        }

        /// <summary>
        /// Tạm dừng bằng Time.timeScale. Gameplay chạy theo Time.deltaTime nên đứng hẳn,
        /// còn UI (GraphicBlinker dùng unscaledTime) và nút bấm vẫn hoạt động.
        /// </summary>
        public void SetPaused(bool paused)
        {
            if (m_IsPaused == paused)
            {
                return;
            }

            m_IsPaused = paused;

            Time.timeScale = paused ? 0.0f : 1.0f;

            if (m_OnPauseChanged != null)
            {
                m_OnPauseChanged.Invoke();
            }
        }

        public void OnCollectCoin()
        {
            m_Score++;

            CoinWallet.Add(1); // Score reset mỗi ván, còn ví coin thì bền vững.

            SoundManager.PlayCoin();

            if (m_OnScoreChanged != null)
            {
                m_OnScoreChanged.Invoke();
            }
        }

        public void OnSceneResetEvent()
        {
            m_IsGameStarted = false;

            m_IsGameOver = false;

            m_IsNewBestScore = false;

            m_Score = 0;

            SoundManager.ResetCoinStreak();

            if (m_OnScoreChanged != null)
            {
                m_OnScoreChanged.Invoke();
            }
        }
    }
}
