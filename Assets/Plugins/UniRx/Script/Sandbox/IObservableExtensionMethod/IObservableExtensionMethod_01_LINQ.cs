using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Sandbox.IObservableExtensionMethod
{
    // 参考:
    // https://blog.okazuki.jp/entry/2015/03/23/203825

    public class IObservableExtensionMethod_01_LINQ : MonoBehaviour
    {
        [ContextMenu(nameof(LINQ))]
        void LINQ()
        {
            var subject = new Subject<int>();
            var source = subject.AsObservable();

            var subscription1 = source.Subscribe(
                i => UnityEngine.Debug.Log($"1## OnNext({i})"),
                ex => UnityEngine.Debug.Log($"1## OnException({ex})"),
                () => UnityEngine.Debug.Log($"1## OnCompleted()")
            );

            // こういう感じで､IObservable(購読)は､LINQっぽい形で別のIObservable(購読)に変換することができる
            var subscription2 = source
                .Where(i => (i % 2) == 1)
                .Select(i => $"{i} is odd number.")
                .Subscribe(
                    i => UnityEngine.Debug.Log($"2## OnNext({i})"),
                    ex => UnityEngine.Debug.Log($"2## OnException({ex})"),
                    () => UnityEngine.Debug.Log($"2## OnCompleted()")
                );

            for (int i = 1; i <= 10; i++)
            {
                subject.OnNext(i);
            }

            subject.OnCompleted();

            // 意味は無いが破棄
            subscription1.Dispose();
            subscription2.Dispose();
        }
    }
}
