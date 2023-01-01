using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Sandbox.IObservableExtensionMethod
{
    // 参考:
    // https://blog.okazuki.jp/entry/2015/03/23/203825

    public class IObservableExtensionMethod_08_TimeFilter : MonoBehaviour
    {
        [ContextMenu(nameof(Sample))]
        async UniTask Sample()
        {
            // Sampleは､一定時間ごとに､最後の値をフィルタして取得するメソッド
            var random = new System.Random();
            var subscription = Observable
                .Interval(TimeSpan.FromMilliseconds(100))
                .Select(_ => random.Next(100))
                .Do(i => UnityEngine.Debug.Log($"{DateTime.Now:HH:mm:ss.FFF} value:{i}"))
                .Sample(TimeSpan.FromMilliseconds(1000))
                .Subscribe(
                    i => UnityEngine.Debug.Log($"{DateTime.Now:HH:mm:ss.FFF} OnNext({i})"),
                    ex => UnityEngine.Debug.Log($"OnException({ex})"),
                    () => UnityEngine.Debug.Log($"OnCompleted()")
                );

            await UniTask.Delay(TimeSpan.FromMilliseconds(1500));

            subscription.Dispose();
        }

        [ContextMenu(nameof(Throttle))]
        async UniTask Throttle()
        {
            // Throttleは､一定時間要素が流れてこなかったら､最後に流れてきた値を流すメソッド
            var source = new Subject<int>();
            var subscription = source
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(
                    i => UnityEngine.Debug.Log($"{DateTime.Now:HH:mm:ss.FFF} {i}"));

            for (int i = 1; i <= 10; i++)
            {
                UnityEngine.Debug.Log($"{DateTime.Now:HH:mm:ss.FFF} OnNext({i})");
                source.OnNext(i);
                await UniTask.Delay(TimeSpan.FromMilliseconds(100));
            }

            UnityEngine.Debug.Log($"{DateTime.Now:HH:mm:ss.FFF} Sleep(1000ms)");
            await UniTask.Delay(TimeSpan.FromMilliseconds(1000));

            subscription.Dispose();
        }

        [ContextMenu(nameof(Delay))]
        async UniTask Delay()
        {
            // Delayはそのまま､指定した時間遅延してから値を流すメソッド
            var source = new Subject<int>();
            var subscription = source
                .Delay(TimeSpan.FromMilliseconds(1000))
                .Subscribe(
                    i => UnityEngine.Debug.Log($"{DateTime.Now:HH:mm:ss.FFF} {i}"));

            for (int i = 1; i <= 5; i++)
            {
                UnityEngine.Debug.Log($"{DateTime.Now:HH:mm:ss.FFF} OnNext({i})");
                source.OnNext(i);
                await UniTask.Delay(TimeSpan.FromMilliseconds(100));
            }

            await UniTask.Delay(TimeSpan.FromMilliseconds(2000));

            subscription.Dispose();
        }
        
        [ContextMenu(nameof(Timeout))]
        async UniTask Timeout()
        {
            // Timeoutは、指定した間隔より値の送信がない場合､OnErrorにして終了にするメソッド
            var source = new Subject<int>();
            var subscription = source
                .Timeout(TimeSpan.FromMilliseconds(500))
                .Subscribe(
                    i => UnityEngine.Debug.Log($"{DateTime.Now:HH:mm:ss.FFF} {i}"),
            ex => UnityEngine.Debug.Log($"OnException({ex})"),
            () => UnityEngine.Debug.Log($"OnCompleted()"));

            for (int i = 1; i <= 5; i++)
            {
                UnityEngine.Debug.Log($"{DateTime.Now:HH:mm:ss.FFF} OnNext({i})");
                source.OnNext(i);
                await UniTask.Delay(TimeSpan.FromMilliseconds(100));
            }

            await UniTask.Delay(TimeSpan.FromMilliseconds(1000));

            // OnErrorになった後も値が流れる... そういうもの?
            for (int i = 1; i <= 5; i++)
            {
                UnityEngine.Debug.Log($"{DateTime.Now:HH:mm:ss.FFF} OnNext({i})");
                source.OnNext(i);
                await UniTask.Delay(TimeSpan.FromMilliseconds(100));
            }

            subscription.Dispose();
        }

        [ContextMenu(nameof(Timestamp))]
        async UniTask Timestamp()
        {
            // TimeStampは､値をラップした構造体TimeStamped<T>(TimeStamp付きの値)にして流す
            var subscription = Observable
                .Interval(TimeSpan.FromMilliseconds(100))
                .Timestamp()
                .Subscribe(
                    i => UnityEngine.Debug.Log($"OnNext({i.Value}) timeStamp:{i:HH:mm:ss.FFF}"),
                    ex => UnityEngine.Debug.Log($"OnException({ex})"),
                    () => UnityEngine.Debug.Log($"OnCompleted()")
                );

            await UniTask.Delay(TimeSpan.FromMilliseconds(1000));

            subscription.Dispose();
        }
        
        [ContextMenu(nameof(TimeInterval))]
        async UniTask TimeInterval()
        {
            // TimeStampは､値をラップした構造体TimeInterval<T>(前回からのインターバル付きの値)にして流す
            var subscription = Observable
                .Interval(TimeSpan.FromMilliseconds(100))
                .TimeInterval()
                .Subscribe(
                    i => UnityEngine.Debug.Log($"OnNext({i.Value}) interval:{i.Interval}"),
                    ex => UnityEngine.Debug.Log($"OnException({ex})"),
                    () => UnityEngine.Debug.Log($"OnCompleted()")
                );

            await UniTask.Delay(TimeSpan.FromMilliseconds(1000));

            subscription.Dispose();
        }
    }
}