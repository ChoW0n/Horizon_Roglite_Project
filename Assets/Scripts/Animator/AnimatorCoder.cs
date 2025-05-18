using System.Collections;
using UnityEngine;
using System;

namespace HRP.AnimatorCoder
{
    // Animator 로직의 기반이 되는 추상 클래스
    public abstract class AnimatorCoder : MonoBehaviour
    {
        #region 레퍼런스
        /// <summary> 특정 레이어의 기본 애니메이션 로직 </summary>
        public abstract void DefaultAnimation(int layer); // 상속 클래스에서 기본 애니메이션 정의

        private Animator animator = null; // Animator 레퍼런스
        private Animations[] currentAnimation; // 현재 재생 중인 애니메이션 (레이어별)
        private bool[] layerLocked; // 각 레이어별 애니메이션 잠금 여부
        private ParameterDisplay[] parameters; // 파라미터 값 저장용
        private Coroutine[] currentCoroutine; // 레이어별로 실행 중인 애니메이션 체인 코루틴
        #endregion

        #region 초기화
        /// <summary> AnimatorCoder를 설정하고 초기화함 </summary>
        public void Initialize(Animator animator = null)
        {
            AnimatorValues.Initialize(); // 애니메이션 이름 → 해시값 초기화

            this.animator = animator != null ? animator : GetComponent<Animator>();

            int layerCount = this.animator.layerCount;
            currentCoroutine = new Coroutine[layerCount];
            layerLocked = new bool[layerCount];
            currentAnimation = new Animations[layerCount];

            // 각 레이어별 현재 재생 중인 애니메이션 초기화
            for (int i = 0; i < layerCount; ++i)
            {
                layerLocked[i] = false;

                int hash = this.animator.GetCurrentAnimatorStateInfo(i).shortNameHash;
                for (int k = 0; k < AnimatorValues.Animations.Length; ++k)
                {
                    if (hash == AnimatorValues.Animations[k])
                    {
                        currentAnimation[i] = (Animations)Enum.GetValues(typeof(Animations)).GetValue(k);
                        break;
                    }
                }
            }

            // 파라미터 초기화 (모든 enum 기반 파라미터를 false로 설정)
            string[] names = Enum.GetNames(typeof(Parameters));
            parameters = new ParameterDisplay[names.Length];
            for (int i = 0; i < names.Length; ++i)
            {
                parameters[i].name = names[i];
                parameters[i].value = false;
            }
        }
        #endregion

        #region 애니메이션 처리 메서드
        /// <summary> 현재 재생 중인 애니메이션 반환 </summary>
        public Animations GetCurrentAnimation(int layer)
        {
            try
            {
                return currentAnimation[layer];
            }
            catch
            {
                LogError("Current Animation을 가져올 수 없습니다. 해결 방법: Start()에서 Initialize()를 호출하고 Animator 레이어 수를 초과하지 마세요.");
                return Animations.RESET;
            }
        }

        /// <summary> 특정 레이어를 잠금/잠금 해제 </summary>
        public void SetLocked(bool lockLayer, int layer)
        {
            try
            {
                layerLocked[layer] = lockLayer;
            }
            catch
            {
                LogError("Lock 설정 실패. 해결 방법: Start()에서 Initialize() 호출");
            }
        }

        /// <summary> 해당 레이어가 잠겨있는지 여부 확인 </summary>
        public bool IsLocked(int layer)
        {
            try
            {
                return layerLocked[layer];
            }
            catch
            {
                LogError("Lock 상태 확인 실패. 해결 방법: Start()에서 Initialize() 호출");
                return false;
            }
        }

        /// <summary> 파라미터 값 설정 (Bool 값) </summary>
        public void SetBool(Parameters id, bool value)
        {
            try
            {
                parameters[(int)id].value = value;
            }
            catch
            {
                LogError("SetBool 실패. 해결 방법: Start()에서 Initialize() 호출");
            }
        }

