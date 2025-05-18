using HRP.AnimatorCoder;
using UnityEngine;

namespace HRP.AnimatorCoder
{
    /// <summary>
    /// Animator 상태가 갱신될 때 지정한 파라미터의 값이 목표 값과 같을 경우,
    /// 이후 애니메이션 시퀀스를 재생하는 StateMachineBehaviour입니다.
    /// </summary>
    public class OnParameter : StateMachineBehaviour
    {
        [SerializeField, Tooltip("체크할 파라미터")] // Tooltip 번역
        private Parameters parameter;

        [SerializeField, Tooltip("파라미터가 이 값일 경우 조건이 성립됩니다 (true 또는 false)")] // Tooltip 번역
        private bool target;

        [SerializeField, Tooltip("조건이 만족되었을 때 재생할 애니메이션들의 연쇄 시퀀스입니다")] // Tooltip 번역
        private AnimationData[] nextAnimations;

        private AnimatorCoder animatorBrain;

        /// <summary>
        /// 상태에 진입할 때 호출됩니다. AnimatorCoder 레퍼런스를 가져옵니다.
        /// </summary>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animatorBrain = animator.GetComponent<AnimatorCoder>();
        }

        /// <summary>
        /// 상태 갱신 시마다 호출됩니다. 파라미터가 원하는 값이면 애니메이션을 실행합니다.
        /// </summary>
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // 현재 파라미터 값이 목표 값과 다르면 실행하지 않음
            if (animatorBrain.GetBool(parameter) != target) return;

            // 현재 레이어의 락 해제
            animatorBrain.SetLocked(false, layerIndex);

            // 다음 애니메이션 체인을 연결
            for (int i = 0; i < nextAnimations.Length - 1; ++i)
                nextAnimations[i].nextAnimation = nextAnimations[i + 1];

            // 첫 번째 애니메이션 재생
            animatorBrain.Play(nextAnimations[0], layerIndex);
        }
    }
}
