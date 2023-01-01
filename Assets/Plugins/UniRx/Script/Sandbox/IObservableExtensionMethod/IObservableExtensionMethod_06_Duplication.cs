using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Sandbox.IObservableExtensionMethod
{
    public class IObservableExtensionMethod_06_Duplication : MonoBehaviour
    {
        [ContextMenu(nameof(Distinct))]
        void Distinct()
        {
            {
                // Distinctは､重複した値をフィルタで除いて返さない
                var subject = new Subject<int>();
                subject
                    .Distinct()
                    .Subscribe(
                        i => UnityEngine.Debug.Log($"OnNext({i})"),
                        ex => UnityEngine.Debug.Log($"OnException({ex})"),
                        () => UnityEngine.Debug.Log($"OnCompleted()")
                    );

                for (int i = 0; i <= 3 ; i++)
                {
                    subject.OnNext(i);
                }
                for (int i = 2; i <= 5 ; i++)
                {
                    subject.OnNext(i);
                }
            }

            UnityEngine.Debug.Log("====");

            {
                var subject = new Subject<int>();
                subject
                    .Distinct(new GenerationEqualityComparer())
                    .Subscribe(
                        i => UnityEngine.Debug.Log($"OnNext({i})"),
                        ex => UnityEngine.Debug.Log($"OnException({ex})"),
                        () => UnityEngine.Debug.Log($"OnCompleted()")
                    );

                var ages = new int[] { 9, 26, 23, 32, 19, 3 };
                for (int i = 0; i < ages.Length; i++)
                {
                    subject.OnNext(ages[i]);
                }
            }
        }
        // 同じ年代(10代､20代､30代､､､)で比較するEqualityComparer
        private class GenerationEqualityComparer : IEqualityComparer<int>
        {
            public bool Equals(int x, int y)
            {
                return (x / 10) == (y / 10);
            }

            public int GetHashCode(int obj)
            {
                return (obj / 10).GetHashCode();
            }
        }

        [ContextMenu(nameof(DistinctUntilChanged))]
        void DistinctUntilChanged()
        {
            // DistinctUntilChangedは､異なるグループの値が来るまでは､重複の排除をし続けるメソッド
            var subject = new Subject<int>();
            subject
                .DistinctUntilChanged()
                .Subscribe(
                    i => UnityEngine.Debug.Log($"OnNext({i})"),
                    ex => UnityEngine.Debug.Log($"OnException({ex})"),
                    () => UnityEngine.Debug.Log($"OnCompleted()")
                );

            // ↓の場合は､0~2は1回ずつしか流れないが
            subject.OnNext(0);
            subject.OnNext(0);
            subject.OnNext(1);
            subject.OnNext(1);
            subject.OnNext(2);
            subject.OnNext(2);

            // ↓直前の値と異なるので､それぞれ1回ずつ流れてくれる
            subject.OnNext(0);
            subject.OnNext(1);
            subject.OnNext(2);
        }

    }
}