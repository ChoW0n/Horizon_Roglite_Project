using System.Collections;
using UnityEngine;
using System;

namespace HRP.AnimatorCoder
{
    // Animator ������ ����� �Ǵ� �߻� Ŭ����
    public abstract class AnimatorCoder : MonoBehaviour
    {
        #region ���۷���
        /// <summary> Ư�� ���̾��� �⺻ �ִϸ��̼� ���� </summary>
        public abstract void DefaultAnimation(int layer); // ��� Ŭ�������� �⺻ �ִϸ��̼� ����

        private Animator animator = null; // Animator ���۷���
        private Animations[] currentAnimation; // ���� ��� ���� �ִϸ��̼� (���̾)
        private bool[] layerLocked; // �� ���̾ �ִϸ��̼� ��� ����
        private ParameterDisplay[] parameters; // �Ķ���� �� �����
        private Coroutine[] currentCoroutine; // ���̾�� ���� ���� �ִϸ��̼� ü�� �ڷ�ƾ
        #endregion

        #region �ʱ�ȭ
        /// <summary> AnimatorCoder�� �����ϰ� �ʱ�ȭ�� </summary>
        public void Initialize(Animator animator = null)
        {
            AnimatorValues.Initialize(); // �ִϸ��̼� �̸� �� �ؽð� �ʱ�ȭ

            this.animator = animator != null ? animator : GetComponent<Animator>();

            int layerCount = this.animator.layerCount;
            currentCoroutine = new Coroutine[layerCount];
            layerLocked = new bool[layerCount];
            currentAnimation = new Animations[layerCount];

            // �� ���̾ ���� ��� ���� �ִϸ��̼� �ʱ�ȭ
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

            // �Ķ���� �ʱ�ȭ (��� enum ��� �Ķ���͸� false�� ����)
            string[] names = Enum.GetNames(typeof(Parameters));
            parameters = new ParameterDisplay[names.Length];
            for (int i = 0; i < names.Length; ++i)
            {
                parameters[i].name = names[i];
                parameters[i].value = false;
            }
        }
        #endregion

        #region �ִϸ��̼� ó�� �޼���
        /// <summary> ���� ��� ���� �ִϸ��̼� ��ȯ </summary>
        public Animations GetCurrentAnimation(int layer)
        {
            try
            {
                return currentAnimation[layer];
            }
            catch
            {
                LogError("Current Animation�� ������ �� �����ϴ�. �ذ� ���: Start()���� Initialize()�� ȣ���ϰ� Animator ���̾� ���� �ʰ����� ������.");
                return Animations.RESET;
            }
        }

        /// <summary> Ư�� ���̾ ���/��� ���� </summary>
        public void SetLocked(bool lockLayer, int layer)
        {
            try
            {
                layerLocked[layer] = lockLayer;
            }
            catch
            {
                LogError("Lock ���� ����. �ذ� ���: Start()���� Initialize() ȣ��");
            }
        }

        /// <summary> �ش� ���̾ ����ִ��� ���� Ȯ�� </summary>
        public bool IsLocked(int layer)
        {
            try
            {
                return layerLocked[layer];
            }
            catch
            {
                LogError("Lock ���� Ȯ�� ����. �ذ� ���: Start()���� Initialize() ȣ��");
                return false;
            }
        }

        /// <summary> �Ķ���� �� ���� (Bool ��) </summary>
        public void SetBool(Parameters id, bool value)
        {
            try
            {
                parameters[(int)id].value = value;
            }
            catch
            {
                LogError("SetBool ����. �ذ� ���: Start()���� Initialize() ȣ��");
            }
        }

        /// <summary> �Ķ���� �� �������� (Bool ��) </summary>
        public bool GetBool(Parameters id)
        {
            try
            {
                return parameters[(int)id].value;
            }
            catch
            {
                LogError("GetBool ����. �ذ� ���: Start()���� Initialize() ȣ��");
                return false;
            }
        }

        /// <summary>
        /// �ִϸ��̼��� ����մϴ�. nextAnimation�� �ִٸ� ü�̴��Ͽ� ����˴ϴ�.
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

                // ���� �ڷ�ƾ�� ������ ����
                if (currentCoroutine[layer] != null) StopCoroutine(currentCoroutine[layer]);

                layerLocked[layer] = data.lockLayer;
                currentAnimation[layer] = data.animation;

                // �ִϸ��̼� ��ȯ (CrossFade)
                animator.CrossFade(AnimatorValues.GetHash(currentAnimation[layer]), data.crossfade, layer);

                // ���� �ִϸ��̼��� �����Ѵٸ� ü�̴� ó��
                if (data.nextAnimation != null)
                {
                    currentCoroutine[layer] = StartCoroutine(Wait());

                    IEnumerator Wait()
                    {
                        animator.Update(0); // ������Ʈ�� ���� ��Ȯ�� �ݿ�
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
                LogError("Play ����. �ذ� ���: Start()���� Initialize() ȣ��");
                return false;
            }
        }

        private void LogError(string message)
        {
            Debug.LogError("AnimatorCoder ����: " + message);
        }
        #endregion
    }

    /// <summary>
    /// ���� �ִϸ��̼� ���� �����͸� ��� Ŭ����
    /// </summary>
    [Serializable]
    public class AnimationData
    {
        public Animations animation;             // ������ �ִϸ��̼�
        public bool lockLayer;                   // ���̾� ��� ����
        public AnimationData nextAnimation;      // ���� ü�̴� �ִϸ��̼�
        public float crossfade = 0;              // ��ȯ �ð�

        public AnimationData(Animations animation = Animations.RESET, bool lockLayer = false, AnimationData nextAnimation = null, float crossfade = 0)
        {
            this.animation = animation;
            this.lockLayer = lockLayer;
            this.nextAnimation = nextAnimation;
            this.crossfade = crossfade;
        }
    }

    /// <summary>
    /// �ִϸ��̼� �̸� �� �ؽð� ĳ�� Ŭ����
    /// </summary>
    public class AnimatorValues
    {
        public static int[] Animations { get { return animations; } }

        private static int[] animations;
        private static bool initialized = false;

        /// <summary> Enum �̸��� ������� �ִϸ��̼� �ؽð� �ʱ�ȭ </summary>
        public static void Initialize()
        {
            if (initialized) return;
            initialized = true;

            string[] names = Enum.GetNames(typeof(Animations));
            animations = new int[names.Length];

            for (int i = 0; i < names.Length; i++)
                animations[i] = Animator.StringToHash(names[i]);
        }

        /// <summary> �ִϸ��̼� enum �� �ؽð� ��ȯ </summary>
        public static int GetHash(Animations animation)
        {
            return animations[(int)animation];
        }
    }

    /// <summary>
    /// �ν����Ϳ��� �Ķ���� ����׿����� �����ֱ� ���� ����ü
    /// </summary>
    [Serializable]
    public struct ParameterDisplay
    {
        [HideInInspector] public string name;
        public bool value;
    }
}