        /// <summary> 파라미터 값 가져오기 (Bool 값) </summary>
        public bool GetBool(Parameters id)
        {
            try
            {
                return parameters[(int)id].value;
            }
            catch
            {
                LogError("GetBool 실패. 해결 방법: Start()에서 Initialize() 호출");
                return false;
            }
        }

        /// <summary>
        /// 애니메이션을 재생합니다. nextAnimation이 있다면 체이닝하여 재생됩니다.
        /// </summary>
        public bool Play(AnimationData data, int layer = 0)
        {
            try
            {
                if (data.animation == Animations.RESET)
                {
                    DefaultAnimation(layer);
                    return false;
                }

                if (layerLocked[layer] || currentAnimation[layer] == data.animation)
                    return false;

                // 기존 코루틴이 있으면 중지
                if (currentCoroutine[layer] != null) StopCoroutine(currentCoroutine[layer]);

                layerLocked[layer] = data.lockLayer;
                currentAnimation[layer] = data.animation;

                // 애니메이션 전환 (CrossFade)
                animator.CrossFade(AnimatorValues.GetHash(currentAnimation[layer]), data.crossfade, layer);

                // 다음 애니메이션이 존재한다면 체이닝 처리
                if (data.nextAnimation != null)
                {
                    currentCoroutine[layer] = StartCoroutine(Wait());

                    IEnumerator Wait()
                    {
                        animator.Update(0); // 업데이트로 길이 정확히 반영
                        float delay = animator.GetNextAnimatorStateInfo(layer).length;

                        if (data.crossfade == 0)
                            delay = animator.GetCurrentAnimatorStateInfo(layer).length;

                        yield return new WaitForSeconds(delay - data.nextAnimation.crossfade);
                        SetLocked(false, layer);
                        Play(data.nextAnimation, layer);
                    }
                }

                return true;
            }
            catch
            {
                LogError("Play 실패. 해결 방법: Start()에서 Initialize() 호출");
                return false;
            }
        }

        private void LogError(string message)
        {
            Debug.LogError("AnimatorCoder 오류: " + message);
        }
        #endregion
    }

    /// <summary>
    /// 개별 애니메이션 실행 데이터를 담는 클래스
    /// </summary>
    [Serializable]
    public class AnimationData
    {
        public Animations animation;             // 실행할 애니메이션
        public bool lockLayer;                   // 레이어 잠금 여부
        public AnimationData nextAnimation;      // 다음 체이닝 애니메이션
        public float crossfade = 0;              // 전환 시간

        public AnimationData(Animations animation = Animations.RESET, bool lockLayer = false, AnimationData nextAnimation = null, float crossfade = 0)
        {
            this.animation = animation;
            this.lockLayer = lockLayer;
            this.nextAnimation = nextAnimation;
            this.crossfade = crossfade;
        }
    }

    /// <summary>
    /// 애니메이션 이름 → 해시값 캐싱 클래스
    /// </summary>
    public class AnimatorValues
    {
        public static int[] Animations { get { return animations; } }

        private static int[] animations;
        private static bool initialized = false;

        /// <summary> Enum 이름을 기반으로 애니메이션 해시값 초기화 </summary>
        public static void Initialize()
        {
            if (initialized) return;
            initialized = true;

            string[] names = Enum.GetNames(typeof(Animations));
            animations = new int[names.Length];

            for (int i = 0; i < names.Length; i++)
                animations[i] = Animator.StringToHash(names[i]);
        }

        /// <summary> 애니메이션 enum → 해시값 반환 </summary>
        public static int GetHash(Animations animation)
        {
            return animations[(int)animation];
        }
    }

    /// <summary>
    /// 인스펙터에서 파라미터 디버그용으로 보여주기 위한 구조체
    /// </summary>
    [Serializable]
    public struct ParameterDisplay
    {
        [HideInInspector] public string name;
        public bool value;
    }
}
