using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Sandbox.ObservableFactoryMethod
{
    // Observableには､ColdなものとHotなものとがある
    // Cold:
    //   複数回購読をした時に､個々のObserver毎に､独立した(別々の)値を発行する
    // Hot:
    //   複数回購読をした時に､全てのObserverに対し､同じタイミングで同じ値を発行する
    public class ObservableFactoryMethod_03_ColdAndHot : MonoBehaviour
    {
        [ContextMenu(nameof(ColdObservable))]
        async UniTask ColdObservable()
        {
            var source = Observable.Interval(TimeSpan.FromMilliseconds(1000));
            var subscription1 = source.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i}) Subscription(1)"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );

            await UniTask.Delay(TimeSpan.FromMilliseconds(2000));

            var subscription2 = source.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i}) Subscription(2)"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );
            
            await UniTask.Delay(TimeSpan.FromMilliseconds(2000));

            subscription1.Dispose();
            subscription2.Dispose();
        }

        [ContextMenu(nameof(HotObservable))]
        async UniTask HotObservable()
        {
            // FromEventは､あるイベントハンドラを購読可能にする.
            // ここだと､TimerインスタンスのElapsedをObservableに変換している(そのままイベントハンドラに登録で良い気がするが､まぁ練習なので...)
            var timer = new System.Timers.Timer(1000);
            var source = Observable.FromEvent<ElapsedEventHandler, ElapsedEventArgs>(
                handler => (sender, e) => handler(e),
                handler => timer.Elapsed += handler,
                handler => timer.Elapsed -= handler
                );
            timer.Start();

            // FromEventは､HotなObservableの一つ.
            // 2つ購読をすると､どちらも同じ状態を返すことが分かる.
            var subscription1 = source.Subscribe(
                e => UnityEngine.Debug.Log($"OnNext({e.SignalTime:yyyy/MM/dd HH:mm:ss.FFF}) Subscription(1)"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );

            await UniTask.Delay(TimeSpan.FromMilliseconds(2000));

            var subscription2 = source.Subscribe(
                e => UnityEngine.Debug.Log($"OnNext({e.SignalTime:yyyy/MM/dd HH:mm:ss.FFF}) Subscription(2)"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );

            await UniTask.Delay(TimeSpan.FromMilliseconds(2000));

            subscription1.Dispose();
            subscription2.Dispose();
        }
        
        // ==== 以下､HotなObservableの代表例 ====

        [ContextMenu(nameof(FromEvent))]
        void FromEvent()
        {
            var eventSource = new EventSource();
            var source = Observable.FromEvent<EventHandler, EventArgs>(
                h => (sender, eventArgs) => h(eventArgs),
                h =>
                {
                    UnityEngine.Debug.Log("Add handler.");
                    eventSource.OnRaised += h;
                },
                h =>
                {
                    UnityEngine.Debug.Log("Remove handler.");
                    eventSource.OnRaised -= h;
                }
            );
            
            var subscription1 = source.Subscribe(
                eventArgs => UnityEngine.Debug.Log($"OnNext({eventArgs}) Subscription(1)"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );
            var subscription2 = source.Subscribe(
                eventArgs => UnityEngine.Debug.Log($"OnNext({eventArgs}) Subscription(2)"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );

            // イベントを2回呼んで見る
            eventSource.Raise();
            eventSource.Raise();

            // 破棄に応じて､イベントの解除処理も呼ばれることを確認
            subscription1.Dispose();
            subscription2.Dispose();
        }
        class EventSource
        {
            public event EventHandler OnRaised;
            public void Raise()
            {
                if (OnRaised != null)
                {
                    OnRaised.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [ContextMenu("Start")]
        async UniTask Start_Observable()
        {
            // Startは､C#標準のRxでは､実行完了前はHot､実行完了後はColdになるはずだが､UniRxだと常時Coldなような挙動に見える...
            var source = Observable.Start(() =>
            {
                // Startは､別スレッドでタスクが実行される
                UnityEngine.Debug.Log($"Background task start. threadId:{System.Threading.Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(1000);
                UnityEngine.Debug.Log($"Background task end. threadId:{System.Threading.Thread.CurrentThread.ManagedThreadId}");
                return 0; // ここでは適当な値を返す
            });
            
            var subscription1 = source.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i}) Subscription(1)"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );
            
            await UniTask.Delay(TimeSpan.FromMilliseconds(2000));
            
            subscription1.Dispose();

            // 本当はここで､バックグランドタスクは終わっていて､OnNextとOnCompleteだけ発火してほしいが､何も起こらない...
            var subscription2 = source.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i}) Subscription(2)"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );
            subscription2.Dispose();
        }

        [ContextMenu(nameof(ToAsync))]
        async UniTask ToAsync()
        {
            // ToAsyncもStartと似ていて､別スレッドでタスクを実行する.
            // ToAsync自体が返すのは､Func<IObservable>で...
            var func = Observable.ToAsync(() =>
            {
                // Startは､別スレッドでタスクが実行される
                UnityEngine.Debug.Log($"Background task start. threadId:{System.Threading.Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(1000);
                UnityEngine.Debug.Log($"Background task end. threadId:{System.Threading.Thread.CurrentThread.ManagedThreadId}");
                return 0; // ここでは適当な値を返す
            });
            
            // 返り値を実行したタイミングで､はじめてバックグラウンドタスクがはじまる
            var source = func();
            
            var subscription1 = source.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i}) Subscription(1)"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );
            
            await UniTask.Delay(TimeSpan.FromMilliseconds(2000));
            
            subscription1.Dispose();

            // ToAsyncは､Startと違い2個目の購読が実行される.
            // 完了後はOnNext/OnCompletedが呼ばれるだけ. バックグラウンドタスクは既に終わっている.
            var subscription2 = source.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i}) Subscription(2)"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );
            subscription2.Dispose();
        }
        
        
        [ContextMenu(nameof(FromAsyncPattern))]
        async UniTask FromAsyncPattern()
        {
            Func<int, int, int> asyncAdd = (x, y) =>
            {
                UnityEngine.Debug.Log($"Background task start. threadId:{System.Threading.Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(1000);
                UnityEngine.Debug.Log($"Background task end. threadId:{System.Threading.Thread.CurrentThread.ManagedThreadId}");
                return x + y;
            };

            var func = Observable.FromAsyncPattern<int, int, int>(
                asyncAdd.BeginInvoke,
                asyncAdd.EndInvoke);
            
            // FromAsyncも､ToAsyncと同じでFuncを返し､呼び出しタイミングはユーザに委ねられる
            var source = func(10, 5);
            
            var subscription1 = source.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i}) Subscription(1)"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );
            
            await UniTask.Delay(TimeSpan.FromMilliseconds(2000));
            
            subscription1.Dispose();

            // ToAsyncと同じで､この時点の購読では､バックグラウンドタスクは既に終わっている.
            var subscription2 = source.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i}) Subscription(2)"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );
            subscription2.Dispose();
        }
    }
}


