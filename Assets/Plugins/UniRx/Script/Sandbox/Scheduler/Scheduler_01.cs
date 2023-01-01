using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Sandbox.SubjectClass
{
    // 参考:
    // https://qiita.com/toRisouP/items/f3cb8ca6a569f1ec6952
    
    public class Scheduler_01 : MonoBehaviour
    {
        // Schedulerの種類
        //
        // Scheduler.Immediate
        //     現在のスレッドにて直ちに処理を行う.
        // Scheduler.CurrentThread 
        //     現在のスレッドにて処理を行う.
        //     一度キューに積まれてから処理される.
        // Scheduler.ThreadPool
        //     スレッドプール上で処理を行う.
        // Scheduler.MainThread
        //     メインスレッド上にて処理される.
        // Scheduler.MainThreadIgnoreTimeScale
        //     メインスレッド上にて処理される.
        //     こちらは名前の通りTimeScaleの影響を受けない.
        // Scheduler.MainThreadFixedUpdate
        //     メインスレッド上にて処理される.
        //     Time.fixedUpdateを基準にする.
        // Scheduler.MainThreadEndOfFrame
        //     メインスレッド上にて処理される.
        //     EndOfFrameを基準にする.

        [ContextMenu(nameof(CurrentThreadScheduler))]
        void CurrentThreadScheduler()
        {
            new Thread(() =>
            {
                // 注意:
                // 別スレッドで実行した場合でも､Schedulerを指定しなければMainThreadSchedulerで処理されてしまうため､以下のコードはメインスレッドで実行される
                Observable
                    .Timer(TimeSpan.FromMilliseconds(500))
                    .Subscribe(i => UnityEngine.Debug.Log($"1## OnNext({i}) tid:{Thread.CurrentThread.ManagedThreadId}"));
            }).Start();
            
            new Thread(() =>
            {
                // 現在のスレッドで実行したい場合は､CurrentThreadSchedulerを使う
                Observable
                    .Timer(TimeSpan.FromMilliseconds(500), Scheduler.CurrentThread)
                    .Subscribe(i => UnityEngine.Debug.Log($"2## OnNext({i}) tid:{Thread.CurrentThread.ManagedThreadId}"));
            }).Start();
        }
        
        [ContextMenu(nameof(ThreadPoolScheduler))]
        void ThreadPoolScheduler()
        {
            // ThreadPoolSchedulerは､スレッドプールにて処理を実行する.
            // 値の発行順も不定になる点に注意.
            var source = Observable
                .Range(0, 100)
                .ObserveOn(Scheduler.ThreadPool)
                .Subscribe(
                    i => UnityEngine.Debug.Log($"OnNext({i}) tid:{Thread.CurrentThread.ManagedThreadId}"),
                    ex => UnityEngine.Debug.Log($"OnError({ex})"),
                    () => UnityEngine.Debug.Log($"OnCompleted()"));
        }

        [ContextMenu(nameof(MainThreadScheduler))]
        async UniTask MainThreadScheduler()
        {
            // MainThreadSchedulerはMainThreadDispatcher上で実行するコルーチンにより処理時間を計測する.
            // そのため､例えば60FPS環境だと､どんなに小さい値を指定しても､16ms間隔に丸められることに注意. 以下はその例.
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(10))
                .TimeInterval(Scheduler.MainThread)
                .Take(10)
                .Subscribe(i => UnityEngine.Debug.Log($"OnNext({i.Value}) {i.Interval.TotalMilliseconds}"));
        }

        [ContextMenu(nameof(MainThreadIgnoreTimeScaleScheduler))]
        async UniTask MainThreadIgnoreTimeScaleScheduler()
        {
            UnityEngine.Debug.Log($"Start time:{Time.time}");
            Time.timeScale = 2.0f;
            Observable
                .Timer(TimeSpan.FromMilliseconds(1000), Scheduler.MainThread)
                .Subscribe(i =>
                {
                    UnityEngine.Debug.Log($"OnNext({i}) time:{Time.time}");
                    Time.timeScale = 1.0f;
                });

            await UniTask.Delay(TimeSpan.FromMilliseconds(2000));
            UnityEngine.Debug.Log($"====");
            
            // TimeScaleの影響を受けたくない場合は､MainThreadIgnoreTimeScaleSchedulerを使う
            UnityEngine.Debug.Log($"Start time:{Time.time}");
            Time.timeScale = 2.0f;
            Observable
                .Timer(TimeSpan.FromMilliseconds(1000), Scheduler.MainThreadIgnoreTimeScale)
                .Subscribe(i =>
                {
                    UnityEngine.Debug.Log($"OnNext({i}) time:{Time.time}");
                    Time.timeScale = 1.0f;
                });
        }
    }
}