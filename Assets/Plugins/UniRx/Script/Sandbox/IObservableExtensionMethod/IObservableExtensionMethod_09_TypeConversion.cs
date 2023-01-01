using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Sandbox.IObservableExtensionMethod
{
    // 参考:
    // https://blog.okazuki.jp/entry/2015/03/23/203825

    public class IObservableExtensionMethod_09_TypeConversion : MonoBehaviour
    {
        [ContextMenu(nameof(Cast))]
        void Cast()
        {
            // Castは名前の通りで､指定された型にキャスト. 失敗した場合はOnErrorを流す.
            var source = new Subject<object>();
            var subscription = source
                .Cast<object, int>()
                .Subscribe(
                    i => UnityEngine.Debug.Log($"OnNext({i})"),
                    ex => UnityEngine.Debug.Log($"OnException({ex})"),
                    () => UnityEngine.Debug.Log($"OnCompleted()")
                );

            source.OnNext(0);
            source.OnNext(1);
            source.OnNext(2);
            source.OnNext("string");
            source.OnNext(3);

            subscription.Dispose();
        }

        [ContextMenu(nameof(OfType))]
        void OfType()
        {
            // OfTypeはCastと同じく型変換を行うが､型変換に失敗した場合はなにもしないのが違い
            var source = new Subject<object>();
            var subscription = source
                .OfType<object, int>()
                .Subscribe(
                    i => UnityEngine.Debug.Log($"OnNext({i})"),
                    ex => UnityEngine.Debug.Log($"OnException({ex})"),
                    () => UnityEngine.Debug.Log($"OnCompleted()")
                );

            source.OnNext(0);
            source.OnNext(1);
            source.OnNext(2);
            source.OnNext("string");
            source.OnNext(3);

            subscription.Dispose();
        }
    }
}