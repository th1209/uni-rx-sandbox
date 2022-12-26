using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Sandbox.IObservableExtensionMethod
{
    public class IObservableExtensionMethod_03_Do : MonoBehaviour
    {
        [ContextMenu(nameof(Do))]
        void Do()
        {
            // Doメソッドはこういう感じで､IObservableのシーケンスの間に､任意の処理を挟める拡張メソッド.
            var subject = new Subject<int>();
            var subscription = subject
                .Do(
                    i => UnityEngine.Debug.Log($"Do## OnNext({i})"),
                    ex => UnityEngine.Debug.Log($"Do## OnException({ex})"),
                    () => UnityEngine.Debug.Log($"Do## OnCompleted()"))
                .Subscribe(
                    i => UnityEngine.Debug.Log($"OnNext({i})"),
                    ex => UnityEngine.Debug.Log($"OnException({ex})"),
                    () => UnityEngine.Debug.Log($"OnCompleted()"));

            subject.OnNext(0);
            subject.OnNext(1);
            subject.OnCompleted();
        }
    }
}
