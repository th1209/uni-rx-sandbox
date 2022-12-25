using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UniRx;
using UnityEngine;

namespace Sandbox.ObservableFactoryMethod
{
    public class ObservableFactoryMethod_02_Timer : MonoBehaviour
    {
        [ContextMenu(nameof(Timer))]
        void Timer()
        {
            // Timerは､指定秒数(&指定間隔)で通知を送る
            var source = Observable.Timer(
                TimeSpan.FromMilliseconds(2000), // 2秒後から
                TimeSpan.FromMilliseconds(1000)); // 1秒間隔で
            var subscription = source.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i})"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );
            // 購読を解除した時点で終わり. ここでは4.5秒の時点で打ち切るので､通知されるのは3回.
            Observable.Timer(TimeSpan.FromMilliseconds(4500)).Subscribe(_ =>
            {
                subscription.Dispose();
                UnityEngine.Debug.Log("DisposeSubscription");
            });
        }
        
        [ContextMenu(nameof(Interval))]
        void Interval()
        {
            // Intervalは､指定間隔で通知を送る
            // この例だと､1秒後､2秒後...みたいな感じ
            var source = Observable.Interval(
                TimeSpan.FromMilliseconds(1000)); // 1秒間隔で
            var subscription = source.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i})"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );
            // 購読を解除した時点で終わり. ここでは3.5秒の時点で打ち切るので､通知されるのは3回.
            Observable.Timer(TimeSpan.FromMilliseconds(3500)).Subscribe(_ =>
            {
                subscription.Dispose();
                UnityEngine.Debug.Log("DisposeSubscription");
            });
        }
    }
}


