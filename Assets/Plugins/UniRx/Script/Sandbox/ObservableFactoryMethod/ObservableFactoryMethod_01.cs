using System;
using UniRx;
using UnityEngine;

namespace Sandbox.ObservableFactoryMethod
{
    // 参考:
    // https://blog.okazuki.jp/entry/2015/03/23/203825

    // Observableのファクトリメソッドの特徴:
    //     購読した時点で即実行される､OnCompleteも呼ばれて完了扱いに
    //     再度購読したら､また同じ処理が呼ばれる
    //     購読が終わった時点で､即破棄処理が呼ばれる cf)Createメソッド
    public class ObservableFactoryMethod_01 : MonoBehaviour
    {
        [ContextMenu(nameof(Return))]
        void Return()
        {
            // Returnは､値をそのまま返す
            var source = Observable.Return(10);
            var subscription = source.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i})"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );
            // 購読の破棄自体に､特に意味はない
            subscription.Dispose();
        }

        [ContextMenu(nameof(Repeat))]
        void Repeat()
        {
            // Repeatは､値を指定回繰り返す
            var source = Observable.Repeat("Repeat!", 3);
            var subscription = source.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i})"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );
            subscription.Dispose();
        }

        [ContextMenu(nameof(Range))]
        void Range()
        {
            // Rangeは指定した範囲で値を実行
            var source = Observable.Range(0, 10);
            var subscription = source.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i})"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );
            subscription.Dispose();
        }

        [ContextMenu(nameof(Defer))]
        void Defer()
        {
            // Deferは名前の通りで､遅延実行用のObservable.
            // 購読させるタイミングで､毎回ラムダに指定した内容が実行される.
            var source = Observable.Defer<int>(() =>
            {
                // ReplaySubjectは､特殊なSubjectクラス.
                // 購読されるまでの操作を覚えておいて､購読された時点でそれまでの操作を実行する
                var s = new ReplaySubject<int>();
                s.OnNext(100);
                s.OnNext(200);
                s.OnNext(300);
                s.OnCompleted();
                return s.AsObservable();
            });
            var subscription = source.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i})"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );
            subscription.Dispose();
            subscription = source.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i})"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );
            subscription.Dispose();
        }

        [ContextMenu(nameof(Create))]
        void Create()
        {
            // Createは､購読時に引数で指定したIObserverの処理を実行する.
            var source = Observable.Create<int>(observer =>
            {
                observer.OnNext(1);
                observer.OnNext(3);
                observer.OnNext(5);
                observer.OnCompleted();
                // 購読解除時の処理を､IDisposableとして返す
                // 他のファクトリメソッドもそうだが､購読解除は､購読時に即行われる
                return Disposable.Create(() => UnityEngine.Debug.Log("Disposed"));
            });
            var subscription = source.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i})"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );
            subscription.Dispose();
            subscription = source.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i})"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );
            subscription.Dispose();
        }

        [ContextMenu(nameof(Throw))]
        void Throw()
        {
            // Throwは､購読時に例外を発生させる
            var source = Observable.Throw<int>(new Exception());
            var subscription = source.Subscribe(
                i => UnityEngine.Debug.Log($"OnNext({i})"),
                ex => UnityEngine.Debug.Log($"OnException({ex})"),
                () => UnityEngine.Debug.Log($"OnCompleted()")
            );
            subscription.Dispose();
        }
    }
}


