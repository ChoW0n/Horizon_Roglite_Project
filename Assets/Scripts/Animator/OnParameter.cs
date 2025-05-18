using HRP.AnimatorCoder;
using UnityEngine;

namespace HRP.AnimatorCoder
{
    /// <summary>
    /// Animator ���°� ���ŵ� �� ������ �Ķ������ ���� ��ǥ ���� ���� ���,
    /// ���� �ִϸ��̼� �������� ����ϴ� StateMachineBehaviour�Դϴ�.
    /// </summary>
    public class OnParameter : StateMachineBehaviour
    {
        [SerializeField, Tooltip("üũ�� �Ķ����")] // Tooltip ����
        private Parameters parameter;

        [SerializeField, Tooltip("�Ķ���Ͱ� �� ���� ��� ������ �����˴ϴ� (true �Ǵ� false)")] // Tooltip ����
        private bool target;

        [SerializeField, Tooltip("������ �����Ǿ��� �� ����� �ִϸ��̼ǵ��� ���� �������Դϴ�")] // Tooltip ����
        private AnimationData[] nextAnimations;

        private AnimatorCoder animatorBrain;

        /// <summary>
        /// ���¿� ������ �� ȣ��˴ϴ�. AnimatorCoder ���۷����� �����ɴϴ�.
        /// </summary>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animatorBrain = animator.GetComponent<AnimatorCoder>();
        }

        /// <summary>
        /// ���� ���� �ø��� ȣ��˴ϴ�. �Ķ���Ͱ� ���ϴ� ���̸� �ִϸ��̼��� �����մϴ�.
        /// </summary>
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // ���� �Ķ���� ���� ��ǥ ���� �ٸ��� �������� ����
            if (animatorBrain.GetBool(parameter) != target) return;

            // ���� ���̾��� �� ����
            animatorBrain.SetLocked(false, layerIndex);

            // ���� �ִϸ��̼� ü���� ����
            for (int i = 0; i < nextAnimations.Length - 1; ++i)
                nextAnimations[i].nextAnimation = nextAnimations[i + 1];

            // ù ��° �ִϸ��̼� ���
            animatorBrain.Play(nextAnimations[0], layerIndex);
        }
    }
}
