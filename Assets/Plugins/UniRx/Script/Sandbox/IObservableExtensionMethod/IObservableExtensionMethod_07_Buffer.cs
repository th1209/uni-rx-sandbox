using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Sandbox.IObservableExtensionMethod
{
    // 参考:
    // https://blog.okazuki.jp/entry/2015/03/23/203825

    // Bufferメソッドは､元になるIObservable<T>から｢要素数｣｢時間｣の観点で値をまとめ､Observable<List<T>>に変換して返す.
    // ｢タイミング｣指定のパターンは､UniRxには存在しないようだ.
    // 
    // また､BufferはIList<T>にして後続のストリームに値を流すのだが､IObservable<T>で後続のストリームに流すWindowというメソッドも存在する.
    // Windowメソッドも､UniRxには存在しない模様.
    public class IObservableExtensionMethod_07_Buffer : MonoBehaviour
    {
        [ContextMenu(nameof(BufferByElementNum))]
        void BufferByElementNum()
        {
            // まずは一番簡単な要素数の例から
            Observable.Range(1, 10)
                .Buffer(3)
                .Subscribe(
                    list => UnityEngine.Debug.Log($"OnNext({string.Join(',', list)})"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                    () => UnityEngine.Debug.Log($"OnCompleted()")
                );
        }
        
        [ContextMenu(nameof(BufferByTime))]
        async UniTask BufferByTime()
        {
            {
                // 一定時間値を貯めるようなBufferの使い方
                var subscription = Observable
                    .Interval(TimeSpan.FromMilliseconds(100))
                    .Buffer(TimeSpan.FromMilliseconds(500))
                    .Subscribe(
                        list => UnityEngine.Debug.Log($"OnNext({string.Join(',', list)})"),
                        ex => UnityEngine.Debug.Log($"OnException({ex})"),
                        () => UnityEngine.Debug.Log($"OnCompleted()")
                    );

                await UniTask.Delay(TimeSpan.FromMilliseconds(2000));

                subscription.Dispose();
            }

            UnityEngine.Debug.Log("====");

            {
                // 第2引数にtimeShiftを取ることができる(次の値を貯め始めるまでの間隔)
                var subscription = Observable
                    .Interval(TimeSpan.FromMilliseconds(100))
                    .Buffer(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(200))
                    .Subscribe(
                        list => UnityEngine.Debug.Log($"OnNext({string.Join(',', list)})"),
                        ex => UnityEngine.Debug.Log($"OnException({ex})"),
                        () => UnityEngine.Debug.Log($"OnCompleted()")
                    );

                await UniTask.Delay(TimeSpan.FromMilliseconds(2000));

                subscription.Dispose();
            }
        }
    }
}