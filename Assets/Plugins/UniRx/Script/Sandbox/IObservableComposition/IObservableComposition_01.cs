using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Sandbox.IObservableComposition
{
    public class IObservableComposition_01 : MonoBehaviour
    {
        [ContextMenu(nameof(Merge))]
        void Merge()
        {
            // Mergeは､複数のIObservableを一つにまとめる拡張メソッド.値が流れる順番は順不同.

            // var subscription = Observable
            //     .Merge(
            //         Observable.Range(0, 3),
            //         Observable.Range(10, 3),
            //         Observable.Range(100, 3))
            //     .Subscribe(
            //         i => UnityEngine.Debug.Log($"OnNext({i})"),
            //         ex => UnityEngine.Debug.Log($"OnError({ex})"),
            //         () => UnityEngine.Debug.Log($"OnCompleted()"));
            
            var subscription = Observable
                .Merge(
                    Observable.Interval(TimeSpan.FromMilliseconds(200)).Select(i => $"1st:{i}").Take(3),
                    Observable.Interval(TimeSpan.FromMilliseconds(100)).Select(i => $"2nd:{i}").Take(3),
                    Observable.Interval(TimeSpan.FromMilliseconds(300)).Select(i => $"3rd:{i}").Take(3))
                .Subscribe(
                    i => UnityEngine.Debug.Log($"OnNext({i})"),
                    ex => UnityEngine.Debug.Log($"OnError({ex})"),
                    () => UnityEngine.Debug.Log($"OnCompleted()"));
        }

        [ContextMenu(nameof(SelectMany))]
        void SelectMany()
        {
            // SelectManyは､あるIObservableを変換することができる拡張メソッド
            var source = new Subject<int>();
            var subscription = source
                .SelectMany(i => Observable
                    .Interval(TimeSpan.FromMilliseconds(1000))
                    .Take(3)
                    .Select(l => i * (l + 1)))
                .Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i})"),
                ex => UnityEngine.Debug.Log($"OnError({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()"));

            UnityEngine.Debug.Log("OnNext 10");
            source.OnNext(10);
            UnityEngine.Debug.Log("OnNext 100");
            source.OnNext(100);
            UnityEngine.Debug.Log("OnNext 1000");
            source.OnNext(1000);
        }
        
        [ContextMenu(nameof(Switch))]
        async UniTask Switch()
        {
            // SwitchはMergeと似ているが､IObservable<T>が値を流している最中に､
            // 他のIObservable<T>から値を流すようになれば､元のIObservable<T>から値を流すようにするのをやめる.
            // 以下URLがわかりやすいので参照する.
            // https://shitakami.hatenablog.com/entry/2021/08/22/204549
            var source = new Subject<int>();
            var subscription = source
                .Select(i => Observable
                    .Interval(TimeSpan.FromMilliseconds(1000))
                    .Take(3)
                    .Select(l => i * (l + 1)))
                .Switch()
                .Subscribe(
                    i => UnityEngine.Debug.Log($"OnNext({i})"),
                    ex => UnityEngine.Debug.Log($"OnError({ex})"),
                    () => UnityEngine.Debug.Log($"OnCompleted()"));
            
            // 以下の例では､｢10,20,30｣ ｢100,200,300｣のストリームが全部流れず､途中で打ち切られることを確認する
            
            source.OnNext(10);

            await UniTask.Delay(TimeSpan.FromMilliseconds(2000));

            source.OnNext(100);
            
            await UniTask.Delay(TimeSpan.FromMilliseconds(2000));
            
            source.OnNext(1000);
            
            await UniTask.Delay(TimeSpan.FromMilliseconds(2000));

            source.OnCompleted();
        }

        [ContextMenu(nameof(Concat))]
        async UniTask Concat()
        {
            // ConcatはMergeと同じく､複数のIObservableを一つにまとめる拡張メソッドなのだが､値が流れる順は直列
            var subscription = Observable
                .Concat(
                // Mergeにした際と比較してみること
                //.Merge(
                    Observable.Interval(TimeSpan.FromMilliseconds(200)).Select(i => $"1st:{i}").Take(3),
                    Observable.Interval(TimeSpan.FromMilliseconds(100)).Select(i => $"2nd:{i}").Take(3),
                    Observable.Interval(TimeSpan.FromMilliseconds(300)).Select(i => $"3rd:{i}").Take(3))
                .Subscribe(
                    i => UnityEngine.Debug.Log($"OnNext({i})"),
                    ex => UnityEngine.Debug.Log($"OnError({ex})"),
                    () => UnityEngine.Debug.Log($"OnCompleted()"));
        }

        [ContextMenu(nameof(Amb))]
        async UniTask Amb()
        {
            // Ambも複数のIObservable<T>を引数に取るが､最初に値を発行したいずれかのIObservable<T>だけを採用して､残りは無視する.
            var subscription = Observable
                .Amb(
                // Mergeにした際と比較してみること
                //.Merge(
                    Observable.Interval(TimeSpan.FromMilliseconds(200)).Select(i => $"1st:{i}").Take(3),
                    Observable.Interval(TimeSpan.FromMilliseconds(100)).Select(i => $"2nd:{i}").Take(3),
                    Observable.Interval(TimeSpan.FromMilliseconds(300)).Select(i => $"3rd:{i}").Take(3))
                .Subscribe(
                    i => UnityEngine.Debug.Log($"OnNext({i})"),
                    ex => UnityEngine.Debug.Log($"OnError({ex})"),
                    () => UnityEngine.Debug.Log($"OnCompleted()"));
        }
        
        [ContextMenu(nameof(Zip))]
        async UniTask Zip()
        {
            // Zipは､2つのIObservable<T>を合成する.
            // 第一引数に2つめのIObservable<T>､第二引数に合成用の述語を取る感じ.
            // LINQのZipメソッドと似たようなイメージ.
            // また､2つのIObservable<T>の実行が完全に終わった段階ではじめて値が流れる.
            {
                var subscription = Observable
                    .Interval(TimeSpan.FromMilliseconds(100)).Take(3)
                    .Zip(Observable.Range(100, 3),
                        (l, i) => $"{l} - {i}")
                    .Subscribe(
                        i => UnityEngine.Debug.Log($"OnNext({i})"),
                        ex => UnityEngine.Debug.Log($"OnError({ex})"),
                        () => UnityEngine.Debug.Log($"OnCompleted()"));
            }

            await UniTask.Delay(TimeSpan.FromMilliseconds(500));
            UnityEngine.Debug.Log("====");

            // いずれか2つのIObservable<T>からはみ出る要素は無視される.
            {
                var subscription = Observable
                    .Interval(TimeSpan.FromMilliseconds(100)).Take(3)
                    .Zip(Observable.Return(100),
                        (l, i) => $"{l} - {i}")
                    .Subscribe(
                        i => UnityEngine.Debug.Log($"OnNext({i})"),
                        ex => UnityEngine.Debug.Log($"OnError({ex})"),
                        () => UnityEngine.Debug.Log($"OnCompleted()"));
            }
        }
        
        [ContextMenu(nameof(CombineLatest))]
        async UniTask CombineLatest()
        {
            // CombineLatestは､Zipと似ていて2つのIObservable<T>を合成する.
            // CombineLatestの場合は､両方のIObservable<T>の値を待たずに､もう片方のIObservable<T>の最新の値を採用すること.
            var source1 = new Subject<string>();
            var source2 = new Subject<int>();
            
            source1
                .CombineLatest(source2, (s, i) => $"{s} - {i}")
                // Zipにした時､違う結果になるので比較してみること
                //.Zip(source2, (s, i) => $"{s} - {i}")
                .Subscribe(
                    i => UnityEngine.Debug.Log($"OnNext({i})"),
                    ex => UnityEngine.Debug.Log($"OnError({ex})"),
                    () => UnityEngine.Debug.Log($"OnCompleted()"));
            
            source1.OnNext("a");
            source2.OnNext(100);
            source2.OnNext(200);
            source1.OnNext("b");
            source2.OnNext(300);
            source1.OnNext("c");
        }

        [ContextMenu(nameof(StartWith))]
        async UniTask StartWith()
        {
            // StartWithは､あるIObservableの前に､任意個の値を差し込めるメソッド
            var subscription = Observable.Range(3,3)
                .StartWith(new int [] {0, 1, 2})
                .Subscribe(
                    i => UnityEngine.Debug.Log($"OnNext({i})"),
                    ex => UnityEngine.Debug.Log($"OnError({ex})"),
                    () => UnityEngine.Debug.Log($"OnCompleted()"));
        }
    }
}