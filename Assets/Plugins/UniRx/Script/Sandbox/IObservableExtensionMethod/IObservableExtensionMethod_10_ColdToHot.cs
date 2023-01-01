using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Sandbox.IObservableExtensionMethod
{
    // 参考:
    // https://blog.okazuki.jp/entry/2015/03/23/203825

    public class IObservableExtensionMethod_10_ColdToHot : MonoBehaviour
    {
        [ContextMenu(nameof(Publish))]
        void Publish()
        {
            var source = Observable.Range(0, 2);

            {
                // ここまでは､通常のColdなObservableの例
                UnityEngine.Debug.Log($"Subscribe1");
                var subscription1 = source
                    .Subscribe(
                        i => UnityEngine.Debug.Log($"1## OnNext({i})"),
                        ex => UnityEngine.Debug.Log($"1## OnException({ex})"),
                        () => UnityEngine.Debug.Log($"1## OnCompleted()")
                    );
                UnityEngine.Debug.Log($"Subscribe2");
                var subscription2 = source
                    .Subscribe(
                        i => UnityEngine.Debug.Log($"2## OnNext({i})"),
                        ex => UnityEngine.Debug.Log($"2## OnException({ex})"),
                        () => UnityEngine.Debug.Log($"2## OnCompleted()")
                    );
            }

            // Publishメソッドで､Cold -> Hotに変換
            var connectableSource = source.Publish();

            {
                // 2回購読. 通常とのRangeメソッドと異なり､値はまだ流れない
                UnityEngine.Debug.Log($"Subscribe1");
                var subscription1 = connectableSource
                    .Subscribe(
                        i => UnityEngine.Debug.Log($"1## OnNext({i})"),
                        ex => UnityEngine.Debug.Log($"1## OnException({ex})"),
                        () => UnityEngine.Debug.Log($"1## OnCompleted()")
                    );
                UnityEngine.Debug.Log($"Subscribe2");
                var subscription2 = connectableSource
                    .Subscribe(
                        i => UnityEngine.Debug.Log($"2## OnNext({i})"),
                        ex => UnityEngine.Debug.Log($"2## OnException({ex})"),
                        () => UnityEngine.Debug.Log($"2## OnCompleted()")
                    );

                // ↓Connectを行うと､はじめて各購読が実行される
                UnityEngine.Debug.Log($"Connect");
                connectableSource.Connect();

                // ↓の購読は､もうConnectが完了した後なので､OnCompletedだけ流れる
                UnityEngine.Debug.Log($"Subscribe3");
                var subscription3 = connectableSource
                    .Subscribe(
                        i => UnityEngine.Debug.Log($"3## OnNext({i})"),
                        ex => UnityEngine.Debug.Log($"3## OnException({ex})"),
                        () => UnityEngine.Debug.Log($"3## OnCompleted()")
                    );
            }
        }

        [ContextMenu(nameof(Publish2))]
        async UniTask Publish2()
        {
            // Publishの例その2.
            // Connect後に時間をおいたり､再度Connectした場合の例
            var connectableSource = Observable
                .Interval(TimeSpan.FromMilliseconds(500))
                .Publish();
            
            UnityEngine.Debug.Log($"Connect");
            using (connectableSource.Connect())
            {
                UnityEngine.Debug.Log($"Sleep 2000 ms");
                await UniTask.Delay(TimeSpan.FromMilliseconds(2000));
                
                // ちょっと時間を置いてから､購読をする.
                // →流れる値に注目! Delayをかけているため 4,5 など途中から値が発行されている
                UnityEngine.Debug.Log($"Subscribe1");
                using (connectableSource.Subscribe(
                           i => UnityEngine.Debug.Log($"OnNext({i})"),
                           () => UnityEngine.Debug.Log($"OnCompleted()")))
                {
                    UnityEngine.Debug.Log($"Sleep 2000 ms");
                    await UniTask.Delay(TimeSpan.FromMilliseconds(2000));
                }
                UnityEngine.Debug.Log($"UnSubscribe1");
            }
            UnityEngine.Debug.Log($"DisConnect");

            // 2回目の購読. 既にIConnectableObservableは破棄済みのため､何も流れない
            UnityEngine.Debug.Log($"Subscribe2");
            using (connectableSource.Subscribe(
                       i => UnityEngine.Debug.Log($"OnNext({i})"),
                       () => UnityEngine.Debug.Log($"OnCompleted()")))
            {
                UnityEngine.Debug.Log($"Sleep 2000 ms");
                await UniTask.Delay(TimeSpan.FromMilliseconds(2000));
            }
            UnityEngine.Debug.Log($"UnSubscribe2");

            UnityEngine.Debug.Log($"Connect");
            using (connectableSource.Connect())
            {
                // 3回目の購読. 再度Connectを行っているため､値が流れる
                UnityEngine.Debug.Log($"Subscribe3");
                using (connectableSource.Subscribe(
                           i => UnityEngine.Debug.Log($"OnNext({i})"),
                           () => UnityEngine.Debug.Log($"OnCompleted()")))
                {
                    UnityEngine.Debug.Log($"Sleep 2000 ms");
                    await UniTask.Delay(TimeSpan.FromMilliseconds(2000));
                }
                UnityEngine.Debug.Log($"UnSubscribe3");
            }
            UnityEngine.Debug.Log($"DisConnect");
        }

        [ContextMenu(nameof(RefCount))]
        async UniTask RefCount()
        {
            // IConnectableObservable<T>には､RefCountというメソッドがある.
            // これを使うと､参照カウント付きのConnectを実現してくれる
            var source = Observable
                .Interval(TimeSpan.FromMilliseconds(500))
                .Publish()
                .RefCount();

            UnityEngine.Debug.Log($"Sleep 1000 ms");
            await UniTask.Delay(TimeSpan.FromMilliseconds(1000));

            // 参照カウント+1(1つでも購読された時点)で勝手にConnectしてくれる
            UnityEngine.Debug.Log($"Subscribe1");
            using (source.Subscribe(
                       i => UnityEngine.Debug.Log($"1## OnNext({i})"),
                       () => UnityEngine.Debug.Log($"1## OnCompleted()")))
            {
                UnityEngine.Debug.Log($"Sleep 1000 ms");
                await UniTask.Delay(TimeSpan.FromMilliseconds(1000));

                UnityEngine.Debug.Log($"Subscribe2");
                using (source.Subscribe(
                           i => UnityEngine.Debug.Log($"2## OnNext({i})"),
                           () => UnityEngine.Debug.Log($"2## OnCompleted()")))
                {
                    UnityEngine.Debug.Log($"Sleep 1000 ms");
                    await UniTask.Delay(TimeSpan.FromMilliseconds(1000));
                }
                UnityEngine.Debug.Log($"UnSubscribe2");
            }
            UnityEngine.Debug.Log($"UnSubscribe1");

            // 参照カウントが0になった後も､再度購読されればConnectされる
            UnityEngine.Debug.Log($"Subscribe3");
            using (source.Subscribe(
                       i => UnityEngine.Debug.Log($"3## OnNext({i})"),
                       () => UnityEngine.Debug.Log($"3## OnCompleted()")))
            {
                UnityEngine.Debug.Log($"Sleep 1000 ms");
                await UniTask.Delay(TimeSpan.FromMilliseconds(1000));
            }
            UnityEngine.Debug.Log($"UnSubscribe3");
        }

        [ContextMenu(nameof(PublishInitialValue))]
        async UniTask PublishInitialValue()
        {
            var connectableSource = Observable
                .Range(1, 3)
                // Publishには初期値を取るオーバーロードがある
                .Publish(0);

            // 初期値は､Subscribeした時点で流れる(Connectした時点じゃないので注意)
            UnityEngine.Debug.Log($"Subscribe");
            connectableSource.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i})"),
                () => UnityEngine.Debug.Log($"OnCompleted()"));
            
            UnityEngine.Debug.Log($"Sleep 1000 ms");
            await UniTask.Delay(TimeSpan.FromMilliseconds(1000));

            UnityEngine.Debug.Log($"Connect");
            connectableSource.Connect();
        }
        
        [ContextMenu(nameof(PublishLast))]
        void PublishLast()
        {
            // PublishLastは､最後に発行した値を流すメソッド
            var connectableSource = Observable
                .Range(1, 3)
                .PublishLast();

            UnityEngine.Debug.Log($"Subscribe1");
            connectableSource.Subscribe(
                i => UnityEngine.Debug.Log($"1## OnNext({i})"),
                () => UnityEngine.Debug.Log($"1## OnCompleted()"));

            UnityEngine.Debug.Log($"Connect");
            connectableSource.Connect();

            // PublishLastの場合は､Connect後の購読でも値が流れる. 最後の値をキャッシュするのでこういう挙動に､ということらしい.
            // 通常のPublishと挙動が違うので､いい感じはしない.
            UnityEngine.Debug.Log($"Subscribe2");
            connectableSource.Subscribe(
                i => UnityEngine.Debug.Log($"2## OnNext({i})"),
                () => UnityEngine.Debug.Log($"2## OnCompleted()"));
        }

        [ContextMenu(nameof(Replay))]
        async UniTask Replay()
        {
            // Replayは､Publishと同じくCold->Hot変換する特性を持ちつつ､それまで発行された値を覚えておいて､後から購読したストリームに流す
            var connectableSource = Observable
                .Interval(TimeSpan.FromMilliseconds(500))
                .Replay();

            UnityEngine.Debug.Log($"Sleep 2000 ms");
            await UniTask.Delay(TimeSpan.FromMilliseconds(2000));

            UnityEngine.Debug.Log($"Subscribe1");
            using (connectableSource.Subscribe(
                       i => UnityEngine.Debug.Log($"1## OnNext({i})"),
                       () => UnityEngine.Debug.Log($"1## OnCompleted()")))
            {
                UnityEngine.Debug.Log($"Connect");
                connectableSource.Connect();

                UnityEngine.Debug.Log($"Sleep 2000 ms");
                await UniTask.Delay(TimeSpan.FromMilliseconds(2000));

                // 後からの購読だが､それまでに発行された値が全て流れてくれる
                UnityEngine.Debug.Log($"Subscribe2");
                using (connectableSource.Subscribe(
                           i => UnityEngine.Debug.Log($"2## OnNext({i})"),
                           () => UnityEngine.Debug.Log($"2## OnCompleted()")))
                {
                    UnityEngine.Debug.Log($"Sleep 2000 ms");
                    await UniTask.Delay(TimeSpan.FromMilliseconds(2000));
                }
                UnityEngine.Debug.Log($"UnSubscribe2");

                UnityEngine.Debug.Log($"Sleep 2000 ms");
                await UniTask.Delay(TimeSpan.FromMilliseconds(2000));
            }
            UnityEngine.Debug.Log($"UnSubscribe1");
        }

        // MulticastもHot変換を行う拡張メソッド.
        // 引数に様々なSubject(IObservable)を取って､振る舞いを変えることができる
        [ContextMenu(nameof(MultiCast))]
        async UniTask MultiCast()
        {
            // Subjectを渡した場合は､PublishによるHot変換と全く同じ
            var connectableSource = Observable
                .Range(1, 3)
                .Multicast(new Subject<int>());

            UnityEngine.Debug.Log($"Subscribe");
            connectableSource.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i})"),
                () => UnityEngine.Debug.Log($"OnCompleted()"));

            UnityEngine.Debug.Log($"Connect");
            connectableSource.Connect();
        }
        
        [ContextMenu(nameof(MultiCast_BehaviourSubject))]
        async UniTask MultiCast_BehaviourSubject()
        {
            // BehaviourSubjectは､初期値持ちのSubject
            // この場合は､Publishに初期値を渡した場合と全く同じ挙動になる
            var connectableSource = Observable
                .Range(1, 3)
                .Multicast(new BehaviorSubject<int>(0));

            UnityEngine.Debug.Log($"Subscribe");
            connectableSource.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i})"),
                () => UnityEngine.Debug.Log($"OnCompleted()"));
            
            UnityEngine.Debug.Log($"Sleep 1000 ms");
            await UniTask.Delay(TimeSpan.FromMilliseconds(1000));
        
            UnityEngine.Debug.Log($"Connect");
            connectableSource.Connect();
        }
        
        [ContextMenu(nameof(MultiCast_AsyncSubject))]
        void MultiCast_AsyncSubject()
        {
            // AsyncSubjectは､最後の値をキャッシュして流すSubject
            // この場合は､PublishLastの場合と全く同じ挙動になる
            var connectableSource = Observable
                .Range(1, 3)
                .Multicast(new AsyncSubject<int>());

            UnityEngine.Debug.Log($"Subscribe1");
            connectableSource.Subscribe(
                i => UnityEngine.Debug.Log($"1## OnNext({i})"),
                () => UnityEngine.Debug.Log($"1## OnCompleted()"));

            UnityEngine.Debug.Log($"Connect");
            connectableSource.Connect();

            // Connect後の購読でも､最後の値が流れてくれる...
            UnityEngine.Debug.Log($"Subscribe2");
            connectableSource.Subscribe(
                i => UnityEngine.Debug.Log($"2## OnNext({i})"),
                () => UnityEngine.Debug.Log($"2## OnCompleted()"));
        }
        
        
        [ContextMenu(nameof(MultiCast_ReplaySubject))]
        async UniTask MultiCast_ReplaySubject()
        {
            // ReplaySubjectは､発行された値を全てキャッシュしておいて､後からの購読にも同じく流すメソッド
            // この場合は､Replayの場合と全く同じ挙動になる
            var connectableSource = Observable
                .Interval(TimeSpan.FromMilliseconds(500))
                .Multicast(new ReplaySubject<long>());

            UnityEngine.Debug.Log($"Sleep 2000 ms");
            await UniTask.Delay(TimeSpan.FromMilliseconds(2000));

            UnityEngine.Debug.Log($"Subscribe1");
            using (connectableSource.Subscribe(
                       i => UnityEngine.Debug.Log($"1## OnNext({i})"),
                       () => UnityEngine.Debug.Log($"1## OnCompleted()")))
            {
                UnityEngine.Debug.Log($"Connect");
                connectableSource.Connect();

                UnityEngine.Debug.Log($"Sleep 2000 ms");
                await UniTask.Delay(TimeSpan.FromMilliseconds(2000));

                // 後からの購読で､ドバっとそれまでの値が流れる
                UnityEngine.Debug.Log($"Subscribe2");
                using (connectableSource.Subscribe(
                           i => UnityEngine.Debug.Log($"2## OnNext({i})"),
                           () => UnityEngine.Debug.Log($"2## OnCompleted()")))
                {
                    UnityEngine.Debug.Log($"Sleep 2000 ms");
                    await UniTask.Delay(TimeSpan.FromMilliseconds(2000));
                }
                UnityEngine.Debug.Log($"UnSubscribe2");

                UnityEngine.Debug.Log($"Sleep 2000 ms");
                await UniTask.Delay(TimeSpan.FromMilliseconds(2000));
            }
            UnityEngine.Debug.Log($"UnSubscribe1");
        }
    }
}