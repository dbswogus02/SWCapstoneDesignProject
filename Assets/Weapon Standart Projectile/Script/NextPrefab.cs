using UnityEngine;
using UnityEngine.UI;

namespace NextPrefab
{
    public class NextPrefab : MonoBehaviour
    {
        #region Variables

        // Elements
        public GameObject[] m_PrefabList;

        // Index of current particle
        int m_CurrentParticleIndex = -1;

        // GameObject of current particle that is showing in the scene
        GameObject m_CurrentParticle = null;

        public Text m_ParticleName;

        #endregion

        #region MonoBehaviour Functions

        void Start()
        {
            // Найти активный префаб на сцене из списка
            FindActiveParticle();

            // Если активного нет, показываем первый префаб
            if (m_CurrentParticle == null && m_PrefabList.Length > 0)
            {
                m_CurrentParticleIndex = 0;
                ShowParticle();
            }
        }

        void Update()
        {
            if (m_PrefabList.Length > 0)
            {
                if (Input.GetKeyUp(KeyCode.Z))
                {
                    m_CurrentParticleIndex--;
                    ShowParticle();
                }
                else if (Input.GetKeyUp(KeyCode.X))
                {
                    m_CurrentParticleIndex++;
                    ShowParticle();
                }
                else if (Input.GetKeyUp(KeyCode.Space))
                {
                    RespawnParticle();
                }
            }
        }

        #endregion

        #region Functions

        void FindActiveParticle()
        {
            // Перебираем все префабы и ищем активный
            for (int i = 0; i < m_PrefabList.Length; i++)
            {
                if (m_PrefabList[i].activeInHierarchy)
                {
                    m_CurrentParticle = m_PrefabList[i];
                    m_CurrentParticleIndex = i;
                    UpdateParticleName();
                    return;
                }
            }
            m_CurrentParticleIndex = -1; // Если активного нет
        }

        void RespawnParticle()
        {
            if (m_CurrentParticle != null)
            {
                m_CurrentParticle.SetActive(false);
            }
            m_CurrentParticle = m_PrefabList[m_CurrentParticleIndex];
            m_CurrentParticle.SetActive(true);
        }

        void ShowParticle()
        {
            // Округляем индекс
            if (m_CurrentParticleIndex >= m_PrefabList.Length)
            {
                m_CurrentParticleIndex = 0;
            }
            else if (m_CurrentParticleIndex < 0)
            {
                m_CurrentParticleIndex = m_PrefabList.Length - 1;
            }

            // Деактивируем старый префаб
            if (m_CurrentParticle != null)
            {
                m_CurrentParticle.SetActive(false);
            }

            // Активируем новый префаб
            m_CurrentParticle = m_PrefabList[m_CurrentParticleIndex];
            m_CurrentParticle.SetActive(true);

            // Обновляем имя
            UpdateParticleName();
        }

        void UpdateParticleName()
        {
            if (m_ParticleName != null && m_CurrentParticle != null)
            {
                m_ParticleName.text = m_CurrentParticle.name;
            }
        }

        public void OnLeftArrowPressed()
        {
            m_CurrentParticleIndex--;
            ShowParticle();
        }

        public void OnRightArrowPressed()
        {
            m_CurrentParticleIndex++;
            ShowParticle();
        }

        #endregion
    }
}
